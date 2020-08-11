﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UniRx.Async;

namespace Naninovel
{
    public static class EditorInitializer
    {
        public static async UniTask InitializeAsync ()
        {
            if (Engine.Initialized) return;

            var configProvider = new ProjectConfigurationProvider();
            var behaviour = new EditorBehaviour();
            var services = new List<IEngineService>();

            var providersManager = new ResourceProviderManager(configProvider.GetConfiguration<ResourceProviderConfiguration>());
            services.Add(providersManager);

            var localizationManager = new LocalizationManager(configProvider.GetConfiguration<LocalizationConfiguration>(), providersManager);
            services.Add(localizationManager);

            var scriptsManager = new ScriptManager(configProvider.GetConfiguration<ScriptsConfiguration>(), providersManager, localizationManager);
            services.Add(scriptsManager);

            var varsManager = new CustomVariableManager(configProvider.GetConfiguration<CustomVariablesConfiguration>());
            services.Add(varsManager);

            await Engine.InitializeAsync(configProvider, behaviour, services);
        }
    }
}
