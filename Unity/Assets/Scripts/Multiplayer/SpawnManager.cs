using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server-side spawn manager that assigns random spawn positions to connecting players.
/// Attach to a GameObject in the SpawnArea scene.
/// Place empty GameObjects with the PlayerSpawnPoint component at desired spawn locations.
/// </summary>
public class SpawnManager : NetworkBehaviour
{
    [Tooltip("If empty, all PlayerSpawnPoint objects in scene are found automatically")]
    public Transform[] spawnPoints;

    [Tooltip("Random offset range to prevent exact overlaps when players share a spawn point")]
    public float spawnOffsetRadius = 2f;

    void Start()
    {
        // Auto-find spawn points if not manually assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            PlayerSpawnPoint[] found = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
            spawnPoints = new Transform[found.Length];
            for (int i = 0; i < found.Length; i++)
            {
                spawnPoints[i] = found[i].transform;
            }
            Debug.Log($"[SpawnManager] Auto-found {spawnPoints.Length} spawn points");
        }

        // Subscribe to connection approval so we can set spawn positions
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        base.OnDestroy();
    }

    void OnClientConnected(ulong clientId)
    {
        // Host player (clientId 0) is positioned by the prefab spawn
        // For the host, the player object is already spawned
        NetworkObject playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
        if (playerObj != null)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            playerObj.transform.position = spawnPos;
            playerObj.transform.rotation = Quaternion.identity;
            Debug.Log($"[SpawnManager] Player {clientId} spawned at {spawnPos}");
        }
    }

    /// <summary>
    /// Returns a random spawn position from available spawn points, with a small random offset.
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[SpawnManager] No spawn points found, spawning at origin");
            return Vector3.zero;
        }

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * spawnOffsetRadius;
        return point.position + new Vector3(offset.x, 0f, offset.y);
    }
}
