using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor principal del juego que controla el flujo entre Hub y Niveles
/// VERSIÓN ACTUALIZADA: Con créditos finales
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Game Flow")]
    [Tooltip("Nombre de la escena del Hub")]
    public string hubSceneName = "Hub";

    [Tooltip("Nombres de las escenas de los niveles en orden")]
    public string[] levelSceneNames = { "Level1", "Level2", "Level3" };
    
    [Header("Lore System")]
    [Tooltip("Datos de lore para cada nivel (debe coincidir con levelSceneNames)")]
    public LoreData[] levelLoreData;
    
    [Tooltip("Lore que aparece al volver al Hub (opcional)")]
    public LoreData hubLoreData;

    [Header("Game State")]
    [Tooltip("Nivel actual (0 = Hub, 1-3 = Niveles)")]
    public int currentLevel = 0;

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
        // Iniciar en el Hub (sin transición la primera vez)
        if (SceneManager.GetActiveScene().name != hubSceneName)
        {
            LoadHub(false);
        }
    }

    #region Scene Management

    /// <summary>
    /// Carga el Hub
    /// </summary>
    public void LoadHub(bool useTransition = true)
    {
        DebugLog("Cargando Hub...");
        playerDiedInLevel = false;
        levelCompleted = false;
        
        if (useTransition && TransitionScreen.Instance != null)
        {
            TransitionScreen.Instance.TransitionToScene(hubSceneName, hubLoreData);
        }
        else
        {
            SceneManager.LoadScene(hubSceneName);
        }
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
        
        // Obtener lore data del nivel si existe
        LoreData loreData = GetLoreForLevel(currentLevel);
        
        if (TransitionScreen.Instance != null)
        {
            TransitionScreen.Instance.TransitionToScene(levelName, loreData);
        }
        else
        {
            Debug.LogWarning("TransitionScreen no encontrado - cargando sin transición");
            SceneManager.LoadScene(levelName);
        }
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

        // Si es el último nivel, terminar el juego (MOSTRAR CRÉDITOS)
        if (currentLevel >= levelSceneNames.Length)
        {
            DebugLog("¡Juego completado! Mostrando créditos...");
            EndGame();
        }
        else
        {
            // Avanzar al siguiente nivel directamente
            currentLevel++;
            LoadCurrentLevel();
        }
    }

    /// <summary>
    /// Llamar cuando el jugador muere o falla el nivel
    /// </summary>
    public void FailLevel()
    {
        playerDiedInLevel = true;
        DebugLog("Nivel fallido - Reiniciando...");
        
        // Esperar un poco antes de reiniciar
        StartCoroutine(ShowDeathScreenAndRestart());
    }

    IEnumerator ShowDeathScreenAndRestart()
    {
        yield return new WaitForSeconds(2f);
        RestartCurrentLevel();
    }

    #endregion

    #region Game End

    /// <summary>
    /// Termina el juego (mostrar créditos)
    /// </summary>
    void EndGame()
    {
        DebugLog("=== JUEGO TERMINADO ===");
        
        // Mostrar créditos
        if (CreditsScreen.Instance != null)
        {
            CreditsScreen.Instance.ShowCredits();
        }
        else
        {
            Debug.LogWarning("CreditsScreen no encontrado - usando secuencia por defecto");
            StartCoroutine(EndGameSequence());
        }
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
        playerDiedInLevel = false;
        levelCompleted = false;
        LoadHub();
    }

    #endregion
    
    #region Lore System

    /// <summary>
    /// Obtiene el LoreData para un nivel específico
    /// </summary>
    LoreData GetLoreForLevel(int level)
    {
        if (levelLoreData == null || levelLoreData.Length == 0)
        {
            DebugLog("No hay lore data configurado");
            return null;
        }
        
        int index = level - 1;
        
        if (index >= 0 && index < levelLoreData.Length)
        {
            return levelLoreData[index];
        }
        
        DebugLog($"No hay lore para el nivel {level}");
        return null;
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
        return $"Nivel: {currentLevel}/{levelSceneNames.Length}";
    }

    #endregion
}
