using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Sistema de interacción que detecta objetos interactuables mediante Raycast
/// Adjuntar a la cámara del jugador
public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Distancia máxima de interacción")]
    public float interactionDistance = 3f;

    [Tooltip("Layer de objetos interactuables")]
    public LayerMask interactableLayer;

    [Header("Input")]
    [Tooltip("Tecla para interactuar")]
    public KeyCode interactKey = KeyCode.E;

    [Header("UI Feedback (Opcional)")]
    [Tooltip("Texto UI que muestra el nombre del objeto al que se mira")]
    public UnityEngine.UI.Text interactionText;

    // Referencias internas
    private Camera playerCamera;
    private IInteractable currentInteractable;

    void Start()
    {
        // Obtener la cámara del componente
        playerCamera = GetComponent<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("InteractionSystem necesita estar en un GameObject con Camera!");
        }

        // Ocultar texto de interacción al inicio
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        CheckForInteractable();
        HandleInteractionInput();
    }

    /// Detecta objetos interactuables frente al jugador
    void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Realizar raycast
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayer))
        {
            // Intentar obtener componente IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                // Nuevo objeto detectado
                if (currentInteractable != interactable)
                {
                    // Deseleccionar anterior si existe
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnLookAway();
                    }

                    currentInteractable = interactable;
                    currentInteractable.OnLookAt();

                    // Actualizar UI
                    ShowInteractionPrompt(interactable.GetInteractionPrompt());
                }

                return;
            }
        }

        // No hay objeto interactuable
        if (currentInteractable != null)
        {
            currentInteractable.OnLookAway();
            currentInteractable = null;
            HideInteractionPrompt();
        }
    }

    /// Maneja el input de interacción
    void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

 
    /// Muestra el texto de interacción en pantalla

    void ShowInteractionPrompt(string prompt)
    {
        if (interactionText != null)
        {
            interactionText.text = prompt;
            interactionText.gameObject.SetActive(true);
        }
    }


    /// Oculta el texto de interacción

    void HideInteractionPrompt()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    // Dibuja el rayo en el editor para debug
    void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance);
        }
    }
}