using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ConnectionManager : Photon.MonoBehaviour {

    void OnEnable()
    {
        //Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled. Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 0)
        {
            PhotonNetwork.JoinRandomRoom();
            GameObject.Find("Source").GetComponent<GameManager>().enabled = true;
        }
    }

    void OnPhotonRandomJoinFailed(){
        PhotonNetwork.CreateRoom(null);
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        if (PhotonNetwork.isMasterClient)
        {
            int team = 0;
            if (newPlayer != PhotonNetwork.masterClient) {

                if ((int)Player.players[Player.players.Count - 1].team % 2 == 0)
                    team = 1;
                else
                    team = 0;
            }
            else team = Random.Range(0, 2);

            photonView.RPC("PlayerConnected", PhotonTargets.All, newPlayer, team);
            photonView.RPC("newClientConnected", newPlayer);
        } 
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        if (PhotonNetwork.isMasterClient)
            photonView.RPC("PlayerDisconnected", PhotonTargets.AllBuffered, player);
    }

    [PunRPC]
    void newClientConnected() {
        Player.myPlayer = Player.FindPlayer(PhotonNetwork.player);
       // gameManager.spawnPlayer();
    }


    [PunRPC]
    void PlayerConnected(PhotonPlayer photonPlayer, int team, PhotonMessageInfo pmi) 
    {
        Player player = new Player();
        Player.players.Add(player);
        player.nick = photonPlayer.NickName;
        player.photonPlayer = photonPlayer;
        player.team = (Team)team;
        PlayerUI.drawLowerScreenMessage(player.nick + " has connected");

        if(photonPlayer != PhotonNetwork.player)//the player who has previously connected sends RPC to the new player
        { 

            int activeWeaponEnum = -1;
            int primaryWeaponEnum = -1;
            int secondaryWeaponEnum = -1;
            bool isWeaponHold = false;
            int photonViewID = -1;

            if (Player.myPlayer.gameObject != null)
            {
                activeWeaponEnum = (int)Player.myPlayer.gameObject.GetComponent<WeaponsManager>().activeWeapon.weapon.Enum;
                primaryWeaponEnum = (int)Player.myPlayer.gameObject.GetComponent<WeaponsManager>().primaryWeapon.weapon.Enum;
                secondaryWeaponEnum = (int)Player.myPlayer.gameObject.GetComponent<WeaponsManager>().secondaryWeapon.weapon.Enum;
                isWeaponHold = Player.myPlayer.gameObject.GetComponent<WeaponsManager>().activeWeapon.weapon.weaponType == Weapon.WeaponType.main;
                photonViewID = Player.myPlayer.gameObject.GetComponent<PhotonView>().viewID;
            }

            photonView.RPC("setPlayerVarriablesRPC", photonPlayer, PhotonNetwork.player,
                (int)Player.myPlayer.team,
                Player.myPlayer.nick,
                Player.myPlayer.kills,
                Player.myPlayer.deaths,
                photonViewID,
                isWeaponHold,
                activeWeaponEnum,
                primaryWeaponEnum,
                secondaryWeaponEnum); // sends RPC to the player who has connected
        }
    }
    [PunRPC]
    void setPlayerVarriablesRPC(PhotonPlayer photonPlayer, int team, string nick, int kills, int deaths, int photonViewID, bool isWeaponHold, int activeWeapon, int primaryWeapon, int secondaryWeapon) { //Wykonuje się dla gracza który się podłączył

        Player player = new Player();
        Player.players.Add(player);
        player.nick = photonPlayer.NickName;
        player.photonPlayer = photonPlayer;
        player.team = (Team)team;
        player.kills = kills;
        player.deaths = deaths;

        if (photonViewID != -1) /* Player has already spawned */
        {
            photonView.RPC("spawnPlayerRPC", PhotonNetwork.player, photonPlayer, photonViewID, primaryWeapon, secondaryWeapon); //Spawn for other players and myself
        }
    }
    [PunRPC]
    void PlayerDisconnected(PhotonPlayer photonPlayer)
    {
        PlayerUI.drawLowerScreenMessage(Player.FindPlayer(photonPlayer).nick + " has disconnected");
        Player.players.Remove(Player.FindPlayer(photonPlayer));
    }

    void OnCreatedRoom()
    {
        OnPhotonPlayerConnected(PhotonNetwork.player);
        //photonView.RPC("PlayerConnected", PhotonTargets.All, PhotonNetwork.player, team);
    }
}
