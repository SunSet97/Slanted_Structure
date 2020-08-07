﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Text;
using UniRx.Async;

namespace Naninovel
{
    public class GDocToScriptAssetConverter : IGoogleDriveConverter<Script>
    {
        public RawDataRepresentation[] Representations => new RawDataRepresentation[] {
            new RawDataRepresentation(null, "application/vnd.google-apps.document")
        };

        public string ExportMimeType => "text/plain";

        public Script Convert (byte[] obj, string name) => Script.FromScriptText(name, Encoding.UTF8.GetString(obj));

        public UniTask<Script> ConvertAsync (byte[] obj, string name) => UniTask.FromResult(Convert(obj, name));

        public object Convert (object obj, string name) => Convert(obj as byte[], name);

        public async UniTask<object> ConvertAsync (object obj, string name) => await ConvertAsync(obj as byte[], name);
    }
}
