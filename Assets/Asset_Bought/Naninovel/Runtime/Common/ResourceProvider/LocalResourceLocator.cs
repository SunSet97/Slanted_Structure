﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx.Async;

namespace Naninovel
{
    public class LocalResourceLocator<TResource> : LocateResourcesRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        public readonly string RootPath;

        private IRawConverter<TResource> converter;

        public LocalResourceLocator (IResourceProvider provider, string rootPath, string resourcesPath, 
            IRawConverter<TResource> converter) : base (provider, resourcesPath)
        {
            RootPath = rootPath;
            this.converter = converter;
        }

        public override UniTask RunAsync ()
        {
            var locatedResourcePaths = LocateResources(RootPath, Path, converter);
            SetResult(locatedResourcePaths);
            return UniTask.CompletedTask;
        }

        public static List<string> LocateResources (string rootPath, string resourcesPath, IRawConverter<TResource> converter)
        {
            var locatedResources = new List<string>();

            // 1. Resolving parent folder.
            var folderPath = rootPath;
            if (!string.IsNullOrEmpty(resourcesPath))
                folderPath += string.Concat('/', resourcesPath);
            var parendFolder = new DirectoryInfo(folderPath);
            if (!parendFolder.Exists) return locatedResources;

            // 2. Searching for the files in the folder.
            var results = new Dictionary<RawDataRepresentation, List<FileInfo>>();
            foreach (var representation in converter.Representations.DistinctBy(r => r.Extension))
            {
                var files = parendFolder.GetFiles(string.Concat("*", representation.Extension)).ToList();
                if (files != null && files.Count > 0) results.Add(representation, files);
            }

            // 3. Create resources using located files.
            foreach (var result in results)
            {
                foreach (var file in result.Value)
                {
                    var fileName = string.IsNullOrEmpty(result.Key.Extension) ? file.Name : file.Name.GetBeforeLast(".");
                    var filePath = string.IsNullOrEmpty(resourcesPath) ? fileName : string.Concat(resourcesPath, '/', fileName);
                    locatedResources.Add(filePath);
                }
            }

            return locatedResources;
        }
    }
}
