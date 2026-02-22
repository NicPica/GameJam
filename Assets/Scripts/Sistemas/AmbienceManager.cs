using UnityEngine;

/// <summary>
/// Gestor de sonidos ambiente para cada nivel
/// Reproduce automáticamente el sonido ambiente al cargar el nivel
/// </summary>
public class AmbienceManager : MonoBehaviour
{
    [Header("Ambient Sound")]
    [Tooltip("Sonido ambiente que se reproduce en loop")]
    public AudioClip ambientSound;
    
    [Tooltip("Volumen del sonido ambiente (0-1)")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.5f;
    
    [Tooltip("¿Hacer fade in al iniciar?")]
    public bool fadeIn = true;
    
    [Tooltip("Duración del fade in (segundos)")]
    public float fadeInDuration = 2f;
    
    [Tooltip("¿Detener ambiente al salir del nivel?")]
    public bool stopOnDestroy = true;
    
    void Start()
    {
        PlayAmbience();
    }
    
    /// <summary>
    /// Reproduce el sonido ambiente
    /// </summary>
    void PlayAmbience()
    {
        if (ambientSound == null)
        {
            Debug.LogWarning("AmbienceManager: No hay sonido ambiente asignado");
            return;
        }
        
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AmbienceManager: AudioManager no encontrado");
            return;
        }
        
        // Reproducir el ambiente
        if (fadeIn)
        {
            // Empezar en volumen 0 y hacer fade in
            AudioManager.Instance.PlayAmbient(ambientSound, 0f);
            AudioManager.Instance.FadeAmbient(ambientVolume, fadeInDuration);
        }
        else
        {
            // Reproducir directamente
            AudioManager.Instance.PlayAmbient(ambientSound, ambientVolume);
        }
        
        Debug.Log($"AmbienceManager: Reproduciendo {ambientSound.name}");
    }
    
    /// <summary>
    /// Detiene el sonido ambiente con fade out
    /// </summary>
    public void StopAmbience(float fadeOutDuration = 1f)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeAmbient(0f, fadeOutDuration);
        }
    }
    
    /// <summary>
    /// Cambia el sonido ambiente a otro
    /// </summary>
    public void ChangeAmbience(AudioClip newAmbient, float crossfadeDuration = 2f)
    {
        if (newAmbient == null || AudioManager.Instance == null)
            return;
        
        // Fade out del actual y fade in del nuevo
        AudioManager.Instance.FadeAmbient(0f, crossfadeDuration * 0.5f);
        
        // Esperar a que termine el fade out y cambiar
        Invoke("PlayNewAmbient", crossfadeDuration * 0.5f);
        ambientSound = newAmbient;
    }
    
    void PlayNewAmbient()
    {
        PlayAmbience();
    }
    
    void OnDestroy()
    {
        // Detener el ambiente al destruir el manager
        if (stopOnDestroy && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAmbient();
        }
    }
}
