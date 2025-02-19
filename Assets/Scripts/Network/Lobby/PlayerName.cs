using TMPro;
using UnityEngine;

public class PlayerName : MonoBehaviour
{
    public TMP_InputField playerName;
    public TMP_Text playerNameMenu;
    public GameObject playerProfile;

    void Start()
    {
        if (PlayerPrefs.GetString("PlayerName") == "")
        {
            playerProfile.SetActive(true);
        }
        else
        {
            playerNameMenu.text = PlayerPrefs.GetString("PlayerName");
            playerProfile.SetActive(false);
        }
    }

    public void SetPlayerName()
    {
        if (playerName.text != "")
        {
            PlayerPrefs.SetString("PlayerName", playerName.text);
            playerNameMenu.text = PlayerPrefs.GetString("PlayerName");
            playerProfile.SetActive(false);
        }
    }
}
