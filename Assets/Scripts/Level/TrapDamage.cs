using System.Collections;
using UnityEngine;

/// <summary>
/// Trampa simple que causa daño al jugador cuando lo toca
/// </summary>
[RequireComponent(typeof(Collider))]
public class TrapDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Daño que causa al jugador")]
    public float damageAmount = 20f;

    [Tooltip("Tipo de trampa (para referencia)")]
    public string trapType = "Spike";

    [Tooltip("¿Causa daño una sola vez o continuamente?")]
    public bool continuousDamage = false;

    [Tooltip("Intervalo entre daños (solo si es continuo)")]
    public float damageInterval = 1f;

    [Header("Visual Effects")]
    [Tooltip("Partículas al activarse")]
    public GameObject activationParticles;

    [Tooltip("Color de aviso (opcional)")]
    public Color warningColor = Color.red;

    [Header("Audio")]
    [Tooltip("Sonido al activarse")]
    public AudioClip activationSound;

    // Estado
    private AudioSource audioSource;
    private Renderer trapRenderer;
    private Material originalMaterial;
    private bool isPlayerInside = false;
    private float lastDamageTime;

    void Start()
    {
        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && activationSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        // Asegurarse de que el collider es trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Obtener renderer para efectos visuales
        trapRenderer = GetComponentInChildren<Renderer>();
        if (trapRenderer != null)
        {
            originalMaterial = trapRenderer.material;
        }
    }

    void Update()
    {
        // Daño continuo si el jugador está dentro
        if (continuousDamage && isPlayerInside)
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                DamagePlayer();
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Verificar si es el jugador
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            lastDamageTime = Time.time;

            Debug.Log($"Jugador entró en trampa: {trapType}");

            // Activar efectos
            ActivateTrap();

            // Causar daño inmediato si no es continuo
            if (!continuousDamage)
            {
                DamagePlayer();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log($"Jugador salió de trampa: {trapType}");
        }
    }

    /// <summary>
    /// Causa daño al jugador
    /// </summary>
    void DamagePlayer()
    {
        PlayerHealth playerHealth = PlayerHealth.Instance;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
            Debug.Log($"Trampa {trapType} causó {damageAmount} de daño");
        }
    }

    /// <summary>
    /// Activa efectos visuales y de sonido
    /// </summary>
    void ActivateTrap()
    {
        // Spawear partículas
        if (activationParticles != null)
        {
            Instantiate(activationParticles, transform.position, Quaternion.identity);
        }

        // Reproducir sonido
        if (activationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Efecto visual simple (parpadeo rojo)
        if (trapRenderer != null)
        {
            StartCoroutine(FlashRed());
        }
    }

    /// <summary>
    /// Parpadeo rojo de la trampa
    /// </summary>
    IEnumerator FlashRed()
    {
        if (trapRenderer != null && originalMaterial != null)
        {
            Material flashMaterial = new Material(originalMaterial);
            flashMaterial.color = warningColor;

            trapRenderer.material = flashMaterial;

            yield return new WaitForSeconds(0.1f);

            trapRenderer.material = originalMaterial;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
