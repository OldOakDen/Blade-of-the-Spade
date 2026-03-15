using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CoilMovement : MonoBehaviour
{
    public Transform centerTargetObject;
    public Transform leftTargetObject;
    public Transform rightTargetObject;
    public Transform leftTargetObjectClose;
    public Transform rightTargetObjectClose;
    Transform leftLimit;
    Transform rightLimit;
    public Transform playerPosObject;
    public float speed = 25;
    public float smoothTime = 10f;
    public GameObject coilObject;
    private string coilDirection;
    public DetectManager detectManager;
    
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
        VisualElement detectingUI = detectManager.sceneUI.GetComponent<UIDocument>().rootVisualElement;
        detectingUI.Q<Label>("DebugText").text = "LEFT_TRGT x:" + leftTargetObject.transform.position.x + " - Coil x:"+ transform.position.x + " Direction:"+ coilDirection;

       /*if (Input.GetMouseButton(0))
        {
            leftLimit = leftTargetObjectClose;
            rightLimit = rightTargetObjectClose;
        }
        else
        {
            leftLimit = leftTargetObject;
            rightLimit = rightTargetObject;
        }*/

        if (!detectManager.pinpoint) //pokud se detektor nenachazi v modu pinpointu, pohybuje se civka automaticky
        {
            if (coilDirection == "right" && transform.position.x < rightLimit.position.x)
            {
                transform.position = Vector3.MoveTowards(transform.position, rightLimit.position, speed * Time.deltaTime);

                //nasleduje kod pro automaticky pohyb civky - SMAZAT, pokud se bude civka pohybovat pomoci mysi
                if (transform.position.x >= rightLimit.position.x)
                {
                    coilDirection = "left";
                    if (Random.Range(1, 3) == 1) //pust zvuk machnuti jen obcas
                    {
                        AudioManager.Instance.PlaySFX("CoilSwing");
                    }
                }
            }
            else if (coilDirection == "left" && transform.position.x > leftLimit.position.x)
            {
                transform.position = Vector3.MoveTowards(transform.position, leftLimit.position, speed * Time.deltaTime);

                //nasleduje kod pro automaticky pohyb civky - SMAZAT, pokud se bude civka pohybovat pomoci mysi
                if (transform.position.x <= leftLimit.position.x)
                {
                    coilDirection = "right";
                    if (Random.Range(1, 3) == 1) //pust zvuk machnuti jen obcas
                    {
                        AudioManager.Instance.PlaySFX("CoilSwing");
                    }
                }
            }
        }
        else //jinak se ale asi detektor nachazi v pinpoint modu a v tom pripade se civka vycentruje a pohybuje s ni hrac pomoci otaceni
        {
           transform.position = Vector3.MoveTowards(transform.position, centerTargetObject.transform.position, speed * Time.deltaTime);
        }

       

        transform.LookAt(playerPosObject); //natoc civku spravne (pouze vizualni efekt)
    }
}
