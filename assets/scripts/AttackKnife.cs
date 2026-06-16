using UnityEngine;

[DisallowMultipleComponent]
public class AttackKnife : MonoBehaviour
{
    void Awake()
    {
        Debug.LogWarning("AttackKnife is deprecated. Use MeleeAttack with AttackMode.Knife instead.");
    }
}
    