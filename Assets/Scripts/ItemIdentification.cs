using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.XR;

public class ItemIdentification : MonoBehaviour
{
    public DetectManager detectManager;
    public DetectTarget detectTarget;

    public string targetIDstat; // string pro ulozeni stavu identifikace predmetu, ktery se po identifikaci ulozi k predmetu do cloudu

    public TMP_Dropdown dropdownPeriod;
    public TMP_Dropdown dropdownMaterial;
    public TMP_Dropdown dropdownType;

    public GameObject imagePeriod;
    public GameObject imageMaterial;
    public GameObject imageType;
    public GameObject imageTotal;

    public GameObject imageNameBckg;

    public Color colorDefault = new Color(1f, 1f, 1f, 1f);
    public Color colorCorrect = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color colorIncorrect = new Color(0.8f, 0.2f, 0.2f, 1f);
    public Color colorScoreCorrect = new Color(0.2f, 0.8f, 0.2f, 1f);
    public Color colorScoreIncorrect = new Color(0.8f, 0.2f, 0.2f, 1f);

    public TMP_Text itemNameText;
    public TMP_Text itemRvlPrc;
    public TMP_Text scoreRevealText;
    public TMP_Text scorePeriodText;
    public TMP_Text scoreMaterialText;
    public TMP_Text scoreTypeText;
    public TMP_Text totalScoreText;

    private Color originalColorImagePeriod;
    private Color originalColorImageMaterial;
    private Color originalColorImageType;
    private Color originalColorImageTotal;

    private int bonusReveal;

    private int scorePeriod = 0;
    private int scoreMaterial = 0;
    private int scoreType = 0;

    private int revealRounds;

    private int variablePeriod;
    private int variableMaterial;
    private int variableType;

    List<string> keysPeriod = new List<string>
        {
            "dtct_hint_idntknw",
            "dtct_hint_prdA",
            "dtct_hint_prdB",
            "dtct_hint_prdC",
            "dtct_hint_prdD",
            "dtct_hint_prdE",
            "dtct_hint_prdF",
            "dtct_hint_prdG"
        };

    List<string> keysMaterial = new List<string>
        {
            "dtct_hint_idntknw",
            "dtct_hint_matG",
            "dtct_hint_matS",
            "dtct_hint_matB",
            "dtct_hint_matC",
            "dtct_hint_matA",
            "dtct_hint_matL",
            "dtct_hint_matI",
        };

    List<string> keysType = new List<string>
        {
            "dtct_hint_idntknw",
            "dtct_hint_typC",
            "dtct_hint_typJ",
            "dtct_hint_typM",
            "dtct_hint_typS",
            "dtct_hint_typU",
            "dtct_hint_typT",
        };


    private Dictionary<int, string> periodMap = new Dictionary<int, string>()
    {
        { 0, "" },
        { 1, "A_StoneAge" },
        { 2, "B_BronzeAge" },
        { 3, "C_IronAge" },
        { 4, "D_ClassicalAntiquity" },
        { 5, "E_MiddleAges" },
        { 6, "F_EarlyModernPeriod" },
        { 7, "G_ModernPeriod" }
    };

    private Dictionary<int, string> materialMap = new Dictionary<int, string>()
    {
        { 0, "" },
        { 1, "G_Gold" },
        { 2, "S_Silver" },
        { 3, "B_Bronze" },
        { 4, "C_CopperBrass" },
        { 5, "A_Aluminium" },
        { 6, "L_Lead" },
        { 7, "I_Iron" }
    };

    private Dictionary<int, string> typeMap = new Dictionary<int, string>()
    {
        { 0, "" },
        { 1, "C_Coins" },
        { 2, "J_Jewelry" },
        { 3, "M_Militaries" },
        { 4, "S_Sculptures" },
        { 5, "U_Usual" },
        { 6, "T_Trash" }
    };

