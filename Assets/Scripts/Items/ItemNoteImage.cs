using UnityEngine;

/// <summary>
/// ScriptableObject que define una nota con imagen personalizada
/// </summary>
[CreateAssetMenu(fileName = "NewItemNote", menuName = "Game/Item Note (Image)")]
public class ItemNoteImage : ScriptableObject
{
    [Header("Note Image")]
    [Tooltip("Tu imagen de nota personalizada (como Nota1.png)")]
    public Sprite noteSprite;
    
    [Header("Audio (Opcional)")]
    [Tooltip("Sonido específico para esta nota")]
    public AudioClip noteSound;
}
