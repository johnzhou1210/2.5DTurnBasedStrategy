using System;
using System.Collections.Generic;
using StrategyGame.Core.Delegates;
using StrategyGame.Factions;
using StrategyGame.Grid.GridData;
using StrategyGame.UI.World;
using StrategyGame.Utils;
using UnityEngine;

namespace StrategyGame.Grid.Rendering {
    public class GridEntitySpawner : MonoBehaviour {
        // ==============================
        // FIELDS & PROPERTIES
        // ==============================
        [SerializeField] private GameObject entityPrefab;
        [SerializeField] private GameObject healthBillboardPrefab;
        [SerializeField] private GameObject unitWeaponTypeBillboardPrefab;
        private Dictionary<int, GameObject> _entityVisuals;
        

        // ==============================
        // MONOBEHAVIOUR LIFECYCLE
        // ==============================
        private void Awake() {
            _entityVisuals = new Dictionary<int, GameObject>();
        }

        private void OnEnable() {
            GridDelegates.OnEntitySpawned += OnEntitySpawned;
            EntityVisualDelegates.OnVisualFace += FaceEntity;
            EntityVisualDelegates.GetEntityVisualTransformByID = GetEntityVisualTransformByID;
           
        }
        
        private void OnDisable() {
            GridDelegates.OnEntitySpawned -= OnEntitySpawned;
            EntityVisualDelegates.OnVisualFace -= FaceEntity;
            EntityVisualDelegates.GetEntityVisualTransformByID = null;
        }

        
        
        // ==============================
        // CORE METHODS
        // ==============================
        private Transform GetEntityVisualTransformByID(int id) {
            return _entityVisuals.GetValueOrDefault(id)?.transform;
        }
        
        private void OnEntitySpawned(GridEntity entity, Vector2Int newPosition) {
            // Instantiate entity visual and attach billboards
            GameObject entityVisual = Instantiate(entityPrefab, transform);
            entityVisual.name =  $"{entity.ID} : {entity.GridEntityData.name}";
            Debug.Log($"{entity.ID} : {entity.DisplayName}");
            
            _entityVisuals[entity.ID] = entityVisual;
            entityVisual.transform.position = VectorUtils.Vector2IntToVector3(newPosition);
            if (entityVisual.TryGetComponent(out EntityVisual entityVisualScript)) {
                entityVisualScript.SetColor(entity.Faction == Faction.Player ? new Color(.05f,.05f,1,1) : new Color(1,.05f,.05f,1));
            }
            AttachBillboards(entity);
            Debug.Log(DictionaryUtils.FormatDictionary(_entityVisuals));
            entityVisualScript.Animator.runtimeAnimatorController = entity.AnimatorController;
        }
        
        
        // ==============================
        // HELPERS
        // ==============================
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

        private void FaceEntity(GridEntity thisEntity, GridEntity otherEntity) {
            if (!_entityVisuals.ContainsKey(thisEntity.ID) || !_entityVisuals.ContainsKey(otherEntity.ID)) {
                throw new Exception("GridEntitySpawner.FaceEntity: One or both of the entities could not be retrieved from _entityVisuals dictionary!");
            }
            EntityVisual entityVisual = _entityVisuals[thisEntity.ID].GetComponent<EntityVisual>();
            EntityVisual otherEntityVisual = _entityVisuals[otherEntity.ID].GetComponent<EntityVisual>();
            if (entityVisual == null || otherEntityVisual == null) {
                throw new Exception("GridEntitySpawner.FaceEntity: One or both of the entity visuals could not be retrieved!");
            }
            if (thisEntity.GridPosition.x == otherEntity.GridPosition.x) return;
            entityVisual.SetSpriteFlipX(thisEntity.GridPosition.x > otherEntity.GridPosition.x);
        }
    }
}
