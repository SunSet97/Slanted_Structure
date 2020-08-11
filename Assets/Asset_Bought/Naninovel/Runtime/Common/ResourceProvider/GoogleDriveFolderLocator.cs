﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

#if UNITY_GOOGLE_DRIVE_AVAILABLE

using System.Collections.Generic;
using UniRx.Async;
using UnityGoogleDrive;

namespace Naninovel
{
    public class GoogleDriveFolderLocator : LocateFoldersRunner
    {
        public readonly string RootPath;

        public GoogleDriveFolderLocator (IResourceProvider provider, string rootPath, string resourcesPath)
            : base (provider, resourcesPath)
        {
            RootPath = rootPath;
        }

        public override async UniTask RunAsync ()
        {
            var result = new List<Folder>();

            var fullpath = PathUtils.Combine(RootPath, Path) + "/";
            var gFolders = await Helpers.FindFilesByPathAsync(fullpath, fields: new List<string> { "files(name)" }, mime: "application/vnd.google-apps.folder");

            foreach (var gFolder in gFolders)
            {
                var folderPath = string.IsNullOrEmpty(Path) ? gFolder.Name : string.Concat(Path, '/', gFolder.Name);
                var folder = new Folder(folderPath);
                result.Add(folder);
            }

            SetResult(result);
        }
    }
}

#endif