    // Start is called before the first frame update
    void Start()
    {
        originalColorImagePeriod   = imagePeriod.GetComponent<Image>().color;
        originalColorImageMaterial = imageMaterial.GetComponent<Image>().color;
        originalColorImageType     = imageType.GetComponent<Image>().color;
        originalColorImageTotal    = imageTotal.GetComponent<Image>().color;

        InitializeValues();
        StartCoroutine(AddDropdownItems());
    }

    // This method is called when the GameObject is enabled
    void OnEnable()
    {
        InitializeValues();

        //nasledujici bonus se pricita uz ted pro lepsi funkcnost
        bonusReveal = Mathf.Max(detectManager.revealAcccr / 10, 0); //bonus za presne vykopani predmetu
        detectManager.AddScore(bonusReveal);
        UpdateScoreText(scoreRevealText, bonusReveal);
        //UpdateScoreText(totalScoreText, bonusReveal);
        itemRvlPrc.text = detectManager.revealAcccr + "%";

    }

    private void InitializeValues()
    {
        bonusReveal = Mathf.Max(detectManager.revealAcccr / 10, 0); //bonus za presne vykopani predmetu
        detectTarget = detectManager.actualSignal.GetComponent<DetectTarget>();
        variablePeriod = GetDropdownValue(detectTarget.period.ToString(), periodMap);
        variableMaterial = GetDropdownValue(detectTarget.material.ToString(), materialMap);
        variableType = GetDropdownValue(detectTarget.type.ToString(), typeMap);
        //print(detectTarget.itemID + " - " + variablePeriod + " - " + variableMaterial + " - " + variableType);

        //zde je potreba tento predmet ulozit do cloudu jako nalezeny
        CloudSaveManager.Instance.SaveItem(detectTarget.itemID); //ulozim do cloudu id tohoto predmetu pro tuto lokaci

        targetIDstat = "000"; //stav identifikace predmetu - na zacatku je neurcen

        scorePeriod = 0;
        scoreMaterial = 0;
        scoreType = 0;

        UpdateScoreText(scorePeriodText, scorePeriod);
        UpdateScoreText(scoreMaterialText, scoreMaterial);
        UpdateScoreText(scoreTypeText, scoreType);
        UpdateScoreText(totalScoreText, scorePeriod + scoreMaterial + scoreType + bonusReveal);

        dropdownPeriod.interactable = true;
        dropdownMaterial.interactable = true;
        dropdownType.interactable = true;

        dropdownPeriod.value = 0;
        dropdownMaterial.value = 0;
        dropdownType.value = 0;

        SetBackgroundColor(dropdownPeriod.gameObject, colorDefault);
        SetBackgroundColor(dropdownMaterial.gameObject, colorDefault);
        SetBackgroundColor(dropdownType.gameObject, colorDefault);
        SetBackgroundColor(imagePeriod,   originalColorImagePeriod);
        SetBackgroundColor(imageMaterial, originalColorImageMaterial);
        SetBackgroundColor(imageType,     originalColorImageType);
        SetBackgroundColor(imageTotal,    originalColorImageTotal);

        revealRounds = detectManager.actualSignal.GetComponent<ClayEffect>().numClayObjects; //pocet odhalovacich kol - pocet kusu hliny na objektu
        //print("Pocet odhalovacich kol (kusu hliny):" + revealRounds);
        itemNameText.text = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("MetalDetectingSceneTable", "dtct_identFormUnkwn").Result;

        
    }

    private int GetDropdownValue(string value, Dictionary<int, string> map)
    {
        foreach (var kvp in map)
        {
            if (kvp.Value == value)
            {
                return kvp.Key;
            }
        }
        return 0; // Default to 0 if not found
    }

