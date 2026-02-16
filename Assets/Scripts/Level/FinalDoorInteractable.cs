using System.Collections;
using UnityEngine;

/// <summary>
/// Puerta/área final que se desbloquea al completar todos los niveles
/// Termina el juego cuando el jugador interactúa
/// </summary>
[RequireComponent(typeof(Collider))]
public class FinalDoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [Tooltip("Prompt cuando se puede interactuar")]
    public string interactionPrompt = "Presiona E para entrar al área secreta";

    [Header("Visual Effects")]
    [Tooltip("Material de la puerta")]
    public Material doorMaterial;

    [Tooltip("Color de hover")]
    public Color hoverColor = Color.white;

    [Tooltip("¿Animar la puerta?")]
    public bool animateDoor = true;

    [Tooltip("Velocidad de animación de flotación")]
    public float floatSpeed = 1f;

    [Tooltip("Amplitud de flotación")]
    public float floatAmplitude = 0.1f;

    [Header("Audio")]
    [Tooltip("Sonido al interactuar")]
    public AudioClip interactSound;

    [Header("Ending Settings")]
    [Tooltip("Tiempo antes de mostrar créditos/reiniciar")]
    public float endingDelay = 3f;

    [Tooltip("Texto que aparece al terminar el juego")]
    [TextArea(3, 5)]
    public string endingText = "Gracias por jugar\n\nDesarrollado para [Game Jam]\n\n¿Reiniciar?";

    // Referencias
    private Renderer doorRenderer;
    private Material originalMaterial;
    private Material hoverMaterial;
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool isBeingLookedAt = false;

    void Start()
    {
        startPosition = transform.position;

        // Obtener renderer
        doorRenderer = GetComponentInChildren<Renderer>();

        if (doorRenderer != null)
        {
            originalMaterial = doorRenderer.material;

            // Crear material de hover
            hoverMaterial = new Material(originalMaterial);
            hoverMaterial.SetColor("_EmissionColor", hoverColor);
            hoverMaterial.EnableKeyword("_EMISSION");
        }

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && interactSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }
    }

    void Update()
    {
        // Animación de flotación
        if (animateDoor)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    #region IInteractable Implementation

    public void Interact()
    {
        Debug.Log("Entrando al área final...");

        // Reproducir sonido
        if (interactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(interactSound);
        }

        // Iniciar secuencia de final
        StartCoroutine(EndGameSequence());
    }

    public void OnLookAt()
    {
        isBeingLookedAt = true;

        if (doorRenderer != null && hoverMaterial != null)
        {
            doorRenderer.material = hoverMaterial;
        }
    }

    public void OnLookAway()
    {
        isBeingLookedAt = false;

        if (doorRenderer != null && originalMaterial != null)
        {
            doorRenderer.material = originalMaterial;
        }
    }

    public string GetInteractionPrompt()
    {
        return interactionPrompt;
    }

    #endregion

    /// <summary>
    /// Secuencia de final del juego
    /// </summary>
    IEnumerator EndGameSequence()
    {
        // Opción 1: Desactivar controles buscando el tag Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Desactivar todos los scripts de movimiento
            MonoBehaviour[] playerScripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in playerScripts)
            {
                // Desactivar scripts comunes de movimiento
                string scriptName = script.GetType().Name;
                if (scriptName.Contains("Controller") || 
                    scriptName.Contains("Movement") ||
                    scriptName.Contains("Input"))
                {
                    script.enabled = false;
                }
            }
        }

        // Esperar un momento
        yield return new WaitForSeconds(endingDelay);

        // Aquí podrías mostrar una pantalla de final/créditos
        Debug.Log("=== JUEGO COMPLETADO ===");
        Debug.Log(endingText);

        // Esperar más tiempo
        yield return new WaitForSeconds(3f);

        // Reiniciar el juego
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
