using System.Collections;
using UnityEngine;

/// <summary>
/// Trampa de pinchos que causa daño al jugador
/// Puede ser estática o animada
/// </summary>
[RequireComponent(typeof(Collider))]
public class SpikeTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Daño que causa al jugador")]
    public float damage = 25f;

    [Tooltip("¿Es un pincho de muerte instantánea?")]
    public bool instantKill = false;

    [Header("Trap Behavior")]
    [Tooltip("Tipo de trampa")]
    public TrapType trapType = TrapType.Static;

    [Tooltip("Tiempo entre daños si el jugador está sobre la trampa (continuo)")]
    public float damageInterval = 1f;

    [Header("Animated Trap Settings")]
    [Tooltip("Tiempo que los pinchos están arriba")]
    public float upTime = 2f;

    [Tooltip("Tiempo que los pinchos están abajo")]
    public float downTime = 2f;

    [Tooltip("Velocidad de la animación")]
    public float animationSpeed = 2f;

    [Tooltip("Altura del movimiento (para pinchos que suben/bajan)")]
    public float moveHeight = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Material cuando está peligroso")]
    public Material dangerMaterial;

    [Tooltip("Material cuando está seguro")]
    public Material safeMaterial;

    [Tooltip("Color de advertencia")]
    public Color warningColor = Color.red;

    [Header("Audio")]
    [Tooltip("Sonido al activarse")]
    public AudioClip activationSound;

    [Tooltip("Sonido cuando causa daño")]
    public AudioClip damageSound;

    [Tooltip("Sonido de advertencia (loop)")]
    public AudioClip warningSound;

    [Header("Particles")]
    [Tooltip("Partículas al activarse")]
    public GameObject activationParticles;

    [Tooltip("Partículas al causar daño")]
    public GameObject damageParticles;

    public enum TrapType
    {
        Static,          // Siempre activa
        Animated,        // Sube y baja
        PressurePlate    // Se activa al pisarla
    }

    // Estado interno
    private bool isActive = true;
    private bool isPlayerInside = false;
    private float lastDamageTime;
    private AudioSource audioSource;
    private Renderer trapRenderer;
    private Vector3 startPosition;
    private Collider trapCollider;
    private bool isMovingUp = true;

    void Start()
    {
        startPosition = transform.position;
        trapCollider = GetComponent<Collider>();
        trapCollider.isTrigger = true;

        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        // Visual
        trapRenderer = GetComponentInChildren<Renderer>();

        // Inicializar según tipo
        switch (trapType)
        {
            case TrapType.Static:
                isActive = true;
                SetDangerousVisuals();
                break;

            case TrapType.Animated:
                StartCoroutine(AnimatedTrapCycle());
                break;

            case TrapType.PressurePlate:
                isActive = false;
                SetSafeVisuals();
                break;
        }
    }

    void Update()
    {
        // Daño continuo si el jugador está dentro
        if (isActive && isPlayerInside)
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                DealDamage();
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        isPlayerInside = true;
        lastDamageTime = Time.time;

        Debug.Log($"Jugador entró en trampa de pinchos");

        // Comportamiento según tipo
        switch (trapType)
        {
            case TrapType.Static:
                DealDamage();
                break;

            case TrapType.Animated:
                if (isActive)
                {
                    DealDamage();
                }
                break;

            case TrapType.PressurePlate:
                ActivateTrap();
                DealDamage();
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log("Jugador salió de trampa de pinchos");
        }
    }

    /// <summary>
    /// Causa daño al jugador
    /// </summary>
    void DealDamage()
    {
        if (!isActive)
            return;

        PlayerHealth playerHealth = PlayerHealth.Instance;
        if (playerHealth == null)
            return;

        if (instantKill)
        {
            Debug.Log("¡PINCHO DE MUERTE INSTANTÁNEA!");
            playerHealth.Die();
        }
        else
        {
            // Verificar armadura
            float finalDamage = damage;
            ArmorUpgrade armor = playerHealth.GetComponent<ArmorUpgrade>();
            if (armor != null)
            {
                finalDamage *= (1f - armor.damageReduction);
                Debug.Log($"Armadura redujo daño de {damage} a {finalDamage}");
            }

            playerHealth.TakeDamage(finalDamage);
        }

        // Efectos
        PlayDamageEffects();
    }

    /// <summary>
    /// Activa la trampa (para pressure plates)
    /// </summary>
    void ActivateTrap()
    {
        if (isActive)
            return;

        isActive = true;
        Debug.Log("¡Trampa activada!");

        SetDangerousVisuals();

        // Efectos
        if (activationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        if (activationParticles != null)
        {
            Instantiate(activationParticles, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// Reproduce efectos de daño
    /// </summary>
    void PlayDamageEffects()
    {
        // Sonido
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Partículas
        if (damageParticles != null)
        {
            Instantiate(damageParticles, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        // Flash visual
        if (trapRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }
    }

    /// <summary>
    /// Ciclo de animación para pinchos que suben/bajan
    /// </summary>
    IEnumerator AnimatedTrapCycle()
    {
        while (true)
        {
            // Subir pinchos (PELIGRO)
            isActive = true;
            SetDangerousVisuals();
            
            if (activationSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(activationSound);
            }

            yield return StartCoroutine(MoveTo(startPosition + Vector3.up * moveHeight, animationSpeed));

            // Mantener arriba
            yield return new WaitForSeconds(upTime);

            // Bajar pinchos (SEGURO)
            isActive = false;
            SetSafeVisuals();

            yield return StartCoroutine(MoveTo(startPosition, animationSpeed));

            // Mantener abajo
            yield return new WaitForSeconds(downTime);
        }
    }

    /// <summary>
    /// Mueve la trampa a una posición
    /// </summary>
    IEnumerator MoveTo(Vector3 targetPosition, float speed)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
    }

    /// <summary>
    /// Configura visuales de peligro
    /// </summary>
    void SetDangerousVisuals()
    {
        if (trapRenderer != null)
        {
            if (dangerMaterial != null)
            {
                trapRenderer.material = dangerMaterial;
            }
            else
            {
                trapRenderer.material.color = warningColor;
            }
        }
    }

    /// <summary>
    /// Configura visuales de seguridad
    /// </summary>
    void SetSafeVisuals()
    {
        if (trapRenderer != null)
        {
            if (safeMaterial != null)
            {
                trapRenderer.material = safeMaterial;
            }
            else
            {
                trapRenderer.material.color = Color.gray;
            }
        }
    }

    /// <summary>
    /// Efecto de flash al causar daño
    /// </summary>
    IEnumerator FlashEffect()
    {
        Color originalColor = trapRenderer.material.color;
        trapRenderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        trapRenderer.material.color = originalColor;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        if (trapType == TrapType.Animated)
        {
            // Mostrar rango de movimiento
            Vector3 basePos = Application.isPlaying ? startPosition : transform.position;
            Gizmos.DrawWireCube(basePos, Vector3.one * 0.5f);
            Gizmos.DrawWireCube(basePos + Vector3.up * moveHeight, Vector3.one * 0.5f);
            Gizmos.DrawLine(basePos, basePos + Vector3.up * moveHeight);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }

    /// <summary>
    /// Debug: Activar trampa manualmente
    /// </summary>
    [ContextMenu("Activate Trap (Debug)")]
    void DebugActivate()
    {
        ActivateTrap();
    }
}
