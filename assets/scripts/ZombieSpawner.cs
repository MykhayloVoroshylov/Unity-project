using UnityEngine;
using UnityEngine.AI; // Required if you use NavMesh later

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Settings")]
    public GameObject zombiePrefab;
    public float spawnIntervalMin = 10f;
    public float spawnIntervalMax = 20f;
    public float minPlayerDistance = 25f;
    public float maxSpawnDistance = 45f;

    [Header("Players")]
    public Transform[] players;
    // Cache the cameras right next to the players to save CPU power
    private Camera[] playerCameras; 

    private float spawnTimer;

    void Start()
    {
        ResetTimer();
        CachePlayerCameras();
    }

    // Call this whenever a player joins or leaves to update the camera cache
    public void CachePlayerCameras()
    {
        if (players == null) return;
        playerCameras = new Camera[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                playerCameras[i] = players[i].GetComponentInChildren<Camera>();
            }
        }
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnZombie();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void SpawnZombie()
    {
        if (players == null || players.Length == 0) return;

        // Pick a random player index
        int targetIndex = Random.Range(0, players.Length);
        Transform player = players[targetIndex];

        // Edge Case: Player disconnected/died mid-game
        if (player == null) return; 

        for (int attempts = 0; attempts < 10; attempts++)
        {
            Vector3 spawnPos = GenerateSpawnPosition(player);

            if (IsValidSpawn(spawnPos, player, targetIndex))
            {
                // WebGL Optimization Reminder: Replace this with Object Pooling later!
                Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
                return;
            }
        }
    }

    Vector3 GenerateSpawnPosition(Transform player)
    {
        float angle = Random.Range(0f, 360f);
        float distance = Random.Range(minPlayerDistance, maxSpawnDistance);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * distance;

        Vector3 spawnPos = player.position + offset;
        
        // Better than forcing Y=0: Keep it relative to the player's ground level
        spawnPos.y = player.position.y; 

        // Optional NavMesh Snap (Highly recommended for WebGL to avoid wall glitches)
        /*
        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas)) {
            spawnPos = hit.position;
        }
        */

        return spawnPos;
    }

    bool IsValidSpawn(Vector3 spawnPos, Transform targetPlayer, int playerIndex)
    {
        // Check if the chosen player can see the spawn point
        // if (IsInView(playerIndex, spawnPos)) return false;

        // Check distance against ALL existing players
        for (int i = 0; i < players.Length; i++)
        {
            Transform otherPlayer = players[i];
            
            // Edge Case: Skip dead/disconnected players
            if (otherPlayer == null) continue; 

            if (Vector3.Distance(otherPlayer.position, spawnPos) < minPlayerDistance)
                return false;
            
            // Co-op Edge Case: Ensure another player isn't looking at this spot!
            if (IsInView(i, spawnPos)) return false;
        }

        return true;
    }

    bool IsInView(int playerIndex, Vector3 point)
    {
        // Safe check if camera cache is missing
        if (playerCameras == null || playerIndex >= playerCameras.Length) return false;
        Camera cam = playerCameras[playerIndex];
        if (cam == null) return false;

        Vector3 viewportPoint = cam.WorldToViewportPoint(point);

        bool inFront = viewportPoint.z > 0;
        bool inScreen = viewportPoint.x > 0 && viewportPoint.x < 1 &&
                        viewportPoint.y > 0 && viewportPoint.y < 1;

        return inFront && inScreen;
    }
}
