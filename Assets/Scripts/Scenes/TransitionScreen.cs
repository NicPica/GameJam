using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla de transición que muestra fade negro y texto de lore entre niveles
/// MODIFICADO: Espera input del jugador antes de continuar
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
    
    [Tooltip("Texto de ayuda (Presiona ESPACIO...)")]
    public Text helpText;
    
    [Tooltip("Canvas que contiene todo")]
    public Canvas transitionCanvas;
    
    [Header("Input Settings")]
    [Tooltip("Teclas que cierran el lore")]
    public KeyCode[] closeKeys = { KeyCode.Space, KeyCode.E, KeyCode.Return };
    
    [Tooltip("¿Hacer parpadear el texto de ayuda?")]
    public bool blinkHelpText = true;
    
    [Tooltip("Velocidad del parpadeo")]
    public float blinkSpeed = 1f;
    
    [Header("Default Settings")]
    [Tooltip("Velocidad de fade por defecto")]
    public float defaultFadeSpeed = 1f;
    
    // Estado
    private bool isTransitioning = false;
    private bool waitingForInput = false;
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
        
        // Resetear estado al cargar nueva escena
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void Update()
    {
        // Detectar input para cerrar lore
        if (waitingForInput)
        {
            foreach (KeyCode key in closeKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    waitingForInput = false;
                    break;
                }
            }
        }
        
        // Parpadeo del texto de ayuda
        if (waitingForInput && blinkHelpText && helpText != null)
        {
            float alpha = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) / 2f;
            Color color = helpText.color;
            color.a = Mathf.Lerp(0.3f, 1f, alpha);
            helpText.color = color;
        }
    }
    
    /// <summary>
    /// Transición completa con lore que espera input
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
        float fadeSpeed = loreData != null ? loreData.fadeSpeed : defaultFadeSpeed;
        yield return StartCoroutine(FadeIn(fadeSpeed));
        
        // 2. Cargar la escena PRIMERO
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 3. Esperar a que la escena se inicialice
        yield return new WaitForSeconds(0.5f);
        
        // 4. SI HAY LORE: Mostrarlo y ESPERAR input
        if (loreData != null)
        {
            // Pausar el juego
            Time.timeScale = 0f;
            
            // Desbloquear cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Desactivar controles del jugador
            DisablePlayerControls();
            
            // Mostrar lore
            ShowLore(loreData);
            
            // ESPERAR INPUT DEL JUGADOR
            waitingForInput = true;
            while (waitingForInput)
            {
                yield return null;
            }
            
            // Ocultar lore
            HideLore();
            
            // Reactivar controles
            EnablePlayerControls();
            
            // Reanudar el juego
            Time.timeScale = 1f;
            
            // Bloquear cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // 5. Fade out (mostrar la escena)
        yield return StartCoroutine(FadeOut(fadeSpeed));
        
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
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, speed * Time.unscaledDeltaTime);
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
        
        if (helpText != null)
        {
            helpText.text = "Presiona ESPACIO para continuar";
            helpText.gameObject.SetActive(true);
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
        
        if (helpText != null)
        {
            helpText.gameObject.SetActive(false);
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
    /// Desactiva controles del jugador
    /// </summary>
    void DisablePlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("Controller") || 
                scriptName.Contains("Movement") || 
                scriptName == "FirstPersonController")
            {
                script.enabled = false;
            }
        }
        
        Camera playerCamera = Camera.main;
        if (playerCamera != null)
        {
            MonoBehaviour[] cameraScripts = playerCamera.GetComponents<MonoBehaviour>();
            foreach (var script in cameraScripts)
            {
                string scriptName = script.GetType().Name;
                if (scriptName != "Camera" && scriptName != "AudioListener")
                {
                    script.enabled = false;
                }
            }
        }
    }
    
    /// <summary>
    /// Reactiva controles del jugador
    /// </summary>
    void EnablePlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("Controller") || 
                scriptName.Contains("Movement") || 
                scriptName == "FirstPersonController")
            {
                script.enabled = true;
            }
        }
        
        Camera playerCamera = Camera.main;
        if (playerCamera != null)
        {
            MonoBehaviour[] cameraScripts = playerCamera.GetComponents<MonoBehaviour>();
            foreach (var script in cameraScripts)
            {
                string scriptName = script.GetType().Name;
                if (scriptName != "Camera" && scriptName != "AudioListener")
                {
                    script.enabled = true;
                }
            }
        }
    }
    
    /// <summary>
    /// Verifica si hay una transición en curso
    /// </summary>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    void OnDestroy()
    {
        // Asegurar que el juego no quede pausado
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Resetea el estado de transición al cargar una nueva escena.
    /// Necesario porque TransitionScreen persiste entre escenas (DontDestroyOnLoad)
    /// y isTransitioning puede quedar en true si la escena se cargó correctamente.
    /// </summary>
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        isTransitioning = false;
    }
}
