﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable data used for <see cref="ResourceLoader"/> construction.
    /// </summary>
    [System.Serializable]
    public class ResourceLoaderConfiguration 
    {
        [Tooltip("Path prefix to add for each requested resource.")]
        public string PathPrefix = string.Empty;
        [Tooltip("Provider types to use, in order." +
            "\n\nBuilt-in options:" +
            "\n • Addressable — For assets managed via the Addressable Asset System." +
            "\n • Project — For assets stored in project's `Resources` folders." +
            "\n • Local — For assets stored on a local file system." +
            "\n • GoogleDrive — For assets stored remotely on a Google Drive account.")]
        public List<string> ProviderTypes = new List<string> { ResourceProviderConfiguration.AddressableTypeName, ResourceProviderConfiguration.ProjectTypeName };

        public ResourceLoader<TResource> CreateFor<TResource> (IResourceProviderManager providerManager) where TResource : Object
        {
            var providerList = providerManager.GetProviders(ProviderTypes);
            return new ResourceLoader<TResource>(providerList, PathPrefix);
        }

        public LocalizableResourceLoader<TResource> CreateLocalizableFor<TResource> (IResourceProviderManager providerManager, ILocalizationManager localizationManager) where TResource : Object
        {
            var providerList = providerManager.GetProviders(ProviderTypes);
            return new LocalizableResourceLoader<TResource>(providerList, localizationManager, PathPrefix);
        }

        public override string ToString () => $"{PathPrefix}- ({string.Join(", ", ProviderTypes.Select(t => t.GetBetween(".", "Resource") ?? t.GetBefore(",")))})";
    }
}
