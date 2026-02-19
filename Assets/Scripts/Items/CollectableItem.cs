using System.Collections;
using UnityEngine;

/// Clase base para todos los items coleccionables del juego
/// Implementa IInteractable para ser detectado por el InteractionSystem
[RequireComponent(typeof(Collider))]
public class CollectableItem : MonoBehaviour, IInteractable
{
    [Header("Item Info")]
    [Tooltip("Nombre del item")]
    public string itemName = "Item";

    [Tooltip("Tipo de item (mineral, artifact, etc.)")]
    public string itemType = "Generic";

    [Tooltip("Descripción del item")]
    [TextArea(2, 4)]
    public string description = "Un objeto extraño...";

    [Header("Interaction")]
    [Tooltip("Texto que aparece cuando el jugador mira el item")]
    public string interactionPrompt = "Presiona E para recoger";

    [Tooltip("¿Se destruye al ser recogido?")]
    public bool destroyOnPickup = true;

    [Header("Visual Feedback")]
    [Tooltip("¿Hacer hover effect cuando se mira?")]
    public bool enableHoverEffect = true;

    [Tooltip("Color del highlight al mirar")]
    public Color hoverColor = Color.yellow;

    [Tooltip("Velocidad de rotación constante (Y axis)")]
    public float rotationSpeed = 30f;

    [Tooltip("¿Hacer flotar arriba y abajo?")]
    public bool enableFloating = true;

    [Tooltip("Amplitud del movimiento de flotación")]
    public float floatAmplitude = 0.2f;

    [Tooltip("Velocidad del movimiento de flotación")]
    public float floatSpeed = 2f;

    [Header("Audio")]
    [Tooltip("Sonido específico al recoger este item (opcional)")]
    public AudioClip pickupSound;

    [Header("Effects (Opcional)")]
    [Tooltip("Partículas al ser recogido")]
    public GameObject pickupParticles;

    // Referencias internas
    private Renderer itemRenderer;
    private Material originalMaterial;
    private Material hoverMaterial;
    private bool isBeingLookedAt = false;
    private Vector3 startPosition;
    private AudioSource audioSource;

    void Start()
    {
        // Guardar posición inicial para floating
        startPosition = transform.position;

        // Obtener renderer
        itemRenderer = GetComponentInChildren<Renderer>();

        // Crear material de hover
        if (itemRenderer != null && enableHoverEffect)
        {
            originalMaterial = itemRenderer.material;
            hoverMaterial = new Material(originalMaterial);
            hoverMaterial.SetColor("_EmissionColor", hoverColor * 0.5f);
            hoverMaterial.EnableKeyword("_EMISSION");
        }

        // Asegurar que tiene collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = false; // Debe ser sólido para raycast
        }
        else
        {
            Debug.LogWarning($"CollectableItem {itemName} no tiene Collider!");
        }

       
    }

    void Update()
    {
        // Rotación constante
        if (rotationSpeed > 0)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Flotación
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    #region IInteractable Implementation

    public void Interact()
    {
        // Intentar añadir al inventario
        InventorySystem inventory = InventorySystem.Instance;

        if (inventory != null)
        {
            bool success = inventory.AddItem(this);

            if (success)
            {
                OnItemCollected();
            }
        }
        else
        {
            Debug.LogWarning("No se encontró InventorySystem en la escena!");
            OnItemCollected(); // Recoger de todas formas
        }
    }

    public void OnLookAt()
    {
        isBeingLookedAt = true;

        // Aplicar efecto de hover
        if (itemRenderer != null && enableHoverEffect && hoverMaterial != null)
        {
            itemRenderer.material = hoverMaterial;
        }
    }

    public void OnLookAway()
    {
        isBeingLookedAt = false;

        // Restaurar material original
        if (itemRenderer != null && enableHoverEffect && originalMaterial != null)
        {
            itemRenderer.material = originalMaterial;
        }
    }

    public string GetInteractionPrompt()
    {
        return $"{interactionPrompt} [{itemName}]";
    }

    #endregion

    /// Llamado cuando el item es exitosamente recolectado
    protected virtual void OnItemCollected()
    {
        // Reproducir sonido a través del AudioManager
        if (pickupSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(pickupSound);
        }
        else if (pickupSound != null)
        {
            // Fallback si no hay AudioManager
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Spawear partículas
        if (pickupParticles != null)
        {
            Instantiate(pickupParticles, transform.position, Quaternion.identity);
        }

        // Destruir o desactivar
        if (destroyOnPickup)
        {
            // Si hay audio, esperar a que termine antes de destruir
            if (pickupSound != null && audioSource != null)
            {
                // Hacer invisible pero mantener audio
                if (itemRenderer != null)
                {
                    itemRenderer.enabled = false;
                }

                // Desactivar collider
                Collider col = GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                Destroy(gameObject, pickupSound.length);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            gameObject.SetActive(false);
        }

        Debug.Log($"{itemName} ha sido recogido!");
    }

    /// Dibuja el gizmo en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}