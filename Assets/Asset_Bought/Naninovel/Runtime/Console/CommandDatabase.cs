﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Naninovel
{
    internal static class CommandDatabase
    {
        private static Dictionary<string, MethodInfo> methodInfoCache;

        public static void ExecuteCommand (string methodName, params string[] args)
        {
            if (methodInfoCache == null || !methodInfoCache.ContainsKey(methodName))
            {
                Debug.LogWarning($"UnityConsole: Command `{methodName}` is not registered in the database.");
                return;
            }
            var methodInfo = methodInfoCache[methodName];
            var parametersInfo = methodInfo.GetParameters();
            if (parametersInfo.Length != args.Length)
            {
                Debug.LogWarning($"UnityConsole: Command `{methodName}` requires {parametersInfo.Length} args, while {args.Length} were provided.");
                return;
            }
            var parameters = new object[parametersInfo.Length];
            for (int i = 0; i < args.Length; i++)
                parameters[i] = Convert.ChangeType(args[i], parametersInfo[i].ParameterType, System.Globalization.CultureInfo.InvariantCulture);
            methodInfo.Invoke(null, parameters);
        }

        public static void RegisterCommands ()
        {
            methodInfoCache = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(assembly => assembly.GetExportedTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                .Where(method => method.GetCustomAttribute<ConsoleCommandAttribute>() != null)
                .ToDictionary(method => method.GetCustomAttribute<ConsoleCommandAttribute>().Alias ?? method.Name, StringComparer.OrdinalIgnoreCase);
        }
    }
}
