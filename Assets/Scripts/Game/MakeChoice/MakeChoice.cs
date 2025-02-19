using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MakeChoice : NetworkBehaviour
{
    public TMP_Text roomText;
    public TMP_Text currentWhoCardText;
    public TMP_Text currentHowCardText;
    public GameObject CardsContiner;

    public bool isWhoPanel = true;
    public string who;
    public string how;
    public string where;

    public Button makeChoiceButton;

    public override void OnNetworkSpawn()
    {
        AssignButton(CardsContiner, "CardButton");
    }

    public void ChoiceRoom(string room)
    {
        roomText.text = room;
        where = room;
    }

    public void ChoiceButton(string name)
    {
        if (isWhoPanel)
        {
            who = name;
            currentWhoCardText.text = name;
        }
        else
        {
            how = name;
            currentHowCardText.text = name;
        }

        if (who != string.Empty && how != string.Empty)
        {
            makeChoiceButton.interactable = true;
        }
    }

    public void ActivePage(Animator animator)
    {
        animator.SetBool("ActivePanel", true);
        isWhoPanel = !isWhoPanel;
    }

    public void PassivePage(Animator animator)
    {
        animator.SetBool("ActivePanel", false);
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
}
