using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ZombieAI : NetworkBehaviour
{
    public float speed = 2f;
    public float stopDistance = 1.5f;

    [Header("Combat Settings")]
    public int attackDamage = 10;
    public float attackRate = 1.5f;

    [Header("Visual Attack (No Rigging!)")]
    public Transform zombieArmTransform;
    public float thrustSpeed = 7f;

    private Transform targetPlayer;
    private float nextAttackTime;
    private Vector3 armOriginalPosition;

    void Start()
    {
        if (zombieArmTransform != null)
            armOriginalPosition = zombieArmTransform.localPosition;
    }

    void Update()
    {
        if (!IsServer) return;

        FindClosestPlayer();
        if (targetPlayer == null) return;

        Vector3 direction = targetPlayer.position - transform.position;
        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            direction.Normalize();
            transform.position += new Vector3(direction.x, 0f, direction.z) * speed * Time.deltaTime;
        }
        else if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackRate;
            AttackTarget();
        }
    }

    void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = player.transform;
            }
        }

        targetPlayer = closest;
    }

    void AttackTarget()
    {
        if (targetPlayer == null) return;

        PlayerHealth hp = targetPlayer.GetComponent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(attackDamage);

        TriggerArmThrustClientRpc();
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

        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * thrustSpeed;
            zombieArmTransform.localPosition = Vector3.Lerp(armOriginalPosition, thrustPos, progress);
            yield return null;
        }

        progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * thrustSpeed;
            zombieArmTransform.localPosition = Vector3.Lerp(thrustPos, armOriginalPosition, progress);
            yield return null;
        }
    }
}
