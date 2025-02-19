using Unity.Netcode;
using UnityEngine;

public class LoadManager : NetworkBehaviour
{
    CurrentLobby currentLobby;

    [SerializeField]
    NetworkVariable<int> playerCount;

    private void Start()
    {
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            OnPlayerConnected();
        };
        base.OnNetworkSpawn();
    }

    public void OnPlayerConnected()
    {
        if (IsServer)
        {
            playerCount.Value++;
            if (currentLobby.currentLobby.Players.Count == playerCount.Value)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(
                    "Game",
                    UnityEngine.SceneManagement.LoadSceneMode.Single
                );
            }
        }
    }
}
