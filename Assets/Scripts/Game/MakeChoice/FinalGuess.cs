using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;

public class FinalGuess : NetworkBehaviour
{
    InterGuess interGuess;
    CurrentLobby currentLobby;
    Manager manager;
    public Animator animator;
    int activePanel = 0;
    public string who;
    public string how;
    public string where;
    public NetworkVariable<bool> finalGuessStarted = new NetworkVariable<bool>(false);

    public GameObject CardsContiner;

    public Button makeChoiceButton;

    public TMP_Text currentWhoCardText;
    public TMP_Text currentHowCardText;
    public TMP_Text currentWhereCardText;

    public GameObject waitPanel;
    public GameObject finalWaitPanel;
    public GameObject makeFinalGuessPanel;
    public GameObject finalGuessPanel;

    public GameObject yourWhoImage;
    public GameObject yourHowImage;
    public GameObject yourWhereImage;

    public GameObject finalWhoImage;
    public GameObject finalHowImage;
    public GameObject finalWhereImage;

    public GameObject finalGuessButton;

    public override void OnNetworkSpawn()
    {
        interGuess = GetComponent<InterGuess>();
        manager = GetComponent<Manager>();
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
        AssignButton(CardsContiner, "CardButton");
    }

    public void SwitchPanel(int value)
    {
        activePanel += value;
        animator.SetInteger("ActivePanel", activePanel);
    }

    public void ChoiceButton(string name)
    {
        switch (activePanel)
        {
            case 0:
                who = name;
                currentWhoCardText.text = name;

                break;
            case 1:
                how = name;
                currentHowCardText.text = name;
                break;
            case 2:
                where = name;
                currentWhereCardText.text = name;
                break;
        }

        if (who != string.Empty && how != string.Empty)
        {
            makeChoiceButton.interactable = true;
        }
    }

    void AssignButton(GameObject parent, string childName)
    {
        foreach (Transform child in parent.transform)
        {
            // İlgili çocuğu kontrol et
            if (child.gameObject.name == childName)
            {
                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    TextMeshProUGUI textComponent = child
                        .GetChild(0)
                        .GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        string cardName = textComponent.text;
                        button.onClick.AddListener(() => ChoiceButton(cardName));
                    }
                    else
                    {
                        Debug.LogWarning($"{childName} altında TextMeshProUGUI bulunamadı.");
                    }
                }
                else
                {
                    Debug.LogWarning($"{childName} bir Button bileşeni içermiyor.");
                }
            }
            else
            {
                // Alt nesnelerde arama yap
                if (child.childCount > 0)
                {
                    AssignButton(child.gameObject, childName);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwitchBoolServerRpc()
    {
        finalGuessStarted.Value = !finalGuessStarted.Value;
    }

    public void GoFinalGuessButton()
    {
        TogglePanels(waitPanel, makeFinalGuessPanel);
        if (IsHost)
        {
            GoFinalGuessClientRpc();
        }
        else
        {
            GoFinalGuessServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GoFinalGuessServerRpc()
    {
        GoFinalGuessClientRpc();
    }

    [ClientRpc]
    public void GoFinalGuessClientRpc()
    {
        if (interGuess.whosStarted != currentLobby.thisPlayer.Id)
        {
            TogglePanels(waitPanel, finalWaitPanel);
        }
    }

    public void FinishFinalGuess()
    {
        void HandleCorrectGuess()
        {
            Debug.Log("Doğru Bildin");
            ShowFinalCard(manager.who, manager.how, manager.where);
        }

        void HandleIncorrectGuess()
        {
            Debug.Log("Yanlış Bildin");
            if (IsHost)
                manager.EleminatePlayer(currentLobby.thisPlayer.Id);
            else
                manager.EleminatePlayerServerRpc(currentLobby.thisPlayer.Id);

            ShowFinalCard(manager.who, manager.how, manager.where);
            UpdateText(finalGuessButton, "Watch Game");
        }

        void TogglePanelsAndButtons()
        {
            TogglePanels(makeFinalGuessPanel, finalGuessPanel);
            if (IsHost)
                FinalGuessButtonClientRpc();
            else
                FinalGuessButtonServerRpc();
        }

        if (IsHost)
        {
            finalGuessStarted.Value = true;
            FinishFinalGuessClientRpc(who, how, where);

            if (manager.who == who && manager.how == how && manager.where == where)
                HandleCorrectGuess();
            else
                HandleIncorrectGuess();
        }
        else
        {
            SwitchBoolServerRpc();
            FinishFinalGuessServerRpc(who, how, where);

            if (manager.who == who && manager.how == how && manager.where == where)
                HandleCorrectGuess();
            else
                HandleIncorrectGuess();
        }

        TogglePanelsAndButtons();
    }

    public void ShowFinalCard(string finalWho, string finalHow, string finalWhere)
    {
        UpdateText(finalWhoImage, finalWho);
        UpdateText(finalHowImage, finalHow);
        UpdateText(finalWhereImage, finalWhere);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowFinalCardServerRpc(string finalWho, string finalHow, string finalWhere)
    {
        ShowFinalCardClientRpc(finalWho, finalHow, finalWhere);
    }

    [ClientRpc]
    public void ShowFinalCardClientRpc(string finalWho, string finalHow, string finalWhere)
    {
        ShowFinalCard(finalWho, finalHow, finalWhere);
        UpdateText(finalGuessButton, "Game Over");
    }

    [ServerRpc(RequireOwnership = false)]
    public void FinalGuessButtonServerRpc()
    {
        FinalGuessButtonClientRpc();
    }

    [ClientRpc]
    public void FinalGuessButtonClientRpc()
    {
        if (interGuess.whosStarted != currentLobby.thisPlayer.Id)
        {
            UpdateText(finalGuessButton, "Return Game");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void FinishFinalGuessServerRpc(string whoRpc, string howRpc, string whereRpc)
    {
        FinishFinalGuessClientRpc(whoRpc, howRpc, whereRpc);
    }

    [ClientRpc]
    public void FinishFinalGuessClientRpc(string whoRpc, string howRpc, string whereRpc)
    {
        UpdateText(yourWhoImage, whoRpc);
        UpdateText(yourHowImage, howRpc);
        UpdateText(yourWhereImage, whereRpc);
        if (interGuess.whosStarted != currentLobby.thisPlayer.Id)
        {
            TogglePanels(finalWaitPanel, finalGuessPanel);
        }
    }

    private void TogglePanels(GameObject fromPanel, GameObject toPanel)
    {
        if (fromPanel != null)
            fromPanel.SetActive(false);
        if (toPanel != null)
            toPanel.SetActive(true);
    }

    private void UpdateText(GameObject target, string text)
    {
        if (target != null)
        {
            var textComponent = target.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }
    }

    public void CancelButton()
    {
        ClearFinalGuess();
    }

    public void ReturnButton()
    {
        ClearFinalGuess();
        TogglePanels(finalGuessPanel, finalWaitPanel);
        if (IsHost)
        {
            StartCoroutine(interGuess.WaitPlayers());
        }
    }

    void ClearFinalGuess()
    {
        activePanel = 0;
        who = string.Empty;
        how = string.Empty;
        where = string.Empty;
        interGuess.AddWaitingPlayerCount();
    }
}
