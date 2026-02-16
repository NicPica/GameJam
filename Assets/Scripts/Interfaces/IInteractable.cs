using UnityEngine;

/// Interfaz que deben implementar todos los objetos interactuables del juego
public interface IInteractable
{
    /// Llamado cuando el jugador interactúa con el objeto (presiona E)
    void Interact();
    /// Llamado cuando el jugador mira el objeto
    void OnLookAt();
    /// Llamado cuando el jugador deja de mirar el objeto
    void OnLookAway();
    /// Retorna el texto que se muestra cuando el jugador mira el objeto
    string GetInteractionPrompt();
}