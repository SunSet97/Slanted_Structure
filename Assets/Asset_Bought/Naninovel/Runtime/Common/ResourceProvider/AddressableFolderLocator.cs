﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

#if ADDRESSABLES_AVAILABLE

using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Naninovel
{
    public class AddressableFolderLocator : LocateFoldersRunner
    {
        private readonly List<IResourceLocation> locations;

        public AddressableFolderLocator (AddressableResourceProvider provider, string resourcePath, List<IResourceLocation> locations)
            : base(provider, resourcePath)
        {
            this.locations = locations;
        }

        public override UniTask RunAsync ()
        {
            var locatedResourcePaths = locations
                .Select(l => l.PrimaryKey.GetAfterFirst("/")) // Remove the addressables prefix.
                .LocateFolderPathsAtFolder(Path)
                .Select(p => new Folder(p));
            SetResult(locatedResourcePaths);

            return UniTask.CompletedTask;
        }
    }
}

#endif
