using UnityEngine;

public class PlayerImaageSetter : MonoBehaviour
{
    CurrentLobby currentLobby;

    void Start()
    {
        //Instantiate playerCard and Player Image
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
        foreach (var item in currentLobby.currentLobby.Players) { }
    }
}
