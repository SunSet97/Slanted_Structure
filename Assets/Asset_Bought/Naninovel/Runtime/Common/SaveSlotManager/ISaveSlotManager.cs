﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UniRx.Async;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to manage serializable data instances (slots).
    /// </summary>
    public interface ISaveSlotManager
    {
        /// <summary>
        /// Event invoked before a save (serialization) operation is started.
        /// </summary>
        event Action OnBeforeSave;
        /// <summary>
        /// Event invoked after a save (serialization) operation is finished.
        /// </summary>
        event Action OnSaved;
        /// <summary>
        /// Event invoked before a load (de-serialization) operation is started.
        /// </summary>
        event Action OnBeforeLoad;
        /// <summary>
        /// Event invoked after a load (de-serialization) operation is finished.
        /// </summary>
        event Action OnLoaded;

        /// <summary>
        /// Whether a save (serialization) operation is currently running.
        /// </summary>
        bool Loading { get; }
        /// <summary>
        /// Whether a load (de-serialization) operation is currently running.
        /// </summary>
        bool Saving { get; }

        /// <summary>
        /// Checks whether a save slot with the provided ID is available.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        bool SaveSlotExists (string slotId);
        /// <summary>
        /// Checks whether any save slot is available.
        /// </summary>
        bool AnySaveExists ();
        /// <summary>
        /// Deletes a save slot with the provided ID.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        void DeleteSaveSlot (string slotId);
        /// <summary>
        /// Renames a save slot from <paramref name="sourceSlotId"/> to <paramref name="destSlotId"/>.
        /// Will overwrite <paramref name="destSlotId"/> slot in case it exists.
        /// </summary>
        /// <param name="sourceSlotId">ID of the slot to rename.</param>
        /// <param name="destSlotId">New ID of the slot.</param>
        void RenameSaveSlot (string sourceSlotId, string destSlotId);
    }

    /// <summary>
    /// Implementation is able to manage serializable data instances (slots) of type <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">Type of the managed data; should be serializable via Unity's serialization system.</typeparam>
    public interface ISaveSlotManager<TData> : ISaveSlotManager where TData : new()
    {
        /// <summary>
        /// Saves (serializes) provided data under the provided save slot ID.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        /// <param name="data">Data to serialize.</param>
        UniTask SaveAsync (string slotId, TData data);
        /// <summary>
        /// Loads (de-serializes) a save slot with the provided ID;
        /// returns null in case requested save slot doesn't exist.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        UniTask<TData> LoadAsync (string slotId);
        /// <summary>
        /// Loads (de-serializes) a save slot with the provided ID; 
        /// will create a new default <typeparamref name="TData"/> and save it under the provided slot ID in case it doesn't exist.
        /// </summary>
        /// <param name="slotId">Unique identifier (name) of the save slot.</param>
        UniTask<TData> LoadOrDefaultAsync (string slotId);
    }
}
