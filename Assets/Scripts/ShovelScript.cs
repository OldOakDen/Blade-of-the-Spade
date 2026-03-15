using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ShovelScript : MonoBehaviour
{
    public DetectManager detectManager;
    public HeadCameraScript headCameraScript;

    public Transform holePosition;
    public Transform environmentParent;
    public GameObject soilPile;
    public GameObject soilPileEffect;

    private bool firstDig=false;

    public void PlayDiggingSound()
    {
        AudioManager.Instance.PlaySFX("Digging01");
        //print("poustim zvuk kopani");
    }

    public void TurnOffDigging()
    {
        detectManager.TurnOffRingAndAnim();
    }

    public void PlayShovelSound()
    {
        AudioManager.Instance.PlaySFX("Shovel");
    }

    public void AnimationEnded()
    {
        detectManager.diggingCycles--;
        //print("Zbyva: " + detectManager.diggingCycles + " cyklu kopani rycem.");
        GetComponent<Animator>().SetInteger("diggingCycles", detectManager.diggingCycles);
    }

    public void FirstDig()
    {
        firstDig = true;
    }

    public void ShovelImpact() //efekt pri narazu ryce na zem pri kopani
    {
        StartCoroutine(headCameraScript.ShakeWithCamera(0.1f, 0.1f));
        // vytvor instanci hromady hliny - pouze vsak pri prvnim kopnuti nad signalem
        if(firstDig)
        {
            GameObject novaInstance = Instantiate(soilPile, holePosition.transform.position, Quaternion.identity);
            novaInstance.transform.SetParent(environmentParent);
            firstDig = false;
        }
        GameObject novePartikly = Instantiate(soilPileEffect, holePosition.transform.position, Quaternion.identity);
        novePartikly.transform.SetParent(environmentParent);
        Destroy(novePartikly, 5f);

    }

    public void TargetRevealing() //funkce, ktera zobrazi hledany predmet, pokud ma byt vyzvednut
    {
        //funkce by mela zapricinit vyzvednuti cile pri poslednim cyklu kopnuti (podle hloubky ulozeni predmetu)
        //musi tedy testovat, jestli se jedna o posledni kopnuti a nazaklade toho potom odhalit predmet
        if (detectManager.diggingCycles <= 0 && detectManager.actualSignal)
        {
            //detectManager.objectPreview.GetComponent<ObjectPreview>().currentMode = ObjectPreview.PreviewMode.Discovery;
            detectManager.objectPreview.GetComponent<ObjectPreview>().InitializeMode(ObjectPreview.PreviewMode.Discovery);
            detectManager.objectPreview.SetActive(true); //zapne predvedeni vykopaneho predmetu (tohle ale musi byt poreseno nejak efektneji behem kopani)
            detectManager.NewDiscovery();
            //print("NALEZ PRI KOPANI OBJEVEN!");
        }
    }
}
