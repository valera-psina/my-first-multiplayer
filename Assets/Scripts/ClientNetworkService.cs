using UnityEngine;
using Unity.Netcode;

internal class ClientNetworkService
{
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("@Client: Started client");
    }
}