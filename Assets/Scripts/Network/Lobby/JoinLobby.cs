using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class JoinLobby : MonoBehaviour
{
    public TMP_InputField lobbyCodeArea;

    public async void JoinLobbyWithLobbyId(string lobbyId)
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        string hexRGBA = "#" + ColorUtility.ToHtmlStringRGBA(randomColor);
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
            options.Player = new Player(AuthenticationService.Instance.PlayerId);
            options.Player.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "PlayerName",
                    new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Public,
                        PlayerPrefs.GetString("PlayerName")
                    )
                },
                {
                    "readyCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0")
                },
                {
                    "RelayClientId",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "")
                },
                {
                    "PlayerColor",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, hexRGBA)
                }
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log("Join Lobby Done:" + lobby.Name);

            GameObject currentLobbyGO = new GameObject("CurrentLobby");
            CurrentLobby currentLobbySC = currentLobbyGO.AddComponent<CurrentLobby>();

            currentLobbySC.currentLobby = lobby;
            currentLobbySC.thisPlayer = options.Player;
            DontDestroyOnLoad(currentLobbyGO);
            LoadLobbyRoom();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void JoinLobbyWithLobbyCode()
    {
        Color randomColor = new Color(Random.value, Random.value, Random.value);
        string hexRGBA = "#" + ColorUtility.ToHtmlStringRGBA(randomColor);
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions();
            options.Player = new Player(AuthenticationService.Instance.PlayerId);
            options.Player.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "PlayerName",
                    new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Public,
                        PlayerPrefs.GetString("PlayerName")
                    )
                },
                {
                    "readyCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "0")
                },
                {
                    "RelayClientId",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "")
                },
                {
                    "PlayerColor",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, hexRGBA)
                }
            };
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(
                lobbyCodeArea.text,
                options
            );
            Debug.Log("Join Lobby Done:" + lobby.Name);

            GameObject currentLobbyGO = new GameObject("CurrentLobby");
            CurrentLobby currentLobbySC = currentLobbyGO.AddComponent<CurrentLobby>();

            currentLobbySC.currentLobby = lobby;
            currentLobbySC.thisPlayer = options.Player;
            DontDestroyOnLoad(currentLobbyGO);
            LoadLobbyRoom();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public static void LoadLobbyRoom()
    {
        SceneManager.LoadScene(1);
        Debug.Log("Lobby");
    }

    public static void ReturnMainMenu()
    {
        Debug.Log("ReturnMenu");
        SceneManager.LoadScene(0);
    }

    public static void Loading()
    {
        Debug.Log("Loading");
        SceneManager.LoadScene(2);
    }

    public static void StartGame()
    {
        Debug.Log("StartGame");
        SceneManager.LoadScene(3);
    }
}
