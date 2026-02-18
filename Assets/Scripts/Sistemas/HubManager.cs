using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestor del Hub simplificado - Solo muestra niveles y permite entrar
/// Sin sistema de mejoras
/// </summary>
public class HubManager : MonoBehaviour
{
    [Header("Level Quests")]
    [Tooltip("Listas de items para cada nivel")]
    public List<QuestData> levelQuests = new List<QuestData>();

    [Header("UI References")]
    public GameObject questDisplayPanel;
    public Text questTitleText;
    public Text questListText;
    public Button startLevelButton;

    public Text currentLevelText;

    [Header("Final Door")]
    public GameObject finalDoor;
    public Text finalDoorText;

    void Start()
    {
        // Desbloquear cursor en el Hub
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        InitializeHub();
    }

    void Update()
    {
        // ESC para desbloquear cursor (seguridad)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void InitializeHub()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager no encontrado!");
            return;
        }

        int currentLevel = GameManager.Instance.currentLevel;

        // Ocultar paneles inicialmente
        if (questDisplayPanel != null)
            questDisplayPanel.SetActive(false);

        if (finalDoor != null)
            finalDoor.SetActive(false);

        // Actualizar info general
        UpdateGeneralInfo();

        // Determinar qué mostrar según el nivel
        if (currentLevel == 0)
        {
            // Primer ingreso al juego
            ShowQuestForLevel(1);
        }
        else if (currentLevel > 0 && currentLevel <= levelQuests.Count)
        {
            // Mostrar quest del siguiente nivel
            ShowQuestForLevel(currentLevel);
        }
        else if (currentLevel > levelQuests.Count)
        {
            // Completó todos los niveles - mostrar puerta final
            ShowFinalDoor();
        }
    }

    /// <summary>
    /// Muestra la quest para un nivel específico
    /// </summary>
    void ShowQuestForLevel(int level)
    {
        if (level <= 0 || level > levelQuests.Count)
        {
            Debug.LogError($"Nivel inválido: {level}");
            return;
        }

        QuestData questData = levelQuests[level - 1];

        if (questDisplayPanel != null)
        {
            questDisplayPanel.SetActive(true);

            if (questTitleText != null)
                questTitleText.text = $"Nivel {level} - Lista de Recolección";

            if (questListText != null)
            {
                string questList = "Items necesarios:\n\n";
                foreach (var item in questData.requiredItems)
                {
                    questList += $"• {item.itemName} x{item.amount}\n";
                }
                questListText.text = questList;
            }

            if (startLevelButton != null)
            {
                startLevelButton.onClick.RemoveAllListeners();
                startLevelButton.onClick.AddListener(() => StartLevel(level));
            }
        }
    }

    /// <summary>
    /// Inicia el nivel actual
    /// </summary>
    void StartLevel(int level)
    {
        Debug.Log($"Iniciando nivel {level}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentLevel = level;
            GameManager.Instance.LoadCurrentLevel();
        }
    }

    /// <summary>
    /// Muestra la puerta final
    /// </summary>
    void ShowFinalDoor()
    {
        Debug.Log("Mostrando puerta final del juego");

        if (finalDoor != null)
        {
            finalDoor.SetActive(true);

            if (finalDoorText != null)
            {
                finalDoorText.text = "¡Has completado todas las misiones!\nPresiona E para descubrir el secreto...";
            }
        }

        // Ocultar otros paneles
        if (questDisplayPanel != null)
            questDisplayPanel.SetActive(false);
    }

    /// <summary>
    /// Actualiza la información general del Hub
    /// </summary>
    void UpdateGeneralInfo()
    {
        if (GameManager.Instance == null)
            return;

        if (currentLevelText != null)
        {
            int level = GameManager.Instance.currentLevel;
            currentLevelText.text = $"Progreso: Nivel {level}/{levelQuests.Count}";
        }
    }

    /// <summary>
    /// Debug: Avanzar al siguiente nivel forzadamente
    /// </summary>
    [ContextMenu("Skip to Next Level (Debug)")]
    public void DebugSkipToNextLevel()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentLevel++;
            InitializeHub();
        }
    }
}

/// <summary>
/// Datos de una quest/lista para un nivel
/// </summary>
[System.Serializable]
public class QuestData
{
    [System.Serializable]
    public class RequiredItem
    {
        public string itemName;
        public int amount = 1;
    }

    public string questName = "Lista de Recolección";
    public List<RequiredItem> requiredItems = new List<RequiredItem>();
}
