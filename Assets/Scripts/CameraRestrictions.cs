using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRestrictions : MonoBehaviour
{
    public Vector3 boundsMax;
    public Vector3 boundsMin;

    public GameObject cameraToMove;
    public int currentRoom = 0;
    public float offsetRoom = 50f;


    void Update()
    {
        //if (ButtonPress)
        //{
          //  transform.position = offsetRoom * currentRoom;
        //}

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, currentRoom * offsetRoom +boundsMin.x, currentRoom * offsetRoom +boundsMax.x), 0f, -10f);
    }
}
