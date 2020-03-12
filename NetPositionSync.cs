using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetPositionSync : Photon.MonoBehaviour {

    public CharacterController characterController;
    public PlayerGO playerGO;
    float multipier = 10f;
    float spinemultipier = 12f;
    Vector3 targetOrigin;
    Quaternion targetRotation;
    Quaternion targetSpineRotation;
    Quaternion lastSpineRotation;

    void LateUpdate () {
       if (!photonView.isMine)
        {
            transform.position = Vector3.Lerp(transform.position, targetOrigin, multipier * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, multipier * Time.deltaTime);
            transform.GetComponent<FPSController>().spine.localRotation = Quaternion.Slerp(lastSpineRotation, targetSpineRotation, spinemultipier * Time.deltaTime);
            lastSpineRotation = transform.GetComponent<FPSController>().spine.localRotation;

            playerGO.audioTR.rotation = Quaternion.identity;
        }
        else {
            playerGO.playerAnim.SetFloat("Speed", characterController.velocity.sqrMagnitude);
            playerGO.playerAnim.SetFloat("SpeedX", transform.InverseTransformVector(characterController.velocity).x);
            playerGO.playerAnim.SetFloat("SpeedY", transform.InverseTransformVector(characterController.velocity).z);
            Player.myPlayer.ping = PhotonNetwork.GetPing();
        }

    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo pmi)
    {
        if(photonView.isMine) //gracz nalezy do mnie, wysylam innym moja pozycje
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(transform.GetComponent<FPSController>().spine.localRotation);

            stream.SendNext(playerGO.playerAnim.GetFloat("Speed"));
            stream.SendNext(playerGO.playerAnim.GetFloat("SpeedX"));
            stream.SendNext(playerGO.playerAnim.GetFloat("SpeedY"));
            stream.SendNext(playerGO.playerAnim.GetFloat("WeaponRunSpeed"));
            stream.SendNext(playerGO.playerAnim.GetInteger("State"));
            stream.SendNext(Player.myPlayer.ping);
        }
        else
        { //Gracz nie nalezy do mnie, odbieram pozycje i ustawiam ja
            targetOrigin = (Vector3)stream.ReceiveNext();
            targetRotation = (Quaternion)stream.ReceiveNext();
            targetSpineRotation = (Quaternion)stream.ReceiveNext();

            playerGO.playerAnim.SetFloat("Speed", (float)stream.ReceiveNext());
            playerGO.playerAnim.SetFloat("SpeedX", (float)stream.ReceiveNext());
            playerGO.playerAnim.SetFloat("SpeedY", (float)stream.ReceiveNext());
            playerGO.playerAnim.SetFloat("WeaponRunSpeed", (float)stream.ReceiveNext());
            playerGO.playerAnim.SetInteger("State", (int)stream.ReceiveNext());
            Player.FindPlayer(pmi.sender).ping = (int)stream.ReceiveNext();
        }
    }
}
