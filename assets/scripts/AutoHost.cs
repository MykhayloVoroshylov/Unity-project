using Unity.Netcode;
using UnityEngine;

public class AutoHost : MonoBehaviour
{
    void Start()
    {
        // Automatically starts the game as Host (Server + Client) immediately on launch!
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Multiplayer Host Automatically Started!");
        }
    }
}