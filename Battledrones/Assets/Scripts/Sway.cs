using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sway : MonoBehaviour
{

    public float intensity;
    public float smooth;
    public bool isMine;

    private Quaternion originRotation;

    // Start is called before the first frame update
    private void Update()
    {
        if (Pause.pause)
        {
            return;
        }

        UpdateSway();
    }

    private void Start()
    {
        originRotation = transform.localRotation;
    }

    // Update is called once per frame
    private void UpdateSway()
    {
        float temp_x_mouse = Input.GetAxis("Mouse X");
        float temp_y_mouse = Input.GetAxis("Mouse Y");

        if (!isMine)
        {
            temp_x_mouse = 0;
            temp_y_mouse = 0;

        }

        Quaternion temp_x_adj = Quaternion.AngleAxis( intensity * temp_x_mouse, Vector3.up);
        Quaternion temp_y_adj = Quaternion.AngleAxis(intensity * temp_y_mouse, Vector3.right);

        Quaternion targetRotation = originRotation * temp_x_adj * temp_y_adj;

        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smooth);

    }
}
