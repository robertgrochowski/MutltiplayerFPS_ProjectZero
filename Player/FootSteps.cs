using UnityEngine;
using System.Collections;

public class FootSteps : MonoBehaviour
{

    // FPS Kit [www.armedunity.com]

    public AudioClip[] concrete;
    public AudioClip[] grass;
    public AudioClip[] wood;
    public AudioClip[] dirt;
    public AudioClip[] metal;

    public AudioClip[] concreteLand;
    public AudioClip[] grassLand;
    public AudioClip[] woodLand;
    public AudioClip[] dirtLand;
    public AudioClip[] metalLand;


    private float audioStepLengthCrouch = 0.75f;
    private float audioStepLengthWalk = 0.4f;
    private float audioStepLengthRun = 0.30f;
    private float minWalkSpeed = 3.5f;
    private float maxWalkSpeed = 5.5f;
    private float audioVolumeCrouch = 0.1f;
    private float audioVolumeWalk = 0.4f;
    private float audioVolumeRun = 0.6f;
    public bool step = true;
    public AudioSource soundsGO;
    public CharacterController cc;
    public FPSController fpscontroller;
    private string curMat;

    void OnEnable()
    {
        step = true;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float speed = cc.velocity.magnitude; //OPT: TODO sqr
        curMat = hit.gameObject.tag;

        // Push Rigidbodys
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body != null && !body.isKinematic && body.mass < 10)
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.velocity += pushDir * 5;
            //body.velocity += cc.velocity;
        }

        if (fpscontroller.state == 2 || !step) return;

        if (cc.isGrounded && hit.normal.y > 0.3f && speed > 0)
        {
            SoundsManager.instance.photonView.RPC("playFootstepSoundRPC", PhotonTargets.All, curMat, speed); 
        }
    }

    public void playFootstepSound(string curMat, float speed) {

        //Debug.Log("Playing footstep on: " + );
        if (curMat == "Untagged" || curMat == "Concrete") {
            if (speed > maxWalkSpeed) StartCoroutine(RunOnConcrete());
            else if (speed < maxWalkSpeed && speed > minWalkSpeed) StartCoroutine(WalkOnConcrete());
            else if (speed < minWalkSpeed && speed > 0.5f) StartCoroutine(CrouchOnConcrete());

        }
        else if (curMat == "Grass") {
            if (speed > maxWalkSpeed) StartCoroutine(RunOnGrass());
            else if (speed < maxWalkSpeed && speed > minWalkSpeed) StartCoroutine(WalkOnGrass());
            else if (speed < minWalkSpeed && speed > 0.5f) StartCoroutine(CrouchOnGrass());

        }
        else if (curMat == "Wood") {
            if (speed > maxWalkSpeed) StartCoroutine(RunOnWood());
            else if (speed < maxWalkSpeed && speed > minWalkSpeed) StartCoroutine(WalkOnWood());
            else if (speed < minWalkSpeed && speed > 0.5f) StartCoroutine(CrouchOnWood());

        }
        else if (curMat == "Dirt") {
            if (speed > maxWalkSpeed) StartCoroutine(RunOnDirt());
            else if (speed < maxWalkSpeed && speed > minWalkSpeed) StartCoroutine(WalkOnDirt());
            else if (speed < minWalkSpeed && speed > 0.5f) StartCoroutine(CrouchOnDirt());

        }
        else if (curMat == "Metal") {
            if (speed > maxWalkSpeed) StartCoroutine(RunOnMetal());
            else if (speed < maxWalkSpeed && speed > minWalkSpeed) StartCoroutine(WalkOnMetal());
            else if (speed < minWalkSpeed && speed > 0.5f) StartCoroutine(CrouchOnMetal());
        }
    }
    
    public IEnumerator JumpLand() //TODO
    {
        if (!soundsGO.enabled) yield break;

        if (curMat == "Untagged" || curMat == "Concrete")
        {
            soundsGO.PlayOneShot(concreteLand[Random.Range(0, concreteLand.Length)], 0.5f);
            yield return new WaitForSeconds(0.1f);
        }
        else if (curMat == "Grass")
        {
            soundsGO.PlayOneShot(grassLand[Random.Range(0, grassLand.Length)], 0.5f);
            yield return new WaitForSeconds(0.12f);
        }
        else if (curMat == "Wood")
        {
            soundsGO.PlayOneShot(woodLand[Random.Range(0, woodLand.Length)], 0.5f);
            yield return new WaitForSeconds(0.12f);
        }
        else if (curMat == "Dirt")
        {
            soundsGO.PlayOneShot(dirtLand[Random.Range(0, dirtLand.Length)], 0.5f);
            yield return new WaitForSeconds(0.11f);
        }
        else if (curMat == "Metal")
        {
            soundsGO.PlayOneShot(metalLand[Random.Range(0, metalLand.Length)], 0.5f);
            yield return new WaitForSeconds(0.12f);
        }
    }
    // Concrete	or Untagged
    IEnumerator CrouchOnConcrete()
    {
        step = false;
        soundsGO.PlayOneShot(concrete[Random.Range(0, concrete.Length)], audioVolumeCrouch);
        yield return new WaitForSeconds(audioStepLengthCrouch);
        step = true;
    }

    IEnumerator WalkOnConcrete()
    {
        step = false;
        soundsGO.PlayOneShot(concrete[Random.Range(0, concrete.Length)], audioVolumeWalk);
        yield return new WaitForSeconds(audioStepLengthWalk);
        step = true;
    }

    IEnumerator RunOnConcrete()
    {
        step = false;
        soundsGO.PlayOneShot(concrete[Random.Range(0, concrete.Length)], audioVolumeRun);
        yield return new WaitForSeconds(audioStepLengthRun);
        step = true;
    }

    // Grass
    IEnumerator CrouchOnGrass()
    {
        step = false;
        soundsGO.PlayOneShot(grass[Random.Range(0, grass.Length)], audioVolumeCrouch);
        yield return new WaitForSeconds(audioStepLengthCrouch);
        step = true;
    }

    IEnumerator WalkOnGrass()
    {
        step = false;
        soundsGO.PlayOneShot(grass[Random.Range(0, grass.Length)], audioVolumeWalk);
        yield return new WaitForSeconds(audioStepLengthWalk);
        step = true;
    }

    IEnumerator RunOnGrass()
    {
        step = false;
        soundsGO.PlayOneShot(grass[Random.Range(0, grass.Length)], audioVolumeRun);
        yield return new WaitForSeconds(audioStepLengthRun);
        step = true;
    }

    // Wood
    IEnumerator CrouchOnWood()
    {
        step = false;
        soundsGO.PlayOneShot(wood[Random.Range(0, wood.Length)], audioVolumeCrouch);
        yield return new WaitForSeconds(audioStepLengthCrouch);
        step = true;
    }

    IEnumerator WalkOnWood()
    {
        step = false;
        soundsGO.PlayOneShot(wood[Random.Range(0, wood.Length)], audioVolumeWalk);
        yield return new WaitForSeconds(audioStepLengthWalk);
        step = true;
    }

    IEnumerator RunOnWood()
    {
        step = false;
        soundsGO.PlayOneShot(wood[Random.Range(0, wood.Length)], audioVolumeRun);
        yield return new WaitForSeconds(audioStepLengthRun);
        step = true;
    }

    // Dirt
    IEnumerator CrouchOnDirt()
    {
        step = false;
        soundsGO.PlayOneShot(dirt[Random.Range(0, dirt.Length)], audioVolumeCrouch);
        yield return new WaitForSeconds(audioStepLengthCrouch);
        step = true;
    }

    IEnumerator WalkOnDirt()
    {
        step = false;
        soundsGO.PlayOneShot(dirt[Random.Range(0, dirt.Length)], audioVolumeWalk);
        yield return new WaitForSeconds(audioStepLengthWalk);
        step = true;
    }

    IEnumerator RunOnDirt()
    {
        step = false;
        soundsGO.PlayOneShot(dirt[Random.Range(0, dirt.Length)], audioVolumeRun);
        yield return new WaitForSeconds(audioStepLengthRun);
        step = true;
    }

    // Metal
    IEnumerator CrouchOnMetal()
    {
        step = false;
        soundsGO.PlayOneShot(metal[Random.Range(0, metal.Length)], audioVolumeCrouch);
        yield return new WaitForSeconds(audioStepLengthCrouch);
        step = true;
    }

    IEnumerator WalkOnMetal()
    {
        step = false;
        soundsGO.PlayOneShot(metal[Random.Range(0, metal.Length)], audioVolumeWalk);
        yield return new WaitForSeconds(audioStepLengthWalk);
        step = true;
    }

    IEnumerator RunOnMetal()
    {
        step = false;
        soundsGO.PlayOneShot(metal[Random.Range(0, metal.Length)], audioVolumeRun);
        yield return new WaitForSeconds(audioStepLengthRun);
        step = true;
    }
}