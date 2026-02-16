using System.Collections;
using UnityEngine;

/// <summary>
/// Punto de salida del nivel
/// Solo se puede usar cuando se completa la quest
/// Implementa IInteractable para usar con el InteractionSystem
/// </summary>
[RequireComponent(typeof(Collider))]
public class LevelExit : MonoBehaviour, IInteractable
{
    [Header("Exit Settings")]
    [Tooltip("¿Requiere completar la quest para salir?")]
    public bool requiresQuestCompletion = true;

    [Tooltip("Prompt cuando la quest NO está completa")]
    public string lockedPrompt = "Necesitas completar la lista primero";

    [Tooltip("Prompt cuando la quest está completa")]
    public string unlockedPrompt = "Presiona E para salir";

    [Header("Visual Feedback")]
    [Tooltip("Material cuando está bloqueado")]
    public Material lockedMaterial;

    [Tooltip("Material cuando está desbloqueado")]
    public Material unlockedMaterial;

    [Tooltip("¿Hacer hover effect?")]
    public bool enableHoverEffect = true;

    [Tooltip("Color del highlight")]
    public Color hoverColor = Color.cyan;

    [Header("Effects")]
    [Tooltip("Partículas cuando se desbloquea")]
    public GameObject unlockParticles;

    [Tooltip("Luz que se enciende al desbloquear")]
    public Light exitLight;

    [Header("Audio")]
    [Tooltip("Sonido cuando se intenta usar bloqueado")]
    public AudioClip lockedSound;

    [Tooltip("Sonido cuando se desbloquea")]
    public AudioClip unlockSound;

    [Tooltip("Sonido al usar la salida")]
    public AudioClip exitSound;

    // Estado
    private bool isUnlocked = false;
    private bool isBeingLookedAt = false;
    private Renderer exitRenderer;
    private Material currentMaterial;
    private Material hoverMaterial;
    private AudioSource audioSource;

    void Start()
    {
        // Obtener componentes
        exitRenderer = GetComponentInChildren<Renderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        // Configurar material inicial
        if (exitRenderer != null)
        {
            if (lockedMaterial != null)
            {
                exitRenderer.material = lockedMaterial;
                currentMaterial = lockedMaterial;
            }

            // Crear material de hover
            if (enableHoverEffect)
            {
                hoverMaterial = new Material(exitRenderer.material);
                hoverMaterial.SetColor("_EmissionColor", hoverColor * 0.5f);
                hoverMaterial.EnableKeyword("_EMISSION");
            }
        }

        // Configurar luz
        if (exitLight != null)
        {
            exitLight.enabled = false;
        }

        // Suscribirse al evento de quest completada
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestCompleted.AddListener(UnlockExit);
        }

        // Si no requiere quest, desbloquear inmediatamente
        if (!requiresQuestCompletion)
        {
            UnlockExit();
        }
    }

    void OnDestroy()
    {
        // Desuscribirse del evento
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.onQuestCompleted.RemoveListener(UnlockExit);
        }
    }

    /// <summary>
    /// Desbloquea la salida
    /// </summary>
    void UnlockExit()
    {
        if (isUnlocked)
            return;

        isUnlocked = true;
        Debug.Log("Salida del nivel desbloqueada!");

        // Cambiar material
        if (exitRenderer != null && unlockedMaterial != null)
        {
            exitRenderer.material = unlockedMaterial;
            currentMaterial = unlockedMaterial;
        }

        // Activar luz
        if (exitLight != null)
        {
            exitLight.enabled = true;
        }

        // Spawear partículas
        if (unlockParticles != null)
        {
            Instantiate(unlockParticles, transform.position, Quaternion.identity);
        }

        // Reproducir sonido
        if (unlockSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
    }

    #region IInteractable Implementation

    public void Interact()
    {
        if (!isUnlocked && requiresQuestCompletion)
        {
            // Salida bloqueada
            Debug.Log("La salida está bloqueada - completa la quest primero");
            
            if (lockedSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
            
            return;
        }

        // Salida desbloqueada - completar nivel
        Debug.Log("Usando salida del nivel...");
        
        if (exitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(exitSound);
        }

        StartCoroutine(ExitLevel());
    }

    IEnumerator ExitLevel()
    {
        // Esperar un momento para el efecto de sonido
        yield return new WaitForSeconds(0.5f);

        // Notificar al GameManager que el nivel fue completado
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel();
        }
        else
        {
            Debug.LogError("GameManager no encontrado!");
        }
    }

    public void OnLookAt()
    {
        isBeingLookedAt = true;

        // Aplicar efecto de hover
        if (exitRenderer != null && enableHoverEffect && hoverMaterial != null && isUnlocked)
        {
            exitRenderer.material = hoverMaterial;
        }
    }

    public void OnLookAway()
    {
        isBeingLookedAt = false;

        // Restaurar material
        if (exitRenderer != null && enableHoverEffect && currentMaterial != null)
        {
            exitRenderer.material = currentMaterial;
        }
    }

    public string GetInteractionPrompt()
    {
        return isUnlocked ? unlockedPrompt : lockedPrompt;
    }

    #endregion

    /// <summary>
    /// Debug: Forzar desbloqueo
    /// </summary>
    [ContextMenu("Unlock Exit (Debug)")]
    public void DebugUnlock()
    {
        UnlockExit();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isUnlocked ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
