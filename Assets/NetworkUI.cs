using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    void OnGUI()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("No NetworkManager in scene!");
            return;
        }

        if (!NetworkManager.Singleton.IsClient &&
            !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(500, 200, 200, 60), "Host"))
            {
                Debug.Log("Starting Host...");
                NetworkManager.Singleton.StartHost();
            }

            if (GUI.Button(new Rect(10, 50, 100, 30), "Client"))
            {
                Debug.Log("Starting Client...");
                NetworkManager.Singleton.StartClient();
            }
        }
    }
}