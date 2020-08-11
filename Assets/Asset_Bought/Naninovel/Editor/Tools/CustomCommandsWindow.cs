// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Naninovel.Commands;

namespace Naninovel
{
    public class CustomCommandsWindow : EditorWindow
    {
        #pragma warning disable 0649

        [System.Serializable]
        private struct CommandMetadata
        {
            public string id;
            public string alias;
            public bool localizable;
            public string summary;
            public string remarks;
            public List<ParameterMetadata> @params;
        }

        [System.Serializable]
        private struct ParameterMetadata
        {
            public string id;
            public string alias;
            public bool nameless;
            public bool required;
            public DataType dataType;
            public string summary;
        }

        [System.Serializable]
        private struct DataType
        {
            public string kind;
            public string contentType;
        }

        #pragma warning restore 0649

        protected string OutputPath
        {
            get => PlayerPrefs.GetString(outputPathKey);
            set { PlayerPrefs.SetString(outputPathKey, value); ValidateOutputPath(); }
        }

        private static readonly GUIContent outputPathContent = new GUIContent("Output Path", $"Path to `{outputFolderName}` folder of the target Naninovel IDE extension.");

        private const string outputPathKey = "Naninovel." + nameof(CustomCommandsWindow) + "." + nameof(OutputPath);
        private const string outputFolderName = "server";
        private const string fileName = "customMetadata.json";
        private const string contentTemplate = "{\"commands\": [\n\n{0}]}";

        private bool isWorking = false;
        private bool outputPathValid = false;

        [MenuItem("Naninovel/Tools/Custom Commands")]
        public static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 135);
            GetWindowWithRect<CustomCommandsWindow>(position, true, "Custom Commands", true);
        }

        private void OnEnable ()
        {
            ValidateOutputPath();
        }

        private void ValidateOutputPath ()
        {
            outputPathValid = OutputPath?.EndsWith(outputFolderName) ?? false;
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Naninovel Custom Commands", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool to generate IDE metadata for custom commands;\nsee `Custom Commands` guide (`IDE Metadata` part) for usage instructions.", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space();

            if (isWorking)
            {
                EditorGUILayout.HelpBox("Working, please wait...", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                OutputPath = EditorGUILayout.TextField(outputPathContent, OutputPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    OutputPath = EditorUtility.OpenFolderPanel("Output Path", "", "");
            }

            GUILayout.FlexibleSpace();

            if (!outputPathValid)
                EditorGUILayout.HelpBox($"Output path is not valid. Make sure it points to a `{outputFolderName}` folder under a Naninovel IDE extension intallation directory.", MessageType.Error);
            else if (GUILayout.Button("Generate Custom Commands Metadata", GUIStyles.NavigationButton))
                GenerateCustomCommandsMetadata();
            EditorGUILayout.Space();
        }

        private void GenerateCustomCommandsMetadata ()
        {
            isWorking = true;

            var outputStrings = new List<string>();
            var customCommandTypes = Command.CommandTypes.Values.Where(t => t.Namespace != typeof(Command).Namespace).ToList();
            foreach (var commandType in customCommandTypes)
            {
                var metadata = new CommandMetadata {
                    id = commandType.Name,
                    alias = commandType.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(Command.CommandAliasAttribute))?.ConstructorArguments[0].Value as string,
                    localizable = commandType.GetInterfaces().Any(t => t is Command.ILocalizable),
                    summary = commandType.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(DocumentationAttribute))?.ConstructorArguments[0].Value as string,
                    remarks = commandType.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(DocumentationAttribute))?.ConstructorArguments[1].Value as string,
                    @params = ExtractParamsMeta(commandType)
                };
                outputStrings.Add(JsonUtility.ToJson(metadata, true));
            }

            var outputString = string.Join($",{Environment.NewLine}{Environment.NewLine}", outputStrings);
            outputString = contentTemplate.Replace("{0}", outputString);

            var fullPath = Path.Combine(OutputPath, fileName);
            File.WriteAllText(fullPath, outputString);

            isWorking = false;
            Repaint();

            List<ParameterMetadata> ExtractParamsMeta (Type commandType)
            {
                var result = new List<ParameterMetadata>();
                var fieldInfos = commandType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => !x.IsSpecialName && !x.GetCustomAttributes<ObsoleteAttribute>().Any())
                    .Where(f => f.FieldType.GetInterface(nameof(ICommandParameter)) != null).ToArray();

                foreach (var fieldInfo in fieldInfos)
                {
                    // Extracting parameter properties.
                    var id = fieldInfo.Name;
                    var alias = fieldInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(Command.ParameterAliasAttribute))?.ConstructorArguments[0].Value as string;
                    var nameless = alias == string.Empty;
                    var required = fieldInfo.CustomAttributes.Any(a => a.AttributeType == typeof(Command.RequiredParameterAttribute));
                    var summary = fieldInfo.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(DocumentationAttribute))?.ConstructorArguments[0].Value as string;

                    // Extracting parameter value type.
                    string ResolveValueType (Type type)
                    {
                        var valueTypeName = type.GetInterface("INullable`1")?.GetGenericArguments()[0]?.Name;
                        switch (valueTypeName)
                        {
                            case "String": case "NullableString": return "string";
                            case "Int32": case "NullableInteger": return "int";
                            case "Single": case "NullableFloat": return "float";
                            case "Boolean": case "NullableBoolean": return "bool";
                        }
                        return null;
                    }
                    var dataType = new DataType();
                    var paramType = fieldInfo.FieldType;
                    var isLiteral = ResolveValueType(paramType) != null;
                    if (isLiteral)
                    {
                        dataType.kind = "literal";
                        dataType.contentType = ResolveValueType(paramType);
                    }
                    else if (paramType.GetInterface("IEnumerable") != null)
                    {
                        var elementType = paramType.GetInterface("INullable`1").GetGenericArguments()[0].GetGenericArguments()[0];
                        if (elementType.GetInterface("INamedValue") != null) // Treating arrays of named liters as maps for the parser.
                        {
                            dataType.kind = "map";
                            dataType.contentType = ResolveValueType(elementType.GetInterface("INamed`1").GetGenericArguments()[0]);
                        }
                        else
                        {
                            dataType.kind = "array";
                            dataType.contentType = ResolveValueType(elementType);
                        }
                    }
                    else
                    {
                        dataType.kind = "namedLiteral";
                        dataType.contentType = ResolveValueType(paramType.GetInterface("INullable`1").GetGenericArguments()[0].GetInterface("INamed`1").GetGenericArguments()[0]);
                    }

                    result.Add(new ParameterMetadata { 
                        id = id,
                        alias = alias,
                        nameless = nameless,
                        required = required,
                        dataType = dataType,
                        summary = summary
                    });
                }

                return result;
            }
        }
    }
}
