﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

namespace Naninovel
{
    public class EditorFolderLocator : LocateFoldersRunner
    {
        private readonly IEnumerable<string> editorResourcePaths;

        public EditorFolderLocator (IResourceProvider provider, string resourcesPath, IEnumerable<string> editorResourcePaths)
            : base (provider, resourcesPath ?? string.Empty)
        {
            this.editorResourcePaths = editorResourcePaths;
        }

        public override UniTask RunAsync ()
        {
            var locatedFolders = LocateEditorFolders(Path, editorResourcePaths);
            SetResult(locatedFolders);
            return UniTask.CompletedTask;
        }

        public static List<Folder> LocateEditorFolders (string path, IEnumerable<string> editorResourcePaths)
        {
            return editorResourcePaths.LocateFolderPathsAtFolder(path).Select(p => new Folder(p)).ToList();
        }
    }
}
