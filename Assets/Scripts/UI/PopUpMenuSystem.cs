using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopUpMenuSystem : NetworkBehaviour
{
    public GameObject currentButton;
    public GameObject popUpBox;

    public CurrentLobby currentLobby;
    public bool popOutCommon = false;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentLobby = GameObject.Find("CurrentLobby").GetComponent<CurrentLobby>();
    }

    public void CheckMark(GameObject popUpBox)
    {
        currentButton.GetComponent<Image>().color = Color.green;
        currentButton = null;
        popUpBox.SetActive(false);
    }

    public void CrossMark(GameObject popUpBox)
    {
        currentButton.GetComponent<Image>().color = Color.red;
        currentButton = null;
        popUpBox.SetActive(false);
    }

    [ClientRpc]
    public void popUpClientRpc(string id, string message, bool popOut)
    {
        if (id == currentLobby.thisPlayer.Id)
        {
            popUp(message, popOut);
        }
    }

    public void popUp(string message, bool popOut)
    {
        popUpBox.SetActive(true);
        TMP_Text banner = FindChildByName(popUpBox, "Message").GetComponent<TextMeshProUGUI>();
        banner.text = message;
        if (popUpBox.TryGetComponent<Animator>(out Animator animator))
        {
            if (popOut)
            {
                animator.SetTrigger("PopOut");
                popOutCommon = true;
                StartCoroutine(PopUpWait(animator));
            }
            else
            {
                StartCoroutine(PopUpWait(animator));
            }
        }
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

    IEnumerator PopUpWait(Animator animator)
    {
        // İlk olarak popOutCommon değişkeninin true olmasını bekle
        yield return new WaitUntil(() => popOutCommon == true);

        // PopOut animasyonunu tetikle
        animator.SetTrigger("PopOut");

        // Animasyonun "PopOut" durumuna geçmesini bekle
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("PopOut"))
        {
            yield return null; // Bir sonraki frame'e kadar bekle
        }

        // "PopOut" animasyonunun tamamlanmasını bekle
        yield return new WaitUntil(
            () =>
                animator.GetCurrentAnimatorStateInfo(0).IsName("PopOut")
                && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f
        );

        // İşlem tamamlandıktan sonra işlemleri sıfırla
        popOutCommon = false;
        popUpBox.SetActive(false);
    }
}
