﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

#if ADDRESSABLES_AVAILABLE

using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Naninovel
{
    public class AddressableResourceProvider : ResourceProvider
    {
        /// <summary>
        /// All the assets managed by this provider should have this label assigned, 
        /// also their addresses are expected to start with the label followed by a slash.
        /// </summary>
        public readonly string MainLabel;
        /// <summary>
        /// When specified, the provider will only work with assets that have the set of labels.
        /// </summary>
        public readonly string[] ExtraLabels;

        private List<IResourceLocation> locations;

        public AddressableResourceProvider (string mainLabel = "UNITY_COMMON", string[] extraLabels = null)
        {
            MainLabel = mainLabel;
            if (extraLabels != null && extraLabels.Length > 0)
            {
                var labels = new List<string>(extraLabels);
                labels.Add(mainLabel);
                ExtraLabels = labels.ToArray();
            }
        }

        public override bool SupportsType<T> () => true;

        public override async UniTask<Resource<T>> LoadResourceAsync<T> (string path)
        {
            if (locations is null) locations = await LoadAllLocations();
            return await base.LoadResourceAsync<T>(path);
        }

        public override async UniTask<IEnumerable<string>> LocateResourcesAsync<T> (string path)
        {
            if (locations is null) locations = await LoadAllLocations();
            return await base.LocateResourcesAsync<T>(path);
        }

        public override async UniTask<IEnumerable<Folder>> LocateFoldersAsync (string path)
        {
            if (locations is null) locations = await LoadAllLocations();
            return await base.LocateFoldersAsync(path);
        }

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (string path)
        {
            return new AddressableResourceLoader<T>(this, path, locations, LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new AddressableResourceLocator<T>(this, path, locations);
        }

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new AddressableFolderLocator(this, path, locations);
        }

        protected override void DisposeResource (Resource resource)
        {
            if (!resource.Valid) return;

            Addressables.Release(resource.Object);
        }

        private async UniTask<List<IResourceLocation>> LoadAllLocations ()
        {
            var task = ExtraLabels != null ? Addressables.LoadResourceLocationsAsync(ExtraLabels, Addressables.MergeMode.Intersection) : Addressables.LoadResourceLocationsAsync(MainLabel);
            while (!task.IsDone) // When awaiting the method directly it fails on WebGL (they're using mutlithreaded Task fot GetAwaiter)
                await AsyncUtils.WaitEndOfFrame;
            var locations = task.Result;
            return locations?.ToList() ?? new List<IResourceLocation>();
        }
    }
}

#endif
