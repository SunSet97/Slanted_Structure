﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

namespace Naninovel
{
    /// <summary>
    /// A mock <see cref="IResourceProvider"/> implementation allowing to add resources at runtime.
    /// </summary>
    public class VirtualResourceProvider : IResourceProvider
    {
        /// <summary>
        /// Whether <see cref="UnloadResource(string)"/> and similar methods should remove the added resources.
        /// </summary>
        public bool RemoveResourcesOnUnload { get; set; } = true;
        public bool IsLoading => false;
        public float LoadProgress => 1;
        public IEnumerable<Resource> LoadedResources => Resources?.Values;

        #pragma warning disable 0067
        public event Action<float> OnLoadProgress;
        public event Action<string> OnMessage;
        #pragma warning restore 0067

        protected readonly Dictionary<string, Resource> Resources;
        protected readonly HashSet<string> FolderPaths;

        public VirtualResourceProvider ()
        {
            Resources = new Dictionary<string, Resource>();
            FolderPaths = new HashSet<string>();
        }

        public bool SupportsType<T> () where T : UnityEngine.Object => true;

        public void AddResource (string path, UnityEngine.Object obj)
        {
            Resources[path] = new Resource(path, obj, this);
        }

        public void RemoveResource (string path)
        {
            Resources.Remove(path);
        }

        public void RemoveAllResources ()
        {
            Resources.Clear();
        }

        public void AddFolder (string folderPath)
        {
            FolderPaths.Add(folderPath);
        }

        public void RemoveFolder (string path)
        {
            FolderPaths.Remove(path);
        }

        public Resource<T> LoadResource<T> (string path) where T : UnityEngine.Object
        {
            return Resources.TryGetValue(path, out var resource) ? resource as Resource<T> : null;
        }

        public UniTask<Resource<T>> LoadResourceAsync<T> (string path) where T : UnityEngine.Object
        {
            var resource = LoadResource<T>(path);
            return UniTask.FromResult(resource);
        }

        public IEnumerable<Resource<T>> LoadResources<T> (string path) where T : UnityEngine.Object
        {
            return Resources.Where(kv => kv.Value is T).Select(kv => kv.Key).LocateResourcePathsAtFolder(path).Select(p => LoadResource<T>(p));
        }

        public UniTask<IEnumerable<Resource<T>>> LoadResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            var resoucres = LoadResources<T>(path);
            return UniTask.FromResult(resoucres);
        }

        public IEnumerable<Folder> LocateFolders (string path)
        {
            return FolderPaths.LocateFolderPathsAtFolder(path).Select(p => new Folder(p));
        }

        public UniTask<IEnumerable<Folder>> LocateFoldersAsync (string path)
        {
            var folders = LocateFolders(path);
            return UniTask.FromResult(folders);
        }

        public IEnumerable<string> LocateResources<T> (string path) where T : UnityEngine.Object
        {
            return Resources.Where(kv => kv.Value is T).Select(kv => kv.Key).LocateResourcePathsAtFolder(path);
        }

        public UniTask<IEnumerable<string>> LocateResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            var resources = LocateResources<T>(path);
            return UniTask.FromResult(resources);
        }

        public bool ResourceExists<T> (string path) where T : UnityEngine.Object
        {
            return Resources.ContainsKey(path) && Resources[path] is T;
        }

        public UniTask<bool> ResourceExistsAsync<T> (string path) where T : UnityEngine.Object
        {
            var result = ResourceExists<T>(path);
            return UniTask.FromResult(result);
        }

        public bool ResourceLoaded (string path)
        {
            return ResourceExists<UnityEngine.Object>(path);
        }

        public bool ResourceLoading (string path)
        {
            return false;
        }

        public void UnloadResource (string path)
        {
            if (RemoveResourcesOnUnload)
                RemoveResource(path);
        }

        public UniTask UnloadResourceAsync (string path)
        {
            UnloadResource(path);
            return UniTask.CompletedTask;
        }

        public void UnloadResources ()
        {
            if (RemoveResourcesOnUnload)
                RemoveAllResources();
        }

        public UniTask UnloadResourcesAsync ()
        {
            UnloadResources();
            return UniTask.CompletedTask;
        }

        public Resource<T> GetLoadedResourceOrNull<T> (string path) where T : UnityEngine.Object
        {
            if (!ResourceLoaded(path)) return null;
            return LoadResource<T>(path);
        }
    }
}
