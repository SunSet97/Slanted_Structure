﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

#if UNITY_GOOGLE_DRIVE_AVAILABLE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Networking;
using UnityGoogleDrive;

namespace Naninovel
{
    public class GoogleDriveResourceLoader<TResource> : LoadResourceRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        public readonly string RootPath;

        private static readonly Type[] nativeRequestTypes = new[] { typeof(AudioClip), typeof(Texture2D) };

        private TResource loadedObject;
        private bool useNativeRequests;
        private Action<string> logAction;
        private GoogleDriveRequest downloadRequest;
        private IRawConverter<TResource> converter;
        private RawDataRepresentation usedRepresentation;
        private GoogleDriveResourceProvider.CacheManifest cacheManifest;
        private byte[] rawData;

        public GoogleDriveResourceLoader (IResourceProvider provider, string rootPath, string resourcePath,
            IRawConverter<TResource> converter, Action<string> logAction) : base (provider, resourcePath)
        {
            RootPath = rootPath;
            useNativeRequests = nativeRequestTypes.Contains(typeof(TResource));
            this.logAction = logAction;

            // MP3 is not supported in native requests on the standalone platforms. Fallback to raw converters.
            #if UNITY_STANDALONE || UNITY_EDITOR
            foreach (var r in converter.Representations)
                if (WebUtils.EvaluateAudioTypeFromMime(r.MimeType) == AudioType.MPEG) useNativeRequests = false;
            #endif

            this.converter = converter;
            usedRepresentation = new RawDataRepresentation();
        }

        public override async UniTask RunAsync ()
        {
            var startTime = Time.time;
            var usedCache = false;

            // Check if cached version of the file could be used.
            rawData = await TryLoadFileCacheAsync(Path);

            // Cached version is not valid or doesn't exist; download or export the file.
            if (rawData is null)
            {
                // 4. Load file metadata from Google Drive.
                var filePath = string.IsNullOrEmpty(RootPath) ? Path : string.Concat(RootPath, '/', Path);
                var fileMeta = await GetFileMetaAsync(filePath);
                if (fileMeta is null)
                {
                    Debug.LogError($"Failed to resolve '{filePath}' Google Drive metadata.");
                    SetResult(new Resource<TResource>(Path, null, Provider));
                    return;
                }

                if (converter is IGoogleDriveConverter<TResource>) rawData = await ExportFileAsync(fileMeta);
                else rawData = await DownloadFileAsync(fileMeta);

                // 5. Cache the downloaded file.
                await WriteFileCacheAsync(Path, fileMeta.Id, rawData);
            }
            else usedCache = true;

            // In case we used native requests the resource will already be set, so no need to use converters.
            if (!ObjectUtils.IsValid(loadedObject))
                loadedObject = await converter.ConvertAsync(rawData, System.IO.Path.GetFileNameWithoutExtension(Path));

            var result = new Resource<TResource>(Path, loadedObject, Provider);
            SetResult(result);

            logAction?.Invoke($"Resource '{Path}' loaded {StringUtils.FormatFileSize(rawData.Length)} over {Time.time - startTime:0.###} seconds from " + (usedCache ? "cache." : "Google Drive."));

            if (downloadRequest != null)
                downloadRequest.Dispose();
        }

        public override void Cancel ()
        {
            base.Cancel();

            if (downloadRequest != null)
            {
                downloadRequest.Abort();
                downloadRequest = null;
            }
        }

        private async UniTask<UnityGoogleDrive.Data.File> GetFileMetaAsync (string filePath)
        {
            foreach (var representation in converter.Representations)
            {
                var fullPath = string.Concat(filePath, representation.Extension);
                var files = await Helpers.FindFilesByPathAsync(fullPath, fields: new List<string> { "files(id, mimeType, modifiedTime)" }, mime: representation.MimeType);
                if (files.Count > 1) Debug.LogWarning($"Multiple '{fullPath}' files been found in Google Drive.");
                if (files.Count > 0) { usedRepresentation = representation; return files[0]; }
            }

            Debug.LogError($"Failed to retrieve '{Path}' resource from Google Drive.");
            return null;
        }

