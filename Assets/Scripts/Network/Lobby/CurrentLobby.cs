using System.Collections;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CurrentLobby : MonoBehaviour
{
    public Lobby currentLobby { get; set; }
    public Player thisPlayer { get; set; }

    private void Start()
    {
        StartCoroutine(PingLobbyCoroutine(currentLobby.Id, 5f));
    }

    public static IEnumerator PingLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
}
