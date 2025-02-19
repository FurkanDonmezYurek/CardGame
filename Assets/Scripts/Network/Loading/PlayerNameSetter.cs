using UnityEngine;
using Unity.Netcode;

public class PlayerNameSetter : NetworkBehaviour
{
    CurrentLobby currentLobby;

    public override void OnNetworkSpawn()
    {
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            if (IsHost)
            {
                SetNameServerRpc(currentLobby.thisPlayer.Id);
            }
            else
            {
                SendNameToServer();
            }
        }
    }

    [ServerRpc]
    public void SetNameServerRpc(string playerName)
    {
        transform.name = playerName;
        UpdateNameClientRpc(playerName);
    }

    [ClientRpc]
    private void UpdateNameClientRpc(string playerName)
    {
        transform.name = playerName;
    }

    private void SendNameToServer()
    {
        string playerName = currentLobby.thisPlayer.Id;
        if (!string.IsNullOrEmpty(playerName))
        {
            SetNameServerRpc(playerName);
        }
    }
}
