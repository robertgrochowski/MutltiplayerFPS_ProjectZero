using UnityEngine;
using System.Collections;

public class FPSController : MonoBehaviour
{
    public static FPSController instance;
    // FPS Kit [www.armedunity.com]
    private MouseLook m_MouseLook;

    private float standHeightChange = 2f;
    private float crouchHeightChange = 1.4f;
    private float proneHeightChange = 0.8f;
    private float normalCenter = 2f;
    private float crouchCenter = 1.4f;
    private float proneCenter = 0.5f;

    public int proneSpeed = 1;
    public int crouchSpeed = 3;
    public int walkSpeed = 5;
    public int runSpeed = 8;
    public float jumpSpeed = 8.0f;
    private float gravity = 24;
    public float baseGravity = 24;
    public float proneGravity = 15;
    private float normalFDTreshold = 5;
    private float proneFDTreshold = 3;
    private float fallingDamageThreshold;
    public float fallDamageMultiplier = 6.0f;
    private float slideSpeed = 8.0f;
    private float antiBumpFactor = .75f;
    private float antiBunnyHopFactor = 1;
    public bool airControl = false;
    

    [HideInInspector]
    public bool run;
    public bool isRunning;
    [HideInInspector]
    public bool canRun = true;

    public Transform cameraGO;
    [HideInInspector]
    Vector3 moveDirection = Vector3.zero;
    [HideInInspector]
    public bool grounded = false;
    private Transform myTransform;
    [HideInInspector]
    public float speed;
    private RaycastHit hit;
    private float fallDistance;
    private bool falling = false;
    public float slideLimit = 45.0f;
    public float rayDistance;
    private Vector3 contactPoint;
    private int jumpTimer;
   

    [HideInInspector]
    public int state = 0;
    // 0 = standing
    // 1 = crouching
    // 2 = prone

    float freezedTime = 0f;

    private float adjustAnimSpeed = 7.0f;

    private Vector3 currentPosition;
    private Vector3 lastPosition;
    private Quaternion lastCameraPosition;
    private float highestPoint;
    public FootSteps footsteps;

    private float crouchProneSpeed = 3;
    private float distanceToObstacle;

    private bool sliding = false;
    [HideInInspector]
    public float velMagnitude;

    public CharacterController controller;
    public Animation cameraAnimations;
    public Animation weaponAnimations;
    public Animator playerAnim;
    public static string runAnimation = "CameraRun";
    public static string idleAnimation = "IdleAnimation";

    public AudioSource aSource;
    public AudioClip bodyHitSound;

    public Transform fallEffect;
    public Transform spine;

    // ladder
    public bool isClimbing = false;
    public float climbSpeed = 3.75f;
    CollisionFlags collision;
    bool canLadder = true;
    bool inLadderTrigger = false;

    void Start()
    {
        instance = this;

        myTransform = transform;
        rayDistance = controller.height / 2 + 1.1f;
        slideLimit = controller.slopeLimit - .2f;
        cameraAnimations[runAnimation].speed = 0.8f;
        weaponAnimations.wrapMode = WrapMode.Loop;
        weaponAnimations.Stop();
        m_MouseLook = new MouseLook();
        m_MouseLook.Init(transform, cameraGO.transform);
        PlayerUI.enableCursor(false);
    }

    void LateUpdate() {

        if (WeaponsManager.instance.activeWeapon != null)
        {
            if (!isRunning && state != 2 && WeaponsManager.instance.activeWeapon.weapon.weaponType != Weapon.WeaponType.additional) //Normal
                spine.transform.localRotation = new Quaternion(Mathf.Clamp(cameraGO.transform.localRotation.x, -0.41f, 0.45f), 0f, Mathf.Clamp(cameraGO.transform.localRotation.x, -0.41f, 0.45f), 1f);
            else if (!isRunning && state != 2) //Pistol
                spine.transform.localRotation = new Quaternion(Mathf.Clamp(cameraGO.transform.localRotation.x, -0.41f, 0.45f), -0.5f, 0f, 1f);
            else
                spine.transform.localRotation = new Quaternion(0f, 0f, 0f, 0f); //slerped in netpossync
        }

        lastPosition = currentPosition;
        lastCameraPosition = myTransform.rotation;
    }

