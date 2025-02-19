using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerCards : NetworkBehaviour
{
    public List<string> Cards;
    public bool isPlayerEleminated = false;

    [ClientRpc]
    public void SendCardsClientRpc(string cardText)
    {
        if (IsLocalPlayer)
        {
            Cards = new List<string>(cardText.Split(','));
        }
    }
}
