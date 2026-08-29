using System;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class HostNetworkService : IDisposable
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.OnClientConnectedClientCallback += OnClientConnectedHostCallback;
        Debug.Log("@Host: Host has been started");
        SpawnView(NetworkManager.Singleton.LocalClientId);
    }

    private void OnClientConnectedHostCallback(ulong connectedClientId)
    {
        Debug.Log($"@Server: Client connected id = {connectedClientId}");
        SpawnView(connectedClientId);
    }

    private void SpawnView(ulong clientId)
    {
        var prefabPath = "Player";
        var prefab = Resources.Load<GameObject>(prefabPath);
        var createdGO = Object.Instantiate(prefab);
        createdGO.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        Debug.Log("@Server: Spawned view for player: " + clientId);
    }
}
