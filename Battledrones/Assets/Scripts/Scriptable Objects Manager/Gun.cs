using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 
public class Gun : ScriptableObject
{
    public string name;
    public GameObject prefab;
    public float firerate;
    public float bloom;
    public float aimBloom;
    public bool recovery;
    public float pellets;
    public float dmg;
    public float recoil;
    public float kickback;
    public float aimSpeed;
    public float reloadTime;
    public int burst;

    public AudioClip shotSound;
    public float randomPitch;
    public float mainFov;
    public float weaponFov;
    public int maxAmmo;
    public int clipSize;

    private int clip;
    private int stash;
    

    public void Initialise()
    {
        stash = maxAmmo;
        clip = clipSize;
    }
    public bool FireBullet()
    {
        if (clip > 0)
        {
            clip -= 1;
            return true;
        }
        else return false;

    }

    public void Reload()
    {
        stash += clip;
        clip = Mathf.Min(clipSize, stash);
        stash -= clip;
        Debug.Log("reloading");
    }

    public int GetStash()
    {
        return stash;
    }

    public int GetClip()
    {
        return clip;
    }

   



}
