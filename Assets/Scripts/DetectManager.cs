using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;
using TMPro;
using System;
using System.Threading.Tasks;

public class DetectManager : MonoBehaviour
{
    public SixthSenseMeter sixthSenseMeter;

    //public TextMeshProUGUI myLocalizedText;
    public HintTextBox hintTextbox;
    public TextMeshProUGUI scoreText;

    public int score = 0;

    public GameObject player; // objekt hrace pro pristup k jeho komponentum
    public GameObject centerMark; //znacka uprostred pro civku atd.
    public GameObject detectRing; //znacka, ktera se ukazuje nam iste civky pri moznosti kopat
    public GameObject swingArm; // ruka s detektorem

    public GameObject objectPreview; //predvedeni vykopaneho predmetu
    public GameObject identifyForm; //formular a script identifikace predmetu
    public GameObject gridButton;

    public Transform detectorHideout; //misto, kde se schovava detektor (swing arm)
    public GameObject coil;
    public GameObject coilColliderShape;
    public Animator shovelAnimator;
    public Animator playerAnimator;
    public GameObject diggingAnim;
    public int diggingCycles = 0; //pocet spusteni animace kopani podle hloubky cile
    public float itemDamage; //procentuleni poskozeni predmetu behem vykopani
    public int revealAcccr; //presnost lokalizace predmetu v procentech 
    public FoundItemsManager foundItemsManager;

    public bool targetUnderMark = false;
    public bool digNow = false;
    public int nrOfFinds = 0; //pocet ucinenych nalezu
    
    public GameObject actualSignal = null; //pokud je detekovan nejaky nalez pod civkou, bude v teto promenne
    public GameObject sceneUI;
    public bool pinpoint = false;
    public Vector2 pinpointMousePosition; //pro zapamatovani si pozice civky pri spusteni pinpointu
    public Transform mapMarkCoords; //pozice pro umistovani znacek nalezu na mape
    public GameObject mapFindMark; //prefab znacky nalezu na mape

    // Start is called before the first frame update
    private async void Start()
    {
        if (!digNow)
        {
            detectRing.SetActive(false); //pro jistotu povypinam objekty, ktery nemaji byt videt
            diggingAnim.SetActive(false);
        }

        AudioManager.Instance.PlayLoopAmbient("Ambient01"); //zapni ambientni zvuk hledani

        //nacteni score z cloudu
        int loadedScore = await CloudSaveManager.Instance.LoadScoreForLocation();
        Debug.Log("Loaded score: " + loadedScore);
        AddScore(loadedScore);
        Debug.Log("Updated score: " + score);

        hintTextbox.DisplayLocalizedText("MetalDetectingSceneTable", "dtct_hint_welcome", "dtct_hint_keys", "dtct_hint_shift", "dtct_hint_mouse", "dtct_hint_mousebuttL", "dtct_hint_mousebuttR", "dtct_hint_spacetodig");
    }

