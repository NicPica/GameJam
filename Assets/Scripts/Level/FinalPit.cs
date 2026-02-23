using UnityEngine;

/// <summary>
/// Pozo final del juego - Al caer aquí, se muestran los créditos
/// </summary>
[RequireComponent(typeof(Collider))]
public class FinalPit : MonoBehaviour
{
    [Header("Final Settings")]
    [Tooltip("¿Mostrar créditos al caer?")]
    public bool showCreditsOnFall = true;
    
    [Tooltip("Delay antes de mostrar créditos (segundos)")]
    public float creditsDelay = 1.5f;
    
    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al caer en el pozo")]
    public AudioClip fallSound;
    
    [Tooltip("Música que se reproduce al caer (opcional)")]
    public AudioClip endingMusic;
    
    [Header("Visual Feedback")]
    [Tooltip("Partículas al caer")]
    public GameObject fallParticles;
    
    [Tooltip("¿Hacer fade a negro antes de los créditos?")]
    public bool fadeToBlack = true;
    
    // Estado
    private bool hasTriggered = false;
    
    void Start()
    {
        // Asegurar que es trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador y si ya se activó
        if (!other.CompareTag("Player") || hasTriggered)
            return;
        
        hasTriggered = true;
        
        Debug.Log("¡Jugador ha caído en el pozo final!");
        
        // Sonido de caída
        if (fallSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(fallSound);
        }
        
        // Partículas
        if (fallParticles != null)
        {
            Instantiate(fallParticles, other.transform.position, Quaternion.identity);
        }
        
        // Desactivar controles del jugador
        DisablePlayerControls(other.gameObject);
        
        // Reproducir música del final (opcional)
        if (endingMusic != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(endingMusic);
        }
        
        // Mostrar créditos después del delay
        if (showCreditsOnFall)
        {
            Invoke("ShowCredits", creditsDelay);
        }
    }
    
    /// <summary>
    /// Desactiva los controles del jugador
    /// </summary>
    void DisablePlayerControls(GameObject player)
    {
        // Desactivar scripts de movimiento
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("Controller") || 
                scriptName.Contains("Movement") ||
                scriptName == "FirstPersonController")
            {
                script.enabled = false;
            }
        }
        
        // Desactivar cámara
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
    /// Muestra los créditos finales
    /// </summary>
    void ShowCredits()
    {
        Debug.Log("Mostrando créditos finales...");
        
        // Opción 1: Usar CreditsScreen si existe
        if (CreditsScreen.Instance != null)
        {
            CreditsScreen.Instance.ShowCredits();
        }
        // Opción 2: Usar GameManager.EndGame()
        else if (GameManager.Instance != null)
        {
            // Forzar que el nivel actual sea el último para activar créditos
            GameManager.Instance.currentLevel = GameManager.Instance.levelSceneNames.Length;
            GameManager.Instance.CompleteLevel();
        }
        else
        {
            Debug.LogError("No se encontró CreditsScreen ni GameManager!");
        }
    }
    
    void OnDrawGizmos()
    {
        // Dibujar área del pozo final
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Dibujar icono de final
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
