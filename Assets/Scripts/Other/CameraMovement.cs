using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 rotation;
    private void Start()
    {
        rotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        float tempsin = Mathf.Sin(Time.realtimeSinceStartup / 4);
        //transform.position = new Vector3(15 * Mathf.Cos(Time.realtimeSinceStartup / 4), 2 * tempsin + 6, 25 * tempsin);
        
        transform.rotation = Quaternion.Euler(rotation.x, rotation.y -14.5f * Time.realtimeSinceStartup - 90, rotation.z);
    }
}   