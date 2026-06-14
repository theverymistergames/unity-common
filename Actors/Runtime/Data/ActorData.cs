using System;
using System.Collections.Generic;
using MisterGames.Common.Attributes;
using UnityEngine;

namespace MisterGames.Actors
{

    [CreateAssetMenu(fileName = nameof(ActorData), menuName = "MisterGames/Actors/" + nameof(ActorData))]
    public class ActorData : ScriptableObject {
        
        [SubclassSelector] [SerializeReference] private IActorData[] _data = Array.Empty<IActorData>();
        
        private readonly SetBuilder<IActorData> _dataBuilder = new();
        private Dictionary<Type, int> _indexMap;
        
        public T GetData<T>() where T : class, IActorData {
            return FindDataOfType(typeof(T)) as T;
        }
        
        public IActorData GetData(Type type) {
            return FindDataOfType(type);
        }

        public IReadOnlyList<IActorData> GetDataArray() {
            return GetResultDataArray();
        }

        protected virtual IReadOnlyList<IActorData> GetResultDataArray() {
            return GetLocalDataArray();
        }

        protected virtual void BuildDataArray(ISetBuilder<IActorData> builder) {
            
        }

        protected IReadOnlyList<IActorData> GetLocalDataArray() {
            return _data ?? Array.Empty<IActorData>();
        }

        protected void SetLocalDataArray(IReadOnlyList<IActorData> dataArray) {
            int count = dataArray?.Count ?? 0;
            
            if (_data == null) {
                _data = count > 0 ? new IActorData[count] : Array.Empty<IActorData>();
            }
            else {
                Array.Resize(ref _data, count);
            }

            for (int i = 0; i < count; i++) {
                _data[i] = dataArray![i];
            }
        }

        private IActorData FindDataOfType(Type type) {
            var dataArray = GetResultDataArray();
            
            if (_indexMap == null) {
                _indexMap = new Dictionary<Type, int>();
                for (int i = 0; i < dataArray.Count; i++) {
                    if (dataArray[i] is var d) _indexMap[d.GetType()] = i;
                }                
            }
            
            return _indexMap.TryGetValue(type, out int index) ? dataArray[index] : null;
        }

#if UNITY_EDITOR
        private void Reset() {
            OnValidate();
        }
        
        private void OnValidate() {
            _indexMap = null;
            
            _dataBuilder.Clear();
            _dataBuilder.Set(_data);
            BuildDataArray(_dataBuilder);
            var dataArray = _dataBuilder.GetResultArray();
            
            for (int i = 0; i < dataArray.Count; i++) {
                dataArray[i]?.OnValidate(this);
            }
            
            SetLocalDataArray(dataArray);
        }
#endif
    }
    
}