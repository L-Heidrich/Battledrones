using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{

    public float speed;
    public float sprintModifier;
    public float jumpForce;
    public Camera normalCam;
    public float crouchModifier;
    public GameObject camParent;
    public LayerMask ground;
    public float length_of_slide;
    public float slideSpeed;

    public ProfileData playerProfile;
    public TextMeshPro AHName;

    public float slideAmount;
    public float crouchAmount;
    public GameObject standingCollider;
    public GameObject crouchCollider;

    public Camera weaponCam;

    private Transform ui_healthbar;
    private Text ui_username;
    private Text ui_ammo;
    
    public Transform groundDetector;
    public Transform weaponParent;

    public float maxHealth;
    private float currentHealth;

    private Vector3 weaponParentOrigin;
    private Vector3 targetBobPosition;
    public Vector3 weaponParentCurrentPosition;

    private float movementCounter;
    private float idleCounter;

    private Rigidbody rb;
    private float basefov;
    private float newfov = 1.5f;
    private Vector3 camOrigin;

    private GameManager manager;
    private Weaponmanager weapon;
    int wClass;

    private bool sliding;
    private bool crouched;
    private bool isAiming;
    private float slideTime;
    private Vector3 slideDirection;
    private float aimAngle;

    private Animator anim;

    
    void Start()
    {

        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        weapon = GetComponent<Weaponmanager>();

        currentHealth = maxHealth;

        camParent.SetActive(photonView.IsMine);

        if (!photonView.IsMine)
        {
            gameObject.layer = 11;
            standingCollider.layer = 11;
            crouchCollider.layer = 11;
        }

        basefov = normalCam.fieldOfView;
        camOrigin = normalCam.transform.localPosition;
        
 
        rb = GetComponent<Rigidbody>();
        weaponParentOrigin = weaponParent.localPosition;
        weaponParentCurrentPosition = weaponParentOrigin;

        if (photonView.IsMine)
        {
            wClass = MainMenu.profile.character;
            Debug.Log(wClass);
            ui_healthbar = GameObject.Find("HUD/Health/Bar").transform;

            ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            ui_username = GameObject.Find("HUD/Username/Text").GetComponent<Text>();

            if (wClass == 0)
            {
                weapon.photonView.RPC("EquipNewWeapon", RpcTarget.All, "Sniper");
            }
            if (wClass == 1)
            {
                weapon.photonView.RPC("EquipNewWeapon", RpcTarget.All, "AR");
            }
            if (wClass == 2)
            {
                weapon.photonView.RPC("EquipNewWeapon", RpcTarget.All, "Shotgun");
            }

            RefreshHealthBar();

            weapon.RefreshAmmo(ui_ammo);
            ui_username.text = MainMenu.profile.username;
            anim = GetComponent<Animator>();

        }

    }

    
    private void Update()
    {

        if (!photonView.IsMine)
        {
            RefresMultiplayerState();
            return;
        }

        float temp_hmove = Input.GetAxisRaw("Horizontal");
        float temp_vmove = Input.GetAxisRaw("Vertical");

        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool crouch = Input.GetKeyDown(KeyCode.C);
        bool pause = Input.GetKeyDown(KeyCode.Escape);


        bool isGroundend = Physics.Raycast(groundDetector.position, Vector3.down, 0.15f, ground);
        bool isJumping = jump && isGroundend;
        bool isSprinting = sprint && temp_vmove > 0 && !isJumping;
        bool isCrouching = crouch;

        Debug.Log("Grounded: " + isGroundend);

        if (pause)
        {
            GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
        }

        if (Pause.pause)
        {
            temp_hmove = 0f;
            temp_vmove = 0f;
            sprint = false;
            crouch = false;
            jump = false;
            isGroundend = false;
            isJumping = false;
            isSprinting = false;
            isCrouching = false;
        }

        if (isCrouching)
        {
            photonView.RPC("SetCrouch", RpcTarget.All, !crouched);
        }

        if (isJumping)
        {
            if (crouched)
            {
                photonView.RPC("SetCrouch", RpcTarget.All, false);
             }
            rb.AddForce(Vector3.up * jumpForce);
        }

       /* if (Input.GetKeyDown(KeyCode.V)) {
            TakeDamage(100f);
        }*/

        if (sliding)
        {
            Headbob(movementCounter, 0.04f, 0.04f);
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobPosition, Time.deltaTime * 8f);

        }

        else if (temp_hmove == 0 && temp_vmove == 0)
        {
            Headbob(idleCounter, 0.025f, 0.025f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobPosition, Time.deltaTime * 2f);

        }
        else if(!isSprinting && !crouched){

                Headbob(movementCounter, 0.1f, 0.1f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobPosition, Time.deltaTime * 4f);

        }
        else if ( crouched)
        {

            Headbob(movementCounter, 0.02f, 0.02f);
            movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobPosition, Time.deltaTime * 4f);

        }
        else {
                Headbob(movementCounter, 0.04f, 0.04f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetBobPosition, Time.deltaTime * 8f);
            }

        RefreshHealthBar();
        photonView.RPC("SyncProfile", RpcTarget.All, MainMenu.profile.username, MainMenu.profile.level, MainMenu.profile.exp, MainMenu.profile.character);

        weapon.RefreshAmmo(ui_ammo);

        
    }
    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        float temp_hmove = Input.GetAxisRaw("Horizontal");
        float temp_vmove = Input.GetAxisRaw("Vertical");

        bool jump = Input.GetKeyDown(KeyCode.Space);
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool slide = Input.GetKeyDown(KeyCode.C);
        bool aim = Input.GetMouseButton(1);

        bool isGroundend = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
        bool isJumping = jump && isGroundend;
        bool isSprinting = sprint && temp_vmove > 0 ;
        bool isSliding = isSprinting && slide && !sliding;
        isAiming = aim && !isSliding && !isSprinting;

        Vector3 temp_direction = Vector3.zero;
        float temp_adjustedSpeed = speed;

        if (Pause.pause)
        {
            temp_hmove = 0f;
            temp_vmove = 0f;
            sprint = false;
             jump = false;
            isGroundend = false;
            isJumping = false;
            isSprinting = false;
            isAiming = false;
         }

        if (!sliding) {

            temp_direction = new Vector3(temp_hmove, 0, temp_vmove);
            temp_direction.Normalize();
            temp_direction = transform.TransformDirection(temp_direction);

            if (isSprinting)
            {
                if (crouched)
                {
                    photonView.RPC("SetCrouch", RpcTarget.All, false);
                }
                temp_adjustedSpeed *= sprintModifier;

            }
            else if (crouched) {

                temp_adjustedSpeed *= crouchModifier;

            }
            

        }
        else
        {
            temp_direction = slideDirection;
            temp_adjustedSpeed *= slideSpeed;
            slideTime -= Time.deltaTime;
            if (slideTime <= 0)
            {
                sliding = false;
                weaponParentCurrentPosition -= Vector3.down * 0.5f;

            }

        }

        Vector3 temp_targetVelocity = temp_direction * temp_adjustedSpeed * Time.deltaTime;
        temp_targetVelocity.y = rb.velocity.y;

        rb.velocity = temp_targetVelocity;


        if (isSliding)
        {
            sliding = true;
            slideDirection = temp_direction;
            slideTime = length_of_slide;
            weaponParentCurrentPosition += Vector3.down * 0.5f;
            if (!crouched){
                photonView.RPC("SetCrouch", RpcTarget.All, true);
            }
        }

        //aim

        weapon.Aim(isAiming);

        //Cams

        if (sliding)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov * newfov * 0.75f, Time.deltaTime * 8f);
            weaponCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov * newfov * 0.75f, Time.deltaTime * 8f);

            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin + Vector3.down * slideAmount, Time.deltaTime * 6f);
            weaponCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin + Vector3.down * slideAmount, Time.deltaTime * 6f);


        }
        else
        {

            if (isSprinting)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov * newfov, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov * newfov, Time.deltaTime * 8f);

            }
            else if(isAiming)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov * weapon.CurrentGunData.mainFov, Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov * weapon.CurrentGunData.weaponFov, Time.deltaTime * 8f);


            }
            else
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov  , Time.deltaTime * 8f);
                weaponCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, basefov, Time.deltaTime * 8f);
            }
            if (crouched)
            {
                 normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
                 weaponCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin + Vector3.down * crouchAmount, Time.deltaTime * 6f);

            }
            else
            {
                normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin , Time.deltaTime * 6f);
                weaponCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin, Time.deltaTime * 6f);

            }

            //   normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition, camOrigin , Time.deltaTime * 6f);
        }

        float anim_horizontal = 0f;
        float anim_vertical = 0f;

        if (isGroundend)
        {
            anim_horizontal = temp_direction.x;
            anim_vertical = temp_direction.y;
        }

        anim.SetFloat("H", anim_horizontal);
        anim.SetFloat("V", anim_vertical);

    }

    [PunRPC]
    void SetCrouch(bool state) {

        if(crouched == state)
        {
            return;
        }

        crouched = state;

        if (crouched)
        {
            standingCollider.SetActive(false);
            crouchCollider.SetActive(true);
            weaponParentCurrentPosition += Vector3.down * crouchAmount;
        }

        else
        {
            standingCollider.SetActive(true);
            crouchCollider.SetActive(false);
            weaponParentCurrentPosition -= Vector3.down * crouchAmount;
        }
    }

    [PunRPC]
    private void SyncProfile(string username, int lvl, int exp, int c)
    {
        playerProfile = new ProfileData(username, lvl, exp, c);
        AHName.text = playerProfile.username; 

    }


    void Headbob(float z, float x_intensity, float y_intesity)
    {
        float temp_aim_adj = 1f;
        if (isAiming)
        {
            temp_aim_adj = 0.1f;
        }
        targetBobPosition = weaponParentCurrentPosition + new Vector3(Mathf.Cos(z) * x_intensity * temp_aim_adj, Mathf.Sin(z*2) * y_intesity * temp_aim_adj, 0);
    }

 
    public void TakeDamage(float damage, int n_actor)
    {

        if (photonView.IsMine)
        {
            currentHealth -= damage;
            RefreshHealthBar();

            if (currentHealth <= 0 )
            {
                Debug.Log("dead");
                manager.Spawn();
                manager.ChangeStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

               
                    manager.ChangeStatSend(n_actor, 0, 1);

 
               PhotonNetwork.Destroy(gameObject);
            }
        }

    }

    private void RefreshHealthBar()
    {
        float temp_healt_ratio = currentHealth / maxHealth;
        ui_healthbar.localScale =  Vector3.Lerp(ui_healthbar.localScale, new Vector3(temp_healt_ratio, 1, 1), Time.deltaTime * 8f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
        }
        else
            {
            aimAngle = (int)stream.ReceiveNext() / 100f;
        }
    }

    void RefresMultiplayerState()
    {
        float eulY = weaponParent.localEulerAngles.y;

        Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
        weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

        Vector3 finalRotation = weaponParent.localEulerAngles;
        finalRotation.y = eulY;

        weaponParent.localEulerAngles = finalRotation;

    }


}
