using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetectCamMovement : MonoBehaviour
{
    public Vector2 camVertAngle;
    // Start is called before the first frame update
    void Start()
    {
       // transform.LookAt(GameObject.Find("Detector").transform);
    }

    // Update is called once per frame
    void Update()
    {
        camVertAngle.y += Input.GetAxis("Mouse Y");
        transform.localRotation = Quaternion.Euler(camVertAngle.y, 0, 0);
    }
}
