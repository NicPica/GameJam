using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla de transición que muestra fade negro y texto de lore entre niveles
/// Singleton persistente entre escenas
/// </summary>
public class TransitionScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel negro de fondo")]
    public Image blackPanel;
    
    [Tooltip("Texto del título")]
    public Text titleText;
    
    [Tooltip("Texto del lore")]
    public Text loreText;
    
    [Tooltip("Canvas que contiene todo")]
    public Canvas transitionCanvas;
    
    [Header("Default Settings")]
    [Tooltip("Velocidad de fade por defecto")]
    public float defaultFadeSpeed = 1f;
    
    [Tooltip("Tiempo de display por defecto")]
    public float defaultDisplayTime = 3f;
    
    // Estado
    private bool isTransitioning = false;
    private CanvasGroup canvasGroup;
    
    // Singleton
    public static TransitionScreen Instance { get; private set; }
    
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
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Asegurar que el canvas esté al frente
        if (transitionCanvas != null)
        {
            transitionCanvas.sortingOrder = 999;
        }
        
        // Empezar invisible
        Hide();
    }
    
    /// <summary>
    /// Transición completa: Fade out → Mostrar lore → Fade in → Cargar escena → Fade out final
    /// </summary>
    public void TransitionToScene(string sceneName, LoreData loreData = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("Ya hay una transición en curso");
            return;
        }
        
        StartCoroutine(TransitionSequence(sceneName, loreData));
    }
    
    /// <summary>
    /// Secuencia completa de transición
    /// </summary>
    IEnumerator TransitionSequence(string sceneName, LoreData loreData)
    {
        isTransitioning = true;
        
        // 1. Fade to black
        yield return StartCoroutine(FadeIn(GetFadeSpeed(loreData)));
        
        // 2. Mostrar lore si existe
        if (loreData != null)
        {
            ShowLore(loreData);
            yield return new WaitForSeconds(GetDisplayDuration(loreData));
            HideLore();
        }
        
        // 3. Mantener pantalla negra un momento
        yield return new WaitForSeconds(0.5f);
        
        // 4. Cargar la escena
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // Esperar a que cargue
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 5. Esperar un frame extra para que la escena se inicialice
        yield return new WaitForSeconds(0.5f);
        
        // 6. Fade out (mostrar la nueva escena)
        yield return StartCoroutine(FadeOut(GetFadeSpeed(loreData)));
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Transición simple sin lore
    /// </summary>
    public void QuickTransition(string sceneName)
    {
        StartCoroutine(QuickTransitionSequence(sceneName));
    }
    
    IEnumerator QuickTransitionSequence(string sceneName)
    {
        isTransitioning = true;
        
        yield return StartCoroutine(FadeIn(defaultFadeSpeed));
        
        SceneManager.LoadScene(sceneName);
        
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(FadeOut(defaultFadeSpeed));
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Fade in (hacia negro)
    /// </summary>
    IEnumerator FadeIn(float speed)
    {
        Show();
        
        float targetAlpha = 1f;
        
        while (canvasGroup.alpha < targetAlpha - 0.01f)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
    /// <summary>
    /// Fade out (desde negro)
    /// </summary>
    IEnumerator FadeOut(float speed)
    {
        float targetAlpha = 0f;
        
        while (canvasGroup.alpha > targetAlpha + 0.01f)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        Hide();
    }
    
    /// <summary>
    /// Muestra el texto de lore
    /// </summary>
    void ShowLore(LoreData loreData)
    {
        if (titleText != null)
        {
            titleText.text = loreData.title;
            titleText.fontSize = loreData.titleFontSize;
            titleText.color = loreData.textColor;
            titleText.gameObject.SetActive(true);
        }
        
        if (loreText != null)
        {
            loreText.text = loreData.loreText;
            loreText.fontSize = loreData.loreFontSize;
            loreText.color = loreData.textColor;
            loreText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Oculta el texto de lore
    /// </summary>
    void HideLore()
    {
        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }
        
        if (loreText != null)
        {
            loreText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Muestra la pantalla de transición
    /// </summary>
    void Show()
    {
        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// Oculta la pantalla de transición
    /// </summary>
    void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        HideLore();
    }
    
    /// <summary>
    /// Obtiene la velocidad de fade
    /// </summary>
    float GetFadeSpeed(LoreData loreData)
    {
        return loreData != null ? loreData.fadeSpeed : defaultFadeSpeed;
    }
    
    /// <summary>
    /// Obtiene la duración del display
    /// </summary>
    float GetDisplayDuration(LoreData loreData)
    {
        return loreData != null ? loreData.displayDuration : defaultDisplayTime;
    }
    
    /// <summary>
    /// Verifica si hay una transición en curso
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}
