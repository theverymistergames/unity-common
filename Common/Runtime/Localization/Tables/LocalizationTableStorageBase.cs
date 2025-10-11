using System;
using UnityEngine;

namespace MisterGames.Common.Localization {

    public abstract class LocalizationTableStorageBase : ScriptableObject, ILocalizationTableStorage {

        public abstract Type GetDataType();

        public abstract int GetLocaleCount();
        public abstract Locale GetLocale(int localeIndex);
        
        public abstract int GetKeyCount();
        public abstract bool TryGetKey(int keyHash, out string key);
        public abstract string GetKey(int keyIndex);

        public abstract string GetValuesPropertyPath(int keyHash);
        public abstract void ClearAll();
    }
    
}