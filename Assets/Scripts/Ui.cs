using Unity.Netcode;
using UnityEngine;

public class Ui : MonoBehaviour
{
    public void StartHostButtonClick()
    {
        NetworkManager.Singleton.StartHost();
    }

    public void StartClientButtonClick()
    {
        NetworkManager.Singleton.StartClient();
    }
}
