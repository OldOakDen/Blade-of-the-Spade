using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionCollision : MonoBehaviour
{
    public DetectManager detectManager;
    private bool pinpointSound = false;
    [SerializeField] private float targetMaxDistance = 0;
    [SerializeField] private float targetActualDistance = 0;
    [SerializeField] private float targetDepth = 0;
    [SerializeField] private float coilDepthRange = 0;
    [SerializeField] private float pitchPercents = 0;

    // Rozdìlení èísla na tøetiny
    [SerializeField] float oneThird;
    [SerializeField] float twoThirds;

    [SerializeField] private Material emissionMaterial; // Materiál s Emission
    [SerializeField] Vector4  emissionColor;


    private Vector3 coilCenter;
    private Vector3 targetCenter;

    private AudioLowPassFilter lowPassFilter;

    private void Start()
    {
        if (AudioManager.Instance.detectSoundsSource == null)
        {
            Debug.LogError("detectSoundsSource is not assigned in AudioManager.");
            return;
        }

        // Pøidání AudioLowPassFilter komponenty, pokud neexistuje
        lowPassFilter = AudioManager.Instance.detectSoundsSource.GetComponent<AudioLowPassFilter>();
        if (lowPassFilter == null)
        {
            lowPassFilter = AudioManager.Instance.detectSoundsSource.gameObject.AddComponent<AudioLowPassFilter>();
        }
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity; //collider se nesmi vychylovat
        //emissionMaterial.SetColor("_EmissionColor", Color.green * pitchPercents); //nastavim magicke oko pohasnute
    }

    private void OnTriggerEnter(Collider collision)
    {
        //dam vedet DetectManagerovi, ze cil je pod civkou
        detectManager.actualSignal = collision.gameObject;
        //Debug.Log(detectManager.GetComponent<DetectManager>().actualSignal.GetComponent<DetectTarget>().itemID + " DETECTED!");
        //Debug.Log("DETECTED!");

        if (!detectManager.pinpoint) //pokud neni detektor v pinpoint modu, spust tento zvuk
        {
            //podle hloubky cile jeste nastav pitch na detectSoundsSource
            coilCenter = transform.position;
            targetCenter = collision.gameObject.transform.position;
            coilCenter.x = 0; //vynuluju osu x a z, abych meril pouze vzdalenost v ose Y
            targetCenter.x = 0;
            coilCenter.z = 0;
            targetCenter.z = 0;
            targetDepth = Vector3.Distance(coilCenter, targetCenter); //zmeri se vzdalenost predmetu od stredu civky - hloubka
            //Debug.DrawLine(coilCenter, targetCenter, Color.red);
            //odectu 1/2 height kolideru a tak alespon trosku nasimuluju velikost predmetu
            coilDepthRange = GetComponent<CapsuleCollider>().height; //polovina vysky detekcni capsule, coz je vlastne dosah civky (100% dosahu)
            // Pøepoèet na procenta
            pitchPercents = (coilDepthRange - targetDepth) + 0.5f;

            // Rozdìlení èísla na tøetiny
            oneThird = coilDepthRange / 3f;
            twoThirds = oneThird * 2f;
            
            // Testování, do které tøetiny hloubkoveho dosahu nalez patøí
            // nemel bych ale nastavovat Pitch zvuku, protoze to pak zrusi rozdeleni materialu podle ton
            // chce to vymyslet neco jineho, napriklad zvuk zeslabovat a zesilovat podle hloubky, to ale muze byt problem s globalnim nastavenim hlasitosti
            if (targetDepth <= oneThird)
            {
                //Debug.Log("Èíslo patøí do první tøetiny.");
                // Nastavte promìnnou podle potøeby
                emissionColor = new Vector4(0f,1.5f,0f,1f);
                lowPassFilter.cutoffFrequency = 5000;
            }
            else if (targetDepth <= twoThirds)
            {
                //Debug.Log("Èíslo patøí do druhé tøetiny.");
                // Nastavte promìnnou podle potøeby
                emissionColor = new Vector4(0.0f, 0.7f, 0.0f, 1f);
                lowPassFilter.cutoffFrequency = 2500;
            }
            else
            {
                //Debug.Log("Èíslo patøí do tøetí tøetiny.");
                // Nastavte promìnnou podle potøeby
                emissionColor = new Vector4(0.0f, 0.3f, 0f, 1f);
                lowPassFilter.cutoffFrequency = 1000;
            }

            AudioManager.Instance.PlayDetectSound(collision.gameObject.GetComponent<DetectTarget>().targetSound);
            //nasledujici nastaveni pitch zvuku je vypnuto, protoze pak nefunguji spravne tony jednotlivych materialu
            //AudioManager.Instance.detectSoundsSource.pitch = soundPitch; //nastavim Pitch
            //emissionMaterial.SetColor("_EmissionColor", new Vector4(0, 0, 0, 1.000f));
            emissionMaterial.SetColor("_EmissionColor", emissionColor); //rozzarim magicke oko na civce
        }

    }

    private void OnTriggerStay(Collider collision)
    {
        // pokud se detektor nachazi v rezimu pinpoint a je stale nad cilem, mej spusten zvuk a reakci na kolizi v rezimu pinpoint
        if(detectManager.pinpoint && !pinpointSound)
        {
            AudioManager.Instance.PlayPinpointSound("Pinpoint"); //zvuk pinpointu
            pinpointSound = true;
            coilCenter = transform.position;
            targetCenter = collision.gameObject.transform.position;
            coilCenter.y = 0; //vynuluju osu y, abych meril pouze vzdalenost v osach X a Z
            targetCenter.y = 0;
            //Debug.DrawLine(coilCenter, targetCenter, Color.red);
            targetMaxDistance = Vector3.Distance(coilCenter, targetCenter); //zmeri se vzdalenost predmetu od stredu civky
            //odectu polomer kolideru a tak alespon trosku nasimuluju velikost predmetu
            targetMaxDistance=targetMaxDistance - GetComponent<CapsuleCollider>().radius;
            if(targetMaxDistance < 0.1)
            {
                targetMaxDistance = 0.1f;
            }
            AudioManager.Instance.pinpointSoundsSource.pitch = 0; //vynuluju Pitch
        }
        //pokud je objekt stale pod civkou a je aktivni pinpoint, upravuj Pitch Audio Source podle toho jak se vzdalenost meni
        if (detectManager.pinpoint && pinpointSound)
        {
            // Aktuální vzdálenost mezi objekty
            coilCenter = transform.position;
            targetCenter = collision.gameObject.transform.position;
            coilCenter.y = 0; //vynuluju osu y, abych meril pouze vzdalenost v osach X a Z
            targetCenter.y = 0;
            targetActualDistance = Vector3.Distance(coilCenter, targetCenter);
            targetActualDistance = targetActualDistance - GetComponent<CapsuleCollider>().radius;
            
            // Pøepoèet na procenta
            pitchPercents = 1 - (targetActualDistance / targetMaxDistance);

            if(pitchPercents>1)
            {
                pitchPercents = 1;
            }
            else if(pitchPercents<0)
            {
                pitchPercents = 0;
            }
           
            // Nastavení hodnoty Pitch
            AudioManager.Instance.pinpointSoundsSource.pitch = pitchPercents;
            emissionMaterial.SetColor("_EmissionColor", Color.green * pitchPercents); //rozzarim magicke oko na civce
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        AudioManager.Instance.detectSoundsSource.Stop(); //vypina zvuky detekce predmetu pri opusteni kolize
        AudioManager.Instance.pinpointSoundsSource.Stop();
        detectManager.actualSignal = null; //vynuluj detekci objektu
        targetMaxDistance = 0;
        targetActualDistance = 0;
        pinpointSound = false;
        pitchPercents = 0;
        AudioManager.Instance.pinpointSoundsSource.pitch = 0; //vynuluju Pitch
        //emissionMaterial.SetColor("_EmissionColor", Color.green * pitchPercents); //pohasnu magicke oko na civce
        emissionMaterial.SetColor("_EmissionColor", new Vector4(0, 0, 0, 1.000f));
    }
}
