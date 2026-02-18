using UnityEngine;

/// <summary>
/// Spawner del jugador en cada nivel - Versión simplificada sin mejoras
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
