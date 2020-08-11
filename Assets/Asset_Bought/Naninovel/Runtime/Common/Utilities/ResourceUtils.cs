﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    public static class ResourceUtils
    {
        /// <summary>
        /// Given the path to the parent folder, returns all the unique resource paths inside that folder (or any sub-folders).
        /// </summary>
        public static IEnumerable<string> LocateResourcePathsAtFolder (this IEnumerable<string> source, string parentFolderPath)
        {
            parentFolderPath = parentFolderPath ?? string.Empty;
            if (string.IsNullOrWhiteSpace(parentFolderPath))
                return source.Where(p => !p.Contains("/") || string.IsNullOrEmpty(p.GetBeforeLast("/")));
            return source.Where(p => p.Contains("/") && (p.StartsWithFast($"{parentFolderPath}/") || p.StartsWithFast($"/{parentFolderPath}/")));
        }

        /// <summary>
        /// Given the path to the parent folder, returns all the unique folder paths inside that folder (or any sub-folders).
        /// </summary>
        public static IEnumerable<string> LocateFolderPathsAtFolder (this IEnumerable<string> source, string parentFolderPath)
        {
            parentFolderPath = parentFolderPath ?? string.Empty;

            if (parentFolderPath.StartsWithFast("/"))
                parentFolderPath = parentFolderPath.GetAfterFirst("/") ?? string.Empty;

            if (parentFolderPath.Length > 0 && !parentFolderPath.EndsWithFast("/"))
                parentFolderPath += "/";

            return source.Where(p => p.StartsWithFast(parentFolderPath) && p.GetAfterFirst(parentFolderPath).Contains("/"))
                .Select(p => parentFolderPath + p.GetBetween(parentFolderPath, "/")).Distinct();
        }
    }
}
