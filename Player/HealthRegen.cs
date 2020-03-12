using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthRegen : MonoBehaviour {

    Player player;
    bool hurtSound = false;
    bool veryHurt = false;
    float newHealth;
    float oldhealth;

    AudioSource localAS;
    public AudioClip[] hurtBreathSounds;
    public AudioClip[] betterBreathSounds;
    const float regenRate = 0.1f;
    const float healthOverlayCutoff = 0.35f;
    const int regularRegenDelay = 5000; //5sek
    int lastSoundTimeRecover = 0;
    int hurtTime = 0;

    void Start () {

        player = Player.myPlayer;
        oldhealth = player.maxhealth;
        localAS = GetComponent<PlayerGO>().localAS;

    }

    void Update () {

        if (player.Health == player.maxhealth || player.Health <= 0)
        {
            veryHurt = false;
            return;
        }

        bool wasVeryHurt = veryHurt;
        float ratio = player.Health / player.maxhealth;

        if (ratio <= healthOverlayCutoff)
        {
            veryHurt = true;
            if(!hurtSound) StartCoroutine(playBreathingSound());
            if (!wasVeryHurt)
            {
                PlayerUI.bloodDefocus.CrossFadeAlpha(1f, .2f, false);
                hurtTime = gettime();
            }
        }

        if (player.Health >= oldhealth)
        {
            if (gettime() - hurtTime < regularRegenDelay)
                return;

            if (gettime() - lastSoundTimeRecover > regularRegenDelay)
            {
                lastSoundTimeRecover = gettime();
                localAS.PlayOneShot(betterBreathSounds[Random.Range(0, betterBreathSounds.Length)]);
                PlayerUI.bloodDefocus.CrossFadeAlpha(0, 3f, false);
            }     

            if (veryHurt)
            {
                newHealth = ratio;
                if (gettime() > hurtTime + 3000)
                    newHealth += regenRate;
            }
            else
                newHealth = 1;

            if (newHealth > 1.0f)
                newHealth = 1.0f;

            if (newHealth <= 0)
                return;

            GameManager.instance.photonView.RPC("addPlayerHealthRPC", PhotonTargets.Others, Player.myPlayer.photonPlayer, (int)(newHealth * player.maxhealth));
            player.Health = (int)(newHealth * player.maxhealth);
            oldhealth = player.Health;
            return;
        }
        if(!veryHurt && player.Health <= 85f) PlayerUI.bloodDefocus.CrossFadeAlpha(1.2f-ratio, .1f, false);
        oldhealth = player.Health;
        hurtTime = gettime();
    }

    IEnumerator playBreathingSound()
    {
        hurtSound = true;
        localAS.PlayOneShot(hurtBreathSounds[Random.Range(0, hurtBreathSounds.Length)]);
        yield return new WaitForSeconds(0.784f);
        yield return new WaitForSeconds(0.3f + Random.Range(0,0.8f));
        hurtSound = false;
    }

    int gettime()
    {
        return (int)(Time.fixedTime * 1000);
    }
}


