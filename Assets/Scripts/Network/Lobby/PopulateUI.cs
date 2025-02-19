using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;

public class PopulateUI : NetworkBehaviour
{
    public int playerCount = 1;

    public TextMeshProUGUI lobbyName;

    public TextMeshProUGUI startButtonText;
    private CurrentLobby currentLobby;

    public GameObject playerCardPrefab;
    public GameObject playerListContainer;

    Player hostPlayer;
    int totalReadyCount = 0;
    public TMP_Text readyCountText;
    public Button startButton;
    public bool setReady = true;
    bool isHost;

    public TMP_Text lobbyCode;

    public List<string> playerIDs = new List<string>();

    private void Start()
    {
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
        lobbyCode.text = currentLobby.currentLobby.LobbyCode;
        PopulateUIElements();
        InvokeRepeating(nameof(UpdateLobby), 1.1f, 1f);

        if (isHost)
        {
            startButton.interactable = false;
        }
    }

    void PopulateUIElements()
    {
        lobbyName.text = currentLobby.currentLobby.Name;
        CleanerContainer();

        foreach (Player player in currentLobby.currentLobby.Players)
        {
            if (GameObject.Find(player.Id) == null)
            {
                CreatePlayerCard(player);
            }
            playerCount++;
            totalReadyCount += System.Convert.ToInt32(player.Data["readyCount"].Value);
            readyCountText.text = $"{totalReadyCount} / {currentLobby.currentLobby.Players.Count}";
            if (isHost)
            {
                if (totalReadyCount == currentLobby.currentLobby.Players.Count)
                {
                    startButton.interactable = true;
                }
                else
                {
                    startButton.interactable = false;
                }
            }
        }
        if (currentLobby.currentLobby.Players.Any(p => p.Id == currentLobby.thisPlayer.Id))
        {
            if (currentLobby.currentLobby.HostId == currentLobby.thisPlayer.Id)
            {
                startButtonText.text = "Başlat";
                isHost = true;
            }
            else
            {
                startButtonText.text = "Hazır";
                isHost = false;
                if (!isHost && currentLobby.currentLobby.Data["joinCode"].Value != "")
                {
                    JoinLobby.Loading();
                }
            }
        }
    }

    void CreatePlayerCard(Player player)
    {
        if (playerIDs.Contains(player.Id))
        {
            Destroy(GameObject.Find(player.Id));
        }
        else
        {
            playerIDs.Add(player.Id);
            GameObject card = Instantiate(playerCardPrefab, Vector3.zero, Quaternion.identity);
            card.name = player.Id;

            GameObject text = card.transform.GetChild(2).gameObject;
            text.GetComponent<TextMeshProUGUI>().text = player.Data["PlayerName"].Value;

            GameObject countText = card.transform.GetChild(0).gameObject;
            countText.GetComponent<TextMeshProUGUI>().text = "#" + playerCount;

            var recTransform = card.GetComponent<RectTransform>();
            recTransform.SetParent(playerListContainer.transform);

            Image playerColor = card.transform.GetChild(1).gameObject.GetComponent<Image>();
            if (ColorUtility.TryParseHtmlString(player.Data["PlayerColor"].Value, out Color color))
            {
                playerColor.color = color;
                Debug.Log("Renk başarıyla uygulandı.");
            }
            else
            {
                Debug.LogError("Geçersiz HEX kodu!");
            }
            if (isHost)
            {
                if (player.Id == currentLobby.thisPlayer.Id)
                {
                    card.transform.GetChild(3).gameObject.SetActive(false);
                }
                card.transform
                    .GetChild(3)
                    .GetComponent<Button>()
                    .onClick.AddListener(
                        delegate
                        {
                            KickLobby(player);
                        }
                    );
            }
            else
            {
                card.transform.GetChild(3).gameObject.SetActive(false);
            }
        }
    }

    async void UpdateLobby()
    {
        currentLobby.currentLobby = await LobbyService.Instance.GetLobbyAsync(
            currentLobby.currentLobby.Id
        );
        if (!currentLobby.currentLobby.Players.Any(p => p.Id == currentLobby.thisPlayer.Id))
        {
            Destroy(currentLobby.gameObject);
            JoinLobby.ReturnMainMenu();
        }
        PopulateUIElements();
    }

    // public void RefreshSettingsPanel()
    // {
    //     setLobbyName.text = "";
    //     lobbyNameText.text = lobbyName.text;
    //     maxPlayers.options[maxPlayers.value].text = Convert.ToString(
    //         currentLobby.currentLobby.MaxPlayers
    //     );
    // }


    private void CleanerContainer()
    {
        // if (playerListContainer is not null && playerListContainer.transform.childCount > 0)
        // {
        //     foreach (Transform item in playerListContainer.transform)
        //     {
        //         Destroy(item.gameObject);
        //         playerCount = 1;
        //         totalReadyCount = 0;
        //     }
        // }
        playerCount = 1;
        totalReadyCount = 0;
    }

    //ButtonEvents

    public void StartGame()
    {
        if (isHost)
        {
            if (totalReadyCount == currentLobby.currentLobby.Players.Count)
            {
                JoinLobby.Loading();
            }
        }
        else if (!isHost && setReady == true)
        {
            SetReady();
        }
    }

    public async void SetReady()
    {
        setReady = false;
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            options.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "readyCount",
                    new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, "1")
                }
            };
            currentLobby.currentLobby = await Lobbies.Instance.UpdatePlayerAsync(
                currentLobby.currentLobby.Id,
                currentLobby.thisPlayer.Id,
                options
            );
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void ExitLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(
                currentLobby.currentLobby.Id,
                currentLobby.thisPlayer.Id
            );
            Destroy(currentLobby.gameObject);
            JoinLobby.ReturnMainMenu();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void KickLobby(Player player)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.currentLobby.Id, player.Id);
            CreatePlayerCard(player);
            playerIDs.Remove(player.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // public async void SetJoinCode()
    // {
    //     try
    //     {
    //         UpdateLobbyOptions options = new UpdateLobbyOptions();
    //         options.Data = new Dictionary<string, DataObject>()
    //         {
    //             { "joinCode", new DataObject(DataObject.VisibilityOptions.Member, "") }
    //         };
    //         currentLobby.currentLobby = await Lobbies.Instance.UpdateLobbyAsync(
    //             currentLobby.currentLobby.Id,
    //             options
    //         );
    //     }
    //     catch (LobbyServiceException e)
    //     {
    //         Debug.LogError(e);
    //     }
    // }

    public void CopyToLobbyCode()
    {
        TextEditor te = new TextEditor();
        te.text = lobbyCode.text;
        te.SelectAll();
        te.Copy();
    }
}
