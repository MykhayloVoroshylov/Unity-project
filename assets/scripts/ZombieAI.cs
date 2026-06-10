using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float stopDistance = 1.5f;

    void Update()
    {
        // Debug.Log("Zombie updating");

        if (player == null)
        {
            Debug.Log("Player is NULL");
            return;
        }

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        if (distance > stopDistance)
        {
            direction.Normalize();
            transform.position += direction * speed * Time.deltaTime;
        }

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }
}