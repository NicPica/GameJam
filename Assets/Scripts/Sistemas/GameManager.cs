using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor principal del juego que controla el flujo entre Hub y Niveles
/// Singleton persistente entre escenas
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Flow")]
    [Tooltip("Nombre de la escena del Hub")]
    public string hubSceneName = "Hub";

    [Tooltip("Nombres de las escenas de los niveles en orden")]
    public string[] levelSceneNames = { "Level1", "Level2", "Level3" };

    [Header("Game State")]
    [Tooltip("Nivel actual (0 = Hub, 1-3 = Niveles)")]
    public int currentLevel = 0;

    [Tooltip("Mejoras desbloqueadas por el jugador")]
    public List<string> unlockedUpgrades = new List<string>();

    [Header("Debug")]
    [Tooltip("¿Mostrar información de debug en consola?")]
    public bool debugMode = true;

    // Estado del juego
    private bool playerDiedInLevel = false;
    private bool levelCompleted = false;

    // Singleton
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Implementar Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        DebugLog("GameManager inicializado");
    }

    void Start()
    {
        // Iniciar en el Hub
        if (SceneManager.GetActiveScene().name != hubSceneName)
        {
            LoadHub();
        }
    }

    #region Scene Management

    /// <summary>
    /// Carga el Hub
    /// </summary>
    public void LoadHub()
    {
        DebugLog("Cargando Hub...");
        playerDiedInLevel = false;
        levelCompleted = false;
        SceneManager.LoadScene(hubSceneName);
    }

    /// <summary>
    /// Carga el nivel correspondiente según el progreso
    /// </summary>
    public void LoadCurrentLevel()
    {
        if (currentLevel <= 0 || currentLevel > levelSceneNames.Length)
        {
            Debug.LogError($"Nivel inválido: {currentLevel}");
            return;
        }

        string levelName = levelSceneNames[currentLevel - 1];
        DebugLog($"Cargando nivel {currentLevel}: {levelName}");
        
        playerDiedInLevel = false;
        levelCompleted = false;
        
        SceneManager.LoadScene(levelName);
    }

    /// <summary>
    /// Reinicia el nivel actual
    /// </summary>
    public void RestartCurrentLevel()
    {
        DebugLog("Reiniciando nivel actual...");
        LoadCurrentLevel();
    }

    #endregion

    #region Level Completion

    /// <summary>
    /// Llamar cuando el jugador completa un nivel exitosamente
    /// </summary>
    public void CompleteLevel()
    {
        levelCompleted = true;
        DebugLog($"Nivel {currentLevel} completado!");

        // Si es el último nivel, terminar el juego
        if (currentLevel >= levelSceneNames.Length)
        {
            DebugLog("¡Juego completado!");
            EndGame();
        }
        else
        {
            // Avanzar al siguiente nivel
            currentLevel++;
            LoadHub();
        }
    }

    /// <summary>
    /// Llamar cuando el jugador muere o falla el nivel
    /// </summary>
    public void FailLevel()
    {
        playerDiedInLevel = true;
        DebugLog("Nivel fallido - Reiniciando...");
        
        // Mostrar pantalla de muerte (lo implementaremos después)
        StartCoroutine(ShowDeathScreenAndRestart());
    }

    IEnumerator ShowDeathScreenAndRestart()
    {
        // Esperar 2 segundos antes de reiniciar
        yield return new WaitForSeconds(2f);
        RestartCurrentLevel();
    }

    #endregion

    #region Upgrades

    /// <summary>
    /// Añade una mejora al jugador
    /// </summary>
    public void AddUpgrade(string upgradeName)
    {
        if (!unlockedUpgrades.Contains(upgradeName))
        {
            unlockedUpgrades.Add(upgradeName);
            DebugLog($"Mejora desbloqueada: {upgradeName}");
        }
    }

    /// <summary>
    /// Verifica si el jugador tiene una mejora específica
    /// </summary>
    public bool HasUpgrade(string upgradeName)
    {
        return unlockedUpgrades.Contains(upgradeName);
    }

    /// <summary>
    /// Obtiene todas las mejoras desbloqueadas
    /// </summary>
    public List<string> GetAllUpgrades()
    {
        return new List<string>(unlockedUpgrades);
    }

    #endregion

    #region Game End

    /// <summary>
    /// Termina el juego (mostrar pantalla de victoria)
    /// </summary>
    void EndGame()
    {
        DebugLog("=== JUEGO TERMINADO ===");
        // Aquí podrías cargar una escena de créditos o victoria
        // Por ahora solo reiniciamos todo
        StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
    {
        yield return new WaitForSeconds(3f);
        ResetGame();
    }

    /// <summary>
    /// Reinicia todo el progreso del juego
    /// </summary>
    public void ResetGame()
    {
        DebugLog("Reiniciando juego completo...");
        currentLevel = 0;
        unlockedUpgrades.Clear();
        playerDiedInLevel = false;
        levelCompleted = false;
        LoadHub();
    }

    #endregion

    #region Utilities

    void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[GameManager] {message}");
        }
    }

    /// <summary>
    /// Obtiene el estado actual del juego
    /// </summary>
    public string GetGameState()
    {
        return $"Nivel: {currentLevel}/{levelSceneNames.Length} | Mejoras: {unlockedUpgrades.Count}";
    }

    #endregion
}
