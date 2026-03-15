using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorSwing : MonoBehaviour
{
    public DetectManager detectManager;

    public Transform centerTargetObject;
    public Transform leftTargetObject;
    public Transform rightTargetObject;
    public Transform leftTargetObjectClose;
    public Transform rightTargetObjectClose;

    public GameObject coil;
    
    public float speed = 5f;

    Transform leftLimit;
    Transform rightLimit;

    string coilDirection = "right";

    // Start is called before the first frame update
    void Start()
    {
        leftLimit = leftTargetObject;
        rightLimit = rightTargetObject;
        coilDirection = "right";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            leftLimit = leftTargetObjectClose;
            rightLimit = rightTargetObjectClose;
        }
        else
        {
            leftLimit = leftTargetObject;
            rightLimit = rightTargetObject;
        }

        Vector3 direction;

        if (coilDirection == "right")
        {
            direction = rightLimit.position - transform.position;
        }
        else
        {
            direction = leftLimit.position - transform.position;
        }

        if (!detectManager.pinpoint)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, speed * Time.deltaTime);


            if (transform.rotation == rotation) //zmena smeru
            {
                if (coilDirection == "right")
                {
                    coilDirection = "left";
                }
                else
                {
                    coilDirection = "right";
                }

                if (Random.Range(1, 3) == 1) //pust zvuk machnuti jen obcas
                {
                    AudioManager.Instance.PlaySFX("CoilSwing");
                }

            }
        }
        else //jinak se ale asi detektor nachazi v pinpoint modu a v tom pripade se civka vycentruje a pohybuje s ni hrac pomoci otaceni
        {
            direction = centerTargetObject.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, speed * Time.deltaTime);
        }
    }
}
