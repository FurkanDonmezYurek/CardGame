using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InterGuess : NetworkBehaviour
{
    public MakeChoice makeChoice;
    public TMP_Text WhoText;
    public TMP_Text HowText;
    public TMP_Text WhereText;
    public List<string> interGuessList;
    public string whosStarted;

    public GameObject makeChoicePanel;
    public GameObject InterGuessPanel;
    public GameObject MainUIPanel;
    public GameObject InterGuessHeader;

    // public PopUpMenuSystem popUpMenuSystem;
    public GameObject popUpBanner;

    public GameObject guessButtonContainer;
    public GameObject guessButtonPref;
    public List<Button> guessButtonList;

    CurrentLobby currentLobby;
    Manager manager;
    GameObject currentPlayer;
    PlayerCards playerCards;

    public bool next = false;

    public GameObject goButton;
    public GameObject waitPanel;

    NetworkVariable<int> waitingPlayerCount = new NetworkVariable<int>(0);
    public bool contiune;

    public GameObject ListPanelPlayerList;
    public List<Image> guessPanelPlayerImageList;

    public GameObject finalGuessButton;
    public GameObject waitPanelGoButton;

    public FinalGuess finalGuess;
    public GameObject finalGuessCurrentPanel;
    public GameObject finalGuessPanel;

    public PopUpMenuSystem popUpMenuSystem;

    public override void OnNetworkSpawn()
    {
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
        manager = GameObject.Find("GameManager").GetComponent<Manager>();
        currentPlayer = GameObject.Find(currentLobby.thisPlayer.Id);
        playerCards = currentPlayer.GetComponent<PlayerCards>();
        StartCoroutine(InstantiateGuessButton());

        finalGuess = GetComponent<FinalGuess>();
    }

    private void Update()
    {
        //Final Guessde Açık objeyi current panel yap
        if (finalGuessPanel.activeSelf)
        {
            finalGuessCurrentPanel = null; // Varsayılan olarak null yap

            // Tüm çocukları döngüyle kontrol et
            foreach (Transform child in finalGuessPanel.transform)
            {
                if (child.gameObject.activeSelf) // Eğer çocuk aktifse
                {
                    finalGuessCurrentPanel = child.gameObject; // currentPanel'e ata
                    break; // İlk aktif çocuğu bulduktan sonra döngüden çık
                }
            }
        }
    }

    public void FinishMakeChoiceButton()
    {
        FinishMakeChoice(makeChoice.who, makeChoice.how, makeChoice.where);
        if (IsHost)
        {
            whosStarted = currentLobby.thisPlayer.Id;
            StartCoroutine(WhosNext());
            SendGuessClientRpc(makeChoice.who, makeChoice.how, makeChoice.where, whosStarted);
        }
        else
        {
            SendGuessServerRpc(
                makeChoice.who,
                makeChoice.how,
                makeChoice.where,
                currentLobby.thisPlayer.Id
            );
        }

        ResetMakeChoicePanel();
    }

    void ResetMakeChoicePanel()
    {
        makeChoicePanel.SetActive(false);
        MakeChoice makeChoice = GetComponent<MakeChoice>();
        makeChoice.roomText.text = string.Empty;
        makeChoice.currentWhoCardText.text = string.Empty;
        makeChoice.currentHowCardText.text = string.Empty;
        makeChoice.who = string.Empty;
        makeChoice.how = string.Empty;
        makeChoice.where = string.Empty;
    }

    void FinishMakeChoice(string who, string how, string where)
    {
        WhoText.text = who;
        HowText.text = how;
        WhereText.text = where;
        interGuessList.Add(who);
        interGuessList.Add(how);
        interGuessList.Add(where);
        if (popUpBanner.activeSelf)
        {
            Animator animator = popUpBanner.GetComponent<Animator>();
            animator.SetTrigger("PopOut");
            StartCoroutine(OpenInterGuess());
        }
        else
        {
            MainUIPanel.SetActive(false);
            InterGuessPanel.SetActive(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendGuessServerRpc(string who, string how, string where, string playerId)
    {
        whosStarted = playerId;
        StartCoroutine(WhosNext());
        if (whosStarted != currentLobby.thisPlayer.Id)
        {
            popUpMenuSystem.popOutCommon = true;
        }
        FinishMakeChoice(who, how, where);
        SendGuessClientRpc(who, how, where, whosStarted);
    }

    [ClientRpc]
    public void SendGuessClientRpc(string who, string how, string where, string id)
    {
        if (id != currentLobby.thisPlayer.Id)
        {
            popUpMenuSystem.popOutCommon = true;
        }
        FinishMakeChoice(who, how, where);
    }

    IEnumerator OpenInterGuess()
    {
        yield return new WaitUntil(() => popUpBanner.activeSelf == false);
        MainUIPanel.SetActive(false);
        InterGuessPanel.SetActive(true);
    }

    IEnumerator InstantiateGuessButton()
    {
        if (IsHost)
        {
            yield return new WaitUntil(() => playerCards.Cards.Count <= 0);
        }

        foreach (var item in playerCards.Cards)
        {
            GameObject currentGuessButton = Instantiate(
                guessButtonPref,
                Vector3.zero,
                Quaternion.identity
            );

            currentGuessButton.transform
                .GetChild(0)
                .gameObject.GetComponent<TextMeshProUGUI>()
                .text = item;

            currentGuessButton.name = item;
            Button buttonUI = currentGuessButton.GetComponent<Button>();
            guessButtonList.Add(buttonUI);

            buttonUI.onClick.AddListener(() => SendCardServerRpc(item, currentLobby.thisPlayer.Id));
            buttonUI.onClick.AddListener(() => OnOffButton(false));

            buttonUI.interactable = false;

            var recTransform = currentGuessButton.GetComponent<RectTransform>();
            recTransform.SetParent(guessButtonContainer.transform);
        }
    }

    IEnumerator WhosNext()
    {
        contiune = true;
        int currentValue = manager.firstPlayer;
        for (int i = 0; i <= manager.playerCount.Value - 1; i++)
        {
            currentValue++;
            if (currentValue == manager.playerCount.Value)
            {
                currentValue = 0;
            }
            string currentPlayerId = manager.playerArr[currentValue].name;
            if (currentValue != manager.firstPlayer && contiune)
            {
                SendNextClientRpc(currentPlayerId);
                yield return new WaitUntil(() => next);
                next = false;
            }
            else
            {
                FinishInterGuessClientRpc();
                StartCoroutine(WaitPlayers());
            }
        }
    }

    [ClientRpc]
    public void SendNextClientRpc(string id)
    {
        if (id == currentLobby.thisPlayer.Id)
        {
            if (!playerCards.isPlayerEleminated)
            {
                StartCoroutine(UplaodGuessList());
            }
            else
            {
                List<string> currentCards = new List<string>();
                for (int i = 0; i == interGuessList.Count - 1; i++)
                {
                    if (playerCards.Cards.Contains(interGuessList[i]))
                    {
                        currentCards.Add(interGuessList[i]);
                    }
                    if (i == interGuessList.Count - 1)
                    {
                        if (currentCards.Count != 0)
                        {
                            int sendCardIndex = Random.Range(0, currentCards.Count);
                            SendCardServerRpc(
                                currentCards[sendCardIndex],
                                currentLobby.thisPlayer.Id
                            );
                        }
                        else
                        {
                            SendCardServerRpc(string.Empty, currentLobby.thisPlayer.Id);
                        }
                    }
                }
            }
        }
    }

    IEnumerator UplaodGuessList()
    {
        yield return new WaitUntil(() => interGuessList.Count > 0);
        OnOffButton(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendCardServerRpc(string cardName, string id)
    {
        SendCardsClientRpc(cardName, id, whosStarted);
    }

    [ClientRpc]
    public void SendCardsClientRpc(string cardName, string cardPlayerId, string startPlayerId)
    {
        whosStarted = startPlayerId;
        AssingCardtoList(cardName, cardPlayerId, startPlayerId);
    }

    void AssingCardtoList(string cardName, string cardPlayerId, string startPlayerId)
    {
        string playerImageName = "PlayerImage" + cardPlayerId;
        GameObject playerImage = FindChildByName(InterGuessHeader, playerImageName);
        Debug.Log(playerImage);
        Image image = playerImage.GetComponent<Image>();
        guessPanelPlayerImageList.Add(image);
        next = true;

        if (cardName != string.Empty)
        {
            image.color = Color.green;
            contiune = false;
            if (startPlayerId == currentLobby.thisPlayer.Id)
            {
                //Assing to List
                string playerListName = "PlayerCard" + cardPlayerId;
                GameObject playerCard = FindChildByName(ListPanelPlayerList, playerListName);
                GameObject cardButton = FindChildByName(playerCard, cardName);
                Button buttonUI = cardButton.GetComponent<Button>();
                cardButton.GetComponent<Image>().color = Color.green;
                buttonUI.interactable = false;
            }
        }
        else
        {
            image.color = Color.red;
            if (startPlayerId == currentLobby.thisPlayer.Id)
            {
                //Assing to List
                string playerListName = "PlayerCard" + cardPlayerId;
                GameObject playerCard = FindChildByName(ListPanelPlayerList, playerListName);
                for (int i = 0; i < interGuessList.Count; i++)
                {
                    GameObject cardButton = FindChildByName(playerCard, interGuessList[i]); // interguessListteki kartlara listede çizik at
                    Button buttonUI = cardButton.GetComponent<Button>();
                    cardButton.GetComponent<Image>().color = Color.red;
                    buttonUI.interactable = false;
                }
            }
        }
    }

    [ClientRpc]
    public void FinishInterGuessClientRpc()
    {
        goButton.SetActive(true);
    }

    void OnOffButton(bool onOff)
    {
        int interactableButtonCount = 0;
        foreach (var item in guessButtonList)
        {
            if (onOff)
            {
                string currentButtonName = item.gameObject.name;
                if (interGuessList.Contains(currentButtonName))
                {
                    item.interactable = true;
                }
                else
                {
                    interactableButtonCount++;
                }
            }
            else
            {
                item.interactable = false;
            }
        }
        if (interactableButtonCount == guessButtonList.Count)
        {
            SendCardServerRpc(string.Empty, currentLobby.thisPlayer.Id);
        }
    }

    public void GoWaitPanel()
    {
        //go Wait panel
        waitPanel.SetActive(true);

        if (whosStarted != currentLobby.thisPlayer.Id)
        {
            AddWaitingPlayerCount();
            waitPanelGoButton.SetActive(false);
            finalGuessButton.SetActive(false);
        }
        else
        {
            StartCoroutine(WhoStartedWaitPlayers());
        }

        //clear Inter Guess Panel
        for (int i = 0; i < guessPanelPlayerImageList.Count; i++)
        {
            guessPanelPlayerImageList[i].color = Color.white;
        }
        interGuessList.Clear();
    }

    public void AddWaitingPlayerCount()
    {
        if (IsHost)
        {
            waitingPlayerCount.Value++;
        }
        else
        {
            SendImWaitingServerRpc();
        }
    }

    IEnumerator WhoStartedWaitPlayers()
    {
        yield return new WaitUntil(() => waitingPlayerCount.Value == manager.playerCount.Value - 1);
        waitPanelGoButton.SetActive(true);
        finalGuessButton.SetActive(true);
    }

    public IEnumerator WaitPlayers()
    {
        yield return new WaitUntil(
            () =>
                waitingPlayerCount.Value == manager.playerCount.Value
                || finalGuess.finalGuessStarted.Value
        );

        // Eğer finalGuessStarted true olduysa coroutine'i sonlandır
        if (finalGuess.finalGuessStarted.Value)
        {
            waitingPlayerCount.Value = 0;
            finalGuess.finalGuessStarted.Value = false;
            yield break;
        }
        waitingPlayerCount.Value = 0;
        GoMainUIClientRpc();
        manager.WhosGoFirst(true);
    }

    [ClientRpc]
    public void GoMainUIClientRpc()
    {
        GoMainUI();
    }

    void GoMainUI()
    {
        waitPanel.SetActive(false);
        if (finalGuessCurrentPanel != null)
        {
            finalGuessCurrentPanel.SetActive(false);
        }
        MainUIPanel.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendImWaitingServerRpc()
    {
        waitingPlayerCount.Value++;
    }

    // Çocuklar arasında ismi belirtilen GameObject'i bulma fonksiyonu
    public GameObject FindChildByName(GameObject parent, string childName)
    {
        if (parent == null)
        {
            Debug.LogError("Parent GameObject boş! Lütfen geçerli bir GameObject sağlayın.");
            return null;
        }

        // Parent'in tüm çocuklarını döngüyle kontrol et
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.name == childName)
            {
                return child.gameObject;
            }

            // Eğer çocuk bir başka parent ise, onun da alt çocuklarını kontrol et
            GameObject found = FindChildByName(child.gameObject, childName);
            if (found != null)
            {
                return found;
            }
        }

        // Hiçbir çocuk eşleşmiyorsa null döndür
        return null;
    }
}
