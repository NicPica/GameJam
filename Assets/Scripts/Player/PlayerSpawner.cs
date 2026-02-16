using UnityEngine;

/// <summary>
/// Spawner del jugador en cada nivel
/// Coloca este script en cada nivel y asigna el prefab del Player
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    [Tooltip("Prefab del jugador que se va a instanciar")]
    public GameObject playerPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Posición donde aparecerá el jugador")]
    public Transform spawnPoint;

    [Tooltip("Si no hay spawn point, usar esta posición")]
    public Vector3 defaultSpawnPosition = new Vector3(0, 1, 0);

    [Header("Auto Setup")]
    [Tooltip("¿Buscar automáticamente el Player si ya existe en la escena?")]
    public bool checkForExistingPlayer = true;

    private GameObject spawnedPlayer;

    void Start()
    {
        SpawnPlayer();
    }

    /// <summary>
    /// Spawns el jugador en el nivel
    /// </summary>
    void SpawnPlayer()
    {
        // Primero verificar si ya hay un player en la escena
        if (checkForExistingPlayer)
        {
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null)
            {
                Debug.Log("Player ya existe en la escena - no se spawneará otro");
                
                // Mover al spawn point si existe
                if (spawnPoint != null)
                {
                    existingPlayer.transform.position = spawnPoint.position;
                    existingPlayer.transform.rotation = spawnPoint.rotation;
                }
                
                return;
            }
        }

        // No hay player, spawnearlo
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerSpawner: No se asignó el prefab del Player!");
            return;
        }

        // Determinar posición de spawn
        Vector3 spawnPos = defaultSpawnPosition;
        Quaternion spawnRot = Quaternion.identity;

        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
            spawnRot = spawnPoint.rotation;
        }

        // Instanciar el player
        spawnedPlayer = Instantiate(playerPrefab, spawnPos, spawnRot);
        spawnedPlayer.name = "Player"; // Limpiar el nombre

        Debug.Log($"Player spawneado en posición: {spawnPos}");

        // Aplicar mejoras del GameManager al player recién spawneado
        ApplyUpgradesToPlayer(spawnedPlayer);
    }

    /// <summary>
    /// Aplica las mejoras guardadas en el GameManager al player
    /// </summary>
    void ApplyUpgradesToPlayer(GameObject player)
    {
        if (GameManager.Instance == null)
            return;

        var upgrades = GameManager.Instance.GetAllUpgrades();

        if (upgrades.Count == 0)
            return;

        Debug.Log($"Aplicando {upgrades.Count} mejoras al player...");

        // Aquí aplicamos las mejoras según corresponda
        foreach (string upgrade in upgrades)
        {
            ApplySpecificUpgrade(player, upgrade);
        }
    }

    /// <summary>
    /// Aplica una mejora específica al player
    /// </summary>
    void ApplySpecificUpgrade(GameObject player, string upgradeName)
    {
        switch (upgradeName)
        {
            case "Linterna Mejorada":
                ApplyFlashlightUpgrade(player);
                break;

            case "Detector de Items":
                ApplyItemDetectorUpgrade(player);
                break;

            case "Botas de Velocidad":
                ApplySpeedUpgrade(player);
                break;

            case "Armadura Reforzada":
                ApplyArmorUpgrade(player);
                break;

            case "Radar de Peligros":
                ApplyTrapRadarUpgrade(player);
                break;

            case "Mochila Grande":
                ApplyInventoryUpgrade(player);
                break;

            default:
                Debug.LogWarning($"Mejora desconocida: {upgradeName}");
                break;
        }
    }

    // === IMPLEMENTACIÓN DE MEJORAS ===

    void ApplyFlashlightUpgrade(GameObject player)
    {
        FlashlightSystem flashlight = player.GetComponentInChildren<FlashlightSystem>();
        if (flashlight != null)
        {
            flashlight.range *= 1.5f;
            flashlight.intensity *= 1.3f;
            Debug.Log("Linterna mejorada aplicada");
        }
    }

    void ApplyItemDetectorUpgrade(GameObject player)
    {
        // Esta mejora se puede aplicar a los items directamente
        // Por ahora solo loggeamos
        Debug.Log("Detector de Items aplicado - los items brillarán más");
        
        // Opcional: Buscar todos los CollectableItem y hacerlos más visibles
        CollectableItem[] items = FindObjectsOfType<CollectableItem>();
        foreach (var item in items)
        {
            item.floatAmplitude *= 1.5f;
            item.hoverColor = Color.cyan;
            
            // Añadir luz al item
            Light itemLight = item.gameObject.GetComponent<Light>();
            if (itemLight == null)
            {
                itemLight = item.gameObject.AddComponent<Light>();
                itemLight.range = 3f;
                itemLight.intensity = 1f;
                itemLight.color = Color.yellow;
            }
        }
    }

    void ApplySpeedUpgrade(GameObject player)
    {
        // Buscar script de movimiento por nombre común
        MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string scriptName = script.GetType().Name;
            
            // Intentar encontrar campo de velocidad usando reflection
            if (scriptName.Contains("Controller") || scriptName.Contains("Movement"))
            {
                var type = script.GetType();
                
                // Buscar campos comunes de velocidad
                var moveSpeedField = type.GetField("moveSpeed") ?? 
                                   type.GetField("speed") ?? 
                                   type.GetField("walkSpeed");
                
                if (moveSpeedField != null)
                {
                    float currentSpeed = (float)moveSpeedField.GetValue(script);
                    moveSpeedField.SetValue(script, currentSpeed * 1.2f);
                    Debug.Log($"Velocidad aumentada 20% en {scriptName}");
                    return;
                }
            }
        }
        
        Debug.LogWarning("No se pudo aplicar mejora de velocidad - campo no encontrado");
    }

    void ApplyArmorUpgrade(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            // Añadir un componente que reduzca el daño
            ArmorUpgrade armor = player.GetComponent<ArmorUpgrade>();
            if (armor == null)
            {
                armor = player.AddComponent<ArmorUpgrade>();
            }
            Debug.Log("Armadura Reforzada aplicada");
        }
    }

    void ApplyTrapRadarUpgrade(GameObject player)
    {
        // Añadir componente de radar de trampas
        TrapRadar radar = player.GetComponent<TrapRadar>();
        if (radar == null)
        {
            radar = player.AddComponent<TrapRadar>();
        }
        Debug.Log("Radar de Peligros aplicado");
    }

    void ApplyInventoryUpgrade(GameObject player)
    {
        InventorySystem inventory = player.GetComponent<InventorySystem>();
        if (inventory != null)
        {
            if (inventory.maxCapacity == 0)
            {
                inventory.maxCapacity = 20; // Si era ilimitado, ponerle límite
            }
            else
            {
                inventory.maxCapacity += 10; // Aumentar capacidad
            }
            Debug.Log("Capacidad de inventario aumentada");
        }
    }

    /// <summary>
    /// Debug: Re-spawnear player
    /// </summary>
    [ContextMenu("Respawn Player (Debug)")]
    void DebugRespawnPlayer()
    {
        if (spawnedPlayer != null)
        {
            Destroy(spawnedPlayer);
        }
        SpawnPlayer();
    }
}

