using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Manager : NetworkBehaviour
{
    [SerializeField]
    private CardsSO cards;

    public string who;
    public string how;
    public string where;
    List<string> whoList;
    List<string> howList;
    List<string> whereList;

    List<string> remainingCards;

    CurrentLobby currentLobby;

    [SerializeField]
    public NetworkVariable<int> playerCount = new NetworkVariable<int>(0);
    public int stringPerPlayer;

    [SerializeField]
    public GameObject[] playerArr;

    public int firstPlayer;

    public PopUpMenuSystem popUpMenuSystem;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
            playerArr = GameObject.FindGameObjectsWithTag("Player");
            StartCoroutine(PopUpMenuInit());
            PrepareGame();
        }
    }

    IEnumerator PopUpMenuInit()
    {
        yield return new WaitUntil(() => popUpMenuSystem.IsSpawned);
        WhosGoFirst();
    }

    void PrepareGame()
    {
        //Prepare Game
        whoList = cards.Who.ToList();
        howList = cards.How.ToList();
        whereList = cards.Where.ToList();

        who = whoList[Random.Range(0, whoList.Count)];
        whoList.Remove(who);
        how = howList[Random.Range(0, howList.Count)];
        howList.Remove(how);
        where = whereList[Random.Range(0, whereList.Count)];
        whereList.Remove(where);

        remainingCards = whoList.Concat(howList).Concat(whereList).ToList();

        playerCount.Value = currentLobby.currentLobby.Players.Count;
        stringPerPlayer = remainingCards.Count / playerCount.Value;

        //
        //Dealing cards

        List<List<string>> playersStringLists = SelectRandomStringsForPlayers(
            playerCount.Value,
            stringPerPlayer
        );

        // Sonuçları göstermek için her oyuncunun listesine bakıyoruz
        for (int i = 0; i < playersStringLists.Count; i++)
        {
            Debug.Log($"Player {i + 1}: {string.Join(" ", playersStringLists[i])}");
        }

        int Index = 0;
        foreach (GameObject player in playerArr)
        {
            player
                .GetComponent<PlayerCards>()
                .SendCardsClientRpc(string.Join(",", playersStringLists[Index]));
            Index++;
        }
        SendFinalCardsClientRpc(who, how, where);
    }

    [ClientRpc]
    public void SendFinalCardsClientRpc(string finalWho, string finalHow, string finalWhere)
    {
        who = finalWho;
        how = finalHow;
        where = finalWhere;
    }

    public void WhosGoFirst(bool nextPlayer = false)
    {
        if (!nextPlayer)
        {
            // firstPlayer = Random.Range(0, playerCount);
            firstPlayer = 0;
        }
        else
        {
            firstPlayer++;
            if (firstPlayer >= playerArr.Length || playerArr[firstPlayer] == null)
            {
                firstPlayer = 0;
            }
        }
        foreach (var item in currentLobby.currentLobby.Players)
        {
            // Eğer bu oyuncu seçilen oyuncuysa
            if (item.Id == playerArr[firstPlayer].name)
            {
                if (item.Id == currentLobby.currentLobby.HostId)
                {
                    popUpMenuSystem.popUp("Sıra Sende", true);
                }
                else
                {
                    popUpMenuSystem.popUpClientRpc(item.Id, "Sıra Sende", true);
                }
            }
            else // Eğer bu oyuncu seçilen oyuncu değilse
            {
                if (item.Id == currentLobby.currentLobby.HostId)
                {
                    popUpMenuSystem.popUp("Bekleyiniz", false);
                }
                else
                {
                    Debug.Log(item.Id);
                    popUpMenuSystem.popUpClientRpc(item.Id, "Bekleyiniz", false);
                }
            }
        }
    }

    [ServerRpc]
    public void EleminatePlayerServerRpc(string playerId)
    {
        EleminatePlayer(playerId);
    }

    public void EleminatePlayer(string playerId)
    {
        int playerIndex = System.Array.FindIndex(
            playerArr,
            player => player != null && player.name == playerId
        );

        if (playerIndex != -1)
        {
            playerArr[playerIndex].GetComponent<PlayerCards>().isPlayerEleminated = true;
        }
        else
        {
            // Oyuncu bulunamadıysa hata mesajı gösterebilirsiniz
            Debug.LogError($"Oyuncu '{playerId}' bulunamadı!");
        }
    }

    List<List<string>> SelectRandomStringsForPlayers(int playerCount, int stringPerPlayer)
    {
        List<List<string>> playersStringLists = new List<List<string>>();

        // Oyuncu sayısı kadar döngü ile her oyuncu için seçim yapıyoruz
        for (int i = 0; i < playerCount; i++)
        {
            List<string> selectedStrings = new List<string>();

            // Belirli sayıda rastgele seçim yapıyoruz
            for (int j = 0; j < stringPerPlayer && remainingCards.Count > 0; j++)
            {
                int randomIndex = Random.Range(0, remainingCards.Count);
                selectedStrings.Add(remainingCards[randomIndex]);
                remainingCards.RemoveAt(randomIndex); // Seçilen elemanı ana listeden çıkar
            }

            playersStringLists.Add(selectedStrings);
        }

        return playersStringLists;
    }
}
