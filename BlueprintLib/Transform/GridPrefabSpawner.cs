using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MisterGames.BlueprintLib {
    public sealed class GridPrefabSpawner : MonoBehaviour {

        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector2Int _grid = new Vector2Int(50, 50);
        [SerializeField] private float _gridStep = 1f;
        [SerializeField] [Range(1, 1000)] private int _spawnAmount = 10;
        [SerializeField] private int _spawnedCount;

        private Vector2Int _nextCell = new Vector2Int(-1, 0);

        public void Spawn() {
            var sw = new Stopwatch();
            sw.Start();

            int count = 0;
            for (int i = 0; i < _spawnAmount; i++) {
                if (!MoveToNextCell()) break;

                Instantiate(_prefab, GetCurrentCellPosition(), Quaternion.identity, transform);
                count++;
            }

            _spawnedCount += count;

            sw.Stop();
            long total = sw.ElapsedMilliseconds;
            long average = count <= 0 ? 0 : total / count;

            Debug.Log($"GridPrefabSpawner.Spawn: " +
                      $"spawned {count} {_prefab.name} prefabs, " +
                      $"total {total} ms, " +
                      $"average per prefab {average} ms");
        }

        private Vector3 GetCurrentCellPosition() {
            return new Vector3(_nextCell.x * _gridStep, 0f, _nextCell.y * _gridStep);
        }

        private bool MoveToNextCell() {
            _nextCell.x += 1;

            if (_nextCell.x > _grid.x) {
                _nextCell.x = 0;
                _nextCell.y += 1;
            }

            return _nextCell.y <= _grid.y;
        }
    }

}