/// <summary>
/// Componente que reduce el daño recibido (Armadura Reforzada)
/// </summary>
public class ArmorUpgrade : MonoBehaviour
{
    public float damageReduction = 0.5f; // 50% menos daño

    void OnEnable()
    {
        // Suscribirse al evento de daño
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.onPlayerDamaged.AddListener(OnDamageReceived);
        }
    }

    void OnDisable()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.onPlayerDamaged.RemoveListener(OnDamageReceived);
        }
    }

    void OnDamageReceived()
    {
        // El daño ya fue aplicado, esto es solo para feedback
        Debug.Log("Armadura redujo el daño recibido");
    }
}

/// <summary>
/// Componente que alerta de trampas cercanas (Radar de Peligros)
/// </summary>
public class TrapRadar : MonoBehaviour
{
    public float detectionRadius = 5f;
    public AudioClip warningSound;
    private AudioSource audioSource;
    private bool isWarning = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
    }

    void Update()
    {
        CheckForNearbyTraps();
    }

    void CheckForNearbyTraps()
    {
        TrapDamage[] traps = FindObjectsOfType<TrapDamage>();
        bool trapNearby = false;

        foreach (var trap in traps)
        {
            float distance = Vector3.Distance(transform.position, trap.transform.position);
            if (distance <= detectionRadius)
            {
                trapNearby = true;
                break;
            }
        }

        if (trapNearby && !isWarning)
        {
            isWarning = true;
            Debug.Log("¡Trampa cercana detectada!");
            
            if (warningSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(warningSound);
            }
        }
        else if (!trapNearby && isWarning)
        {
            isWarning = false;
        }
    }
}
