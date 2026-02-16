using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// Sistema de inventario simple para rastrear items recolectados
/// Este script debe estar en el GameObject del jugador o en un GameManager

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("Capacidad máxima de items (0 = ilimitado)")]
    public int maxCapacity = 0;

    [Header("UI References (Opcional)")]
    [Tooltip("Texto que muestra el conteo de items")]
    public UnityEngine.UI.Text itemCountText;

    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al recoger un item")]
    public AudioClip pickupSound;

    [Header("Events")]
    [Tooltip("Evento que se dispara cuando se recoge un item")]
    public UnityEvent<CollectableItem> onItemCollected;

    [Tooltip("Evento que se dispara cuando el inventario está lleno")]
    public UnityEvent onInventoryFull;

    // Diccionario para rastrear cantidad de cada tipo de item
    private Dictionary<string, int> itemCounts = new Dictionary<string, int>();

    // Lista de todos los items recolectados
    private List<CollectableItem> collectedItems = new List<CollectableItem>();

    private AudioSource audioSource;

    // Singleton
    public static InventorySystem Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        UpdateUI();
    }

    /// Añade un item al inventario
    public bool AddItem(CollectableItem item)
    {
        // Verificar capacidad
        if (maxCapacity > 0 && GetTotalItemCount() >= maxCapacity)
        {
            Debug.Log("Inventario lleno!");
            onInventoryFull?.Invoke();
            return false;
        }

        // Añadir al diccionario
        string itemType = item.itemName;
        if (itemCounts.ContainsKey(itemType))
        {
            itemCounts[itemType]++;
        }
        else
        {
            itemCounts[itemType] = 1;
        }

        // Añadir a la lista
        collectedItems.Add(item);

        // Reproducir sonido
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }

        // Disparar evento
        onItemCollected?.Invoke(item);

        // Actualizar UI
        UpdateUI();

        Debug.Log($"Item recogido: {item.itemName} (Total: {itemCounts[itemType]})");

        return true;
    }

    /// Remueve items del inventario
    public bool RemoveItem(string itemType, int amount = 1)
    {
        if (!itemCounts.ContainsKey(itemType) || itemCounts[itemType] < amount)
        {
            return false;
        }

        itemCounts[itemType] -= amount;

        if (itemCounts[itemType] <= 0)
        {
            itemCounts.Remove(itemType);
        }

        UpdateUI();
        return true;
    }

    /// Obtiene la cantidad de un tipo específico de item
    public int GetItemCount(string itemType)
    {
        return itemCounts.ContainsKey(itemType) ? itemCounts[itemType] : 0;
    }

    /// Obtiene el conteo total de todos los items
    public int GetTotalItemCount()
    {
        return collectedItems.Count;
    }

    /// Verifica si tiene suficientes items de un tipo
    public bool HasItem(string itemType, int requiredAmount = 1)
    {
        return GetItemCount(itemType) >= requiredAmount;
    }

    /// Limpia el inventario
    public void ClearInventory()
    {
        itemCounts.Clear();
        collectedItems.Clear();
        UpdateUI();
        Debug.Log("Inventario limpiado");
    }

    /// Devuelve un diccionario con todos los items y sus cantidades
    public Dictionary<string, int> GetAllItems()
    {
        return new Dictionary<string, int>(itemCounts);
    }

    /// Actualiza el texto UI del inventario
    void UpdateUI()
    {
        if (itemCountText != null)
        {
            if (maxCapacity > 0)
            {
                itemCountText.text = $"Items: {GetTotalItemCount()}/{maxCapacity}";
            }
            else
            {
                itemCountText.text = $"Items: {GetTotalItemCount()}";
            }
        }
    }

    /// Debug: Muestra el contenido del inventario en consola
    public void PrintInventory()
    {
        Debug.Log("=== INVENTARIO ===");
        foreach (var item in itemCounts)
        {
            Debug.Log($"{item.Key}: {item.Value}");
        }
        Debug.Log($"Total: {GetTotalItemCount()} items");
    }
}