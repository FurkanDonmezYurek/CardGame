using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    CurrentLobby currentLobby;
    public PopUpMenuSystem popUpMenuSystem;

    //list button and Instantiate playerCard
    public GameObject[] buttons;
    public GameObject popUp;
    public GameObject playerListPref;
    public GameObject playerListContainer;
    public GameObject playerImagePref;
    public Image playerImageList;
    public GameObject playerImageContainer;
    public GameObject playerImageContainerInterGuess;
    public GameObject ListUIPanel;

    //yourCard
    public GameObject yourCardTextPref;
    public GameObject yourCardPanel;
    PlayerCards playerCards;

    public void Start()
    {
        //Instantiate playerCard and Player Image
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
        foreach (var item in currentLobby.currentLobby.Players)
        {
            GameObject currentPlayer = Instantiate(
                playerListPref,
                Vector3.zero,
                Quaternion.identity
            );

            currentPlayer.name = "PlayerCard" + item.Id;
            var recTransform = currentPlayer.GetComponent<RectTransform>();
            recTransform.SetParent(playerListContainer.transform);

            // İlk Player Image objesi
            GameObject playerImage1 = Instantiate(
                playerImagePref,
                Vector3.zero,
                Quaternion.identity
            );

            // İkinci Player Image objesi
            GameObject playerImage2 = Instantiate(
                playerImagePref,
                Vector3.zero,
                Quaternion.identity
            );

            // Objeleri isimlendir
            playerImage1.name = "PlayerImage" + item.Id;
            playerImage2.name = "PlayerImage" + item.Id;

            // Parent ayarla
            playerImage1.transform.SetParent(playerImageContainer.transform, false);
            playerImage2.transform.SetParent(playerImageContainerInterGuess.transform, false);

            // Renk ayarla
            if (ColorUtility.TryParseHtmlString(item.Data["PlayerColor"].Value, out Color color))
            {
                playerImage1.GetComponent<Image>().color = color;
                playerImage2.GetComponent<Image>().color = color;
                currentPlayer.transform
                    .GetChild(0)
                    .GetChild(0)
                    .gameObject.GetComponent<Image>()
                    .color = color;
                playerImageList.color = color;
                Debug.Log("Renk başarıyla uygulandı.");
            }
            else
            {
                Debug.LogError("Geçersiz HEX kodu!");
            }

            if (item.Id == currentLobby.thisPlayer.Id)
            {
                currentPlayer.transform.SetSiblingIndex(0);
                playerImage1.transform.SetSiblingIndex(0);
                playerImage2.transform.SetSiblingIndex(0);
            }

            //yourCard

            if (item.Id == currentLobby.thisPlayer.Id)
            {
                playerCards = GameObject.Find(item.Id).GetComponent<PlayerCards>();
                for (int i = 0; i < playerCards.Cards.Count; i++)
                {
                    GameObject currentText = Instantiate(
                        yourCardTextPref,
                        Vector3.zero,
                        Quaternion.identity
                    );
                    currentText.GetComponent<TextMeshProUGUI>().text = playerCards.Cards[i];
                    var recTransformYourCard = currentText.GetComponent<RectTransform>();
                    recTransformYourCard.SetParent(yourCardPanel.transform);
                }
            }
        }

        //Button
        buttons = GameObject.FindGameObjectsWithTag("Button");
        foreach (GameObject button in buttons)
        {
            Button buttonUI = button.GetComponent<Button>();
            buttonUI.onClick.AddListener(
                delegate
                {
                    OpenPopUpMenu(button);
                }
            );

            foreach (var item in playerCards.Cards)
            {
                if (item == button.name)
                {
                    button.GetComponent<Image>().color = Color.black;
                    buttonUI.interactable = false;
                }
            }
        }
    }

    public void OpenPopUpMenu(GameObject button)
    {
        popUp.transform.position = button.transform.position + new Vector3(250, 0, 0f);
        popUpMenuSystem.currentButton = button;
        popUp.SetActive(true);
    }

    public void OpenCloseListUI(bool ListCase)
    {
        Animator animator = ListUIPanel.GetComponent<Animator>();
        animator.SetBool("IsOpen", ListCase);
    }
}
