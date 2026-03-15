using UnityEngine;
using UnityEngine.UI;

public class DistanceScript : MonoBehaviour
{
    // Kamera
    public GameObject cameraObject;

    // Child objekt
    public GameObject childObject;

    // Rodiè child objektu
    public GameObject parentObject;

    // V kolika procentech vzdalenosti ma zacit reagovat progress bar (da se tak nasymulovat treba velikost objevovaneho mista)
    public float progressBarActiveIn = 0.02f;
    
    // Nejmenší a nejvìtší možná vzdálenost
    private float minDistance;
    private float maxDistance;

    private bool indicatorAnim = false;
    public bool isDiscovered = false;

    public GameObject objectForMap;

    private float minBarValue; //vzdalenost, ktera je brana jako mezni a kdz je v teto vzdalenosti objekt bran jako viditelny
   
    void Start()
    {
        isDiscovered = (Random.value > 0.5f); // prozatimne nastavim objektu jestli je mozne jej objevit nebo ne pro testy

        // Vypoèítání nejmenší a nejvìtší možné vzdálenosti na zaèátku
        // minimalni vzdalenost objektu od kamery je vzdalenost kamery od stredu - vzdalenost tohoto objektu od stredu
        minDistance = Vector2.Distance(new Vector2(cameraObject.transform.position.x, cameraObject.transform.position.z), new Vector2(parentObject.transform.position.x, parentObject.transform.position.z));
        minDistance -= Vector2.Distance(new Vector2(childObject.transform.position.x, childObject.transform.position.z), new Vector2(parentObject.transform.position.x, parentObject.transform.position.z));
        //maximalni vzdalenost je vzdalenost objektu kamery od stredu + vzdalenost tohoto objektu od stredu
        maxDistance = Vector2.Distance(new Vector2(childObject.transform.position.x, childObject.transform.position.z), new Vector2(parentObject.transform.position.x, parentObject.transform.position.z));
        maxDistance += Vector2.Distance(new Vector2(cameraObject.transform.position.x, cameraObject.transform.position.z), new Vector2(parentObject.transform.position.x, parentObject.transform.position.z));

        //z maximalni vzdalenosti vypocitame, v jakych hodnotach ma zacit reagovat progressbar
        minBarValue = minDistance + (maxDistance * progressBarActiveIn);
        /*Debug.Log("Maximalni vzdalenost je: " + maxDistance + ".");
        Debug.Log("Minimalni vzdalenost je: " + minDistance + ".");
        Debug.Log("Vzdálenost, ve ktere ma zacit fungovat bar: " + minBarValue + ".");*/
    }

    void Update()
    {
        // Vypoèítání aktuální vzdálenosti
        float currentDistance = Vector3.Distance(new Vector2(cameraObject.transform.position.x, cameraObject.transform.position.z), new Vector2(childObject.transform.position.x, childObject.transform.position.z));

        // pokud je vzdalenost mensi nez minimalni vzdalenost kdy muze byt objekt lokalizovan
        // a neni to ve zrychlenem modu (+shift)
        if (currentDistance < minBarValue && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            // zapni na objektu zvyrazneni na mape
            objectForMap.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");

            if (!isDiscovered) // pokud je tento objekt neobjeveny
            {
                SpotSceneManager.Instance.focusedObject = gameObject; //nastav tento objekt jako mozny pro objeveni
            }

            if (!indicatorAnim)
            {
                //SpotSceneManager.Instance.ObjectIsNear();
                indicatorAnim = true;
            }
        }

        if (currentDistance > minBarValue)
        {
            indicatorAnim = false;
            objectForMap.GetComponent<Renderer>().material.DisableKeyword("_EMISSION");

            //musim nejak vymyslet, jak tento konkretni objekt odstanit z promenne v SpotSceneManageru a pritom nesmazat v te promenne jinej objekt
            //mozna porovnam jmeno tohoto objektu se jmenem objektu v manageru a pokud je shoda, objekt z promenne v manageru vymazu
            if (SpotSceneManager.Instance.focusedObject != null && gameObject.name == SpotSceneManager.Instance.focusedObject.name)
            {
                // Nastavení promìnné na null
                SpotSceneManager.Instance.focusedObject = null;
            }
        }

    }
}
