using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class CreateLobby : MonoBehaviour
{
    public TMP_InputField lobbyName;
    public TMP_Dropdown maxPlayers;

    // public Toggle isLobbyPrivate;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        //for SignIn
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobbyFunc()
    {
        string lobbyname = lobbyName.text;
        int maxplayers = System.Convert.ToInt32(maxPlayers.options[maxPlayers.value].text);
        CreateLobbyOptions options = new CreateLobbyOptions();
        // options.IsPrivate = isLobbyPrivate.isOn;

        Color randomColor = new Color(Random.value, Random.value, Random.value);
        string hexRGBA = "#" + ColorUtility.ToHtmlStringRGBA(randomColor);

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
            { "readyCount", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "1") },
            {
                "RelayClientId",
                new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "")
            },
            {
                "PlayerColor",
                new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, hexRGBA)
            }
        };

        options.Data = new Dictionary<string, DataObject>()
        {
            { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, "") }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyname, maxplayers, options);

        GameObject currentLobbyGO = new GameObject("CurrentLobby", typeof(CurrentLobby));
        CurrentLobby currentLobbySC = currentLobbyGO.GetComponent<CurrentLobby>();

        currentLobbySC.currentLobby = lobby;
        currentLobbySC.thisPlayer = options.Player;
        DontDestroyOnLoad(currentLobbyGO);
        Debug.Log("Create Lobby Done");
        Debug.Log(currentLobbySC.currentLobby.LobbyCode);

        if (AuthenticationService.Instance.IsSignedIn)
        {
            JoinLobby.LoadLobbyRoom();
        }
    }
}
