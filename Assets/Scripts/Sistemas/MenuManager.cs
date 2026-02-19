using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla el menú principal y sus submenús
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Panel principal del menú")]
    public GameObject mainMenuPanel;
    
    [Tooltip("Panel de configuración")]
    public GameObject settingsPanel;
    
    [Tooltip("Panel de créditos")]
    public GameObject creditsPanel;
    
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
    [Tooltip("Nombre de la primera escena del juego")]
    public string firstLevelScene = "Level1";
    
    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al hacer click en botones")]
    public AudioClip buttonClickSound;
    
    [Tooltip("Música del menú")]
    public AudioClip menuMusic;
    
    void Start()
    {
        // Desbloquear cursor en el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Mostrar panel principal
        ShowMainMenu();
        
        // Configurar sliders de audio
        SetupAudioSliders();
        
        // Reproducir música del menú si existe
        if (menuMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }
    }
    
    #region Panel Navigation
    
    /// <summary>
    /// Muestra el menú principal
    /// </summary>
    public void ShowMainMenu()
    {
        SetActivePanel(mainMenuPanel);
        PlayButtonSound();
    }
    
    /// <summary>
    /// Muestra el menú de configuración
    /// </summary>
    public void ShowSettings()
    {
        SetActivePanel(settingsPanel);
        PlayButtonSound();
    }
    
    /// <summary>
    /// Muestra el menú de créditos
    /// </summary>
    public void ShowCredits()
    {
        SetActivePanel(creditsPanel);
        PlayButtonSound();
    }
    
    /// <summary>
    /// Activa un panel y desactiva los demás
    /// </summary>
    void SetActivePanel(GameObject panelToActivate)
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(mainMenuPanel == panelToActivate);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(settingsPanel == panelToActivate);
        
        if (creditsPanel != null)
            creditsPanel.SetActive(creditsPanel == panelToActivate);
    }
    
    #endregion
    
    #region Game Flow
    
    /// <summary>
    /// Inicia el juego (botón JUGAR)
    /// </summary>
    public void StartGame()
    {
        PlayButtonSound();
        
        // Detener música del menú
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
        
        // Cargar primera escena
        if (GameManager.Instance != null)
        {
            // Si tienes GameManager con sistema de niveles
            GameManager.Instance.currentLevel = 1;
            GameManager.Instance.LoadCurrentLevel();
        }
        else
        {
            // Carga directa de la escena
            if (TransitionScreen.Instance != null)
            {
                TransitionScreen.Instance.QuickTransition(firstLevelScene);
            }
            else
            {
                SceneManager.LoadScene(firstLevelScene);
            }
        }
    }
    
    /// <summary>
    /// Sale del juego (botón SALIR)
    /// </summary>
    public void QuitGame()
    {
        PlayButtonSound();
        
        Debug.Log("Saliendo del juego...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
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
    
    /// <summary>
    /// Llamado cuando cambia el slider de Master Volume
    /// </summary>
    void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
            UpdateVolumeLabel(masterVolumeLabel, value);
        }
    }
    
    /// <summary>
    /// Llamado cuando cambia el slider de SFX Volume
    /// </summary>
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
    
    /// <summary>
    /// Llamado cuando cambia el slider de Music Volume
    /// </summary>
    void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            UpdateVolumeLabel(musicVolumeLabel, value);
        }
    }
    
    /// <summary>
    /// Llamado cuando cambia el slider de Voice Volume
    /// </summary>
    void OnVoiceVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVoiceVolume(value);
            UpdateVolumeLabel(voiceVolumeLabel, value);
        }
    }
    
    /// <summary>
    /// Actualiza el texto del label de volumen
    /// </summary>
    void UpdateVolumeLabel(Text label, float value)
    {
        if (label != null)
        {
            label.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }
    
    #endregion
    
    #region Audio Feedback
    
    /// <summary>
    /// Reproduce sonido al hacer click en botones
    /// </summary>
    void PlayButtonSound()
    {
        if (buttonClickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSound);
        }
    }
    
    #endregion
}
