using UnityEngine;

/// <summary>
/// ScriptableObject que contiene el texto de lore para un nivel
/// </summary>
[CreateAssetMenu(fileName = "NewLore", menuName = "Game/Lore Data")]
public class LoreData : ScriptableObject
{
    [Header("Lore Info")]
    [Tooltip("Título del nivel o capítulo")]
    public string title = "NIVEL 1";
    
    [Tooltip("Texto de lore que aparece antes del nivel")]
    [TextArea(5, 10)]
    public string loreText = "Tu texto de lore aquí...";
    
    [Header("Timing")]
    [Tooltip("Tiempo que se muestra el texto (segundos)")]
    public float displayDuration = 4f;
    
    [Tooltip("Velocidad del fade in/out")]
    public float fadeSpeed = 1f;
    
    [Header("Visual")]
    [Tooltip("Color del texto")]
    public Color textColor = Color.white;
    
    [Tooltip("Tamaño de fuente del título")]
    public int titleFontSize = 36;
    
    [Tooltip("Tamaño de fuente del lore")]
    public int loreFontSize = 20;
}