    void Update()
    {
        isRunning = false;
        velMagnitude = controller.velocity.magnitude;
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f) ? .7071f : 1.0f;
        bool freezePlayer = PlayerUI.isTyping || PlayerUI.activeMenu != null;

        if (freezePlayer) {
            inputX = 0;
            inputY = 0;
        }
        else m_MouseLook.LookRotation(transform, cameraGO.transform);

        currentPosition = myTransform.position;
        if (!isClimbing)
        {
            if (grounded)
            {
                gravity = baseGravity;

                if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance))
                {
                    float hitangle = Vector3.Angle(hit.normal, Vector3.up);
                    if (hitangle > slideLimit)
                    {
                        sliding = true;
                    }
                    else
                    {
                        sliding = false;
                    }
                }

                if (canRun && state == 0)
                {
                    if (Input.GetKey(Controls.inputKeyCode[Controls.sprintKey]) && Input.GetKey("w") && !Input.GetMouseButton(0) && WeaponsManager.instance.activeWeapon.status != WeaponStatus.reloading && !WeaponsManager.instance.isKnife)
                        run = true;
                    else run = false;
                }

                if (falling)
                {
                    if (state == 2)
                        fallingDamageThreshold = proneFDTreshold;
                    else
                        fallingDamageThreshold = normalFDTreshold;

                    falling = false;
                    fallDistance = highestPoint - currentPosition.y;
                    if (fallDistance > fallingDamageThreshold)
                    {
                        ApplyFallingDamage(fallDistance);
                    }

                    if (fallDistance < fallingDamageThreshold && fallDistance > 0.1f)
                    {
                        if (state < 2) { StartCoroutine(footsteps.JumpLand()); }
                        else if (bodyHitSound) aSource.PlayOneShot(bodyHitSound, 0.5f);

                        StartCoroutine(FallCamera(new Vector3(7, Random.Range(-1.0f, 1.0f), 0), new Vector3(3, Random.Range(-0.5f, 0.5f), 0), 0.15f));
                    }
                }

