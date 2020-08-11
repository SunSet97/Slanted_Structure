﻿// Copyright 2017-2020 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Arguments associated with the <see cref="IUnlockableManager.OnItemUpdated"/> event. 
    /// </summary>
    public class UnlockableItemUpdatedArgs : EventArgs
    {
        /// <summary>
        /// ID of the updated unlockable item.
        /// </summary>
        public readonly string Id;
        /// <summary>
        /// Whether the item is now unlocked.
        /// </summary>
        public readonly bool Unlocked;
        /// <summary>
        /// Whether the item has beed added to the unlockables map for the first time.
        /// </summary>
        public readonly bool Added;

        public UnlockableItemUpdatedArgs (string id, bool unlocked, bool added)
        {
            Id = id;
            Unlocked = unlocked;
            Added = added;
        }
    }
}
