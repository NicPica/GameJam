using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sistema simple para mostrar imágenes de notas personalizadas
/// Usa tus propias imágenes pre-diseñadas
/// </summary>
public class ItemNoteDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Canvas de la nota")]
    public Canvas noteCanvas;
    
    [Tooltip("Image que muestra la nota (tus imágenes custom)")]
    public Image noteImage;
    
    [Tooltip("Panel de fondo negro (opcional)")]
    public Image backgroundPanel;
    
    [Header("Input Settings")]
    [Tooltip("Teclas que cierran la nota")]
    public KeyCode[] closeKeys = { KeyCode.Space, KeyCode.E, KeyCode.Return };
    
    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al abrir nota")]
    public AudioClip noteOpenSound;
    
    [Tooltip("Sonido al cerrar nota")]
    public AudioClip noteCloseSound;
    
    // Estado
    private bool isNoteActive = false;
    private bool waitingForInput = false;
    private CanvasGroup canvasGroup;
    private System.Action onNoteClosed;
    
    // Singleton
    public static ItemNoteDisplay Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        canvasGroup = noteCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = noteCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        Hide();
    }
    
    void Update()
    {
        if (waitingForInput && isNoteActive)
        {
            foreach (KeyCode key in closeKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    CloseNote();
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Muestra una nota con tu imagen personalizada
    /// </summary>
    public void ShowNote(Sprite noteSprite, System.Action onClosed = null)
    {
        if (isNoteActive || noteSprite == null)
        {
            Debug.LogWarning("Nota ya activa o sprite nulo");
            return;
        }
        
        StartCoroutine(ShowNoteSequence(noteSprite, onClosed));
    }
    
    /// <summary>
    /// Secuencia de mostrar nota
    /// </summary>
    System.Collections.IEnumerator ShowNoteSequence(Sprite noteSprite, System.Action onClosed)
    {
        isNoteActive = true;
        onNoteClosed = onClosed;
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Desbloquear cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Desactivar controles del jugador
        DisablePlayerControls();
        
        // Configurar la imagen
        if (noteImage != null)
        {
            noteImage.sprite = noteSprite;
            noteImage.gameObject.SetActive(true);
        }
        
        // Mostrar canvas
        Show();
        
        // Sonido
        if (noteOpenSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(noteOpenSound);
        }
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Esperar input
        waitingForInput = true;
    }
    
    /// <summary>
    /// Cierra la nota
    /// </summary>
    void CloseNote()
    {
        waitingForInput = false;
        
        // Sonido
        if (noteCloseSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(noteCloseSound);
        }
        
        StartCoroutine(CloseNoteSequence());
    }
    
    /// <summary>
    /// Secuencia de cerrar nota
    /// </summary>
    System.Collections.IEnumerator CloseNoteSequence()
    {
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Ocultar
        Hide();
        
        // Reanudar el juego
        Time.timeScale = 1f;
        
        // Reactivar controles
        EnablePlayerControls();
        
        // Bloquear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        isNoteActive = false;
        
        // Callback
        onNoteClosed?.Invoke();
        onNoteClosed = null;
    }
    
    /// <summary>
    /// Fade in
    /// </summary>
    System.Collections.IEnumerator FadeIn()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    /// <summary>
    /// Fade out
    /// </summary>
    System.Collections.IEnumerator FadeOut()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
    
    /// <summary>
    /// Muestra el canvas
    /// </summary>
    void Show()
    {
        noteCanvas.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// Oculta el canvas
    /// </summary>
    void Hide()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        noteCanvas.gameObject.SetActive(false);
        
        if (noteImage != null)
        {
            noteImage.gameObject.SetActive(false);
        }
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
    /// Verifica si hay una nota activa
    /// </summary>
    public bool IsNoteActive()
    {
        return isNoteActive;
    }
    
    void OnDestroy()
    {
        Time.timeScale = 1f;
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
