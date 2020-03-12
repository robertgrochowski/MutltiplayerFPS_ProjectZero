using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {

    public static Player myPlayer;
    public string nick = "";
    public PhotonPlayer photonPlayer;
    public GameObject gameObject;
    public WeaponClass weaponClass;
    public Team team;
    public bool inGame;
    public int kills;
    public int deaths;
    public int score;
    public int ping;
    private float health;
    public float maxhealth = 100;

    public static List<Player> players = new List<Player> ();

    public Player() {
       // myPlayer = this;
        Health = 100f;
        deaths = 0;
        kills = 0;
        team = Team.unassigned;
        inGame = false;
        score = 0;
        ping = 0;
    }

    public float Health {
        get {
            return health;
        }

        set {
            if (this == Player.myPlayer) {
                PlayerUI.health.text = "HP: "+value;
            }
            health = value;
        }
    }

    public static void Clear() {
        players.Clear();
        myPlayer = null;
    }

    public static void PlayersListDebug()
    {
         string debugStr = "Debug listy graczy! ilosc graczy: " + players.Count + " wszyscy gracze: ";

         for (int i = 0; i < players.Count; i++)
         {
             Player player = players[i];
             debugStr += "ID: ["+i+"] Nick: "+player.nick + ", Team:" + player.team +"\n";
         }
         Debug.Log(debugStr);
        Player.myPlayer.kills++;
        Debug.Log("kills: " + Player.myPlayer.kills);

    }
    public static Player FindPlayer(PhotonPlayer player)
    {
        foreach (var listPlayer in players)
        {
            if (listPlayer.photonPlayer == player)
            {
                return listPlayer;
            }
        }
        return null;
    }
}

public enum Team{ CT, TT, Both, unassigned }