                if (sliding)
                {
                    Vector3 hitNormal = hit.normal;
                    moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                    Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);
                    moveDirection *= slideSpeed;
                }
                else
                {
                    if (state == 0)
                    {
                        if (run)
                            speed = runSpeed;
                        else if (Input.GetMouseButton(1))
                            speed = crouchSpeed;
                        else
                            speed = walkSpeed;

                    }
                    else if (state == 1)
                    {
                        speed = crouchSpeed;
                        run = false;
                    }
                    else if (state == 2)
                    {
                        speed = proneSpeed;
                        run = false;
                    }
                    // if (Cursor.lockState == CursorLockMode.Locked) todo?
                    moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
                    // else
                    //     moveDirection = new Vector3(0, -antiBumpFactor, 0);

                    moveDirection = myTransform.TransformDirection(moveDirection);
                    moveDirection *= speed;

                    if (!Input.GetKey(KeyCode.Space))
                    {
                        jumpTimer++;
                    }
                    else if (jumpTimer >= antiBunnyHopFactor && !freezePlayer)
                    {
                        jumpTimer = 0;
                        if (state == 0)
                        {
                            moveDirection.y = jumpSpeed;
                        }

                        if (state > 0)
                        {
                            CheckDistance();
                            if (distanceToObstacle > 1.6f)
                            {
                                state = 0;
                            }
                        }
                    }
                }

            }
            else
            {
                if (currentPosition.y > lastPosition.y)
                {
                    highestPoint = myTransform.position.y;
                    falling = true;
                }

                if (!falling)
                {
                    highestPoint = myTransform.position.y;
                    falling = true;
                }

                if (airControl)
                {
                    moveDirection.x = inputX * speed;
                    moveDirection.z = inputY * speed;
                    moveDirection = myTransform.TransformDirection(moveDirection);
                }
            }
        }
        /* Weapons Anim */
        if (grounded && !WeaponsManager.instance.weaponDisabled)
        {
            
            if (run && velMagnitude > walkSpeed) {
                isRunning = true;
                weaponAnimations.CrossFade("SprintLoop");
                cameraAnimations.CrossFade(runAnimation);
            }
            else if (velMagnitude > crouchSpeed && !run)
            {
                //weaponAnimations["Walk"].speed = velMagnitude / adjustAnimSpeed;
                weaponAnimations.CrossFade("Walk");
            }
            else
            {
                cameraAnimations.CrossFade(idleAnimation);
                if (lastPosition == currentPosition && lastCameraPosition == myTransform.rotation) {

                    if (freezedTime > .2f) {
                        weaponAnimations.CrossFade("Breath");
                    }
                    else freezedTime += Time.deltaTime;
                }
                else freezedTime = 0;
            }
        }
        else if(!WeaponsManager.instance.weaponDisabled)
        {
            weaponAnimations.CrossFade(idleAnimation);
            cameraAnimations.CrossFade(idleAnimation);
            run = false;
        }

        if (!freezePlayer) {
            if (Input.GetKeyDown(Controls.inputKeyCode[Controls.crouchKey])) {
                CheckDistance();

                if (state == 0) {
                    state = 1;

                }
                else if (state == 1) {
                    if (distanceToObstacle > 1.6f) {
                        state = 0;
                    }
                }
                else if (state == 2) {
                    if (distanceToObstacle > 1) {
                        state = 1;
                    }
                }
            }

            if (Input.GetKeyDown(Controls.inputKeyCode[Controls.proneKey])) {
                CheckDistance();
                if (state == 0 || state == 1) {
                    state = 2;
                }
                else if (state == 2) {
                    if (distanceToObstacle > 1.6f) {
                        state = 0;
                    }
                }

                if (!grounded) gravity = proneGravity;
            }
        }
        /* States (camera set) */
        if (state == 0)
        { //Stand Position
            controller.height = standHeightChange;
            controller.center = new Vector3(0, 1f, 0);

            if (cameraGO.localPosition.y > normalCenter)
            {
                cameraGO.localPosition = new Vector3(cameraGO.localPosition.x, normalCenter, cameraGO.localPosition.z);
            }
            else if (cameraGO.localPosition.y < normalCenter)
            {
                cameraGO.localPosition = new Vector3(cameraGO.localPosition.x, cameraGO.localPosition.y + Time.deltaTime * crouchProneSpeed, cameraGO.localPosition.z);
            }

        }
        else if (state == 1)
        { //Crouch Position

            controller.height = crouchHeightChange;
            controller.center = new Vector3(0, 0.7f, 0);
            if (cameraGO.localPosition.y != crouchCenter)
            {
                if (cameraGO.localPosition.y > crouchCenter)
                {
                    cameraGO.localPosition = new Vector3(cameraGO.localPosition.x, cameraGO.localPosition.y - Time.deltaTime * crouchProneSpeed, cameraGO.localPosition.z);
                }
                if (cameraGO.localPosition.y < crouchCenter)
                {
                    cameraGO.localPosition = new Vector3(cameraGO.localPosition.x, cameraGO.localPosition.y + Time.deltaTime * crouchProneSpeed, cameraGO.localPosition.z);
                }

            }

        }
        else if (state == 2)
        { //Prone Position

            controller.height = proneHeightChange;
            controller.center = new Vector3(0, 0.4f, 0);

            if (cameraGO.localPosition.y < proneCenter)
            {
                cameraGO.localPosition = new Vector3(cameraGO.localPosition.x, proneCenter, cameraGO.localPosition.z);
            }
            else if (cameraGO.localPosition.y > proneCenter)
            {
                cameraGO.localPosition = new Vector3(cameraGO.localPosition.x, cameraGO.localPosition.y - Time.deltaTime * crouchProneSpeed, cameraGO.localPosition.z);
            }
        }

        if(isClimbing && Input.GetKeyDown(KeyCode.Space)) //Jump out from ladder
        {
            moveDirection = controller.transform.forward * -4f;
            moveDirection.y += jumpSpeed;
            exitLadder();
            Debug.Log("exit: jumped");
        }
        else if(isClimbing)
        {
            grounded = (controller.Move(controller.transform.up * climbSpeed * inputY * Time.deltaTime) & CollisionFlags.Below) != 0;
        }
        
        if(!isClimbing)
        {
            moveDirection.y -= gravity * Time.deltaTime;
            grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            debug();
        }

        playerAnim.SetInteger("State", state);
    }

    IEnumerator waitForGround() 
    {
        while (true)
        {
            if (grounded && !inLadderTrigger)
            {
                canLadder = true;
                yield break;
            }    
            yield return 0;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag == "Ladder" && canLadder && isLookingAtLadder(other.transform.eulerAngles.y, controller.transform.eulerAngles.y))
        {
            if(grounded)
            {
                Debug.Log("grounded");
            }
            grounded = false;
            canLadder = false;
            isClimbing = true;

            m_MouseLook.Lock360Rotation(true, other.transform.eulerAngles.y);
            WeaponsManager.instance.EnableWeapon(false);

            Debug.Log(">>>entering");
        }
    }

    private void OnTriggerStay(Collider other) {
        if(isClimbing && other.tag == "Ladder")
        {
            inLadderTrigger = true;
            if(grounded) //controller touches ground
            {
                controller.Move(controller.transform.forward * -4f * Time.deltaTime);
                exitLadder();

                Debug.Log("exit: on ground");
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag == "Ladder" && isClimbing)
        {
            exitLadder();
            Debug.Log("exit: trigger");
        }
    }

    void debug() {
        Debug.Log("isClimbing: " + isClimbing);
        Debug.Log("inLadderTrigger: " + inLadderTrigger);
        Debug.Log("grounded: " + grounded);
    }

    void exitLadder() 
    {
        inLadderTrigger = false;
        isClimbing = false;
        StartCoroutine(waitForGround());

        m_MouseLook.Lock360Rotation(false);
        WeaponsManager.instance.EnableWeapon(true);
    }

    bool isLookingAtLadder(float centerAngle, float CurrentAngle) {
        float max = centerAngle + 90;
        float min = centerAngle - 90;
        if (max > 360)
            max -= 360;
        if (min < 0)
            min += 360;
        return !((CurrentAngle >= max && CurrentAngle <= min && max < min) || (max > min && (CurrentAngle >= max || CurrentAngle <= min)));
    }

    void CheckDistance()
    {
        Vector3 pos = myTransform.position + controller.center - new Vector3(0, controller.height / 2, 0);
        RaycastHit hit;
        if (Physics.SphereCast(pos, controller.radius, myTransform.up, out hit, 10))
        {
            distanceToObstacle = hit.distance;
            Debug.DrawLine(pos, hit.point, Color.red, 2.0f);
        }
        else
        {
            distanceToObstacle = 3;
        }
    }

    

    IEnumerator FallCamera(Vector3 d, Vector3 dw, float ta)
    {
        Quaternion s = fallEffect.localRotation;
        Quaternion e = fallEffect.localRotation * Quaternion.Euler(d);
        float r = 1.0f / ta;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * r;
            fallEffect.localRotation = Quaternion.Slerp(s, e, t);
            yield return null;
        }
    }


    void ApplyFallingDamage(float fallDistance) {
        float diff = fallDistance - fallingDamageThreshold;
        int damage = (int)(100 * (diff / 4));
 
        GameManager.instance.photonView.RPC("makePlayerDamageRPC", PhotonTargets.All, Player.myPlayer.photonPlayer, (int)AvailableWeapon.none, (int)MeansOfDeath.falldamage, damage);
        if (state < 2) StartCoroutine(footsteps.JumpLand());
        StartCoroutine(FallCamera(new Vector3(12, Random.Range(-2.0f, 2.0f), 0), new Vector3(4, Random.Range(-1.0f, 1.0f), 0), 0.1f));
    }
}