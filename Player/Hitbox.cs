using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {

    public List<Rigidbody> ragdoll = new List<Rigidbody>();

    void Awake() {
        Activate(false, Vector3.zero, 0f);
    }

    public void Activate(bool active, Vector3 direction, float force) {

        GetComponent<CharacterController>().enabled = !active;
        
        for (int i = 0; i < ragdoll.Count; i++) {
            ragdoll[i].isKinematic = !active;
            ragdoll[i].GetComponent<Collider>().isTrigger = !active;

            ragdoll[i].AddForce(direction * force, ForceMode.Impulse);
        }

        if (active) {
            GetComponent<PlayerGO>().playerAnim.enabled = !active;
            var playerScripts = GetComponents<MonoBehaviour>();
             for (int i = 0; i < playerScripts.Length; i++) {
                if (playerScripts[i] != this)
                    playerScripts[i].enabled = !active;
            }
        } 
    }
    
	
	
	void Update () {
		
	}
}
