using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Look : MonoBehaviourPunCallbacks
{

    public Transform player;
    public Transform cam;
    public Transform weaponCam;

    public Transform weapon;

    public static bool locked = true;
    public float xsens;
    public float ysens;
    public float maxAngle;

    private Quaternion center;

    void Start()
    {
        center = cam.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!photonView.IsMine)
        {
            return;
        }
        if (Pause.pause)
        {
            return;
        }

        SetY();
        SetX();
        UpdateCursorLock();
        weaponCam.rotation = cam.rotation;
    }

    void SetY()
    {

        float temp_input = Input.GetAxis("Mouse Y") * ysens * Time.deltaTime;
        Quaternion temp_adj = Quaternion.AngleAxis(temp_input, -Vector3.right);
        Quaternion temp_delta = cam.localRotation * temp_adj;

        if (Quaternion.Angle(center, temp_delta) < maxAngle)
        {
            cam.localRotation = temp_delta;
            weapon.localRotation = temp_delta;

        }
        weapon.rotation = cam.rotation;
    }

    void SetX()
    {
        float temp_input = Input.GetAxis("Mouse X") * xsens * Time.deltaTime;
        Quaternion temp_adj = Quaternion.AngleAxis(temp_input, Vector3.up);
        Quaternion temp_delta = player.localRotation * temp_adj;
        player.localRotation = temp_delta;

    }
    
    void UpdateCursorLock()
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape)){
                locked = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape)){
                locked = true;
            }
        }
    }
}
