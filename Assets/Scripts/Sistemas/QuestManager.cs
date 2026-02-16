using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestor de misiones/listas de recolección
/// Rastrea qué items necesita el jugador recoger para completar el nivel
/// </summary>
public class QuestManager : MonoBehaviour
{
    [System.Serializable]
    public class QuestItem
    {
        public string itemName;
        public int requiredAmount = 1;
        [HideInInspector]
        public int currentAmount = 0;

        public bool IsCompleted()
        {
            return currentAmount >= requiredAmount;
        }

        public float GetProgress()
        {
            return (float)currentAmount / requiredAmount;
        }
    }

    [Header("Quest Configuration")]
    [Tooltip("Lista de items necesarios para completar el nivel")]
    public List<QuestItem> questItems = new List<QuestItem>();

    [Tooltip("¿Se puede completar el nivel sin todos los items?")]
    public bool allowPartialCompletion = false;

    [Header("Events")]
    public UnityEvent onQuestStarted;
    public UnityEvent<QuestItem> onItemCollected;
    public UnityEvent onQuestCompleted;
    public UnityEvent onQuestFailed;

    [Header("UI References (Opcional)")]
    public UnityEngine.UI.Text questStatusText;

    // Estado
    private bool questActive = false;
    private bool questCompleted = false;

    // Singleton para fácil acceso
    public static QuestManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        StartQuest();
    }

    void OnEnable()
    {
        // Suscribirse al evento de inventario
        // Esperar un frame para asegurarse de que el Player se haya cargado
        StartCoroutine(SubscribeToInventoryDelayed());
    }

    void OnDisable()
    {
        // Desuscribirse del evento
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.onItemCollected.RemoveListener(OnItemPickedUp);
        }
    }

    IEnumerator SubscribeToInventoryDelayed()
    {
        // Esperar un frame para que el Player se inicialice
        yield return null;

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.onItemCollected.AddListener(OnItemPickedUp);
            DebugLog("QuestManager suscrito al InventorySystem");
        }
        else
        {
            Debug.LogWarning("InventorySystem no encontrado. Asegúrate de que el Player está en la escena.");
        }
    }

    void DebugLog(string message)
    {
        Debug.Log($"[QuestManager] {message}");
    }

    /// <summary>
    /// Inicia la quest
    /// </summary>
    public void StartQuest()
    {
        questActive = true;
        questCompleted = false;

        // Resetear contadores
        foreach (var item in questItems)
        {
            item.currentAmount = 0;
        }

        UpdateQuestUI();
        onQuestStarted?.Invoke();

        Debug.Log("Quest iniciada - Items necesarios:");
        foreach (var item in questItems)
        {
            Debug.Log($"  - {item.itemName} x{item.requiredAmount}");
        }
    }

    /// <summary>
    /// Llamado cuando el jugador recoge un item
    /// </summary>
    void OnItemPickedUp(CollectableItem item)
    {
        if (!questActive || questCompleted)
            return;

        // Buscar el item en la lista de quest
        QuestItem questItem = questItems.Find(q => q.itemName == item.itemName);

        if (questItem != null && !questItem.IsCompleted())
        {
            questItem.currentAmount++;
            Debug.Log($"Progreso de quest: {questItem.itemName} ({questItem.currentAmount}/{questItem.requiredAmount})");

            onItemCollected?.Invoke(questItem);
            UpdateQuestUI();

            // Verificar si se completó la quest
            if (IsQuestComplete())
            {
                CompleteQuest();
            }
        }
    }

    /// <summary>
    /// Verifica si todos los items requeridos fueron recolectados
    /// </summary>
    public bool IsQuestComplete()
    {
        if (questItems.Count == 0)
            return false;

        foreach (var item in questItems)
        {
            if (!item.IsCompleted())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Completa la quest
    /// </summary>
    void CompleteQuest()
    {
        if (questCompleted)
            return;

        questCompleted = true;
        questActive = false;

        Debug.Log("¡Quest completada!");
        onQuestCompleted?.Invoke();

        UpdateQuestUI();
    }

    /// <summary>
    /// Obtiene el progreso total de la quest (0-1)
    /// </summary>
    public float GetTotalProgress()
    {
        if (questItems.Count == 0)
            return 0f;

        int totalRequired = 0;
        int totalCollected = 0;

        foreach (var item in questItems)
        {
            totalRequired += item.requiredAmount;
            totalCollected += item.currentAmount;
        }

        return (float)totalCollected / totalRequired;
    }

    /// <summary>
    /// Obtiene cuántos items faltan recoger
    /// </summary>
    public int GetRemainingItemsCount()
    {
        int remaining = 0;
        foreach (var item in questItems)
        {
            remaining += Mathf.Max(0, item.requiredAmount - item.currentAmount);
        }
        return remaining;
    }

    /// <summary>
    /// Actualiza el texto UI de la quest
    /// </summary>
    void UpdateQuestUI()
    {
        if (questStatusText == null)
            return;

        if (questCompleted)
        {
            questStatusText.text = "¡Quest completada! Ve a la salida";
            questStatusText.color = Color.green;
        }
        else
        {
            string statusText = "Lista de recolección:\n";
            foreach (var item in questItems)
            {
                string checkmark = item.IsCompleted() ? "✓" : "•";
                statusText += $"{checkmark} {item.itemName}: {item.currentAmount}/{item.requiredAmount}\n";
            }
            questStatusText.text = statusText;
            questStatusText.color = Color.white;
        }
    }

    /// <summary>
    /// Obtiene la lista de items como string (para mostrar en el Hub)
    /// </summary>
    public string GetQuestListAsString()
    {
        string list = "Items necesarios:\n\n";
        foreach (var item in questItems)
        {
            list += $"• {item.itemName} x{item.requiredAmount}\n";
        }
        return list;
    }

    /// <summary>
    /// Debug: Completa la quest automáticamente
    /// </summary>
    [ContextMenu("Complete Quest (Debug)")]
    public void DebugCompleteQuest()
    {
        foreach (var item in questItems)
        {
            item.currentAmount = item.requiredAmount;
        }
        CompleteQuest();
    }
}
