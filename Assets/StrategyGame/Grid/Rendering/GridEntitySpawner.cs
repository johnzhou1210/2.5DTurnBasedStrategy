using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Grid.GridData;
using StrategyGame.UI.World;
using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.Grid.Rendering {
    public class GridEntitySpawner : MonoBehaviour {
        [SerializeField] private GameObject entityPrefab;
        [SerializeField] private GameObject healthBillboardPrefab;
        [SerializeField] private GameObject unitWeaponTypeBillboardPrefab;
        private Dictionary<int, GameObject> _entityVisuals;
        

        private void Awake() {
            _entityVisuals = new Dictionary<int, GameObject>();
        }

        private void OnEnable() {
            GridDelegates.OnEntitySpawned += OnEntitySpawned;
            EntityDelegates.GetEntityVisualTransformByID = GetEntityVisualTransformByID;
        }

        private void OnDisable() {
            GridDelegates.OnEntitySpawned -= OnEntitySpawned;
        }

        private Transform GetEntityVisualTransformByID(int id) {
            return _entityVisuals[id].transform;
        }
        
        private void OnEntitySpawned(GridEntity entity, Vector2Int newPosition) {
            GameObject entityVisual = Instantiate(entityPrefab, transform);
            entityVisual.name =  $"{entity.ID} : {entity.GridEntityData.name}";
            Debug.Log($"{entity.ID} : {entity.GetSpritePrefab()}");
            GameObject entitySpriteGameObject = Instantiate(entity.GetSpritePrefab(), entityVisual.transform);
            entitySpriteGameObject.transform.position += new Vector3(0, .75f, 0);
            _entityVisuals[entity.ID] = entityVisual;
            entityVisual.transform.position = VectorUtils.Vector2IntToVector3(newPosition);

            AttachBillboards(entity);
            
            Debug.Log(DictionaryUtils.FormatDictionary(_entityVisuals));
        }
        
        
        private void AttachBillboards(GridEntity entity) {
            Transform billboardCanvasTransform = BillboardDelegates.GetBillboardCanvasTransform?.Invoke();

            if (entity.MaxHealth > 0) {
                // Attach health billboard
                AttachHealthBillboard(entity, billboardCanvasTransform);
            }
            if (entity is GridUnit unit) {
                AttachWeaponTypeBillboard(unit, billboardCanvasTransform);
            }
        }

        private void AttachHealthBillboard(GridEntity entity, Transform billboardCanvasTransform) {
            GameObject healthBillboard = Instantiate(healthBillboardPrefab, billboardCanvasTransform);
            healthBillboard.name = $"{entity.ID} : {entity.GridEntityData.name}'s Health Billboard";
            if (healthBillboard.TryGetComponent(out HealthBillboard healthBillboardComponent)) {
                Debug.Log(healthBillboardComponent);
                healthBillboardComponent.Initialize(entity);
            }
        }

        private void AttachWeaponTypeBillboard(GridUnit unit, Transform billboardCanvasTransform) {
            GameObject weaponTypeBillboard = Instantiate(unitWeaponTypeBillboardPrefab, billboardCanvasTransform);
            weaponTypeBillboard.name = $"{unit.ID} : {unit.GridEntityData.name}'s Weapon Type Billboard";
            if (weaponTypeBillboard.TryGetComponent(out WeaponTypeBillboard billboardComponent)) {
                billboardComponent.Initialize(unit);
            }
        }
    }
}
