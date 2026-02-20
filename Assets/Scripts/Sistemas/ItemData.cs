using UnityEngine;

/// <summary>
/// ScriptableObject que define un tipo de item
/// Usado para crear el pool de items posibles
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    [Tooltip("Nombre del item (debe coincidir con itemName en CollectableItem)")]
    public string itemName = "Item";
    
    [Tooltip("Tipo de item (debe coincidir con itemType en CollectableItem)")]
    public string itemType = "Generic";
    
    [Tooltip("Icono del item para UI (opcional)")]
    public Sprite icon;
    
    [Header("Quota Settings")]
    [Tooltip("¿Puede aparecer en cuotas?")]
    public bool canBeInQuota = true;
    
    [Tooltip("Peso de probabilidad (mayor = más común en cuotas)")]
    [Range(1, 100)]
    public int spawnWeight = 50;
    
    [Tooltip("Cantidad mínima en cuota")]
    [Range(1, 20)]
    public int minQuotaAmount = 1;
    
    [Tooltip("Cantidad máxima en cuota")]
    [Range(1, 20)]
    public int maxQuotaAmount = 5;
    
    [Header("Visual (Opcional)")]
    [Tooltip("Prefab del item en el mundo")]
    public GameObject itemPrefab;
    
    [Tooltip("Color asociado al item")]
    public Color itemColor = Color.white;
}
