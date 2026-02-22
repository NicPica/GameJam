using System.Collections;
using UnityEngine;

/// <summary>
/// Piso que se rompe después de que el jugador camina sobre él
/// </summary>
[RequireComponent(typeof(Collider))]
public class BreakableFloor : MonoBehaviour
{
    [Header("Break Settings")]
    [Tooltip("Tiempo antes de romperse (segundos)")]
    public float breakDelay = 1f;
    
    [Tooltip("¿El piso se destruye o solo se desactiva?")]
    public bool destroyOnBreak = true;
    
    [Tooltip("¿El piso reaparece después de un tiempo?")]
    public bool respawn = false;
    
    [Tooltip("Tiempo antes de reaparecer (solo si respawn está activo)")]
    public float respawnDelay = 5f;
    
    [Header("Visual Feedback")]
    [Tooltip("¿Hacer que el piso tiemble antes de romperse?")]
    public bool shakeBeforeBreak = true;
    
    [Tooltip("Intensidad del temblor")]
    public float shakeIntensity = 0.05f;
    
    [Tooltip("Velocidad del temblor")]
    public float shakeSpeed = 20f;
    
    [Tooltip("¿Cambiar color cuando el jugador está sobre él?")]
    public bool changeColorOnStep = true;
    
    [Tooltip("Color de advertencia")]
    public Color warningColor = Color.red;
    
    [Header("Audio")]
    [Tooltip("Sonido cuando el jugador pisa el piso")]
    public AudioClip stepSound;
    
    [Tooltip("Sonido cuando el piso se rompe")]
    public AudioClip breakSound;
    
    [Header("Effects")]
    [Tooltip("Partículas cuando se rompe")]
    public GameObject breakParticles;
    
    [Tooltip("¿Hacer camera shake al romperse?")]
    public bool enableCameraShake = true;
    
    [Tooltip("Intensidad del camera shake")]
    public float cameraShakeMagnitude = 0.1f;
    
    // Estado interno
    private bool isTriggered = false;
    private bool isBroken = false;
    private Vector3 originalPosition;
    private Renderer floorRenderer;
    private Material originalMaterial;
    private Color originalColor;
    private Collider floorCollider;
    
    void Start()
    {
        originalPosition = transform.position;
        floorRenderer = GetComponent<Renderer>();
        floorCollider = GetComponent<Collider>();
        
        // Guardar material y color original
        if (floorRenderer != null)
        {
            originalMaterial = floorRenderer.material;
            originalColor = originalMaterial.color;
        }
        
        // El collider NO debe ser trigger para que el jugador camine sobre él
        if (floorCollider != null)
        {
            floorCollider.isTrigger = false;
        }
    }
    
    void OnCollisionStay(Collision collision)
    {
        // Verificar si es el jugador
        if (!collision.gameObject.CompareTag("Player") || isTriggered || isBroken)
            return;
        
        Debug.Log($"{gameObject.name}: Jugador detectado, iniciando countdown...");
        
        isTriggered = true;
        
        // Iniciar proceso de rotura
        StartCoroutine(BreakSequence());
    }
    
    /// <summary>
    /// Secuencia completa de rotura del piso
    /// </summary>
    IEnumerator BreakSequence()
    {
        // Sonido de paso
        PlaySound(stepSound);
        
        // Cambiar color de advertencia
        if (changeColorOnStep && floorRenderer != null)
        {
            floorRenderer.material.color = warningColor;
        }
        
        // Shake del piso mientras espera
        float elapsed = 0f;
        while (elapsed < breakDelay)
        {
            if (shakeBeforeBreak)
            {
                // Hacer que el piso tiemble
                float offsetY = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity;
                transform.position = originalPosition + new Vector3(0, offsetY, 0);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Romper el piso
        BreakFloor();
    }
    
    /// <summary>
    /// Rompe el piso
    /// </summary>
    void BreakFloor()
    {
        if (isBroken)
            return;
        
        isBroken = true;
        
        Debug.Log($"{gameObject.name}: ¡Piso roto!");
        
        // Restaurar posición antes de romper (por si estaba temblando)
        transform.position = originalPosition;
        
        // Sonido de rotura
        PlaySound(breakSound);
        
        // Partículas
        if (breakParticles != null)
        {
            Instantiate(breakParticles, transform.position, Quaternion.identity);
        }
        
        // Camera shake
        if (enableCameraShake && CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.3f, cameraShakeMagnitude);
        }
        
        // Desactivar visualmente el piso
        if (floorRenderer != null)
        {
            floorRenderer.enabled = false;
        }
        
        if (floorCollider != null)
        {
            floorCollider.enabled = false;
        }
        
        // Destruir o desactivar según configuración
        if (destroyOnBreak)
        {
            if (respawn)
            {
                // Si debe reaparecer, no destruir, solo esperar
                StartCoroutine(RespawnFloor());
            }
            else
            {
                // Destruir completamente
                Destroy(gameObject, 0.5f);
            }
        }
        else
        {
            // Solo desactivar
            if (respawn)
            {
                StartCoroutine(RespawnFloor());
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Reaparece el piso después del delay
    /// </summary>
    IEnumerator RespawnFloor()
    {
        yield return new WaitForSeconds(respawnDelay);
        
        Debug.Log($"{gameObject.name}: Reapareciendo...");
        
        // Restaurar estado
        isTriggered = false;
        isBroken = false;
        
        // Restaurar visual
        if (floorRenderer != null)
        {
            floorRenderer.enabled = true;
            floorRenderer.material.color = originalColor;
        }
        
        if (floorCollider != null)
        {
            floorCollider.enabled = true;
        }
        
        transform.position = originalPosition;
    }
    
    /// <summary>
    /// Reproduce un sonido
    /// </summary>
    void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXAtPoint(clip, transform.position);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
    
    /// <summary>
    /// Resetea el piso manualmente (útil para debug)
    /// </summary>
    [ContextMenu("Reset Floor (Debug)")]
    public void ResetFloor()
    {
        StopAllCoroutines();
        isTriggered = false;
        isBroken = false;
        
        if (floorRenderer != null)
        {
            floorRenderer.enabled = true;
            floorRenderer.material.color = originalColor;
        }
        
        if (floorCollider != null)
        {
            floorCollider.enabled = true;
        }
        
        transform.position = originalPosition;
    }
    
    void OnDrawGizmos()
    {
        // Dibujar indicador visual en el editor
        Gizmos.color = isBroken ? Color.red : Color.yellow;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
