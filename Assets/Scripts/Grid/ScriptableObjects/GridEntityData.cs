using UnityEngine;

[CreateAssetMenu(fileName = "New Grid Entity Data", menuName = "Grid Entity Data")]
public class GridEntityData : ScriptableObject {
   public GameObject SpritePrefab;
   public int Health;
   public int MaxHealth;
}
