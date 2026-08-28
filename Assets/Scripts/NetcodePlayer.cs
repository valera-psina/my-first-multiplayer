using UnityEngine;
using Unity.Netcode;

public class NetcodePlayer : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;

    void Update()
    {
        if (!IsOwner)
            return;

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var inputDirection = new Vector3(horizontal, 0, vertical);

        if (inputDirection.sqrMagnitude > 0)
            MoveServerRpc(inputDirection);
    }

    [ServerRpc]
    private void MoveServerRpc(Vector3 inputDirection)
    {
        transform.position += inputDirection * _speed * Time.deltaTime;
    }
}
