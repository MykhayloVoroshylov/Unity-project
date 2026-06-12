using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Zombie Settings")]
    public GameObject zombiePrefab;
    public float spawnIntervalMin = 10f;
    public float spawnIntervalMax = 20f;
    public float minPlayerDistance = 20f;
    public float maxSpawnDistance = 50f;

    [Header("Spawn Tuning")]
    [Tooltip("Zombies won't spawn within this many seconds of being in view — prevents pop-in without blocking spawns entirely")]
    public float viewCooldown = 3f;
    public int maxZombies = 20; // WebGL cap — important

    [Header("Players")]
    public Transform[] players;
    private Camera[] playerCameras;

    private float spawnTimer;
    private int currentZombieCount = 0;
    private ObjectPool<GameObject> zombiePool;

    void Awake()
    {
        zombiePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(zombiePrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            maxSize: maxZombies
        );
    }

    void Start()
    {
        ResetTimer();
        CachePlayerCameras();
    }

    public void CachePlayerCameras()
    {
        if (players == null) return;
        playerCameras = new Camera[players.Length];
        for (int i = 0; i < players.Length; i++)
            if (players[i] != null)
                playerCameras[i] = players[i].GetComponentInChildren<Camera>();
    }

    // Call this from your zombie's death/despawn logic
    public void ReturnZombie(GameObject zombie)
    {
        currentZombieCount--;
        zombiePool.Release(zombie);
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            TrySpawn();
            ResetTimer();
        }
    }

    void ResetTimer() => spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);

    void TrySpawn()
    {
        if (players == null || players.Length == 0) return;
        if (currentZombieCount >= maxZombies) return; // WebGL: hard cap

        // Try each player as a spawn anchor, in random order
        int[] order = RandomOrder(players.Length);
        foreach (int i in order)
        {
            if (players[i] == null) continue;
            Vector3 pos;
            if (TryGetSpawnPosition(players[i], out pos))
            {
                GameObject zombie = zombiePool.Get();
                zombie.transform.SetPositionAndRotation(pos, Quaternion.identity);
                currentZombieCount++;
                return;
            }
        }
    }

    bool TryGetSpawnPosition(Transform anchor, out Vector3 result)
    {
        for (int attempt = 0; attempt < 15; attempt++)
        {
            // Use a random direction on the XZ plane
            Vector2 circle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(minPlayerDistance, maxSpawnDistance);
            Vector3 candidate = anchor.position + new Vector3(circle.x, 0f, circle.y) * distance;

            // NavMesh snap — required for non-flat terrain
            NavMeshHit hit;
            if (!NavMesh.SamplePosition(candidate, out hit, 5f, NavMesh.AllAreas))
                continue; // Not on walkable ground, skip

            candidate = hit.position;

            if (!IsTooCloseToAnyPlayer(candidate) && !IsVisibleToAllPlayers(candidate))
            {
                result = candidate;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    bool IsTooCloseToAnyPlayer(Vector3 pos)
    {
        foreach (Transform p in players)
        {
            if (p == null) continue;
            if (Vector3.Distance(p.position, pos) < minPlayerDistance)
                return true;
        }
        return false;
    }

    // Only block spawn if ALL players can see it — one player looking doesn't lock the whole map
    bool IsVisibleToAllPlayers(Vector3 pos)
    {
        int visibleCount = 0;
        int aliveCount = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;
            aliveCount++;
            if (IsInView(i, pos)) visibleCount++;
        }

        // Only truly blocked if every living player is watching this spot
        return aliveCount > 0 && visibleCount == aliveCount;
    }

    bool IsInView(int playerIndex, Vector3 point)
    {
        if (playerCameras == null || playerIndex >= playerCameras.Length) return false;
        Camera cam = playerCameras[playerIndex];
        if (cam == null) return false;

        Vector3 vp = cam.WorldToViewportPoint(point);
        return vp.z > 0 && vp.x > 0.1f && vp.x < 0.9f && vp.y > 0.1f && vp.y < 0.9f;
        // Slightly inset (0.1) so screen edges don't count — reduces edge cases
    }

    int[] RandomOrder(int count)
    {
        int[] arr = new int[count];
        for (int i = 0; i < count; i++) arr[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
        return arr;
    }
}
