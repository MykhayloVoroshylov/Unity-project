using Unity.Netcode;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnIntervalMin = 10f;
    public float spawnIntervalMax = 20f;
    public int maxZombies = 20;

    private float spawnTimer;
    private int currentZombieCount;

    void Start()
    {
        spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        TrySpawn();
        spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void TrySpawn()
    {
        if (currentZombieCount >= maxZombies) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        Transform randomPlayer = players[Random.Range(0, players.Length)].transform;
        Vector3 spawnPos = randomPlayer.position + new Vector3(
            Random.Range(-15f, 15f),
            0f,
            Random.Range(-15f, 15f));

        GameObject zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        zombie.GetComponent<NetworkObject>().Spawn();

        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>();
        if (zombieHealth != null)
            zombieHealth.AssignSpawner(this);

        currentZombieCount++;
    }

    public void ReturnZombie(GameObject zombie)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
        currentZombieCount = Mathf.Max(0, currentZombieCount - 1);
    }
}
