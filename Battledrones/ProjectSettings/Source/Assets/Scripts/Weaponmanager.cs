 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Weaponmanager : MonoBehaviourPunCallbacks
{

    public List<Gun> loadout;
 
    public Transform weaponParent;
    public Gun CurrentGunData;

    public Gun AR;
    public Gun pistol;
    public Gun Sniper;
    public Gun Shotgun;


    public GameObject buletHolePrefab;
    public LayerMask shootable;
    public bool isAimin;
    public AudioSource sfx;

    private int currentIndex;
    private GameObject currentWeapon;
    private float cd = 0f;
    private Image hitmarker;
    private float hitmarkerWait;
    public AudioClip hitmarkerSound;
    private bool isReloading;
    private Color white = new Color(1, 1, 1, 0);

    public void RefreshAmmo(Text text)
    {
        int temp_clip = loadout[currentIndex].GetClip();
        int temp_stash = loadout[currentIndex].GetStash();
 
        text.text = temp_clip.ToString() + " / " + temp_stash.ToString();

    }

    void Start()
    {

        //SetLoadout(MainMenu.profile.character);

        hitmarker = GameObject.Find("HUD/Hitmarker/Image").GetComponent<Image>();
        hitmarker.color = new Color(1, 1, 1, 0);

    }
 

     [PunRPC]
     void EquipNewWeapon(string name)
    {
        Gun newWeapon = WeaponLibrary.FindGun(name);
        newWeapon.Initialise();
        loadout[currentIndex] = newWeapon;
        Equip(currentIndex);

        foreach (Gun a in loadout)
        {
            a.Initialise();
            Equip(0);

        }

    
    }



    // Update is called once per frame
    void Update()
    {    
        if (Pause.pause && photonView.IsMine)
        {
            return;
        }

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1))
        {
           photonView.RPC("Equip", RpcTarget.All, 0);
        }

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2))
        {
            photonView.RPC("Equip", RpcTarget.All, 1);
        }

        if (currentWeapon != null) {

            if(photonView.IsMine)
            {
 
                if (loadout[currentIndex].burst == 1)
                {

                    if (Input.GetMouseButtonDown(0) && cd <= 0 && !isReloading )
                    {
                        if (loadout[currentIndex].FireBullet())
                        {
                            photonView.RPC("Shoot", RpcTarget.All);
                        }
                        else
                        {
                            StartCoroutine((Reload(loadout[currentIndex].reloadTime)));
                        }
                    }
                }
                else
                {
                    if (Input.GetMouseButton(0) && cd <= 0 && !isReloading)
                    {
                        if (loadout[currentIndex].FireBullet())
                        {
                            photonView.RPC("Shoot", RpcTarget.All);
                        }
                        else
                        {
                            StartCoroutine((Reload(loadout[currentIndex].reloadTime)));
                        }
                    }
                }
                    

                if (Input.GetKeyDown(KeyCode.R) && !isReloading && !isAimin)
                {
                    photonView.RPC("ReloadRPC", RpcTarget.All);
                    StartCoroutine((Reload(loadout[currentIndex].reloadTime)));
                }

                if (cd >=0)
                {
                    cd -= Time.deltaTime;
                }
            }  
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);

            if (photonView.IsMine)
            {
                    if(hitmarkerWait > 0)
                {
                    hitmarkerWait -= Time.deltaTime;
                }
                else if(hitmarker.color.a > 0)
                {
                    hitmarker.color = Color.Lerp(hitmarker.color,white, Time.deltaTime * 0.5f);
                }
            }
          
        }
      

    }

  
    [PunRPC]
    void Equip(int id)
    {
 
        if (currentWeapon != null)
        {
            
            StopCoroutine("Reload");
           

            Destroy(currentWeapon);
        }

        currentIndex = id;
        GameObject temp_newWeapon = Instantiate(loadout[id].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        temp_newWeapon.transform.localPosition = Vector3.zero;
        temp_newWeapon.transform.localEulerAngles = Vector3.zero;
        temp_newWeapon.GetComponent<Sway>().isMine = photonView.IsMine;

        if (photonView.IsMine) {
            ChangeLayersRec(temp_newWeapon, 10);
                }
        else
        {
            ChangeLayersRec(temp_newWeapon, 0);
        }

        currentWeapon = temp_newWeapon;

        CurrentGunData = loadout[id];

    }

    private void ChangeLayersRec(GameObject target, int layer)
    {
        target.layer = layer;
        foreach(Transform a in target.transform)
        {
            ChangeLayersRec(a.gameObject, layer);
        }
    }

    public void Aim(bool aiming)
    {
        if (!currentWeapon)
        {
            return;
        }

        isAimin = aiming;
        
        Transform temp_anchor = currentWeapon.transform.Find("Anchor");
        Transform temp_state_ads = currentWeapon.transform.Find("States/Ads");

        Transform temp_state_hip = currentWeapon.transform.Find("States/Hip");

 

        if (aiming)
        {
             temp_anchor.position = Vector3.Lerp(temp_anchor.position, temp_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
        }
        else
        {
 
            temp_anchor.position = Vector3.Lerp(temp_anchor.position, temp_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);

        }
    }

    [PunRPC]
    void Shoot()
    {
        Transform temp_spawn = transform.Find("Cameras/Camera");

        Vector3 temp_bloom = temp_spawn.position + temp_spawn.forward * 1000f;


        for(int i = 0; i < Mathf.Max(1, CurrentGunData.pellets); i++)
        {
            if (isAimin)
            {
                Debug.Log(isAimin);

                temp_bloom += Random.Range(-loadout[currentIndex].aimBloom, loadout[currentIndex].aimBloom) * temp_spawn.up;
                temp_bloom += Random.Range(-loadout[currentIndex].aimBloom + (loadout[currentIndex].aimBloom) / 2, loadout[currentIndex].aimBloom - (loadout[currentIndex].aimBloom) / 2) * temp_spawn.right;
            }
            if (!isAimin)
            {
                Debug.Log(isAimin);

                temp_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * temp_spawn.up;
                temp_bloom += Random.Range(-loadout[currentIndex].bloom + (loadout[currentIndex].bloom) / 2, loadout[currentIndex].bloom - (loadout[currentIndex].bloom) / 2) * temp_spawn.right;
            }

            temp_bloom -= temp_spawn.position;

            temp_bloom.Normalize();

            cd = loadout[currentIndex].firerate;



            RaycastHit temp_hit = new RaycastHit();
            if (Physics.Raycast(temp_spawn.position, temp_bloom, out temp_hit, 1000f, shootable))
            {

                GameObject temp_bhole = Instantiate(buletHolePrefab, temp_hit.point + temp_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                temp_bhole.transform.LookAt(temp_hit.point + temp_hit.normal);
                Destroy(temp_bhole, 5f);

                if (photonView.IsMine)
                {
                    if (temp_hit.collider.gameObject.layer == 11)
                    {

                        temp_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].dmg, PhotonNetwork.LocalPlayer.ActorNumber);
                        hitmarker.color = Color.white;
                        sfx.PlayOneShot(hitmarkerSound);
                        hitmarkerWait = 0.5f;
                    }
                }
            }

        }


        sfx.clip = CurrentGunData.shotSound;
        sfx.pitch = 1 - CurrentGunData.randomPitch + Random.Range(-CurrentGunData.randomPitch, CurrentGunData.randomPitch);
        sfx.Play();
     
        currentWeapon.transform.Rotate(-loadout[currentIndex].recoil,0,0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;

        if (CurrentGunData.recovery)
        {
            currentWeapon.GetComponent<Animator>().Play("Recovery", 0, 0);
        }
    }

    [PunRPC]
    private void TakeDamage(float damage, int n_actor)
    {
        GetComponent<PlayerScript>().TakeDamage(damage, n_actor);
    }

    [PunRPC]
    private void ReloadRPC()
    {
        StartCoroutine((Reload(loadout[currentIndex].reloadTime)));

    }

    IEnumerator Reload(float wait)
    {
        if (currentWeapon.GetComponent<Animator>())
        {
            currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);

        }
        else
        {
            currentWeapon.SetActive(false);
        }
        isReloading = true;
        yield return new WaitForSeconds(wait);
        loadout[currentIndex].Reload();
        isReloading = false;
        currentWeapon.SetActive(true);

    }

}
