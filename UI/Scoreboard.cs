using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Scoreboard : MonoBehaviour {

    public GameObject playerScoreboardItem;
    public Dictionary<Player, PlayerScoreboardItem> playerList;
    Coroutine updateListCoroutine;

    public Color playerRowColor;
    public Color myRowColor;


    void OnEnable() {
        playerList = new Dictionary<Player, PlayerScoreboardItem>();
        updateListCoroutine = StartCoroutine(updateList());
    }

    void OnDisable() {
        foreach(Transform child in gameObject.transform) {
            if (child.name == "Headers") continue;
            Destroy(child.gameObject);
            StopCoroutine(updateListCoroutine);
        }
    }

    IEnumerator updateList() {
        List<Player> sortedPlayerList = Player.players.OrderByDescending(o => o.score).ToList();
        while (true)
        {
            foreach (var player in sortedPlayerList)
            {
                PlayerScoreboardItem item;
                Color color;

                if (!playerList.ContainsKey(player))
                {
                    GameObject itemGO = Instantiate<GameObject>(playerScoreboardItem, gameObject.transform, false);
                    item = itemGO.GetComponent<PlayerScoreboardItem>();
                    
                    playerList[player] = item;
                }
                else item = playerList[player];

                color = player == Player.myPlayer ? myRowColor : playerRowColor;
                item.Setup(player.nick, player.kills, player.deaths, player.score, player.ping, color);
            }
            yield return 0;
        }
    }
}
