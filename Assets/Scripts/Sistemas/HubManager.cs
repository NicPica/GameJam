using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestor del Hub donde el jugador recibe listas y elige mejoras
/// Versión simplificada que crea la UI dinámicamente
/// </summary>
public class HubManager : MonoBehaviour
{
    [System.Serializable]
    public class Upgrade
    {
        public string upgradeName;
        [TextArea(2, 4)]
        public string description;
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

    public Text currentLevelText;
    public Text upgradesText;

    [Header("Canvas Reference")]
    [Tooltip("Canvas principal del Hub")]
    public Canvas mainCanvas;

    [Header("Final Door")]
    public GameObject finalDoor;
    public Text finalDoorText;

    // Referencias internas para el panel de mejoras (creado dinámicamente)
    private GameObject upgradePanel;
    private bool waitingForUpgradeSelection = false;

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

        // Destruir panel de mejoras si existe
        if (upgradePanel != null)
        {
            Destroy(upgradePanel);
            upgradePanel = null;
        }

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
    /// Muestra la selección de mejoras (crea UI dinámicamente)
    /// </summary>
    void ShowUpgradeSelection(int completedLevel)
    {
        Debug.Log($"Mostrando selección de mejoras para nivel {completedLevel}");
        waitingForUpgradeSelection = true;

        // Verificar que tenemos un Canvas
        if (mainCanvas == null)
        {
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("No se encontró Canvas en la escena!");
                return;
            }
        }

        // Obtener las mejoras disponibles
        List<Upgrade> availableUpgrades = GetUpgradesForLevel(completedLevel - 1);

        if (availableUpgrades.Count == 0)
        {
            Debug.LogWarning($"No hay mejoras configuradas para el nivel {completedLevel - 1}");
            // Continuar sin mejora
            ContinueWithoutUpgrade();
            return;
        }

        // Crear panel de mejoras desde cero
        CreateUpgradePanel(availableUpgrades, completedLevel);
    }

    /// <summary>
    /// Crea el panel de mejoras dinámicamente
    /// </summary>
    void CreateUpgradePanel(List<Upgrade> upgrades, int completedLevel)
    {
        // Panel principal (fondo oscuro)
        upgradePanel = new GameObject("UpgradeSelectionPanel");
        upgradePanel.transform.SetParent(mainCanvas.transform, false);

        RectTransform panelRect = upgradePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = upgradePanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f); // Fondo negro semi-transparente

        // Título
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(upgradePanel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -50);
        titleRect.sizeDelta = new Vector2(600, 60);

        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = $"¡Nivel {completedLevel - 1} Completado!";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 36;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.yellow;

        // Subtítulo
        GameObject subtitleObj = new GameObject("Subtitle");
        subtitleObj.transform.SetParent(upgradePanel.transform, false);

        RectTransform subtitleRect = subtitleObj.AddComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0.5f, 1f);
        subtitleRect.anchorMax = new Vector2(0.5f, 1f);
        subtitleRect.pivot = new Vector2(0.5f, 1f);
        subtitleRect.anchoredPosition = new Vector2(0, -120);
        subtitleRect.sizeDelta = new Vector2(500, 40);

        Text subtitleText = subtitleObj.AddComponent<Text>();
        subtitleText.text = "Elige una mejora:";
        subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subtitleText.fontSize = 24;
        subtitleText.alignment = TextAnchor.MiddleCenter;
        subtitleText.color = Color.white;

        // Crear botones de mejoras
        float startY = -200;
        float buttonHeight = 100;
        float buttonSpacing = 20;

        for (int i = 0; i < upgrades.Count; i++)
        {
            CreateUpgradeButton(upgrades[i], upgradePanel.transform, startY - (i * (buttonHeight + buttonSpacing)));
        }

        Debug.Log($"Panel de mejoras creado con {upgrades.Count} opciones");
    }

    /// <summary>
    /// Crea un botón de mejora individual
    /// </summary>
    void CreateUpgradeButton(Upgrade upgrade, Transform parent, float posY)
    {
        // Botón
        GameObject buttonObj = new GameObject($"Button_{upgrade.upgradeName}");
        buttonObj.transform.SetParent(parent, false);

        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0, posY);
        buttonRect.sizeDelta = new Vector2(500, 90);

        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        
        // Configurar colores del botón
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.2f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.3f, 1f);
        button.colors = colors;

        button.onClick.AddListener(() => SelectUpgrade(upgrade));

        // Texto del botón
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 10);
        textRect.offsetMax = new Vector2(-15, -10);

        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = $"<b>{upgrade.upgradeName}</b>\n<size=14>{upgrade.description}</size>";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 18;
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.color = Color.white;

        Debug.Log($"Botón creado: {upgrade.upgradeName}");
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

        // Destruir panel de mejoras
        if (upgradePanel != null)
        {
            Destroy(upgradePanel);
            upgradePanel = null;
        }

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
    /// Continúa sin elegir mejora (si no hay disponibles)
    /// </summary>
    void ContinueWithoutUpgrade()
    {
        waitingForUpgradeSelection = false;

        int nextLevel = GameManager.Instance.currentLevel;
        if (nextLevel <= levelQuests.Count)
        {
            ShowQuestForLevel(nextLevel);
        }
        else
        {
            ShowFinalDoor();
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

    /// <summary>
    /// Debug: Mostrar panel de mejoras
    /// </summary>
    [ContextMenu("Test Show Upgrades (Debug)")]
    void TestShowUpgrades()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentLevel = 1;
        }
        ShowUpgradeSelection(1);
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
