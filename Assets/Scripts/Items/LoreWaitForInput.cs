using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Añade este script a tu Canvas de lore existente
/// Hace que espere input del jugador en vez de desaparecer automáticamente
/// </summary>
public class LoreWaitForInput : MonoBehaviour
{
    [Header("Input Settings")]
    [Tooltip("Teclas que cierran el lore")]
    public KeyCode[] closeKeys = { KeyCode.Space, KeyCode.E, KeyCode.Return };
    
    [Header("Help Text (Opcional)")]
    [Tooltip("Texto que muestra 'Presiona ESPACIO...' (opcional)")]
    public Text helpText;
    
    [Tooltip("Texto del help")]
    public string helpMessage = "Presiona ESPACIO para continuar";
    
    [Tooltip("¿Hacer parpadear el texto?")]
    public bool blinkText = true;
    
    [Tooltip("Velocidad del parpadeo")]
    public float blinkSpeed = 1f;
    
    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al cerrar")]
    public AudioClip closeSound;
    
    // Estado
    private bool waitingForInput = false;
    private CanvasGroup canvasGroup;
    
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Configurar texto de ayuda si existe
        if (helpText != null)
        {
            helpText.text = helpMessage;
        }
        
        // Pausar el juego
        Time.timeScale = 0f;
        
        // Desbloquear cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Desactivar controles del jugador
        DisablePlayerControls();
        
        // Empezar a esperar input
        waitingForInput = true;
    }
    
    void Update()
    {
        // Detectar input para cerrar
        if (waitingForInput)
        {
            foreach (KeyCode key in closeKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    CloseLore();
                    break;
                }
            }
        }
        
        // Parpadeo del texto de ayuda
        if (waitingForInput && blinkText && helpText != null)
        {
            float alpha = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) / 2f;
            Color color = helpText.color;
            color.a = Mathf.Lerp(0.3f, 1f, alpha);
            helpText.color = color;
        }
    }
    
    /// <summary>
    /// Cierra el lore y continúa el juego
    /// </summary>
    void CloseLore()
    {
        waitingForInput = false;
        
        // Sonido
        if (closeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(closeSound);
        }
        
        // Reanudar el juego
        Time.timeScale = 1f;
        
        // Reactivar controles
        EnablePlayerControls();
        
        // Bloquear cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Ocultar el canvas
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Fade out del canvas
    /// </summary>
    System.Collections.IEnumerator FadeOut()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Desactiva controles del jugador
    /// </summary>
    void DisablePlayerControls()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Desactivar scripts de movimiento
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
        
        // Desactivar scripts de cámara
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
        
        // Reactivar scripts de movimiento
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
        
        // Reactivar scripts de cámara
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
    
    void OnDestroy()
    {
        // Asegurar que el juego no quede pausado
        Time.timeScale = 1f;
    }
}
