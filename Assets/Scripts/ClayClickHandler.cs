using UnityEngine;

public class ClayClickHandler : MonoBehaviour
{
    public GameObject soilDropEffect;
    public Transform environmentParent;
    public ItemIdentification itemIdentification;

    private void Start()
    {
        environmentParent = GameObject.Find("Environment").transform;
        soilDropEffect = Resources.Load("Soil Removing  Effect") as GameObject;

        GameObject itemIdentificationObject = GameObject.Find("Item Identification");

        // Získej skript ItemIdentification z nalezeného objektu
        if (itemIdentificationObject != null)
        {
            itemIdentification = itemIdentificationObject.GetComponent<ItemIdentification>();
        }
        else
        {
            Debug.LogError("Item Identification object not found in the scene.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Levé tlaèítko myši
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject) // Zkontroluj, zda je kliknutý objekt tento objekt
                {
                    if (Random.Range(0, 2) == 1)
                    {
                        AudioManager.Instance.PlaySFX("SoilRemoving01");
                    }
                    else
                    {
                        AudioManager.Instance.PlaySFX("SoilRemoving02");
                    }

                    itemIdentification.CompareDropdownValues(); // Zavolej funkci CompareDropdownValues
                    //Debug.Log(">> CompareDropdownValues CALLED!");

                    GameObject newParticles = Instantiate(soilDropEffect, transform.position, Quaternion.identity);
                    newParticles.transform.SetParent(environmentParent);
                    Destroy(newParticles, 5f);

                    Destroy(gameObject);
                }
            }
        }
    }
}
