using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using Unity.Netcode;
using System.Collections; // Make sure this is exactly "System.Collections" NOT "System.Collections.Generic"

// ==========================================
// 1. ZOMBIE HEALTH
// ==========================================
public class ZombieHealth : NetworkBehaviour
{
    public int maxHealth = 100;
    
    // Assigning the default value via the constructor syntax avoids the Inspector crash!
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        // Only the host/server is allowed to change network variable values!
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsServer) return; // Guard clause: Only server computes damage
        
        currentHealth.Value -= damageAmount;
        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Your existing death/despawn logic goes here
        GetComponent<NetworkObject>().Despawn();
    }
}

// ==========================================
// 2. ZOMBIE MULTIPLAYER TARGETING AI
// ==========================================
public class ZombieAI : NetworkBehaviour
{
    private Transform targetPlayer;
    public float speed = 2f;
    public float stopDistance = 1.5f;

    [Header("Combat Settings")]
    public int attackDamage = 10;
    public float attackRate = 1.5f;
    private float nextAttackTime = 0f;

    [Header("Visual Attack (No Rigging!)")]
    public Transform zombieArmTransform; 
    public float thrustSpeed = 7f;
    private Vector3 armOriginalPosition;

    void Start()
    {
        if (zombieArmTransform != null) armOriginalPosition = zombieArmTransform.localPosition;
    }

    void Update()
    {
        if (!IsServer) return; // AI calculations ONLY run on host/server

        FindClosestPlayer();
        if (targetPlayer == null) return;

        Vector3 direction = targetPlayer.position - transform.position;
        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            direction.Normalize();
            // Server shifts position, syncing naturally through NetworkTransform
            transform.position += new Vector3(direction.x, 0, direction.z) * speed * Time.deltaTime;
        }
        else
        {
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackRate;
                AttackTarget();
            }
        }
    }

    void FindClosestPlayer()
    {
        // Fixes your friend's single player tag bug by searching all active multiplayer clones
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        Transform target = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                target = p.transform;
            }
        }
        targetPlayer = target;
    }

    void AttackTarget()
    {
        if (targetPlayer != null)
        {
            PlayerHealth hp = targetPlayer.GetComponent<PlayerHealth>();
            if (hp != null) hp.TakeDamage(attackDamage);
            
            // Execute the arm animation via network ClientRPC so all players see it slam
            TriggerArmThrustClientRpc();
        }
    }

    [ClientRpc]
    void TriggerArmThrustClientRpc()
    {
        StartCoroutine(ThrustAnimationRoutine());
    }

    IEnumerator ThrustAnimationRoutine()
    {
        if (zombieArmTransform == null) yield break;
        Vector3 thrustPos = armOriginalPosition + Vector3.forward * 0.5f;
        
        float p = 0f;
        while(p < 1f){
            p += Time.deltaTime * thrustSpeed;
            zombieArmTransform.localPosition = Vector3.Lerp(armOriginalPosition, thrustPos, p);
            yield return null;
        }
        p = 0f;
        while(p < 1f){
            p += Time.deltaTime * thrustSpeed;
            zombieArmTransform.localPosition = Vector3.Lerp(thrustPos, armOriginalPosition, p);
            yield return null;
        }
    }
}

// ==========================================
// 3. ZOMBIE POOLING AND RE-INDEXING SPAWNER
// ==========================================
public class ZombieSpawner : NetworkBehaviour
{
    public GameObject zombiePrefab;
    public float spawnIntervalMin = 10f;
    public float spawnIntervalMax = 20f;
    public int maxZombies = 20;

    private float spawnTimer;
    private int currentZombieCount = 0;

    void Start()
    {
        spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
    }

    void Update()
    {
        if (!IsServer) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            TrySpawn();
            spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
        }
    }

    void TrySpawn()
    {
        if (currentZombieCount >= maxZombies) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 0) return;

        Transform randomPlayer = players[Random.Range(0, players.Length)].transform;
        Vector3 spawnPos = randomPlayer.position + new Vector3(Random.Range(-15f, 15f), 0, Random.Range(-15f, 15f));

        GameObject zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        zombie.GetComponent<ZombieHealth>().AssignSpawner(this);
        
        // Spawn across the network topology
        zombie.GetComponent<NetworkObject>().Spawn();
        currentZombieCount++;
    }

    public void ReturnZombie(GameObject zombie)
    {
        if (!IsServer) return;
        currentZombieCount--;
        zombie.GetComponent<NetworkObject>().Despawn(); // Despawn returns/clears object safely from net views
    }
}