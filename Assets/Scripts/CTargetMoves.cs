using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTargetMoves : MonoBehaviour
{
    public Vector2 turn;
    public float speed;

    public Vector2 mouseTurn;
    public string coilDirection;
    public Vector2 pinpointMousePosition; //pro zapamatovani si pozice civky pri spusteni pinpointu
    public bool pinpoint = false;
    float xMouse;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        xMouse = Input.GetAxis("Mouse X");
        //inicializace smeru pohybu civky - JEN POKUD SE BUDE CIVKA POHYBOVAT SAMA, JINAK TOHLE SMAZAT
        coilDirection = "right";
    }

    // Update is called once per frame
    void Update()
    {

        /* otaceni se pomoci mysi
         turn.x += Input.GetAxis("Mouse X");
        //turn.y += Input.GetAxis("Mouse Y");
        turn.y = 0;
        if (Input.GetMouseButton(1))
        {
            transform.localRotation = Quaternion.Euler(-turn.y, turn.x, 0);
        }
        //print(turn.x);
        */

        //otaceni pomoci mysi
        turn.x += Input.GetAxis("Mouse X");
        turn.y += Input.GetAxis("Mouse Y");

        //otaceni se pomoci klaves
        if (Input.GetKey(KeyCode.Q))
        {turn.x -= speed/5;}
        else if (Input.GetKey(KeyCode.E))
        { turn.x += speed/5; }

        transform.localRotation = Quaternion.Euler(turn.y, turn.x, 0);
        //transform.localRotation = Quaternion.Lerp(transform.localRotation, turn, speed * Time.deltaTime);



        mouseTurn.x = Input.GetAxis("Mouse X");
         
        //machani civkou pomoci mysi
        /*
        if (mouseTurn.x < xMouse) // test pohybu mysi kvuli swingu civky do prislusneho smeru
        {
            if(coilDirection== "right" && !pinpoint)
            {
                GetComponent<AudioSource>().Play();
                print("ZMENA SMERU - doleva");
            }
            coilDirection = "left";
            //xMouse = mouseTurn.x;
            //print(coilDirection);
        }
        else if (mouseTurn.x > xMouse)
        {
            if (coilDirection == "left" && !pinpoint)
            {
                GetComponent<AudioSource>().Play();
                print("ZMENA SMERU - doprava");
            }
            coilDirection = "right";
            //xMouse = mouseTurn.x;
            //print(coilDirection);
        }
        */

        if (Input.GetMouseButton(1) && !pinpoint) // pokud hrac stiskne leve mysitko a neni jeste v pinpoint modu, prepne se do nej
        {
            /*if(!pinpoint)//pokud jeste neni spusten pinpoint, tak pred jeho spustenim si uloz pozici kurzoru mysi
            {
                pinpointMousePosition.x = Input.GetAxis("Mouse X");
            }*/
            pinpoint = true;
            pinpointMousePosition.x = Input.GetAxis("Mouse X");
            //xMouse = mouseTurn.x;
            //coilDirection = "";
            //this.GetComponent<AudioSource>().Stop();
        }
        else if(Input.GetMouseButton(1) && pinpoint) //kdyz hrac stiskne prave mysitko a byl v pinpointu, tento se vypne a civka zacne opet kmitat
        {
            pinpoint = false;
            coilDirection = "right";
        }

        //pohyb dopredu a do boku
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        }
    }
}
