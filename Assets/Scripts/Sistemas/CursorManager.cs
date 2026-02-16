using UnityEngine;

/// <summary>
/// Gestor del cursor que maneja el bloqueo/desbloqueo en diferentes estados del juego
/// </summary>
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Bloquea el cursor (para gameplay)
    /// </summary>
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Cursor bloqueado");
    }

    /// <summary>
    /// Desbloquea el cursor (para UI/menús)
    /// </summary>
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor desbloqueado");
    }

    /// <summary>
    /// Verifica si el cursor está bloqueado
    /// </summary>
    public bool IsCursorLocked()
    {
        return Cursor.lockState == CursorLockMode.Locked;
    }
}
