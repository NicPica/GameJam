using System.Collections;
using UnityEngine;

/// <summary>
/// Sistema de shake de cámara para efectos de temblor
/// Adjuntar a la cámara principal del jugador
/// </summary>
public class CameraShake : MonoBehaviour
{
    // Singleton para fácil acceso
    public static CameraShake Instance { get; private set; }
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isShaking = false;
    
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
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }
    
    /// <summary>
    /// Inicia un shake de cámara
    /// </summary>
    /// <param name="duration">Duración del shake en segundos</param>
    /// <param name="magnitude">Intensidad del shake</param>
    public void Shake(float duration = 0.5f, float magnitude = 0.1f)
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, magnitude));
        }
    }
    
    /// <summary>
    /// Corrutina que ejecuta el shake
    /// </summary>
    IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            // Shake de posición
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            // Shake de rotación (opcional, más sutil)
            float rotZ = Random.Range(-1f, 1f) * magnitude * 5f;
            transform.localRotation = originalRotation * Quaternion.Euler(0, 0, rotZ);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Restaurar posición y rotación original
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isShaking = false;
    }
    
    /// <summary>
    /// Detiene el shake inmediatamente
    /// </summary>
    public void StopShake()
    {
        if (isShaking)
        {
            StopAllCoroutines();
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            isShaking = false;
        }
    }
}
