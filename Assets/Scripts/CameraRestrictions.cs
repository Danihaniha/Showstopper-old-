using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRestrictions : MonoBehaviour
{
    public Vector3 boundsMax;
    public Vector3 boundsMin;

    public GameObject cameraToMove;

    void Update()
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, boundsMin.x, boundsMax.x), Mathf.Clamp(transform.position.y, boundsMin.y, boundsMax.y), Mathf.Clamp(transform.position.z, boundsMin.z, boundsMax.z));
        //if (Input.GetKeyDown(KeyCode.E))
        //{
            //cameraToMove.transform.position = new Vector3(50, 0, -10);
            //Debug.Log("Camera moved! Hoozah!");
        //}
    }
}
