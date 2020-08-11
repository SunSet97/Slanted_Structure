﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

namespace Naninovel
{
    public class ProjectFolderLocator : LocateFoldersRunner
    {
        public readonly string RootPath;

        private readonly ProjectResources projectResources;

        public ProjectFolderLocator (IResourceProvider provider, string rootPath, string resourcesPath, ProjectResources projectResources)
            : base (provider, resourcesPath ?? string.Empty)
        {
            RootPath = rootPath;
            this.projectResources = projectResources;
        }

        public override UniTask RunAsync ()
        {
            var locatedFolders = LocateProjectFolders(RootPath, Path, projectResources);
            SetResult(locatedFolders);
            return UniTask.CompletedTask;
        }

        public static List<Folder> LocateProjectFolders (string rootPath, string resourcesPath, ProjectResources projectResources)
        {
            var path = string.IsNullOrEmpty(rootPath) ? resourcesPath : string.IsNullOrEmpty(resourcesPath) ? rootPath : $"{rootPath}/{resourcesPath}";
            return projectResources.ResourcePaths.LocateFolderPathsAtFolder(path)
                .Select(p => new Folder(string.IsNullOrEmpty(rootPath) ? p : p.GetAfterFirst(rootPath + "/"))).ToList();
        }
    }
}
