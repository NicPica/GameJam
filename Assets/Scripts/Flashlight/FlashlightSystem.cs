using System.Collections;
using UnityEngine;

/// Sistema de linterna simple que se adjunta a la cámara del jugador
/// Controla una luz que se puede encender/apagar
public class FlashlightSystem : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [Tooltip("Luz que actuará como linterna")]
    public Light flashlight;

    [Tooltip("Tecla para encender/apagar la linterna")]
    public KeyCode flashlightKey = KeyCode.F;

    [Tooltip("¿La linterna empieza encendida?")]
    public bool startOn = false;

    [Header("Light Properties")]
    [Tooltip("Intensidad de la luz cuando está encendida")]
    public float intensity = 2f;

    [Tooltip("Rango de alcance de la luz")]
    public float range = 15f;

    [Tooltip("Ángulo del cono de luz (Spotlight)")]
    public float spotAngle = 60f;

    [Header("Toggle Animation (Opcional)")]
    [Tooltip("¿Animar el encendido/apagado?")]
    public bool animateToggle = true;

    [Tooltip("Duración de la animación de encendido/apagado")]
    public float toggleDuration = 0.1f;

    [Header("Audio (Opcional)")]
    [Tooltip("Sonido al encender/apagar")]
    public AudioClip toggleSound;

    private AudioSource audioSource;
    private bool isOn;
    private bool isToggling = false;

    void Start()
    {
        // Crear luz si no existe
        if (flashlight == null)
        {
            CreateDefaultFlashlight();
        }

        // Configurar propiedades de la luz
        ConfigureLight();

        // Estado inicial
        isOn = startOn;
        flashlight.enabled = isOn;

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && toggleSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
    }

    void Update()
    {
        // Toggle de la linterna
        if (Input.GetKeyDown(flashlightKey) && !isToggling)
        {
            ToggleFlashlight();
        }
    }

    /// Crea una luz por defecto si no se asignó ninguna
    void CreateDefaultFlashlight()
    {
        GameObject lightObj = new GameObject("Flashlight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        lightObj.transform.localRotation = Quaternion.identity;

        flashlight = lightObj.AddComponent<Light>();
        flashlight.type = LightType.Spot;

        Debug.Log("FlashlightSystem: Luz creada automáticamente como hijo de la cámara");
    }

    /// Configura las propiedades de la luz
    void ConfigureLight()
    {
        flashlight.intensity = intensity;
        flashlight.range = range;
        flashlight.type = LightType.Spot;
        flashlight.spotAngle = spotAngle;

        // Configuración para mejor rendimiento
        flashlight.shadows = LightShadows.None; // Cambiar a Hard o Soft si quieres sombras
        flashlight.renderMode = LightRenderMode.ForcePixel;
    }

    /// Enciende/apaga la linterna
    public void ToggleFlashlight()
    {
        isOn = !isOn;

        // Reproducir sonido
        if (toggleSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }

        // Animar o cambiar instantáneamente
        if (animateToggle)
        {
            StartCoroutine(AnimateToggle(isOn));
        }
        else
        {
            flashlight.enabled = isOn;
        }
    }

    /// Anima el encendido/apagado de la luz
    IEnumerator AnimateToggle(bool turnOn)
    {
        isToggling = true;

        float targetIntensity = turnOn ? intensity : 0f;
        float startIntensity = flashlight.intensity;
        float elapsed = 0f;

        flashlight.enabled = true; // Asegurar que está habilitada para animar

        while (elapsed < toggleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / toggleDuration;
            flashlight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        flashlight.intensity = targetIntensity;
        flashlight.enabled = isOn;

        isToggling = false;
    }

    /// Enciende la linterna
    public void TurnOn()
    {
        if (!isOn)
        {
            ToggleFlashlight();
        }
    }

    /// Apaga la linterna
    public void TurnOff()
    {
        if (isOn)
        {
            ToggleFlashlight();
        }
    }

    /// Devuelve si la linterna está encendida
    public bool IsOn()
    {
        return isOn;
    }
}