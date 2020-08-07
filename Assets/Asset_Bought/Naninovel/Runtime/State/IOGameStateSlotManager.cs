﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.IO;

namespace Naninovel
{
    public class IOGameStateSlotManager : IOSaveSlotManager<GameStateMap>
    {
        protected override string SaveDataPath => $"{GameDataPath}/{savesFolderPath}";
        protected override string Extension => Binary ? "nson" : "json";
        protected override bool Binary => config.BinarySaveFiles;

        private readonly string savePattern, quickSavePattern;
        private readonly string savesFolderPath;
        private readonly StateConfiguration config;

        public IOGameStateSlotManager (StateConfiguration config, string savesFolderPath) 
        {
            this.savesFolderPath = savesFolderPath;
            this.config = config;
            savePattern = string.Format(config.SaveSlotMask, "*") + $".{Extension}"; 
            quickSavePattern = string.Format(config.QuickSaveSlotMask, "*") + $".{Extension}";
        }

        public override bool AnySaveExists ()
        {
            if (!Directory.Exists(SaveDataPath)) return false;
            var saveExists = Directory.GetFiles(SaveDataPath, savePattern, SearchOption.TopDirectoryOnly).Length > 0;
            var qSaveExists = Directory.GetFiles(SaveDataPath, quickSavePattern, SearchOption.TopDirectoryOnly).Length > 0;
            return saveExists || qSaveExists;
        }
    }
}