    // Update is called once per frame
    void Update()
    {
        // pokud hrac neni v pinpoint ani kopacim modu a stiskne TAB, otevre se prohlizec nalezu
        //Mod ITEM DISPLAY
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!pinpoint && !digNow && !objectPreview.activeSelf)
            {
                print("Prohlizeni nalezu.");
                StopAllCoroutines();
                StartCoroutine(ShiftCoil(detectorHideout.transform.position));//odsunu detektor
                player.GetComponent<PlayerMoves>().enabled = false;//zakazu pohyb hrace
                                                                   // Nastavíme režim
                                                                   //ObjectPreview previewComponent = objectPreview.GetComponent<ObjectPreview>();
                                                                   //previewComponent.currentMode = ObjectPreview.PreviewMode.Examination;

                // Poté aktivujeme objectPreview a zinicializujeme pro Examination
                objectPreview.SetActive(true);
                objectPreview.GetComponent<ObjectPreview>().InitializeMode(ObjectPreview.PreviewMode.Examination);
                
            }
            else if (objectPreview.activeSelf && objectPreview.GetComponent<ObjectPreview>().currentMode == ObjectPreview.PreviewMode.Examination)
            {
                StopAllCoroutines();
                Destroy(objectPreview.GetComponent<ObjectPreview>().previewedTarget);//destroynu prohlizeny objekt
                objectPreview.GetComponent<ObjectPreview>().currentMode = ObjectPreview.PreviewMode.Discovery; //pro jistotu prehodim mod prohlizeni predmetu pro pristi zapnuti objectPreview
                objectPreview.GetComponent<ObjectPreview>().findsScrollView.SetActive(false);
                objectPreview.SetActive(false);
                pinpoint = false; //pokud doslo nechtene k zapnuti pinpointu, tohle bude eliminovano
                print("Konec prohlizeni nalezu.");
                StartCoroutine(ShiftCoilBack(player.transform.position));
            }
        }

        //Mod PINPOINT
        if (Input.GetMouseButtonUp(1) && !pinpoint && !digNow) // pokud hrac stiskne leve mysitko a neni jeste v pinpoint modu a zaroven v kopacim modu, prepne se do nej
        {
            pinpoint = true;
            pinpointMousePosition.x = coil.transform.position.x;
            hintTextbox.DisplayLocalizedText("MetalDetectingSceneTable", "dtct_hint_pinpointHint", "dtct_hint_spacetodig", "dtct_hint_mousebuttR"); //hint text pro pinpoint
        }
        else if (Input.GetMouseButtonUp(1) && pinpoint) //kdyz hrac stiskne prave mysitko a byl v pinpointu, tento se vypne a civka zacne opet kmitat
        {
            pinpoint = false;
        }


        //Mod DIGGING
        if (Input.GetKeyDown(KeyCode.Space) && !digNow && !objectPreview.activeSelf) //pokud bylo zmacknuto tlacitko pro kopani, aktivuj kopaci mod a deaktivuj hledaci mod
        {
            StopAllCoroutines();
            hintTextbox.DisplayLocalizedText("MetalDetectingSceneTable", "dtct_hint_mouseLdig", "dtct_hint_spacetodtct");
            digNow = true;
            //vypni pohyb civky a odsun ji
            swingArm.GetComponent<DetectorSwing>().enabled = false;
            StartCoroutine(ShiftCoil(detectorHideout.transform.position));

            diggingAnim.SetActive(true); //ukaz rycek
            detectRing.SetActive(true);//zobraz znacku, ktera urcuje misto kopani
        }
        else if (Input.GetKeyDown(KeyCode.Space) && digNow && diggingCycles == 0) //pokud je znovu stisknuto tlacitko pro mod kopani a hrac uz byl v tomto modu, znamena to, ze chce mod kopani vypnout (to ale nelze, pokud probihaji animace kopani)
        {
            //pokud byl hrac v preview modu, tak je potreba nejdriv ulozit stav urcovani predmetu a potom znicit objekt, ktery byl predvaden
            // Zavolej asynchronní metodu pro aktualizaci stavu
            if (objectPreview.activeSelf)
            {
                _ = UpdateStatusAsync(identifyForm.GetComponent<ItemIdentification>().detectTarget.itemID, identifyForm.GetComponent<ItemIdentification>().targetIDstat); // Podtržítko znamená, že ignorujeme návratovou hodnotu
                CloudSaveManager.Instance.SaveScoreForLocation(score); //ulozim skore pro tuto lokaci do cloudu
                Destroy(objectPreview.GetComponent<ObjectPreview>().previewedTarget);
            }

            AudioManager.Instance.loopSfxSource.Stop();
            hintTextbox.DisplayLocalizedText("MetalDetectingSceneTable", "dtct_hint_keys", "dtct_hint_shift", "dtct_hint_mouse", "dtct_hint_mousebuttL", "dtct_hint_mousebuttR", "dtct_hint_spacetodig");
            StopAllCoroutines();
            coil.SetActive(true); //pro jistotu se zapne objekt detektoru
            objectPreview.SetActive(false); //vypne se pro jistotu preview vykopaneho predmetu
            identifyForm.SetActive(false); // vypne formular a script pro identifikoveni predmetu
            gridButton.SetActive(false); //vypne se button pro vypinani a zapinani gridu

            digNow = false;
            pinpoint = false;

            shovelAnimator.SetBool("outOfDigging", true);

            //diggingAnim.SetActive(false);
            //detectRing.SetActive(false);//schovej znacku, ktera urcuje misto kopani
            
            //player.GetComponent<PlayerMoves>().enabled = true;//povolim pohyb hrace
            StartCoroutine(ShiftCoilBack(player.transform.position));

            //myLocalizedText.text = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("InitialTable", "dtct_hint_spacetodig").Result;
        }
        // Proces kopani
        if (Input.GetMouseButtonDown(0) && digNow && !objectPreview.activeSelf) //jde se kopat
        {
            player.GetComponent<PlayerMoves>().enabled = false;//vypnu pohyb hrace
            playerAnimator.SetBool("isMoving", false); //vypnu animaci pohybu
            
            if (actualSignal) //pokud existuje nejaky kopany signal, neco s nim udelej
            {
                //pokud se uz kope, musi nesmi byt detekovano stisknuti mezerniku
                //print("Vykopan cil " + actualSignal.name + " v hloubce " + actualSignal.transform.position.y + "!");
                //pokud je cil v hloubce mensi nez -1, kopni 1x
                //pokud je cil v hloubce vetsi nez -1, kopni 2x

                //object preview by se mel zapnout az pri poslednim kopnuti a behem faze animace - vyhozu hliny
                //objectPreview.SetActive(true); //zapne predvedeni vykopaneho predmetu (tohle ale musi byt poreseno nejak efektneji behem kopani)

                diggingCycles = CountDiggingCycles(actualSignal.transform);

                //budou asi max tri kopani podle hloubky, zatim je to experimentalni

                shovelAnimator.SetInteger("diggingCycles", diggingCycles);
                //print("Animace kopani bude pustena: " + diggingCycles + "x.");

                //dale zjistim vzdalenost predmetu od stredu civky v osach x a z pro vypocet poskozeni predmetu behem vykopani
                itemDamage = CountItemDamage(actualSignal.transform);
                revealAcccr = AccuracyReveal(actualSignal.transform);

            }
            else //pokud signal neexistuje, neco udelej (treba napis, ze to tady nebudes bezduvodne preryvat)
            {
                // spis bych to ale videl na jedno kopnuti a pokud se nejaky signal nachazi v hloubce jednoho kopnuti, tak to proste hrac vykope
                diggingCycles = 1;
                shovelAnimator.SetInteger("diggingCycles", diggingCycles);
            }
                     
        }
    }

    public void NewDiscovery()
    {
        //tady neco udelam s cilem, ktery je zamereny (pokud nejaky je)
        string materialOfSIgnal = actualSignal.GetComponent<DetectTarget>().itemName;
        nrOfFinds++; //pricti novy nalez
        foundItemsManager.AddFoundItem(actualSignal.GetComponent<DetectTarget>().itemID);
        // vypis pocet nalezu do UI
        //print ("TREASURES: " + nrOfFinds + ". LAST ONE: " + materialOfSIgnal + ".");

        //zaznamenam misto nalezu na mape
        GameObject mapCross = Instantiate(mapFindMark, this.transform);
        mapCross.transform.position = mapMarkCoords.position;
    }

    IEnumerator ShiftCoil(Vector3 destination)
    {
        swingArm.GetComponent<DetectorSwing>().enabled = false; //prestan mavat detektorem
        Vector3 start = swingArm.transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < 0.7f) //presun civku
        {
            swingArm.transform.position = Vector3.Lerp(start, destination, elapsedTime / 0.7f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        swingArm.transform.position = destination;
        coil.SetActive(false); //docasne vypni detektor
    }

    IEnumerator ShiftCoilBack(Vector3 destination)
    {
        player.GetComponent<PlayerMoves>().enabled = false;//zakazu pohyb hrace
        Vector3 start = swingArm.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < 0.5f)
        {
            swingArm.transform.position = Vector3.Lerp(start, destination, elapsedTime / 0.5f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        swingArm.transform.position = destination;
        //nastrc zpet civku a rozpohybuj ji
        swingArm.GetComponent<DetectorSwing>().enabled = true;
        player.GetComponent<PlayerMoves>().enabled = true;//povolim pohyb hrace
        coil.SetActive(true); //zapni detektor
    }

    public void TurnOffRingAndAnim()
    {
        diggingAnim.SetActive(false);
        detectRing.SetActive(false);//schovej znacku, ktera urcuje misto kopani
    }

    public int CountDiggingCycles(Transform targetTransform) //vraci pocet cyklu kopani, podle toho v jake tretine dosahu civky se cil nachazi
    {
        Vector3 coilCenter = coilColliderShape.transform.position;
        Vector3 targetCenter = targetTransform.position;
        
    coilCenter.x = 0; //vynuluju osu x a z, abych meril pouze vzdalenost v ose Y
        targetCenter.x = 0;
        coilCenter.z = 0;
        targetCenter.z = 0;
        float targetDepth = Vector3.Distance(coilCenter, targetCenter); //zmeri se vzdalenost predmetu od stredu civky - hloubka
                                                                  //Debug.DrawLine(coilCenter, targetCenter, Color.red);
                                                                  //odectu 1/2 height kolideru a tak alespon trosku nasimuluju velikost predmetu
        float coilDepthRange = coilColliderShape.GetComponent<CapsuleCollider>().height; //polovina vysky detekcni capsule, coz je vlastne dosah civky (100% dosahu)
                                                                 // Pøepoèet na procenta
     
        // Rozdìlení èísla na tøetiny
        float oneThird = coilDepthRange / 3f;
        float twoThirds = oneThird * 2f;
        
        // Testování, do které tøetiny hloubkoveho dosahu nalez patøí
        if (targetDepth <= oneThird)
        {
            // Vraci pocet cyklu kopnuti
            return 1;
        }
        else if (targetDepth <= twoThirds)
        {
            // Vraci pocet cyklu kopnuti
            return 2;
        }
        else
        {
            // Vraci pocet cyklu kopnuti
            return 3;
        }
    }

    public int CountItemDamage(Transform targetTransform) //vypocet poskozeni predmetu nepresnym zamerenim pred vykopanim
    {
        // Získání pozice støedù objektù
        Vector3 coilCenterMark = centerMark.transform.position;
        Vector3 targetCenter = targetTransform.position;

        // Ignorování osy y, aby se vzdálenost poèítala pouze v rovinì xz
        coilCenterMark.y = 0;
        targetCenter.y = 0;

        // Vypoèítání vzdálenosti mezi støedy objektù v rovinì xz
        float distance = Vector3.Distance(coilCenterMark, targetCenter);

        // Získání polomìru kapsule
        float coilRadius = coilColliderShape.GetComponent<CapsuleCollider>().radius;

        // Polomìr cívky pøedstavuje 100% možné vzdálenosti
        float fullDistance = coilRadius * 2; // Polomìr * 2 odpovídá prùmìru cívky

        // Vypoèítání procenta vzdálenosti vzhledem k plné možné vzdálenosti
        float distancePercentage = (distance / fullDistance) * 100;

        // Vydìlení výsledku cislem damageResistance a zaokrouhlení na celá èísla
        int roundedResult = Mathf.FloorToInt(distancePercentage / actualSignal.GetComponent<DetectTarget>().damageResistance); //duvodem je nefrustrovat hrace prilisnym poskozenim predmetu
        if (roundedResult < 0) { roundedResult = 0; }; //pro sychr
        //print("Vzdalenost objektu od stredu civky: " + distance + " + plna vzdalenost: " + fullDistance);

        return roundedResult;
    }

    public int AccuracyReveal(Transform targetTransform) // Vypocet presnosti lokalizace artefaktu
    {
        // Získání pozice støedù objektù
        Vector3 coilCenterMark = centerMark.transform.position;
        Vector3 targetCenter = targetTransform.position;

        // Ignorování osy y, aby se vzdálenost poèítala pouze v rovinì xz
        coilCenterMark.y = 0;
        targetCenter.y = 0;

        // Vypoèítání vzdálenosti mezi støedy objektù v rovinì xz
        float distance = Vector3.Distance(coilCenterMark, targetCenter);

        // Získání polomìru kapsule
        float coilRadius = coilColliderShape.GetComponent<CapsuleCollider>().radius;

        // Polomìr cívky pøedstavuje 100% možné vzdálenosti
        float fullDistance = coilRadius; // Polomìr pøedstavuje 100% pøesnosti

        // Vypoèítání procenta pøesnosti
        float accuracyPercentage = Mathf.Clamp01(1 - (distance / fullDistance)) * 100;

        int roundedResult = Mathf.FloorToInt(accuracyPercentage);
        if (roundedResult < 5) { roundedResult = 5; } // Pro jistotu

        // Debug výstup
        //print("Vzdálenost objektu od støedu cívky: " + distance + " + plná vzdálenost: " + fullDistance);

        return roundedResult;
    }

    public void AddScore(int scorePlus)
    {
        score = score + scorePlus;
        scoreText.text = "Score: " + score.ToString();
    }

    private async Task UpdateStatusAsync(string itemID, string newStatus)
    {
        bool success = await CloudSaveManager.Instance.UpdateItemIdentificationStatus(itemID, newStatus);

        if (success)
        {
            Debug.Log("Status successfully updated!");
        }
        else
        {
            Debug.LogWarning("Failed to update the status.");
        }
    }
}
