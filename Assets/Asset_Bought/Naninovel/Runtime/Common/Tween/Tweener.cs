﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UniRx.Async;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows tweening a <see cref="ITweenValue"/> using coroutine.
    /// </summary>
    public class Tweener<TTweenValue> 
        where TTweenValue : struct, ITweenValue
    {
        public TTweenValue TweenValue { get; private set; }
        public bool Running { get; private set; }

        private readonly Action onCompleted;
        private float elapsedTime;
        private Guid lastRunGuid;

        public Tweener (Action onCompleted = null)
        {
            this.onCompleted = onCompleted;
        }

        public Tweener (TTweenValue tweenValue, Action onCompleted = null)
            : this(onCompleted)
        {
            TweenValue = tweenValue;
        }

        public void Run (TTweenValue tweenValue, CancellationToken cancellationToken = default)
        {
            TweenValue = tweenValue;
            Run(cancellationToken);
        }

        public void Run (CancellationToken cancellationToken = default) => TweenAsyncAndForget(cancellationToken).Forget();

        public UniTask RunAsync (TTweenValue tweenValue, CancellationToken cancellationToken = default)
        {
            TweenValue = tweenValue;
            return RunAsync(cancellationToken);
        }

        public UniTask RunAsync (CancellationToken cancellationToken = default) => TweenAsync(cancellationToken);

        public void Stop ()
        {
            lastRunGuid = Guid.Empty;
            Running = false;
        }

        public void CompleteInstantly ()
        {
            Stop();
            TweenValue.TweenValue(1f);
            onCompleted?.Invoke();
        }

        protected async UniTask TweenAsync (CancellationToken cancellationToken = default)
        {
            PrepareTween();
            if (TweenValue.TweenDuration <= 0f) { CompleteInstantly(); return; }

            var currentRunGuid = lastRunGuid;
            while (!cancellationToken.CancellationRequested && TweenValue.TargetValid && elapsedTime <= TweenValue.TweenDuration)
            {
                PeformTween();
                await AsyncUtils.WaitEndOfFrame;
                if (lastRunGuid != currentRunGuid) return; // The tweener was completed instantly or stopped.
            }

            if (cancellationToken.CancelASAP) return;
            if (cancellationToken.CancelLazy) CompleteInstantly();
            else FinishTween();
        }

        // Required to prevent garbage when await is not required (fire and forget).
        // Remember to keep both methods identical.
        protected async UniTaskVoid TweenAsyncAndForget (CancellationToken cancellationToken = default)
        {
            PrepareTween();
            if (TweenValue.TweenDuration <= 0f) { CompleteInstantly(); return; }

            var currentRunGuid = lastRunGuid;
            while (!cancellationToken.CancellationRequested && TweenValue.TargetValid && elapsedTime <= TweenValue.TweenDuration)
            {
                PeformTween();
                await AsyncUtils.WaitEndOfFrame;
                if (lastRunGuid != currentRunGuid) return; // The tweener was completed instantly or stopped.
            }

            if (cancellationToken.CancelASAP) return;
            if (cancellationToken.CancelLazy) CompleteInstantly();
            else FinishTween();
        }

        private void PrepareTween ()
        {
            if (Running) CompleteInstantly();

            Running = true;
            elapsedTime = 0f;
            lastRunGuid = Guid.NewGuid();
        }

        private void PeformTween ()
        {
            elapsedTime += TweenValue.TimeScaleIgnored ? Time.unscaledDeltaTime : Time.deltaTime;
            var tweenPercent = Mathf.Clamp01(elapsedTime / TweenValue.TweenDuration);
            TweenValue.TweenValue(tweenPercent);
        }

        private void FinishTween ()
        {
            Running = false;
            onCompleted?.Invoke();
        }
    }
}