    public void CompareDropdownValues()
    {
        //zde zkusim nacucnout informace o stavu identifikace predmetu. V tomto pripade jde jen o test, protoze predmet, byl prave pridan a je stale neurcen (takze string 000)
        CloudSaveManager.Instance.GetItemIdentificationStatus(detectTarget.itemID).ContinueWith(task =>
        {
            if (task.Result != null)
            {
               // targetIDstat = task.Result; //stav identifikace tohoto predmetu, promenna je momentalne naplnena v incializacnim procesu jako 000
                Debug.Log("Identifika�n� stav p�edm�tu: " + targetIDstat + " Toto je konstrukce pro volani loadu stavidentifikace z Cloudu.");
            }
            else
            {
                Debug.Log("P�edm�t " + detectTarget.itemID + " nebyl nalezen. Toto je konstrukce pro volani loadu stavidentifikace z Cloudu.");
            }
        }); //tuto konstrukci ve finale odstranit, zbytecne zatezuje a navysuje cteni z Cloudu

        int dropdownValue1 = dropdownPeriod.value;
        int dropdownValue2 = dropdownMaterial.value;
        int dropdownValue3 = dropdownType.value;

        int preTotalScore = scorePeriod + scoreMaterial + scoreType;

        if (dropdownValue1 == variablePeriod)
        {
            //prvni kategorie je spravne urcena
            targetIDstat = SetIdentificationState(targetIDstat, 0, '1'); // Nastav�me prvn� kategorii na identifikovan� stav
            dropdownPeriod.interactable = false;
            scorePeriod = scorePeriod + 1;
            UpdateScoreText(scorePeriodText, scorePeriod);
            detectManager.AddScore(1);
            //efekt pricteni bodu
            SetBackgroundColor(dropdownPeriod.gameObject, colorCorrect);
            imagePeriod.GetComponent<FlashEffect>().Flash();
        }
        if (dropdownValue2 == variableMaterial)
        {
            //druha kategorie je spravne urcena
            targetIDstat = SetIdentificationState(targetIDstat, 1, '1'); // Nastav�me druhou kategorii na identifikovan� stav
            dropdownMaterial.interactable = false;
            scoreMaterial = scoreMaterial + 1;
            UpdateScoreText(scoreMaterialText, scoreMaterial);
            detectManager.AddScore(1);
            //efekt pricteni bodu
            SetBackgroundColor(dropdownMaterial.gameObject, colorCorrect);
            imageMaterial.GetComponent<FlashEffect>().Flash();
        }
        if (dropdownValue3 == variableType)
        {
            //treti kategorie je spravne urcena
            targetIDstat = SetIdentificationState(targetIDstat, 2, '1'); // Nastav�me treti kategorii na identifikovan� stav
            dropdownType.interactable = false;
            scoreType = scoreType + 1;
            UpdateScoreText(scoreTypeText, scoreType);
            detectManager.AddScore(1);
            //efekt pricteni bodu
            SetBackgroundColor(dropdownType.gameObject, colorCorrect);
            imageType.GetComponent<FlashEffect>().Flash();
        }

        Debug.Log("Identifika�n� stav p�edm�tu: " + targetIDstat);

        int totalScore = scorePeriod + scoreMaterial + scoreType + bonusReveal;
        UpdateScoreText(totalScoreText, totalScore);
        if(preTotalScore<totalScore)
        {
            imageTotal.GetComponent<FlashEffect>().Flash();
            AudioManager.Instance.PlaySFX("BonusPlus"); //zvuk pricteni bodu
        }
        
        revealRounds--; //odecitani odkryvacich kol (pocet kusu hliny na objektu)
        //print("Zbyva odhalovacich kol: " + revealRounds);
        if (revealRounds == 0) //pokud jsou odstraneny vsechny kusy hliny, otestuj, jestli jsou spravne odhalene vsechny kategorie
        {
            if (!dropdownPeriod.interactable && !dropdownMaterial.interactable && !dropdownType.interactable) //pokud ano, tak zobraz jmeno odkryteho predmetu
            {
                AudioManager.Instance.PlaySFX("ItemIdentified"); //zvuk uspesne identifikace
                imageNameBckg.GetComponent<FlashEffect>().Flash();
                //itemNameText.text = detectManager.actualSignal.GetComponent<DetectTarget>().itemName;
                string itemID = detectTarget.GetComponent<DetectTarget>().itemID;
                LocalizationSettings.StringDatabase.GetLocalizedStringAsync("ItemsList", itemID).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        itemNameText.text = handle.Result;
                    }
                    else
                    {
                        Debug.LogError("Localization failed");
                    }
                };

                detectManager.hintTextbox.DisplayLocalizedText("MetalDetectingSceneTable", "dtct_hint_idtfDone01", "dtct_hint_idtfDone03", "dtct_hint_idtfDone04", "dtct_hint_spacetodtct");
                

            }
            else
            {
                detectManager.hintTextbox.DisplayLocalizedText("MetalDetectingSceneTable", "dtct_hint_idtfDone01", "dtct_hint_idtfDone02", "dtct_hint_idtfDone04", "dtct_hint_spacetodtct");
                // Vypni interaktivitu u zb�vaj�c�ch polo�ek, kter� jsou st�le interaktivn�, a nastav hodnotu Dropdown menu na Value 0
                if (dropdownPeriod.interactable)
                {
                    dropdownPeriod.interactable = false;
                    dropdownPeriod.value = 0;
                    SetBackgroundColor(dropdownPeriod.gameObject, colorIncorrect);
                }

                if (dropdownMaterial.interactable)
                {
                    dropdownMaterial.interactable = false;
                    dropdownMaterial.value = 0;
                    SetBackgroundColor(dropdownMaterial.gameObject, colorIncorrect);
                }

                if (dropdownType.interactable)
                {
                    dropdownType.interactable = false;
                    dropdownType.value = 0;
                    SetBackgroundColor(dropdownType.gameObject, colorIncorrect);
                }
            }

