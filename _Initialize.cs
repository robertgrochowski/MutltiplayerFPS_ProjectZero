using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Initialize : MonoBehaviour {
    static bool initialized = false;
	void Awake () {
        if (!initialized)
        {
            if (GameSettings.gameSettings == null)
                GameSettings.loadSettings();

            Object[] weapons = Resources.LoadAll("Weapons", typeof(Weapon));
            foreach (Weapon weapon in weapons)
                weapon.initalizeWeapon();

            SoundsManager.Initialize(); 
            PlayerUI.Initialize();

            initialized = true;
        }
    }
}
