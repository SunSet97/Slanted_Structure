﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UniRx.Async;
using UnityEngine;

namespace Naninovel
{
    public class ProjectResourceLoader<TResource> : LoadResourceRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        public readonly string RootPath;

        private readonly Action<string> logAction;
        private readonly ProjectResourceProvider.TypeRedirector redirector;

        public ProjectResourceLoader (IResourceProvider provider, string rootPath, string resourcePath, 
            ProjectResourceProvider.TypeRedirector redirector, Action<string> logAction) : base (provider, resourcePath)
        {
            RootPath = rootPath;
            this.redirector = redirector;
            this.logAction = logAction;
        }

        public override async UniTask RunAsync ()
        {
            var startTime = Time.time;

            var resourcePath = string.IsNullOrEmpty(RootPath) ? Path : string.Concat(RootPath, "/", Path);
            var resourceType = redirector != null ? redirector.RedirectType : typeof(TResource);
            var asset = await Resources.LoadAsync(resourcePath, resourceType);
            var assetName = System.IO.Path.GetFileNameWithoutExtension(Path);
            var obj = redirector is null ? asset as TResource : await redirector.ToSourceAsync<TResource>(asset, assetName);

            var result = new Resource<TResource>(Path, obj, Provider);
            SetResult(result);

            logAction?.Invoke($"Resource '{Path}' loaded over {Time.time - startTime:0.###} seconds.");
        }
    }
}
