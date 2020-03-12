using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour {

    public static ChatManager instance;
    

    void Start() {
        instance = this;
    }

    [PunRPC]
    void recieveMessageRPC(string message, PhotonMessageInfo pmi) {
        Player sender = Player.FindPlayer(pmi.sender);
        drawMessage(sender.nick, message);
    }

    public void sendMessage(string message) {
        GetComponent<PhotonView>().RPC("recieveMessageRPC", PhotonTargets.All, message);
    }

    void drawMessage(string playerName, string message) {
        GameObject itemGO = Instantiate<GameObject>(PlayerUI.instance.chatMessageItem, PlayerUI.instance.chatMessageItem.transform, false);
        ChatMessageUI item = itemGO.GetComponent<ChatMessageUI>();

        if (item != null) {
            item.Setup(playerName, message);
        }
    }
}
