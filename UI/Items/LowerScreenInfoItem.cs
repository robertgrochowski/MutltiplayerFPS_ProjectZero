using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LowerScreenInfoItem : MonoBehaviour {

    public InfoType messageType;

    [Header("Kill Info")]
    public Text killerText;
    public Text victimText;
    public Image weaponImage;

    [Header("Message Info")]
    public Text messageText;

    void Init() {
        setAlpha(0, 0);
        setAlpha(1, 0.4f);
        StartCoroutine(fadeAndDestroy());
    }

    public void SetupMessage(string message) {
        messageText.text = message;
        Init();
    }

    public void SetupKillInfo(string killer, string victim, AvailableWeapon weapon, MeansOfDeath meansOfDeath) {
        killerText.text = killer;
        victimText.text = victim;

        if (meansOfDeath == MeansOfDeath.falldamage)
        {
            weaponImage.sprite = PlayerUI.instance.fallDamageHudIcon;
            killerText.text = "";
        }
        else
            weaponImage.sprite = PlayerUI.weaponHudSprites[weapon];

        Init();
    }

    void setAlpha(float val, float time) 
    {
        if (messageType == InfoType.killinfo)
        {
            killerText.CrossFadeAlpha(val, time, false);
            victimText.CrossFadeAlpha(val, time, false);
            weaponImage.CrossFadeAlpha(val, time, false);
        }
        else if(messageType == InfoType.message)
        {
            messageText.CrossFadeAlpha(val, time, false);
        }
    }

    IEnumerator fadeAndDestroy() {
        yield return new WaitForSeconds(5f);
        setAlpha(0, 2f);
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }

    public enum InfoType
    {
        killinfo,
        message,
        suicide
    }
}
