using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

namespace MisterGames.Common.Pooling {
  
    public class ObjectPoolAsync<T> : IObjectPoolAsync<T>, IDisposable where T : class { 
      
        private readonly List<T> m_List;
        private readonly Func<T, T> m_CreateFunc;
        private readonly Func<T, UniTask<T>> m_CreateFuncAsync;
        private readonly Action<T> m_ActionOnGet;
        private readonly Action<T> m_ActionOnRelease;
        private readonly Action<T> m_ActionOnDestroy;
        private readonly Func<T, bool> m_IsNull;
        private readonly int m_MaxSize;
        private readonly bool m_CollectionCheck; 
        private T m_FreshlyReleased;
      
        public int CountAll { get; private set; }
        public int CountActive => CountAll - CountInactive;
        public int CountInactive => m_List.Count + (m_IsNull(m_FreshlyReleased) ? 0 : 1);

        public ObjectPoolAsync(
            Func<T, T> createFunc,
            Func<T, UniTask<T>> createFuncAsync,
            Action<T> actionOnGet = null, 
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            Func<T, bool> isNull = null, 
            bool collectionCheck = true,
            int defaultCapacity = 10,
            int maxSize = 10000) 
        {
            if (maxSize <= 0) 
                throw new ArgumentException("Max Size must be greater than 0", nameof (maxSize));
      
            m_List = new List<T>(defaultCapacity);
            m_CreateFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
            m_CreateFuncAsync = createFuncAsync ?? throw new ArgumentNullException(nameof(createFuncAsync));
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_IsNull = isNull ?? (t => t == null);
            m_CollectionCheck = collectionCheck;
        }
        
        public void Dispose() {
            Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(T sample) {
            T obj = null;

            if (!m_IsNull(m_FreshlyReleased)) {
                obj = m_FreshlyReleased;
                m_FreshlyReleased = null;
            }
            else {
                while (obj == null && m_List.Count > 0) {
                    int index = m_List.Count - 1;
                    var candidate = m_List[index];
                    m_List.RemoveAt(index);
                    if (m_IsNull(candidate)) --CountAll;
                    else obj = candidate;
                }
            }

            if (obj == null) {
                obj = m_CreateFunc(sample);
                ++CountAll;
            }

            m_ActionOnGet?.Invoke(obj);
            return obj;
        }

        public async UniTask<T> GetAsync(T sample) {
            T obj = null;

            if (!m_IsNull(m_FreshlyReleased)) {
                obj = m_FreshlyReleased;
                m_FreshlyReleased = null;
            }
            else {
                while (obj == null && m_List.Count > 0) {
                    int index = m_List.Count - 1;
                    var candidate = m_List[index];
                    m_List.RemoveAt(index);
                    if (m_IsNull(candidate)) --CountAll;
                    else obj = candidate;
                }
            }

            if (obj == null) {
                obj = await m_CreateFuncAsync(sample);
                ++CountAll;
            }

            m_ActionOnGet?.Invoke(obj);
            return obj;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(T element) {
            if (m_CollectionCheck && (m_List.Count > 0 || !m_IsNull(m_FreshlyReleased))) {
                if (ReferenceEquals(element, m_FreshlyReleased))
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");

                for (int i = 0; i < m_List.Count; ++i) {
                    if (ReferenceEquals(element, m_List[i]))
                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }
            }

            m_ActionOnRelease?.Invoke(element);

            if (m_IsNull(m_FreshlyReleased)) {
                m_FreshlyReleased = element;
            }
            else if (CountInactive < m_MaxSize) {
                m_List.Add(element);
            }
            else {
                --CountAll;
                m_ActionOnDestroy?.Invoke(element);
            }
        }
        
        public void Clear() {
            if (m_ActionOnDestroy != null) {
                for (int i = 0; i < m_List.Count; i++) {
                    if (!m_IsNull(m_List[i])) m_ActionOnDestroy(m_List[i]);
                }

                if (!m_IsNull(m_FreshlyReleased)) {
                    m_ActionOnDestroy(m_FreshlyReleased);
                }
            }
            
            m_FreshlyReleased = null;
            m_List.Clear();
            CountAll = 0;
        }
    }
    
}