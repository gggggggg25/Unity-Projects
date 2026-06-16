using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    private int index;

    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    void OnServerStarted()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        Transform spawn = spawnPoints[index];
        index = (index + 1) % spawnPoints.Length;

        GameObject player = Instantiate(playerPrefab, spawn.position, spawn.rotation);

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }
}