            StartCoroutine(ApplyFinalScoreColors());
        }
    }

    private IEnumerator ApplyFinalScoreColors()
    {
        yield return new WaitForSeconds(imagePeriod.GetComponent<FlashEffect>().flashDuration);
        SetBackgroundColor(imagePeriod,   scorePeriod   > 0 ? colorScoreCorrect : colorScoreIncorrect);
        SetBackgroundColor(imageMaterial, scoreMaterial > 0 ? colorScoreCorrect : colorScoreIncorrect);
        SetBackgroundColor(imageType,     scoreType     > 0 ? colorScoreCorrect : colorScoreIncorrect);
        int totalScore = scorePeriod + scoreMaterial + scoreType + bonusReveal;
        SetBackgroundColor(imageTotal,    totalScore    > 0 ? colorScoreCorrect : colorScoreIncorrect);
    }

    private void SetBackgroundColor(GameObject obj, Color color)
    {
        Image img = obj.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    public string SetIdentificationState(string state, int index, char newState) //meni znaky v retezci string pro urcovani spravne urcenych kategorii
    {
        char[] stateArray = state.ToCharArray();
        stateArray[index] = newState;
        return new string(stateArray);
    }

    private void UpdateScoreText(TMP_Text textElement, int score)
    {
        textElement.text = score.ToString();
    }

    private IEnumerator AddDropdownItems()
    {
        // P�id�n� polo�ek do dropdownPeriod
        yield return AddDropdownOptions(keysPeriod, dropdownPeriod);

        // P�id�n� polo�ek do dropdownMaterial
        yield return AddDropdownOptions(keysMaterial, dropdownMaterial);

        // P�id�n� polo�ek do dropdownType
        yield return AddDropdownOptions(keysType, dropdownType);
    }

    private IEnumerator AddDropdownOptions(List<string> keys, TMP_Dropdown dropdown)
    {
        foreach (string key in keys)
        {
            // Z�sk�n� lokalizovan�ho textu asynchronn�
            AsyncOperationHandle<string> handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("MetalDetectingSceneTable", key);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                string localizedString = handle.Result;

                // Vytvo�en� nov� polo�ky a p�id�n� do dropdown menu
                TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData();
                newOption.text = localizedString;
                dropdown.options.Add(newOption);
            }
            else
            {
                Debug.LogError("Localization failed for key: " + key + " with error: " + handle.OperationException);
            }
        }

        // Obnoven� zobrazen� hodnoty
        dropdown.RefreshShownValue();
    }
}
