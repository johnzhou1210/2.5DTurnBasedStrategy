using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid {
    public class GridRenderer : MonoBehaviour {
        [SerializeField] private GameObject[] tilePrefabs;
        [SerializeField] private GameObject entityPrefab, healthBillboardPrefab;
        
        private GameObject[,] _tileVisuals;
        private Dictionary<int, GameObject> _entityVisuals;
        
        
        [SerializeField] private GridManager grid;
        public GridManager Grid {
            get {
                return grid;
            }
            set {
                OnGridRedraw();
                grid = value;
            }
        }
        
        
        private void OnEnable() {
            _tileVisuals = new GameObject[grid.GetSize().x, grid.GetSize().y];
            _entityVisuals = new Dictionary<int, GameObject>();
            GridDelegates.OnEntitySpawned += OnEntitySpawned;
            GridDelegates.GetEntityVisualTransformById = GetEntityVisualTransformById;
            OnGridRedraw();
        }

        private void OnDisable() {
            GridDelegates.OnEntitySpawned -= OnEntitySpawned;
        }

        private Transform GetEntityVisualTransformById(int id) {
            return _entityVisuals[id].transform;
        }

        private Vector3 Vector2IntToVector3(Vector2Int vector2Int) {
            return new Vector3(vector2Int.x, 0, vector2Int.y);
        }
        
        private void OnEntitySpawned(GridEntity entity, Vector2Int newPosition) {
            GameObject entityVisual = Instantiate(entityPrefab, transform);
            GameObject entitySpriteGameObject = Instantiate(entity.GetSpritePrefab(), entityVisual.transform);
            entitySpriteGameObject.transform.position += new Vector3(0, 0.25f, 0);
            _entityVisuals[entity.Id] = entityVisual;
            entityVisual.transform.position = Vector2IntToVector3(newPosition);
            
            // Attach health billboard
            Transform billboardCanvasTransform = BillboardDelegates.GetBillboardCanvasTransform?.Invoke();
            GameObject healthBillboard = Instantiate(healthBillboardPrefab, billboardCanvasTransform);
            if (healthBillboard.TryGetComponent(out HealthBillboard healthBillboardComponent)) {
                Debug.Log(healthBillboardComponent);
                healthBillboardComponent.Initialize(entity);
            }
            
        }
        

        private void OnGridRedraw() {
            Vector2Int dimensions = grid.GetSize();
            for (int y = 0; y < dimensions.y; y++) {
                for (int x = 0; x < dimensions.x; x++) {
                    Vector3 position = new Vector3(x, 0f, y);
                    GameObject newTile = Instantiate((x + y) % 2 == 0 ? tilePrefabs[0] : tilePrefabs[1], transform);
                    newTile.transform.position = position;
                    _tileVisuals[x, y] = newTile;
                }
            }
        }
    
    }
}

