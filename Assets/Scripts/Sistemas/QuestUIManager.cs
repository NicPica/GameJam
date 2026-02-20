using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja la UI visual de las quests
/// Muestra sprites de items con contadores (ej: sprite + "2/3")
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    [Header("UI Container")]
    [Tooltip("Panel contenedor donde se crean los elementos de quest")]
    public Transform questContainer;
    
    [Tooltip("Prefab de un elemento de quest individual")]
    public GameObject questItemPrefab;
    
    [Header("Layout Settings")]
    [Tooltip("Espacio entre elementos")]
    public float spacing = 10f;
    
    [Tooltip("Tamaño del icono")]
    public float iconSize = 64f;
    
    [Header("Colors")]
    [Tooltip("Color cuando el item NO está completo")]
    public Color incompleteColor = Color.white;
    
    [Tooltip("Color cuando el item está completo")]
    public Color completeColor = Color.green;
    
    // Lista de elementos UI activos
    private List<QuestItemUI> activeQuestItems = new List<QuestItemUI>();
    
    /// <summary>
    /// Clase interna para representar un elemento de quest en la UI
    /// </summary>
    [System.Serializable]
    public class QuestItemUI
    {
        public GameObject gameObject;
        public Image iconImage;
        public Text countText;
        public string itemName;
        public int requiredAmount;
        public int currentAmount;
        
        public void UpdateDisplay(Color incompleteColor, Color completeColor)
        {
            if (countText != null)
            {
                countText.text = $"{currentAmount}/{requiredAmount}";
                
                // Cambiar color según si está completo
                bool isComplete = currentAmount >= requiredAmount;
                countText.color = isComplete ? completeColor : incompleteColor;
                
                if (iconImage != null)
                {
                    iconImage.color = isComplete ? completeColor : incompleteColor;
                }
            }
        }
    }
    
    void Start()
    {
        // Asegurar que el prefab existe
        if (questItemPrefab == null)
        {
            Debug.LogError("QuestUIManager: questItemPrefab no está asignado!");
        }
        
        if (questContainer == null)
        {
            Debug.LogError("QuestUIManager: questContainer no está asignado!");
        }
    }
    
    /// <summary>
    /// Crea la UI para una lista de quest items
    /// </summary>
    public void CreateQuestUI(List<QuestManager.QuestItem> questItems, List<ItemData> itemDataList)
    {
        // Limpiar UI anterior
        ClearQuestUI();
        
        if (questItems == null || questItems.Count == 0)
        {
            Debug.LogWarning("QuestUIManager: No hay quest items para mostrar");
            return;
        }
        
        // Crear un elemento UI por cada quest item
        foreach (var questItem in questItems)
        {
            // Buscar el ItemData correspondiente
            ItemData itemData = itemDataList?.Find(data => data.itemName == questItem.itemName);
            
            if (itemData == null)
            {
                Debug.LogWarning($"QuestUIManager: No se encontró ItemData para '{questItem.itemName}'");
                continue;
            }
            
            CreateQuestItemElement(questItem, itemData);
        }
    }
    
    /// <summary>
    /// Crea un elemento UI individual para un quest item
    /// </summary>
    void CreateQuestItemElement(QuestManager.QuestItem questItem, ItemData itemData)
    {
        if (questItemPrefab == null || questContainer == null)
            return;
        
        // Instanciar el prefab
        GameObject itemObject = Instantiate(questItemPrefab, questContainer);
        
        // Obtener referencias a los componentes
        Image iconImage = itemObject.transform.Find("Icon")?.GetComponent<Image>();
        Text countText = itemObject.transform.Find("CountText")?.GetComponent<Text>();
        
        if (iconImage == null || countText == null)
        {
            Debug.LogError("QuestUIManager: El prefab no tiene los componentes necesarios (Icon Image y CountText)");
            Destroy(itemObject);
            return;
        }
        
        // Configurar el icono
        if (itemData.icon != null)
        {
            iconImage.sprite = itemData.icon;
        }
        else
        {
            Debug.LogWarning($"QuestUIManager: '{itemData.itemName}' no tiene icono asignado");
        }
        
        // Configurar tamaño del icono
        RectTransform iconRect = iconImage.GetComponent<RectTransform>();
        if (iconRect != null)
        {
            iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        }
        
        // Crear y guardar el QuestItemUI
        QuestItemUI questItemUI = new QuestItemUI
        {
            gameObject = itemObject,
            iconImage = iconImage,
            countText = countText,
            itemName = questItem.itemName,
            requiredAmount = questItem.requiredAmount,
            currentAmount = questItem.currentAmount
        };
        
        // Actualizar display inicial
        questItemUI.UpdateDisplay(incompleteColor, completeColor);
        
        // Añadir a la lista
        activeQuestItems.Add(questItemUI);
    }
    
    /// <summary>
    /// Actualiza el contador de un item específico
    /// </summary>
    public void UpdateQuestItem(string itemName, int currentAmount, int requiredAmount)
    {
        QuestItemUI questItemUI = activeQuestItems.Find(item => item.itemName == itemName);
        
        if (questItemUI != null)
        {
            questItemUI.currentAmount = currentAmount;
            questItemUI.requiredAmount = requiredAmount;
            questItemUI.UpdateDisplay(incompleteColor, completeColor);
        }
    }
    
    /// <summary>
    /// Actualiza todos los contadores basándose en la lista de quest items
    /// </summary>
    public void UpdateAllQuestItems(List<QuestManager.QuestItem> questItems)
    {
        foreach (var questItem in questItems)
        {
            UpdateQuestItem(questItem.itemName, questItem.currentAmount, questItem.requiredAmount);
        }
    }
    
    /// <summary>
    /// Limpia todos los elementos de quest de la UI
    /// </summary>
    public void ClearQuestUI()
    {
        foreach (var questItemUI in activeQuestItems)
        {
            if (questItemUI.gameObject != null)
            {
                Destroy(questItemUI.gameObject);
            }
        }
        
        activeQuestItems.Clear();
    }
    
    /// <summary>
    /// Muestra un mensaje de quest completada
    /// </summary>
    public void ShowQuestComplete()
    {
        // Actualizar todos los items a color completo
        foreach (var questItemUI in activeQuestItems)
        {
            questItemUI.currentAmount = questItemUI.requiredAmount;
            questItemUI.UpdateDisplay(incompleteColor, completeColor);
        }
    }
}
