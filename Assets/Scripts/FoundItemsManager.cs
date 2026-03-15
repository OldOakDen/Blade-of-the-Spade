using static ObjectPreview;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Zajistìte, že používáte správný UI namespace

public class FoundItemsManager : MonoBehaviour
{
    public List<string> foundItemIDs = new List<string>(); // Seznam nalezených ID
    public List<GameObject> foundItemInstances = new List<GameObject>(); // Seznam instancí nalezených pøedmìtù
    public GameObject buttonPrefab; // Prefab pro tlaèítko ve ScrollRectu
    public Transform scrollContent; // Místo, kam se tlaèítka pøidají
    public GameObject objectPreview;
    public LocationConfigurationSO locationConfig;

    private void Start()
    {
        print("LOKACE PRO FOUND ITEM MANAGER: " + LocationHolder.Instance.selectedLocation + " ID: " + LocationHolder.Instance.selectedLocation.id);
        locationConfig = LocationHolder.Instance.selectedLocation;
    }

    public void AddFoundItem(string itemID)
    {
        // Pøidáme ID nalezeného pøedmìtu
        foundItemIDs.Add(itemID);

        // Najdeme správný prefab podle ID
        GameObject prefab = locationConfig.GetItemPrefabByID(itemID);
        if (prefab != null)
        {
            // Vytvoøíme instanci prefabu a pøidáme ji do seznamu
            GameObject instance = Instantiate(prefab, new Vector3(500, 500, 500), Quaternion.identity);
            foundItemInstances.Add(instance);

            // Získáme komponentu DetectTarget a její sprite
            DetectTarget detectTarget = instance.GetComponent<DetectTarget>();
            if (detectTarget != null)
            {
                // Vytvoøíme tlaèítko ve ScrollRectu (pokud je to potøeba)
                GameObject newButton = Instantiate(buttonPrefab, scrollContent);
                newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Item " + itemID; // Zmìníme text tlaèítka pomocí TextMeshPro

                // Získáme komponentu Image a nastavíme její sprite
                Image buttonImage = newButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = detectTarget.icon; // Nastavíme sprite tlaèítka
                }

                // Nastavíme akci pøi kliknutí na tlaèítko pomocí AddListener
                newButton.GetComponent<Button>().onClick.AddListener(() => ShowItem(itemID)); // Použijte AddListener

                // Nastavte velikost contentu na základì poètu tlaèítek
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent.GetComponent<RectTransform>());
            }
            else
            {
                Debug.LogWarning("Komponenta DetectTarget nebyla nalezena v instanci prefabu pro ID " + itemID);
            }
        }
        else
        {
            Debug.LogWarning("Prefab s ID " + itemID + " nebyl nalezen v locationConfig.");
        }
    }


    private void ShowItem(string itemID)
    {
        // Zlikvidujeme pøedchozí pøedmìt
        Destroy(objectPreview.GetComponent<ObjectPreview>().previewedTarget);

        objectPreview.GetComponent<ObjectPreview>().showByButton = true; // Inicializace pøes tlaèítko

        // Najdeme instanci pøedmìtu podle itemID
        print("Hledám instanci objektu " + itemID);
        GameObject foundInstance = foundItemInstances.Find(instance =>
            instance.GetComponent<DetectTarget>().itemID == itemID);

        if (foundInstance != null)
        {
            // Aktivujeme objectPreview
            objectPreview.SetActive(true);

            // Vytvoøíme klon instance a pøiøadíme ho do previewedTarget
            GameObject newPreviewedObject = Instantiate(foundInstance);
            objectPreview.GetComponent<ObjectPreview>().previewedTarget = newPreviewedObject;

            // Inicializujeme mód pro prohlížení Examination
            objectPreview.GetComponent<ObjectPreview>().InitializeMode(ObjectPreview.PreviewMode.Examination);
        }
        else
        {
            Debug.LogWarning("Instance s ID " + itemID + " nebyla nalezena.");
        }
    }

    // Metoda pro získání jména prefabu posledního nalezeného pøedmìtu
    public string GetLastFoundItemName()
    {
        if (foundItemIDs.Count == 0)
        {
            Debug.LogWarning("Seznam nalezených pøedmìtù je prázdný.");
            return string.Empty;
        }

        // Získáme poslední ID
        string lastItemID = foundItemIDs[foundItemIDs.Count - 1];

        // Najdeme instanci podle ID
        GameObject foundInstance = foundItemInstances.Find(instance =>
            instance.GetComponent<DetectTarget>().itemID == lastItemID);

        if (foundInstance != null)
        {
            //print(foundInstance.GetComponent<DetectTarget>().itemID);
            return foundInstance.name; // Vrátíme jméno prefabu
        }
        else
        {
            Debug.LogWarning("Instance s ID " + lastItemID + " nebyla nalezena.");
            return string.Empty; // Pokud nenalezeno, vrátíme prázdný øetìzec
        }
    }

    public GameObject GetLastFoundItemInstance()
    {
        if (foundItemIDs.Count == 0)
        {
            Debug.LogWarning("Seznam nalezených pøedmìtù je prázdný.");
            return null; // Pokud je seznam prázdný, vrátíme null
        }

        // Získáme poslední ID
        string lastItemID = foundItemIDs[foundItemIDs.Count - 1];

        // Najdeme prefab podle ID
        GameObject prefab = foundItemInstances.Find(instance =>
            instance.GetComponent<DetectTarget>().itemID == lastItemID);

        if (prefab != null)
        {
            // Vytvoøíme novou instanci prefabu
            GameObject newInstance = Instantiate(prefab);
            //print(newInstance.GetComponent<DetectTarget>().itemID);
            return newInstance; // Vrátíme novou instanci objektu
        }
        else
        {
            Debug.LogWarning("Prefab s ID " + lastItemID + " nebyl nalezen.");
            return null; // Pokud nenalezeno, vrátíme null
        }
    }
}
