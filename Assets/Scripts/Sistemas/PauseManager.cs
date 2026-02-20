using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja el menú de pausa durante el gameplay
/// Controla la pausa del juego, panel de pausa y acceso a configuración
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Pause Settings")]
    [Tooltip("Tecla para pausar/despausar")]
    public KeyCode pauseKey = KeyCode.P;
    
    [Header("UI Panels")]
    [Tooltip("Canvas que contiene toda la UI de pausa")]
    public Canvas pauseCanvas;
    
    [Tooltip("Panel principal del menú de pausa")]
    public GameObject pauseMenuPanel;
    
    [Tooltip("Panel de configuración (compartido con menú principal)")]
    public GameObject settingsPanel;
    
    [Header("Settings - Audio Sliders")]
    [Tooltip("Slider de volumen master")]
    public Slider masterVolumeSlider;
    
    [Tooltip("Slider de volumen SFX")]
    public Slider sfxVolumeSlider;
    
    [Tooltip("Slider de volumen música")]
    public Slider musicVolumeSlider;
    
    [Tooltip("Slider de volumen voz")]
    public Slider voiceVolumeSlider;
    
    [Header("Settings - Volume Labels (Opcional)")]
    public Text masterVolumeLabel;
    public Text sfxVolumeLabel;
    public Text musicVolumeLabel;
    public Text voiceVolumeLabel;
    
    [Header("Game Settings")]
    [Tooltip("Nombre de la escena del menú principal")]
    public string mainMenuSceneName = "Hub";
    
    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al pausar")]
    public AudioClip pauseSound;
    
    [Tooltip("Sonido al reanudar")]
    public AudioClip resumeSound;
    
    [Tooltip("Sonido al hacer click en botones")]
    public AudioClip buttonClickSound;
    
    // Estado
    private bool isPaused = false;
    private bool isInSettings = false;
    
    // Singleton (para fácil acceso)
    public static PauseManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton por escena (no persistente entre escenas)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        // Inicialmente ocultar todo
        if (pauseCanvas != null)
        {
            pauseCanvas.gameObject.SetActive(false);
        }
        
        // Configurar sliders de audio
        SetupAudioSliders();
        
        // Asegurar que el juego NO esté pausado al inicio
        Resume();
    }
    
    void Update()
    {
        // Detectar input de pausa
        if (Input.GetKeyDown(pauseKey))
        {
            if (!isPaused)
            {
                Pause();
            }
            else if (!isInSettings) // Solo despausar si no está en settings
            {
                Resume();
            }
            else // Si está en settings, volver al menú de pausa
            {
                ShowPauseMenu();
            }
        }
    }
    
    #region Pause/Resume
    
    /// <summary>
    /// Pausa el juego
    /// </summary>
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // Pausar el tiempo del juego
        
        // Desbloquear y mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Desactivar controles de cámara
        DisableCameraControls();
        
        // Mostrar UI de pausa
        if (pauseCanvas != null)
        {
            pauseCanvas.gameObject.SetActive(true);
        }
        
        ShowPauseMenu();
        
        // Sonido
        PlaySound(pauseSound);
        
        Debug.Log("Juego pausado");
    }
    
    /// <summary>
    /// Reanuda el juego
    /// </summary>
    public void Resume()
    {
        isPaused = false;
        isInSettings = false;
        Time.timeScale = 1f; // Reanudar el tiempo
        
        // Reactivar controles de cámara
        EnableCameraControls();
        
        // Bloquear y ocultar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Ocultar UI de pausa
        if (pauseCanvas != null)
        {
            pauseCanvas.gameObject.SetActive(false);
        }
        
        // Sonido
        PlaySound(resumeSound);
        
        Debug.Log("Juego reanudado");
    }
    
    /// <summary>
    /// Alterna entre pausado y no pausado
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
    
    #endregion
    
    #region Panel Navigation
    
    /// <summary>
    /// Muestra el menú de pausa principal
    /// </summary>
    public void ShowPauseMenu()
    {
        isInSettings = false;
        SetActivePanel(pauseMenuPanel);
        PlaySound(buttonClickSound);
    }
    
    /// <summary>
    /// Muestra el panel de configuración
    /// </summary>
    public void ShowSettings()
    {
        isInSettings = true;
        SetActivePanel(settingsPanel);
        PlaySound(buttonClickSound);
    }
    
    /// <summary>
    /// Activa un panel y desactiva los demás
    /// </summary>
    void SetActivePanel(GameObject panelToActivate)
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(pauseMenuPanel == panelToActivate);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(settingsPanel == panelToActivate);
    }
    
    #endregion
    
    #region Game Flow
    
    /// <summary>
    /// Reanuda el juego (botón REANUDAR)
    /// </summary>
    public void OnResumeButtonClicked()
    {
        Resume();
    }
    
    /// <summary>
    /// Vuelve al menú principal (botón VOLVER AL MENÚ)
    /// </summary>
    public void ReturnToMainMenu()
    {
        PlaySound(buttonClickSound);
        
        // Asegurar que el tiempo esté normal antes de cambiar escena
        Time.timeScale = 1f;
        
        Debug.Log("Volviendo al menú principal...");
        
        // Detener música del nivel
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
        
        // Cargar menú principal
        if (TransitionScreen.Instance != null)
        {
            TransitionScreen.Instance.QuickTransition(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
    
    /// <summary>
    /// Reinicia el nivel actual
    /// </summary>
    public void RestartLevel()
    {
        PlaySound(buttonClickSound);
        
        Time.timeScale = 1f;
        
        Debug.Log("Reiniciando nivel...");
        
        // Recargar la escena actual
        Scene currentScene = SceneManager.GetActiveScene();
        
        if (TransitionScreen.Instance != null)
        {
            TransitionScreen.Instance.QuickTransition(currentScene.name);
        }
        else
        {
            SceneManager.LoadScene(currentScene.name);
        }
    }
    
    #endregion
    
    #region Audio Settings
    
    /// <summary>
    /// Configura los sliders de audio con los valores guardados
    /// </summary>
    void SetupAudioSliders()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager no encontrado - Sliders de audio no funcionarán");
            return;
        }
        
        // Cargar valores actuales
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            UpdateVolumeLabel(masterVolumeLabel, masterVolumeSlider.value);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            UpdateVolumeLabel(sfxVolumeLabel, sfxVolumeSlider.value);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateVolumeLabel(musicVolumeLabel, musicVolumeSlider.value);
        }
        
        if (voiceVolumeSlider != null)
        {
            voiceVolumeSlider.value = AudioManager.Instance.GetVoiceVolume();
            voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            UpdateVolumeLabel(voiceVolumeLabel, voiceVolumeSlider.value);
        }
    }
    
    void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
            UpdateVolumeLabel(masterVolumeLabel, value);
        }
    }
    
    void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
            UpdateVolumeLabel(sfxVolumeLabel, value);
            
            // Reproducir sonido de ejemplo
            if (buttonClickSound != null)
            {
                AudioManager.Instance.PlaySFX(buttonClickSound);
            }
        }
    }
    
    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            UpdateVolumeLabel(musicVolumeLabel, value);
        }
    }
    
    void OnVoiceVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVoiceVolume(value);
            UpdateVolumeLabel(voiceVolumeLabel, value);
        }
    }
    
    void UpdateVolumeLabel(Text label, float value)
    {
        if (label != null)
        {
            label.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }
    
    #endregion
    
    #region Audio Playback
    
    /// <summary>
    /// Reproduce un sonido
    /// </summary>
    void PlaySound(AudioClip clip)
    {
        if (clip != null && AudioManager.Instance != null)
        {
            // Usar PlayOneShot no afectado por timeScale
            AudioManager.Instance.PlaySFX(clip);
        }
    }
    
    #endregion
    
    #region Camera Control
    
    /// <summary>
    /// Desactiva los scripts de control de cámara
    /// </summary>
    void DisableCameraControls()
    {
        // Buscar la cámara del player
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogWarning("PauseManager: No se encontró Camera.main");
            return;
        }
        
        // Desactivar todos los scripts de la cámara excepto Camera y AudioListener
        MonoBehaviour[] cameraScripts = playerCamera.GetComponents<MonoBehaviour>();
        foreach (var script in cameraScripts)
        {
            string scriptName = script.GetType().Name;
            
            // No desactivar componentes esenciales
            if (scriptName != "Camera" && 
                scriptName != "AudioListener" && 
                scriptName != "FlashlightSystem") // Mantener FlashlightSystem activo
            {
                script.enabled = false;
                Debug.Log($"PauseManager: Desactivado {scriptName}");
            }
        }
        
        // También buscar en el player si tiene scripts de movimiento
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in playerScripts)
            {
                string scriptName = script.GetType().Name;
                
                // Desactivar scripts de movimiento/input del player
                if (scriptName.Contains("Controller") || 
                    scriptName.Contains("Movement") || 
                    scriptName.Contains("Input") ||
                    scriptName.Contains("PlayerController"))
                {
                    script.enabled = false;
                    Debug.Log($"PauseManager: Desactivado {scriptName} del player");
                }
            }
        }
    }
    
    /// <summary>
    /// Reactiva los scripts de control de cámara
    /// </summary>
    void EnableCameraControls()
    {
        // Buscar la cámara del player
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
            return;
        
        // Reactivar todos los scripts de la cámara
        MonoBehaviour[] cameraScripts = playerCamera.GetComponents<MonoBehaviour>();
        foreach (var script in cameraScripts)
        {
            string scriptName = script.GetType().Name;
            
            // Reactivar todos los scripts excepto componentes core
            if (scriptName != "Camera" && scriptName != "AudioListener")
            {
                script.enabled = true;
                Debug.Log($"PauseManager: Reactivado {scriptName}");
            }
        }
        
        // También reactivar scripts del player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in playerScripts)
            {
                string scriptName = script.GetType().Name;
                
                // Reactivar scripts de movimiento/input
                if (scriptName.Contains("Controller") || 
                    scriptName.Contains("Movement") || 
                    scriptName.Contains("Input") ||
                    scriptName.Contains("PlayerController"))
                {
                    script.enabled = true;
                    Debug.Log($"PauseManager: Reactivado {scriptName} del player");
                }
            }
        }
    }
    
    #endregion
    
    #region Utilities
    
    /// <summary>
    /// Verifica si el juego está pausado
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// Llamado al destruir (asegurar que timeScale vuelva a normal)
    /// </summary>
    void OnDestroy()
    {
        // Asegurar que el tiempo vuelva a normal
        Time.timeScale = 1f;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    #endregion
}
