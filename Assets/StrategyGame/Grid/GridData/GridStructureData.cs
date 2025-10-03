using UnityEngine;

namespace StrategyGame.Grid.GridData {


    public enum StructureType {
        Regular,
        Spawner,
        Fort,
        Ballista
    }

    [CreateAssetMenu(menuName = "Strategy Game/Grid Structure")]
    public class GridStructureData : GridEntityData {
        [SerializeField] private StructureType structureType;


    }

}

