using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestor del Hub donde el jugador recibe listas y elige mejoras
/// </summary>
public class HubManager : MonoBehaviour
{
    [System.Serializable]
    public class Upgrade
    {
        public string upgradeName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;
    }

    [Header("Level Quests")]
    [Tooltip("Listas de items para cada nivel")]
    public List<QuestData> levelQuests = new List<QuestData>();

    [Header("Upgrades per Level")]
    [Tooltip("Mejoras disponibles después del nivel 1")]
    public List<Upgrade> upgradesLevel1 = new List<Upgrade>();

    [Tooltip("Mejoras disponibles después del nivel 2")]
    public List<Upgrade> upgradesLevel2 = new List<Upgrade>();

    [Header("UI References")]
    public GameObject questDisplayPanel;
    public Text questTitleText;
    public Text questListText;
    public Button startLevelButton;

    public GameObject upgradeSelectionPanel;
    public Text upgradePromptText;
    public GameObject upgradeButtonPrefab;
    public Transform upgradeButtonContainer;

    public Text currentLevelText;
    public Text upgradesText;

    [Header("Final Door")]
    public GameObject finalDoor;
    public Text finalDoorText;

    // Estado
    private bool waitingForUpgradeSelection = false;

    void Start()
    {
        InitializeHub();
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

        if (upgradeSelectionPanel != null)
            upgradeSelectionPanel.SetActive(false);

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
        else if (currentLevel <= levelQuests.Count && !waitingForUpgradeSelection)
        {
            // Volvió de un nivel - mostrar selección de mejoras
            ShowUpgradeSelection(currentLevel);
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
    /// Muestra la selección de mejoras
    /// </summary>
    void ShowUpgradeSelection(int completedLevel)
    {
        waitingForUpgradeSelection = true;

        if (upgradeSelectionPanel != null)
        {
            upgradeSelectionPanel.SetActive(true);

            if (upgradePromptText != null)
            {
                upgradePromptText.text = $"¡Nivel {completedLevel - 1} completado!\nElige una mejora:";
            }

            // Limpiar botones anteriores
            foreach (Transform child in upgradeButtonContainer)
            {
                Destroy(child.gameObject);
            }

            // Obtener las mejoras correspondientes
            List<Upgrade> availableUpgrades = GetUpgradesForLevel(completedLevel - 1);

            // Crear botones de mejoras
            foreach (var upgrade in availableUpgrades)
            {
                CreateUpgradeButton(upgrade);
            }
        }
    }

    /// <summary>
    /// Obtiene las mejoras disponibles para un nivel
    /// </summary>
    List<Upgrade> GetUpgradesForLevel(int level)
    {
        switch (level)
        {
            case 1:
                return upgradesLevel1;
            case 2:
                return upgradesLevel2;
            default:
                return new List<Upgrade>();
        }
    }

    /// <summary>
    /// Crea un botón de mejora
    /// </summary>
    void CreateUpgradeButton(Upgrade upgrade)
    {
        if (upgradeButtonPrefab == null || upgradeButtonContainer == null)
            return;

        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
        Button button = buttonObj.GetComponent<Button>();

        // Configurar texto
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = $"{upgrade.upgradeName}\n<size=10>{upgrade.description}</size>";
        }

        // Configurar imagen (opcional)
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null && upgrade.icon != null)
        {
            buttonImage.sprite = upgrade.icon;
        }

        // Configurar acción del botón
        if (button != null)
        {
            button.onClick.AddListener(() => SelectUpgrade(upgrade));
        }
    }

    /// <summary>
    /// Selecciona una mejora
    /// </summary>
    void SelectUpgrade(Upgrade upgrade)
    {
        Debug.Log($"Mejora seleccionada: {upgrade.upgradeName}");

        // Añadir mejora al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddUpgrade(upgrade.upgradeName);
        }

        waitingForUpgradeSelection = false;

        // Ocultar panel de mejoras
        if (upgradeSelectionPanel != null)
            upgradeSelectionPanel.SetActive(false);

        // Mostrar la quest del siguiente nivel
        int nextLevel = GameManager.Instance.currentLevel;
        if (nextLevel <= levelQuests.Count)
        {
            ShowQuestForLevel(nextLevel);
        }
        else
        {
            ShowFinalDoor();
        }

        UpdateGeneralInfo();
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

        if (upgradeSelectionPanel != null)
            upgradeSelectionPanel.SetActive(false);
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

        if (upgradesText != null)
        {
            List<string> upgrades = GameManager.Instance.GetAllUpgrades();
            if (upgrades.Count > 0)
            {
                upgradesText.text = "Mejoras activas:\n" + string.Join("\n", upgrades);
            }
            else
            {
                upgradesText.text = "Sin mejoras aún";
            }
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
