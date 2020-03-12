using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreboardItem : MonoBehaviour {

    [SerializeField]
    Text playernameText;
    [SerializeField]
    Text killsText;
    [SerializeField]
    Text deathsText;
    [SerializeField]
    Text pingText;
    [SerializeField]
    Text KDRatioText;
    [SerializeField]
    Text scoreText;

    public void Setup (string username, int kills, int deaths, int score, int ping, Color color) {
        playernameText.text = username;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
        KDRatioText.text = (deaths == 0 ? kills : (kills / deaths)).ToString();
        scoreText.text = score.ToString();
        pingText.text = ping.ToString();
        GetComponent<Image>().color = color;
    }
    
}
