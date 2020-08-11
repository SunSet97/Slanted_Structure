﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UniRx.Async;

namespace Naninovel
{
    public abstract class ResourceRunner
    {
        public readonly IResourceProvider Provider;
        public readonly string Path;
        public readonly Type ResourceType;

        public ResourceRunner (IResourceProvider provider, string path, Type resourceType)
        {
            Provider = provider;
            Path = path;
            ResourceType = resourceType;
        }

        public UniTask.Awaiter GetAwaiter () => GetAwaiterImpl();

        public abstract UniTask RunAsync ();
        public abstract void Cancel ();

        protected abstract UniTask.Awaiter GetAwaiterImpl ();
    }

    public abstract class ResourceRunner<TResult> : ResourceRunner
    {
        public TResult Result { get; private set; }

        private UniTaskCompletionSource<TResult> completionSource = new UniRx.Async.UniTaskCompletionSource<TResult>();

        public ResourceRunner (IResourceProvider provider, string path, Type resourceType)
            : base(provider, path, resourceType) { }

        public new UniTask<TResult>.Awaiter GetAwaiter () => completionSource.Task.GetAwaiter();

        public override void Cancel ()
        {
            completionSource.TrySetCanceled();
        }

        protected void SetResult (TResult result)
        {
            Result = result;
            completionSource.TrySetResult(Result);
        }

        protected override UniTask.Awaiter GetAwaiterImpl () => ((UniTask)completionSource.Task).GetAwaiter();
    }

    public abstract class LocateResourcesRunner<TResource> : ResourceRunner<IEnumerable<string>> 
        where TResource : UnityEngine.Object
    {
        public LocateResourcesRunner (IResourceProvider provider, string path)
            : base(provider, path, typeof(TResource)) { }
    }

    public abstract class LoadResourceRunner<TResource> : ResourceRunner<Resource<TResource>> 
        where TResource : UnityEngine.Object
    {
        public LoadResourceRunner (IResourceProvider provider, string path)
            : base(provider, path, typeof(TResource)) { }
    }

    public abstract class LocateFoldersRunner : ResourceRunner<IEnumerable<Folder>>
    {
        public LocateFoldersRunner (IResourceProvider provider, string path)
            : base(provider, path, typeof(Folder)) { }
    }
}