        private async UniTask<byte[]> DownloadFileAsync (UnityGoogleDrive.Data.File fileMeta)
        {
            if (useNativeRequests)
            {
                if (typeof(TResource) == typeof(AudioClip)) downloadRequest = GoogleDriveFiles.DownloadAudio(fileMeta.Id, WebUtils.EvaluateAudioTypeFromMime(fileMeta.MimeType));
                else if (typeof(TResource) == typeof(Texture2D)) downloadRequest = GoogleDriveFiles.DownloadTexture(fileMeta.Id, true);
            }
            else downloadRequest = new GoogleDriveFiles.DownloadRequest(fileMeta);

            await downloadRequest.SendNonGeneric();
            if (downloadRequest.IsError || downloadRequest.GetResponseData<UnityGoogleDrive.Data.File>().Content == null)
            {
                Debug.LogError($"Failed to download {Path}{usedRepresentation.Extension} resource from Google Drive.");
                return null;
            }

            if (useNativeRequests)
            {
                if (typeof(TResource) == typeof(AudioClip)) loadedObject = downloadRequest.GetResponseData<UnityGoogleDrive.Data.AudioFile>().AudioClip as TResource;
                else if (typeof(TResource) == typeof(Texture2D)) loadedObject = downloadRequest.GetResponseData<UnityGoogleDrive.Data.TextureFile>().Texture as TResource;
            }

            return downloadRequest.GetResponseData<UnityGoogleDrive.Data.File>().Content;
        }

        private async UniTask<byte[]> ExportFileAsync (UnityGoogleDrive.Data.File fileMeta)
        {
            Debug.Assert(converter is IGoogleDriveConverter<TResource>);

            var gDriveConverter = converter as IGoogleDriveConverter<TResource>;

            downloadRequest = new GoogleDriveFiles.ExportRequest(fileMeta.Id, gDriveConverter.ExportMimeType);
            await downloadRequest.SendNonGeneric();
            if (downloadRequest.IsError || downloadRequest.GetResponseData<UnityGoogleDrive.Data.File>().Content == null)
            {
                Debug.LogError($"Failed to export '{Path}' resource from Google Drive.");
                return null;
            }
            return downloadRequest.GetResponseData<UnityGoogleDrive.Data.File>().Content;
        }

        private async UniTask<byte[]> TryLoadFileCacheAsync (string resourcePath)
        {
            resourcePath = resourcePath.Replace("/", GoogleDriveResourceProvider.SlashReplace);
            var filePath = string.Concat(GoogleDriveResourceProvider.CacheDirPath, "/", resourcePath);
            //if (!string.IsNullOrEmpty(usedRepresentation.Extension))
            //    filePath += string.Concat(".", usedRepresentation.Extension);
            if (!File.Exists(filePath)) return null;

            if (useNativeRequests)
            {
                // Web requests over IndexedDB are not supported; we should either use raw converters or disable caching.
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    // Binary convertion of the audio is fucked on WebGL (can't use buffers), so disable caching here.
                    if (typeof(TResource) == typeof(AudioClip)) return null;
                    // Use raw converters for other native types.
                    return await IOUtils.ReadFileAsync(filePath);
                }

                UnityWebRequest request = null;
                if (typeof(TResource) == typeof(AudioClip)) request = UnityWebRequestMultimedia.GetAudioClip(filePath, WebUtils.EvaluateAudioTypeFromMime(converter.Representations[0].MimeType));
                else if (typeof(TResource) == typeof(Texture2D)) request = UnityWebRequestTexture.GetTexture(filePath, true);
                using (request)
                {
                    await request.SendWebRequest();

                    if (typeof(TResource) == typeof(AudioClip)) loadedObject = DownloadHandlerAudioClip.GetContent(request) as TResource;
                    else if (typeof(TResource) == typeof(Texture2D)) loadedObject = DownloadHandlerTexture.GetContent(request) as TResource;
                    return request.downloadHandler.data;
                }
            }
            else return await IOUtils.ReadFileAsync(filePath);
        }

        private async UniTask WriteFileCacheAsync (string resourcePath, string fileId, byte[] fileRawData)
        {
            resourcePath = resourcePath.Replace("/", GoogleDriveResourceProvider.SlashReplace);
            var filePath = string.Concat(GoogleDriveResourceProvider.CacheDirPath, "/", resourcePath);
            //if (!string.IsNullOrEmpty(usedRepresentation.Extension))
            //    filePath += string.Concat(".", usedRepresentation.Extension);

            await IOUtils.WriteFileAsync(filePath, fileRawData);

            // Add info for the smart caching policy.
            if (cacheManifest == null) cacheManifest = await GoogleDriveResourceProvider.CacheManifest.ReadOrCreateAsync();
            cacheManifest[fileId] = resourcePath;
            await cacheManifest.WriteAsync();
        }
    }
}

#endif
