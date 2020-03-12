using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponsManager : MonoBehaviour {

    public static WeaponsManager instance;

    public WeaponInGame activeWeapon;
    public WeaponInGame primaryWeapon;
    public WeaponInGame secondaryWeapon;
    public bool isKnife = false;

    public GUIStyle guiSkin;
    public Camera mainCamera;
    public Transform kickGO;
    public FPSController FPSController;

    Coroutine weaponNameCoroutine;
    float reloadCooldown;
    float shootCooldown;
    float weaponTimeDrawn;
    bool drawWeaponName = false;
    bool isFiring = false;
    bool isFireHold = false;
    public bool weaponDisabled = false;

    public float inaccuracyIncreaseOverTime = 0.2f;
    public float inaccuracyDecreaseOverTime = 0.5f;
    private float maximumInaccuracy = 5.0f;

    private float triggerTime = 0.05f;
    private float baseInaccuracy = 1.5f;

    //CrossHair
    public Texture2D crosshairFirstModeHorizontal;
    public Texture2D crosshairFirstModeVertical;
    private float adjustMaxCroshairSize = 6.0f;

    //Aiming
    private bool forceReload = false;
    public bool aiming = false;
    public bool aimFinished = false;
    public bool aim = false;
    private Vector3 curVect;
    private float aimSpeed = 0.20f;
    public float backAnimScale = 1.1f;
    public int FOV = 40;
    public float camFOV = 60.0f;

    [Header("Bulletmarks")]
    public GameObject Concrete;
    public GameObject Wood;
    public GameObject Metal;
    public GameObject Dirt;
    public GameObject Untagged;
    public GameObject Body;

    [Header("Bullet Impact")]
    public AudioClip[] concreteImpact;
    public AudioClip[] grassImpact;
    public AudioClip[] woodImpact;
    public AudioClip[] dirtImpact;
    public AudioClip[] metalImpact;

    public AudioClip hitIndication;
    private GameObject activeWeaponGO;

    public Dictionary<string, string> hitboxPoints = new Dictionary<string, string>();
    public GameObject[] handsHips;
    public GameObject[] legsHips;
    public GameObject[] bodyHips;
    public GameObject[] headHips;

    private void Start(){
        instance = this;

        foreach(GameObject GO in bodyHips)  hitboxPoints.Add(GO.name, "body");
        foreach(GameObject GO in legsHips)  hitboxPoints.Add(GO.name, "leg");
        foreach(GameObject GO in handsHips) hitboxPoints.Add(GO.name, "hand");
        foreach(GameObject GO in headHips)  hitboxPoints.Add(GO.name, "head");
    }

    void OnGUI() {

        if (!PlayerUI.HUDEnabled) return;

        float w = crosshairFirstModeHorizontal.width;
        float h = crosshairFirstModeHorizontal.height;
        Rect position1 = new Rect((Screen.width + w) / 2 + (triggerTime * adjustMaxCroshairSize), (Screen.height - h) / 2, w, h);
        Rect position2 = new Rect((Screen.width - w) / 2, (Screen.height + h) / 2 + (triggerTime * adjustMaxCroshairSize), w, h);
        Rect position3 = new Rect((Screen.width - w) / 2 - (triggerTime * adjustMaxCroshairSize) - w, (Screen.height - h) / 2, w, h);
        Rect position4 = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2 - (triggerTime * adjustMaxCroshairSize) - h, w, h);

        if (!aiming) {
            GUI.DrawTexture(position1, crosshairFirstModeHorizontal);   //Right
            GUI.DrawTexture(position2, crosshairFirstModeVertical);     //Up
            GUI.DrawTexture(position3, crosshairFirstModeHorizontal);   //Left
            GUI.DrawTexture(position4, crosshairFirstModeVertical);     //Down
        }
    }

    int getDamageAmout(Weapon weapon, string hitpoint)
    {
        float multiplier = 1f;

        switch(hitpoint)
        {
            case "head":
                multiplier = 1.6f;
                break;
            case "leg":
                multiplier = 0.8f;
                break;
            case "hand":
                multiplier = 0.6f;
                break;
        }
        multiplier += Random.Range(-0.3f, 0.3f);
        return (int) (weapon.damage*multiplier);
    }

    void Reload() {
        
        if(aiming) {
            forceReload = true;
            doAim(false);
            return;
        }

        forceReload = false;
        activeWeapon.status = WeaponStatus.reloading;
        reloadCooldown = activeWeapon.weapon.reloadTime;
        PlayerGO playerGO = GetComponent<PlayerGO>();
        playerGO.weaponsReloadSoundsAS.PlayOneShot(SoundsManager.weaponsReloadSounds[activeWeapon.weapon.Enum]);
        playerGO.weaponHolderFP.Find(activeWeapon.weapon.name).GetComponent<Animation>().Play("reload");
        GetComponent<PhotonView>().RPC("setReloadAnim", PhotonTargets.All);
    }

    void doAim(bool Aim) {
        aim = true;
        aiming = Aim;
        if (Aim) curVect = activeWeapon.weapon.aimPosition - activeWeaponGO.transform.localPosition; //aim
        else curVect = activeWeapon.weapon.hipPosition - activeWeaponGO.transform.localPosition;
    }

    void Shoot() {

        /* Init */
        isFiring = true;
        activeWeapon.mag--;

        float bulletPrecision = 1f;

        /* Recoil, Kickback */
        kickGO.localRotation = Quaternion.Euler(kickGO.localRotation.eulerAngles - new Vector3(activeWeapon.weapon.kickup, Random.Range(-activeWeapon.weapon.recoil, activeWeapon.weapon.recoil), 0));
        WeaponSwing.kickBack = new Vector3(0, 0, -activeWeapon.weapon.kickback);

        /* Animations, Sounds */
        SoundsManager.instance.photonView.RPC("playShootSoundRPC", PhotonTargets.All);
        GetComponent<PhotonView>().RPC("setFiringAnim", PhotonTargets.All, true, activeWeapon.weapon.weaponType == Weapon.WeaponType.additional);

        if (activeWeapon.weapon.weaponType == Weapon.WeaponType.additional)
        {
            GetComponent<PlayerGO>().weaponHolderFP.Find(activeWeapon.weapon.name).GetComponent<Animation>().CrossFadeQueued("fire", 0.08f, QueueMode.PlayNow);
        }
            

        /* RayCast */
        Vector3 dir = Camera.main.transform.TransformDirection(new Vector3(Random.Range(-0.01f, 0.01f) * triggerTime * bulletPrecision, Random.Range(-0.01f, 0.01f) * triggerTime * bulletPrecision, 1));
        Ray ray = new Ray(Camera.main.transform.position, dir);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f).OrderBy(e => e.distance).ToArray(); 
        string hitpoint = "body";

        foreach (RaycastHit hit in hits) 
        {
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.blue, 1f);

            if (hit.transform.root == transform)
                continue;

            if (hitboxPoints.ContainsKey(hit.transform.gameObject.name))
                hitpoint = hitboxPoints[hit.transform.gameObject.name];
            else if (hit.transform.root.tag == "Player")
            {
                Debug.LogWarning("No hitbox on gameobject: " + hit.transform.gameObject.name);
                continue;
            }

            Vector3 contact = hit.point;
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            string tag = hit.collider.tag;
            Transform victim = hit.transform.root;
               
            if (victim.tag == "Player") {
                int damage = getDamageAmout(activeWeapon.weapon, hitpoint);
                GameManager.instance.photonView.RPC("makePlayerDamageRPC", PhotonTargets.All, victim.GetComponent<PlayerGO>().myPlayer.photonPlayer, (int)activeWeapon.weapon.Enum, (int)MeansOfDeath.kill, damage);
                GetComponent<PlayerGO>().localAS.PlayOneShot(hitIndication);
                StopCoroutine(fadeHitmark());
                StartCoroutine(fadeHitmark());
                tag = victim.tag;
            }

            GetComponent<PhotonView>().RPC("spawnImpactMarkRPC", PhotonTargets.All, tag, hit.point.x, hit.point.y, hit.point.z, hit.normal.x, hit.normal.y, hit.normal.z);
            break;
        }

        shootCooldown = activeWeapon.weapon.fireRate;
    }

    void Update() {
        if (activeWeapon == null)
            return;

        if (PlayerUI.isTyping || PlayerUI.activeMenu != null || weaponDisabled)
            return;

        if (activeWeaponGO == null) //todo
            activeWeaponGO = GetComponent<PlayerGO>().weaponHolderFP.Find(activeWeapon.weapon.name).gameObject;

        if (activeWeapon != null) { //Todo change on mag change
            PlayerUI.clipAmmo.text = activeWeapon.mag.ToString();
            PlayerUI.ammo.text = activeWeapon.ammo.ToString();
        }

       if (shootCooldown > 0f)
           shootCooldown -= Time.deltaTime;

        /* DEBUG ONLY!*/

        if(Input.GetKeyDown(KeyCode.H) && Application.isEditor)
        {   
           activeWeapon.weapon = GetComponent<PlayerGO>().weaponHolderFP.Find(activeWeapon.weapon.name).gameObject.GetComponent<Weapon>();
            
            Weapon.reloadWeaponProperties((AvailableWeapon)activeWeapon.id, activeWeapon.weapon);
            Debug.Log(activeWeapon.weapon.fireRate);
        }

        /* Fire */

        if (Input.GetMouseButton(0) && activeWeapon != null)
        {
            if (shootCooldown > 0 || isKnife || (activeWeapon.weapon.firingMode == Weapon.FiringMode.singleFire && isFireHold)) return;

            isFireHold = true;

            if (activeWeapon.status == WeaponStatus.ready && activeWeapon.mag > 0)
                Shoot();
            else if (activeWeapon.status != WeaponStatus.reloading && activeWeapon.ammo > 0)
            {
                Reload();
            }
        }
        else isFireHold = false;

        /* Fire Off */
        if (!Input.GetMouseButton(0) || activeWeapon.status != WeaponStatus.ready || activeWeapon.mag < 1) {
            if(isFiring) GetComponent<PhotonView>().RPC("setFiringAnim", PhotonTargets.All, false, false);
            isFiring = false;
        }
        /* Knife */
        if (Input.GetKeyDown(Controls.inputKeyCode[Controls.knifeKey]) && !isKnife) {
            isKnife = true;
            GetComponent<PlayerGO>().weaponHolderFP.Find("knife").gameObject.SetActive(true);
            GetComponent<PlayerGO>().weaponHolderFP.Find(activeWeapon.weapon.name).gameObject.SetActive(false);
            GetComponent<PlayerGO>().weaponsDrawSoundsAS.PlayOneShot(SoundsManager.weaponsDrawSounds[AvailableWeapon.knife]);
        }
        /* Change Weapon */
        else if (Input.GetKeyDown(KeyCode.Alpha1)){
            reloadCooldown = 0;
            GetComponent<PlayerGO>().weaponsReloadSoundsAS.Stop();

            if (primaryWeapon != null && activeWeapon.weapon != primaryWeapon.weapon){
                GetComponent<PhotonView>().RPC("changeWeaponRPC", PhotonTargets.All, (int)primaryWeapon.weapon.Enum, Player.myPlayer.photonPlayer);

                if(primaryWeapon.status == WeaponStatus.reloading) 
                    Reload();
            }
            drawWeaponName = true; weaponTimeDrawn = 4;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            reloadCooldown = 0;
            GetComponent<PlayerGO>().weaponsReloadSoundsAS.Stop();

            if (secondaryWeapon != null && activeWeapon.weapon != secondaryWeapon.weapon) {
                GetComponent<PhotonView>().RPC("changeWeaponRPC", PhotonTargets.All, (int)secondaryWeapon.weapon.Enum, Player.myPlayer.photonPlayer);

                if (secondaryWeapon.status == WeaponStatus.reloading)
                    Reload();
            }
            drawWeaponName = true; weaponTimeDrawn = 4;
        }
        /* Reload */
        else if (Input.GetKeyDown(KeyCode.R)) {
            if(activeWeapon.status != WeaponStatus.reloading && activeWeapon.ammo > 0 && activeWeapon.mag < activeWeapon.weapon.magCapacity) {
                Reload();
            }
        }
        /* Aim */
        else if (Input.GetMouseButtonDown(1) && !FPSController.instance.isRunning ) {
            doAim(!aiming);
        }
        else if (FPSController.instance.isRunning) doAim(false);

        if (activeWeapon.status != WeaponStatus.reloading && aim) { //
            Vector3 weaponloc = activeWeaponGO.transform.localPosition;
            aimFinished = false;

            if (weaponloc != activeWeapon.weapon.aimPosition && aiming) { //Zoom in
                if (Mathf.Abs(Vector3.Distance(weaponloc, activeWeapon.weapon.aimPosition)) <= curVect.magnitude / aimSpeed * Time.deltaTime) {
                    aimFinished = true;
                    activeWeaponGO.transform.localPosition = activeWeapon.weapon.aimPosition;
                }
                else
                    activeWeaponGO.transform.localPosition += curVect / aimSpeed * Time.deltaTime;

                mainCamera.fieldOfView -= FOV * Time.deltaTime / 0.5f;
                if (mainCamera.fieldOfView < FOV) {
                    mainCamera.fieldOfView = FOV;
                }
            }
            else if (!aiming) { //Zoom out
                if (Mathf.Abs(Vector3.Distance(activeWeaponGO.transform.localPosition, activeWeapon.weapon.hipPosition)) <= curVect.magnitude / aimSpeed* backAnimScale * Time.deltaTime) {
                    aimFinished = true;
                    activeWeaponGO.transform.localPosition = activeWeapon.weapon.hipPosition;
                }
                else
                    activeWeaponGO.transform.localPosition += curVect / aimSpeed * backAnimScale * Time.deltaTime;

                mainCamera.fieldOfView += camFOV * Time.deltaTime * 3;
                if (mainCamera.fieldOfView > camFOV) {
                    mainCamera.fieldOfView = camFOV;
                }
            }

            if (aimFinished) aim = false;
            if (aimFinished && forceReload) Reload();
        }

        if (activeWeapon != null && activeWeapon.status == WeaponStatus.reloading) {
 
            if (reloadCooldown > 0f)
                reloadCooldown -= Time.deltaTime;
            else if (reloadCooldown <= 0f) {

                int addAmmo = 0;

                if (activeWeapon.ammo >= activeWeapon.weapon.magCapacity)
                    addAmmo = activeWeapon.weapon.magCapacity - activeWeapon.mag;
                else
                    addAmmo = (int)Mathf.Clamp(activeWeapon.ammo, 0, activeWeapon.weapon.magCapacity - activeWeapon.mag);

                activeWeapon.ammo -= addAmmo;
                activeWeapon.mag += addAmmo;

                if (addAmmo > 0)
                    activeWeapon.status = WeaponStatus.ready;
            }
        }

        if (FPSController.velMagnitude > 3.0f) triggerTime += inaccuracyIncreaseOverTime;
        if (isFiring)  triggerTime += inaccuracyIncreaseOverTime;
        else if (FPSController.velMagnitude < 3.0f) triggerTime -= inaccuracyDecreaseOverTime;

        if (triggerTime >= maximumInaccuracy)       triggerTime = maximumInaccuracy;
        else if (triggerTime <= baseInaccuracy)     triggerTime = baseInaccuracy;
        
    }

    public void EnableWeapon(bool enable)
    {
        if ((enable && !weaponDisabled) || (!enable && weaponDisabled))
            return;

        PlayerGO playerGO = GetComponent<PlayerGO>();
        Animation anim = playerGO.weaponHolderFP.Find(activeWeapon.weapon.name).GetComponent<Animation>();
        /* if (!enable) { 
             anim["draw"].speed = -1;
             anim["draw"].time = anim["draw"].length;
         }
         else {
             anim["draw"].speed = 1;
         }*/

        if (!enable)
        {
            FPSController.instance.weaponAnimations.wrapMode = WrapMode.Once;
            FPSController.instance.weaponAnimations.Play("WeaponDown");
        }
        else
        {
            FPSController.instance.weaponAnimations.Play("WeaponUp");
            FPSController.instance.weaponAnimations.wrapMode = WrapMode.Loop;
        }

        weaponDisabled = !enable;
    }

    public void onKnifeAnimationHitPoint()
    {
        Vector3 dir = Camera.main.transform.TransformDirection(new Vector3(Random.Range(-0.01f, 0.01f), 0f, 1f));
        Ray ray = new Ray(Camera.main.transform.position, dir);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1.75f).OrderBy(e => e.distance).ToArray(); //sort po dystansie aby to co trafilismy bylo po kolei

        foreach (RaycastHit hit in hits)
        {
            Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 1f);

            if (hit.transform.root != transform)
            {
                Vector3 contact = hit.point;
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);

                if (hit.transform.root.tag == "Player")
                {
                    GameManager.instance.photonView.RPC("makePlayerDamageRPC", PhotonTargets.All, hit.transform.root.GetComponent<PlayerGO>().myPlayer.photonPlayer, (int)AvailableWeapon.knife, (int)MeansOfDeath.kill, Weapon.availableWeapons[AvailableWeapon.knife].damage);
                    GetComponent<PlayerGO>().localAS.PlayOneShot(hitIndication);
                    StopCoroutine(fadeHitmark());
                    StartCoroutine(fadeHitmark());
                }
                break;
            }
        }
    }

    public IEnumerator onKnifeFinishAnimation()
    {
        //returning 0 will make it wait 1 frame
        yield return 0;
        isKnife = false;
        GetComponent<PlayerGO>().weaponHolderFP.Find("knife").gameObject.SetActive(false);
        GetComponent<PhotonView>().RPC("changeWeaponRPC", PhotonTargets.All, (int)activeWeapon.weapon.Enum, Player.myPlayer.photonPlayer);
    }

    IEnumerator fadeHitmark() {
        PlayerUI.hitmark.CrossFadeAlpha(1f, 0f, false);
        yield return new WaitForSeconds(1f);
        PlayerUI.hitmark.CrossFadeAlpha(0f, 1f, false);
    }
    [PunRPC]
    void setFiringAnim(bool value, bool isPistol) {
        GetComponent<PlayerGO>().playerAnim.SetBool("Shooting", value);
        if(isPistol) GetComponent<PlayerGO>().playerAnim.SetTrigger("PistolShoot");
    }
    [PunRPC]
    void setReloadAnim() {
        GetComponent<PlayerGO>().playerAnim.SetTrigger("Reload");
    }

    [PunRPC]
    public void changeWeaponRPC(int enumWeaponID, PhotonPlayer changer) {
        Weapon newWeapon = Weapon.availableWeapons[(AvailableWeapon)enumWeaponID];
        Weapon oldWeapon = null;

        if (activeWeapon != null)
            oldWeapon = activeWeapon.weapon;

       if (newWeapon.weaponType == Weapon.WeaponType.main) {
            activeWeapon = primaryWeapon;
            GetComponent<PlayerGO>().playerAnim.SetBool("Weapon", true);
            GetComponent<PlayerGO>().playerAnim.SetBool("Pistol", false);
        }
        else if (newWeapon.weaponType == Weapon.WeaponType.additional) {
            activeWeapon = secondaryWeapon;
            GetComponent<PlayerGO>().playerAnim.SetBool("Weapon", false);
            GetComponent<PlayerGO>().playerAnim.SetBool("Pistol", true);
        }
       
        /* First Person model */
        if (changer == PhotonNetwork.player) {
            shootCooldown = newWeapon.takeTime;

            if(weaponNameCoroutine != null)
                StopCoroutine(weaponNameCoroutine);

            weaponNameCoroutine = StartCoroutine(PlayerUI.fadeOutAfter(PlayerUI.weaponName));

            PlayerUI.weaponName.CrossFadeAlpha(1f, 0f, true);
            PlayerUI.weaponName.text = activeWeapon.weapon.fullName;

            //włączamy model lokalny
            if (oldWeapon != null) {
                GetComponent<PlayerGO>().weaponHolderFP.Find(oldWeapon.name).gameObject.SetActive(false);
            }
            GetComponent<PlayerGO>().weaponHolderFP.Find(newWeapon.name).gameObject.SetActive(true);
            GetComponent<PlayerGO>().weaponsDrawSoundsAS.PlayOneShot(SoundsManager.weaponsDrawSounds[activeWeapon.weapon.Enum]);
            activeWeaponGO = GetComponent<PlayerGO>().weaponHolderFP.Find(newWeapon.name).gameObject;

            GetComponent<PlayerGO>().weaponHolderFP.Find(newWeapon.name).gameObject.transform.localPosition = newWeapon.hipPosition;

        } else {
            //MODEL TPS
            if (oldWeapon != null) {
                GetComponent<PlayerGO>().weaponHolderTP.Find(oldWeapon.name+ "_tp").gameObject.SetActive(false);
            }
            Debug.Log(newWeapon.name + "_tp");
            GetComponent<PlayerGO>().weaponHolderTP.Find(newWeapon.name + "_tp").gameObject.SetActive(true);
        }
    }

    [PunRPC]
    void spawnImpactMarkRPC(string tag, float dirX, float dirY, float dirZ, float normalX, float normalY, float normalZ)
    {
        Vector3 contact = new Vector3(dirX, dirY, dirZ);
        Vector3 normal = new Vector3(normalX, normalY, normalZ);
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
        float rScale = Random.Range(0.5f, 1.0f);
        if(tag == "Player") {
            //TODO
            //GameObject concMark = Instantiate(Body, contact, rot) as GameObject;
            //concMark.transform.localPosition += .02f * normal;
            //concMark.transform.localScale = new Vector3(rScale, rScale, rScale);
            //concMark.GetComponent<AudioSource>().PlayOneShot(concreteImpact[Random.Range(0, concreteImpact.Length)]);
        }
        else if (tag == "Concrete") {
            GameObject concMark = Instantiate(Concrete, contact, rot) as GameObject;
            concMark.transform.localPosition += .02f * normal;
            concMark.transform.localScale = new Vector3(rScale, rScale, rScale);
            concMark.GetComponent<AudioSource>().PlayOneShot(concreteImpact[Random.Range(0, concreteImpact.Length)]);
        }
        else if (tag == "Wood") {
            GameObject woodMark = Instantiate(Wood, contact, rot) as GameObject;
            woodMark.transform.localPosition += .02f * normal;
            woodMark.transform.localScale = new Vector3(rScale, rScale, rScale);
            woodMark.GetComponent<AudioSource>().PlayOneShot(woodImpact[Random.Range(0, woodImpact.Length)]);
        }
        else if (tag == "Metal") {
            GameObject metalMark = Instantiate(Metal, contact, rot) as GameObject;
            metalMark.transform.localPosition += .02f * normal;
            metalMark.transform.localScale = new Vector3(rScale, rScale, rScale);
            metalMark.GetComponent<AudioSource>().PlayOneShot(metalImpact[Random.Range(0, metalImpact.Length)]);
        }
        else if (tag == "Dirt" || tag == "Grass") {
            GameObject dirtMark = Instantiate(Dirt, contact, rot) as GameObject;
            dirtMark.transform.localPosition += .02f * normal;
            dirtMark.transform.localScale = new Vector3(rScale, rScale, rScale);
            dirtMark.GetComponent<AudioSource>().PlayOneShot(dirtImpact[Random.Range(0, dirtImpact.Length)]);
        }
        else {
            GameObject def = Instantiate(Untagged, contact, rot) as GameObject;
            def.transform.localPosition += .02f * normal;
            def.transform.localScale = new Vector3(rScale, rScale, rScale);
            def.GetComponent<AudioSource>().PlayOneShot(concreteImpact[Random.Range(0, concreteImpact.Length)]);
        }
    }
}
