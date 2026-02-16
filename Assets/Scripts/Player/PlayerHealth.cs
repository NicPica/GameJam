using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Sistema de vida del jugador con respawn/reinicio de nivel
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Vida máxima del jugador")]
    public float maxHealth = 100f;

    [Tooltip("Vida actual")]
    private float currentHealth;

    [Tooltip("¿Es invulnerable?")]
    public bool isInvulnerable = false;

    [Header("Damage Feedback")]
    [Tooltip("Imagen roja que aparece al recibir daño")]
    public Image damageOverlay;

    [Tooltip("Duración del efecto de daño")]
    public float damageFeedbackDuration = 0.2f;

    [Tooltip("Sonido al recibir daño")]
    public AudioClip damageSound;

    [Tooltip("Sonido de muerte")]
    public AudioClip deathSound;

    [Header("Death Settings")]
    [Tooltip("Tiempo antes de reiniciar tras morir")]
    public float respawnDelay = 2f;

    [Tooltip("Pantalla de muerte")]
    public GameObject deathScreen;

    [Tooltip("Texto de muerte")]
    public Text deathText;

    [Header("UI")]
    [Tooltip("Slider de vida")]
    public Slider healthBar;

    [Tooltip("Texto de vida")]
    public Text healthText;

    [Header("Events")]
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onPlayerDamaged;
    public UnityEvent onPlayerDeath;

    // Referencias
    private AudioSource audioSource;
    private bool isDead = false;

    // Singleton
    public static PlayerHealth Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Inicializar vida
        currentHealth = maxHealth;

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        // Ocultar pantalla de muerte
        if (deathScreen != null)
            deathScreen.SetActive(false);

        // Ocultar overlay de daño
        if (damageOverlay != null)
            damageOverlay.gameObject.SetActive(false);

        UpdateHealthUI();
    }

    /// <summary>
    /// Aplica daño al jugador
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Jugador recibió {damage} de daño. Vida: {currentHealth}/{maxHealth}");

        // Feedback visual
        StartCoroutine(ShowDamageFeedback());

        // Sonido
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Eventos
        onHealthChanged?.Invoke(currentHealth);
        onPlayerDamaged?.Invoke();

        UpdateHealthUI();

        // Verificar muerte
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Cura al jugador
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        Debug.Log($"Jugador curado {amount}. Vida: {currentHealth}/{maxHealth}");

        onHealthChanged?.Invoke(currentHealth);
        UpdateHealthUI();
    }

    /// <summary>
    /// Mata instantáneamente al jugador
    /// </summary>
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Debug.Log("El jugador ha muerto");

        // Sonido de muerte
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Desactivar controles del jugador
        // Buscar y desactivar todos los scripts de movimiento en el jugador
        MonoBehaviour[] playerScripts = GetComponents<MonoBehaviour>();
        foreach (var script in playerScripts)
        {
            // Desactivar scripts comunes de movimiento (pero no PlayerHealth)
            string scriptName = script.GetType().Name;
            if (script != this && // No desactivar este mismo script
                (scriptName.Contains("Controller") || 
                 scriptName.Contains("Movement") ||
                 scriptName.Contains("Input")))
            {
                script.enabled = false;
            }
        }

        // Eventos
        onPlayerDeath?.Invoke();

        // Mostrar pantalla de muerte
        ShowDeathScreen();

        // Reiniciar nivel después de un delay
        StartCoroutine(RespawnAfterDelay());
    }

    /// <summary>
    /// Muestra feedback visual de daño
    /// </summary>
    IEnumerator ShowDamageFeedback()
    {
        if (damageOverlay != null)
        {
            damageOverlay.gameObject.SetActive(true);
            Color color = damageOverlay.color;
            color.a = 0.5f;
            damageOverlay.color = color;

            float elapsed = 0f;
            while (elapsed < damageFeedbackDuration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(0.5f, 0f, elapsed / damageFeedbackDuration);
                damageOverlay.color = color;
                yield return null;
            }

            damageOverlay.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra la pantalla de muerte
    /// </summary>
    void ShowDeathScreen()
    {
        if (deathScreen != null)
        {
            deathScreen.SetActive(true);

            if (deathText != null)
            {
                deathText.text = "Has muerto\n\nReiniciando nivel...";
            }
        }
    }

    /// <summary>
    /// Reinicia el nivel después de un delay
    /// </summary>
    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        // Notificar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FailLevel();
        }
        else
        {
            Debug.LogError("GameManager no encontrado - no se puede reiniciar");
        }
    }

    /// <summary>
    /// Actualiza la UI de vida
    /// </summary>
    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Ceil(currentHealth)} / {maxHealth}";
        }
    }

    /// <summary>
    /// Obtiene el porcentaje de vida actual
    /// </summary>
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// Verifica si el jugador está vivo
    /// </summary>
    public bool IsAlive()
    {
        return !isDead;
    }

    /// <summary>
    /// Restaura la vida completa
    /// </summary>
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    /// <summary>
    /// Debug: Quitar vida
    /// </summary>
    [ContextMenu("Take 20 Damage (Debug)")]
    void DebugTakeDamage()
    {
        TakeDamage(20f);
    }

    /// <summary>
    /// Debug: Matar jugador
    /// </summary>
    [ContextMenu("Kill Player (Debug)")]
    void DebugKillPlayer()
    {
        Die();
    }
}
