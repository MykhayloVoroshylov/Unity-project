using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    public float attackRange = 3f;
    public int damage = 25;
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        RaycastHit hit;

        if (Physics.Raycast(
            playerCamera.transform.position,
            playerCamera.transform.forward,
            out hit,
            attackRange))
        {
            Debug.Log("Hit: " + hit.collider.name);

            ZombieHealth zombie =
                hit.collider.GetComponentInParent<ZombieHealth>();

            if (zombie != null)
            {
                zombie.TakeDamage(damage);
            }
        }
    }
}