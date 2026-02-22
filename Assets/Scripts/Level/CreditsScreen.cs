using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla de créditos finales que aparece al terminar el juego
/// </summary>
public class CreditsScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel negro de fondo")]
    public Image blackPanel;
    
    [Tooltip("Texto de 'GRACIAS POR JUGAR'")]
    public Text thanksText;
    
    [Tooltip("Texto de los créditos")]
    public Text creditsText;
    
    [Tooltip("Canvas que contiene todo")]
    public Canvas creditsCanvas;
    
    [Header("Credits Content")]
    [Tooltip("Texto de los créditos")]
    [TextArea(10, 20)]
    public string creditsContent = @"CRÉDITOS

Desarrollado por:
Tu Nombre

Arte y Diseño:
Tu Nombre

Música y Sonido:
[Artista]

Agradecimientos Especiales:
[Nombres]

Gracias por jugar!";
    
    [Header("Timing")]
    [Tooltip("Tiempo antes de mostrar créditos (segundos)")]
    public float delayBeforeCredits = 2f;
    
    [Tooltip("Velocidad del fade")]
    public float fadeSpeed = 1f;
    
    [Tooltip("Tiempo mostrando créditos antes de volver al menú")]
    public float creditsDuration = 10f;
    
    [Header("Navigation")]
    [Tooltip("Escena a la que volver después de los créditos")]
    public string mainMenuScene = "Hub";
    
    [Tooltip("¿Permitir saltar créditos con ESC?")]
    public bool allowSkip = true;
    
    [Header("Audio")]
    [Tooltip("Música de créditos (opcional)")]
    public AudioClip creditsMusic;
    
    // Estado
    private bool isShowingCredits = false;
    private bool canSkip = false;
    private CanvasGroup canvasGroup;
    
    // Singleton
    public static CreditsScreen Instance { get; private set; }
    
    void Awake()
    {
        // Singleton persistente
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Obtener CanvasGroup
        canvasGroup = creditsCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = creditsCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Asegurar que el canvas esté al frente
        if (creditsCanvas != null)
        {
            creditsCanvas.sortingOrder = 1000; // Más alto que TransitionScreen
        }
        
        // Empezar oculto
        Hide();
        
        Debug.Log("CreditsScreen inicializado");
    }
    
    void Update()
    {
        // Permitir saltar créditos con ESC
        if (canSkip && allowSkip && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipCredits();
        }
    }
    
    /// <summary>
    /// Muestra la pantalla de créditos
    /// </summary>
    public void ShowCredits()
    {
        if (isShowingCredits)
        {
            Debug.LogWarning("Créditos ya están mostrándose");
            return;
        }
        
        StartCoroutine(CreditsSequence());
    }
    
    /// <summary>
    /// Secuencia completa de créditos
    /// </summary>
    IEnumerator CreditsSequence()
    {
        isShowingCredits = true;
        canSkip = false;
        
        Debug.Log("Iniciando secuencia de créditos");
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Desbloquear cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 1. Delay inicial
        yield return new WaitForSecondsRealtime(delayBeforeCredits);
        
        // 2. Fade to black
        yield return StartCoroutine(FadeIn());
        
        // 3. Mostrar texto
        ShowTexts();
        
        // 4. Reproducir música de créditos
        if (creditsMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(creditsMusic, true);
        }
        
        // 5. Esperar duración de créditos (permitiendo skip)
        canSkip = true;
        float elapsed = 0f;
        
        while (elapsed < creditsDuration)
        {
            if (!isShowingCredits) // Si se saltó
            {
                break;
            }
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        canSkip = false;
        
        // 6. Fade out
        yield return StartCoroutine(FadeOut());
        
        // 7. Volver al menú principal
        ReturnToMenu();
    }
    
    /// <summary>
    /// Salta los créditos
    /// </summary>
    void SkipCredits()
    {
        if (!canSkip) return;
        
        Debug.Log("Créditos saltados");
        isShowingCredits = false;
    }
    
    /// <summary>
    /// Fade in (hacia negro con texto)
    /// </summary>
    IEnumerator FadeIn()
    {
        Show();
        
        float targetAlpha = 1f;
        
        while (canvasGroup.alpha < targetAlpha - 0.01f)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
    /// <summary>
    /// Fade out
    /// </summary>
    IEnumerator FadeOut()
    {
        float targetAlpha = 0f;
        
        while (canvasGroup.alpha > targetAlpha + 0.01f)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        Hide();
    }
    
    /// <summary>
    /// Muestra los textos de créditos
    /// </summary>
    void ShowTexts()
    {
        if (thanksText != null)
        {
            thanksText.text = "GRACIAS POR JUGAR";
            thanksText.gameObject.SetActive(true);
        }
        
        if (creditsText != null)
        {
            creditsText.text = creditsContent;
            creditsText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Oculta los textos
    /// </summary>
    void HideTexts()
    {
        if (thanksText != null)
        {
            thanksText.gameObject.SetActive(false);
        }
        
        if (creditsText != null)
        {
            creditsText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Muestra el canvas
    /// </summary>
    void Show()
    {
        creditsCanvas.gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// Oculta el canvas
    /// </summary>
    void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        HideTexts();
        
        if (creditsCanvas != null)
        {
            creditsCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Vuelve al menú principal
    /// </summary>
    void ReturnToMenu()
    {
        Debug.Log("Volviendo al menú principal...");
        
        // Detener música de créditos
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
        
        // Reanudar el juego
        Time.timeScale = 1f;
        
        // Cargar menú principal
        if (TransitionScreen.Instance != null)
        {
            TransitionScreen.Instance.QuickTransition(mainMenuScene);
        }
        else
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        
        isShowingCredits = false;
    }
    
    void OnDestroy()
    {
        // Asegurar que el juego no quede pausado
        Time.timeScale = 1f;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
