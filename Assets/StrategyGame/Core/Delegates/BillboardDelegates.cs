using System;
using StrategyGame.Grid;
using StrategyGame.Grid.GridData;
using UnityEngine;

namespace StrategyGame.Core.Delegates {
    public static class BillboardDelegates
    {
        // ==============================
        // EVENTS
        // ==============================
        public static event Action<int, int, int> OnHealthChanged;
        public static event Action<GridUnit> OnUnitWeaponTypeChanged;

        public static void InvokeOnHealthChanged(int id, int health, int maxHealth) {
            OnHealthChanged?.Invoke(id, health, maxHealth);
        }
        public static void InvokeOnUnitWeaponTypeChanged(GridUnit unit) {
            OnUnitWeaponTypeChanged?.Invoke(unit);
        }
       

        // ==============================
        // FUNCS
        // ==============================
        public static Func<Transform> GetBillboardCanvasTransform;
        
    }
}
