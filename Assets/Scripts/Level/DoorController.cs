using System.Collections;
using UnityEngine;

/// <summary>
/// Controlador de puertas que se abren hacia arriba
/// Puede estar bloqueada y requerir un item específico para abrirse
/// </summary>
[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour, IInteractable
{
    [Header("Door State")]
    [Tooltip("¿La puerta comienza bloqueada?")]
    public bool isLocked = false;
    
    [Tooltip("Nombre del item necesario para desbloquear (debe coincidir con CollectableItem.itemName)")]
    public string requiredItemName = "Llave";
    
    [Tooltip("¿El item se consume al abrir? (se elimina del inventario)")]
    public bool consumeItemOnUse = true;
    
    [Header("Door Movement")]
    [Tooltip("Altura a la que sube la puerta")]
    public float openHeight = 4f;
    
    [Tooltip("Velocidad de apertura")]
    public float openSpeed = 2f;
    
    [Tooltip("¿La puerta se cierra automáticamente?")]
    public bool autoClose = false;
    
    [Tooltip("Tiempo antes de cerrarse (si autoClose está activo)")]
    public float autoCloseDelay = 5f;
    
    [Header("Visual Feedback")]
    [Tooltip("Material cuando está desbloqueada")]
    public Material unlockedMaterial;
    
    [Tooltip("Material cuando está bloqueada")]
    public Material lockedMaterial;
    
    [Tooltip("Color del outline cuando se mira")]
    public Color hoverColor = Color.yellow;
    
    [Tooltip("¿Hacer hover effect?")]
    public bool enableHoverEffect = true;
    
    [Header("Audio")]
    [Tooltip("Sonido de temblor al abrir")]
    public AudioClip openSound;
    
    [Tooltip("Sonido cuando está bloqueada")]
    public AudioClip lockedSound;
    
    [Tooltip("Sonido al desbloquear")]
    public AudioClip unlockSound;
    
    [Header("Camera Shake")]
    [Tooltip("¿Hacer shake de cámara al abrir?")]
    public bool enableCameraShake = true;
    
    [Tooltip("Duración del shake")]
    public float shakeDuration = 0.7f;
    
    [Tooltip("Intensidad del shake")]
    public float shakeMagnitude = 0.15f;
    
    [Header("Interaction Prompts")]
    [Tooltip("Texto cuando está desbloqueada")]
    public string unlockedPrompt = "Presiona E para abrir";
    
    [Tooltip("Texto cuando está bloqueada")]
    public string lockedPrompt = "Bloqueada - Necesitas una {item}";
    
    [Tooltip("Texto cuando tienes el item pero aún no has abierto")]
    public string hasKeyPrompt = "Presiona E para abrir con {item}";
    
    // Estado interno
    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Renderer doorRenderer;
    private Material originalMaterial;
    private AudioSource audioSource;
    private bool isBeingLookedAt = false;
    
    void Start()
    {
        // Guardar posición inicial
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;
        
        // Obtener renderer
        doorRenderer = GetComponentInChildren<Renderer>();
        if (doorRenderer != null)
        {
            originalMaterial = doorRenderer.material;
            UpdateDoorMaterial();
        }
        
        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
        
        // Asegurar que tiene collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false; // Debe ser sólido para raycast
        }
    }
    
    #region IInteractable Implementation
    
    public void Interact()
    {
        if (isMoving || isOpen)
            return;
        
        // Si está bloqueada, verificar si tiene el item
        if (isLocked)
        {
            if (HasRequiredItem())
            {
                UnlockAndOpen();
            }
            else
            {
                ShowLockedFeedback();
            }
        }
        else
        {
            // Está desbloqueada, abrir directamente
            OpenDoor();
        }
    }
    
    public void OnLookAt()
    {
        isBeingLookedAt = true;
        
        // Aplicar efecto de hover
        if (doorRenderer != null && enableHoverEffect)
        {
            doorRenderer.material.SetColor("_EmissionColor", hoverColor * 0.5f);
            doorRenderer.material.EnableKeyword("_EMISSION");
        }
    }
    
    public void OnLookAway()
    {
        isBeingLookedAt = false;
        
        // Restaurar material
        if (doorRenderer != null && enableHoverEffect)
        {
            doorRenderer.material.DisableKeyword("_EMISSION");
        }
    }
    
    public string GetInteractionPrompt()
    {
        if (isOpen)
            return "Puerta abierta";
        
        if (isLocked)
        {
            if (HasRequiredItem())
            {
                return hasKeyPrompt.Replace("{item}", requiredItemName);
            }
            else
            {
                return lockedPrompt.Replace("{item}", requiredItemName);
            }
        }
        
        return unlockedPrompt;
    }
    
    #endregion
    
    #region Door Logic
    
    /// <summary>
    /// Verifica si el jugador tiene el item requerido
    /// </summary>
    bool HasRequiredItem()
    {
        if (InventorySystem.Instance == null)
            return false;
        
        return InventorySystem.Instance.HasItem(requiredItemName, 1);
    }
    
    /// <summary>
    /// Desbloquea y abre la puerta usando el item
    /// </summary>
    void UnlockAndOpen()
    {
        Debug.Log($"Desbloqueando puerta con {requiredItemName}");
        
        // Consumir el item si es necesario
        if (consumeItemOnUse && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.RemoveItem(requiredItemName, 1);
            Debug.Log($"{requiredItemName} consumido");
        }
        
        // Desbloquear
        isLocked = false;
        UpdateDoorMaterial();
        
        // Reproducir sonido de desbloqueo
        if (unlockSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(unlockSound);
        }
        
        // Abrir después de un pequeño delay
        StartCoroutine(DelayedOpen(0.5f));
    }
    
    IEnumerator DelayedOpen(float delay)
    {
        yield return new WaitForSeconds(delay);
        OpenDoor();
    }
    
    /// <summary>
    /// Abre la puerta
    /// </summary>
    void OpenDoor()
    {
        if (isMoving || isOpen)
            return;
        
        Debug.Log("Abriendo puerta...");
        
        // Reproducir sonido de apertura (temblor)
        if (openSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPoint(openSound, transform.position, 1f);
            }
            else
            {
                AudioSource.PlayClipAtPoint(openSound, transform.position);
            }
        }
        
        // Camera shake
        if (enableCameraShake && CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(shakeDuration, shakeMagnitude);
        }
        
        // Iniciar movimiento
        StartCoroutine(MoveDoor(openPosition, true));
    }
    
    /// <summary>
    /// Cierra la puerta
    /// </summary>
    void CloseDoor()
    {
        if (isMoving || !isOpen)
            return;
        
        Debug.Log("Cerrando puerta...");
        
        StartCoroutine(MoveDoor(closedPosition, false));
    }
    
    /// <summary>
    /// Mueve la puerta a una posición
    /// </summary>
    IEnumerator MoveDoor(Vector3 targetPosition, bool opening)
    {
        isMoving = true;
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                openSpeed * Time.deltaTime
            );
            yield return null;
        }
        
        transform.position = targetPosition;
        isMoving = false;
        isOpen = opening;
        
        // Si se abrió y tiene auto-close, programar cierre
        if (isOpen && autoClose)
        {
            StartCoroutine(AutoCloseAfterDelay());
        }
    }
    
    /// <summary>
    /// Auto-cierre después de un delay
    /// </summary>
    IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseDoor();
    }
    
    /// <summary>
    /// Feedback cuando la puerta está bloqueada
    /// </summary>
    void ShowLockedFeedback()
    {
        Debug.Log("¡La puerta está bloqueada!");
        
        // Reproducir sonido de puerta bloqueada
        if (lockedSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(lockedSound);
        }
        
        // Efecto visual de shake ligero (la puerta tiembla un poco)
        StartCoroutine(ShakeDoor());
    }
    
    /// <summary>
    /// Hace que la puerta tiemble ligeramente cuando está bloqueada
    /// </summary>
    IEnumerator ShakeDoor()
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        float duration = 0.2f;
        
        while (elapsed < duration)
        {
            float offsetX = Random.Range(-0.05f, 0.05f);
            transform.position = originalPos + new Vector3(offsetX, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPos;
    }
    
    #endregion
    
    #region Visual Updates
    
    /// <summary>
    /// Actualiza el material de la puerta según su estado
    /// </summary>
    void UpdateDoorMaterial()
    {
        if (doorRenderer == null)
            return;
        
        if (isLocked && lockedMaterial != null)
        {
            doorRenderer.material = lockedMaterial;
        }
        else if (!isLocked && unlockedMaterial != null)
        {
            doorRenderer.material = unlockedMaterial;
        }
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Desbloquea la puerta externamente (sin consumir item)
    /// </summary>
    public void Unlock()
    {
        isLocked = false;
        UpdateDoorMaterial();
        Debug.Log("Puerta desbloqueada");
    }
    
    /// <summary>
    /// Bloquea la puerta externamente
    /// </summary>
    public void Lock()
    {
        isLocked = true;
        UpdateDoorMaterial();
        Debug.Log("Puerta bloqueada");
    }
    
    /// <summary>
    /// Fuerza la apertura de la puerta (ignora bloqueo)
    /// </summary>
    public void ForceOpen()
    {
        isLocked = false;
        OpenDoor();
    }
    
    #endregion
    
    #region Gizmos
    
    void OnDrawGizmos()
    {
        // Dibujar posición cerrada y abierta
        Vector3 closedPos = Application.isPlaying ? closedPosition : transform.position;
        Vector3 openPos = closedPos + Vector3.up * openHeight;
        
        // Posición cerrada
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(closedPos, transform.localScale);
        
        // Posición abierta
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(openPos, transform.localScale);
        
        // Línea de movimiento
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(closedPos, openPos);
    }
    
    #endregion
    
    #region Debug
    
    [ContextMenu("Toggle Lock (Debug)")]
    void DebugToggleLock()
    {
        if (isLocked)
            Unlock();
        else
            Lock();
    }
    
    [ContextMenu("Force Open (Debug)")]
    void DebugForceOpen()
    {
        ForceOpen();
    }
    
    #endregion
}
