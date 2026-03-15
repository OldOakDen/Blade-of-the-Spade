using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiggingCollisions : MonoBehaviour
{
    public DetectManager detectManager;
    private void OnTriggerStay(Collider collision)
    {
        // pokud je co vykopat, vykopej to (kopej ale jen to, na co dosahne zvolena civka)
        
        detectManager.actualSignal = collision.gameObject;
        //print("neco jde vykopat "+ detectManager.targetUnderMark);
    }

    private void OnTriggerExit(Collider other)
    {

        detectManager.actualSignal = null;
        //print("cil opustil moznost vykopani " + detectManager.targetUnderMark);
    }
}
