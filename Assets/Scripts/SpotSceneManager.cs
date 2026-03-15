using UnityEngine;
using UnityEngine.UI;

public class SpotSceneManager : MonoBehaviour
{
    public static SpotSceneManager Instance;

    // Reference na UI sesteho smyslu
    public GameObject ssmUI;
    private bool fullFocusIndicator = false; //testovaci promenna pro circle bar
    public Image focusCircleBar; //vlastni circle bar pro spotovani objektu a lokalit
    public float fillDuration; // Doba trvání vyplòování (v sekundách)
    private float elapsedTime = 0f; // Uplynulý èas
    private bool focusSound = false;

    public GameObject focusedObject = null; //objekt, ktery je prave zkouman, pokud tedy existuje

    private void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        //asi by nebylo od veci otestovat, jestli je promenna v manageru prazdna pokud ano a tento objekt se nachazi v oblasti focusu a hrac stoji, tak by mohlo zacit reagovat prozkoumavani
        //ale musi se to promyslet, aby to nedelalo kokotiny

        if (!Input.anyKey && !fullFocusIndicator) //pokud neni stisknuta zadna klavesa indikator hledani na tomto miste jeste nebyl full
        {

            // Zvyšte uplynulý èas
            elapsedTime += Time.deltaTime;

            // Vypoèítejte aktuální hodnotu fillAmount (0-1)
            float fillAmount = Mathf.Clamp01(elapsedTime / fillDuration);
            // Nastavte fillAmount na Image
            focusCircleBar.fillAmount = fillAmount;

            if (!focusSound)
            {
                AudioManager.Instance.PlayDetectSound("Focusing");
                focusSound = true;
            }

            // Pokud je fillAmount 1 (100 %), zastavte vyplòování
            if (fillAmount >= 1f)
            {
                fullFocusIndicator = true;

                AudioManager.Instance.detectSoundsSource.Stop();
                //Debug.Log("Image bylo úspìšnì vyplnìno!");
                //pokud ecistuje predmet, ktery je mozne objevit na tomto miste a neni jest eobjeven
                if (focusedObject && !focusedObject.GetComponent<DistanceScript>().isDiscovered)
                {
                    //objev ho a neco s nim udelej
                    focusedObject.GetComponent<DistanceScript>().isDiscovered = true;
                    //pro tentokrat ho asi i destroynu
                    Debug.Log("OBJEKT OBJEVEN!");
                    ObjectIsNear();
                    Destroy(focusedObject);
                }
            }
        }
        else if (Input.anyKey)
        {
            elapsedTime = 0;
            focusCircleBar.fillAmount = 0f;
            fullFocusIndicator = false;
            focusSound = false;
            AudioManager.Instance.detectSoundsSource.Stop();
        }

    }
    public void ObjectIsNear()
    {
        print("OBJEKT JE NADOHLED!");
        AudioManager.Instance.PlaySFX("Heartbeat");
        ssmUI.GetComponent<Animation>().Play("SSM_blink");
    }
}
