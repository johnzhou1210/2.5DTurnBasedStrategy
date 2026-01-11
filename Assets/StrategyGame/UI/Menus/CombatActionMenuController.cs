using StrategyGame.Core.Delegates;
using UnityEngine;

namespace StrategyGame.UI.Menus {
public class CombatActionMenuController : MonoBehaviour {
   [SerializeField] private GameObject actionMenuContents;

   private int _currentSelectedIndex = 0;
   
   private void OnEnable() {
      UIDelegates.OnSetCombatActionMenuVisibility += SetVisible;
   }

   private void OnDisable() {
      UIDelegates.OnSetCombatActionMenuVisibility -= SetVisible;
   }

   private void SetVisible(bool visible) {
      actionMenuContents.SetActive(visible);
   }
}
}
