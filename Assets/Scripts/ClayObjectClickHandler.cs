using UnityEngine;

public class ClayObjectClickHandler : MonoBehaviour
{
    private bool hasBeenClicked = false;
    private ClayClickHandler clayClickHandler;
    private GameObject soilDropEffect;
    private Transform environmentParent;
    private ItemIdentification itemIdentification;

    private void Start()
    {
        clayClickHandler = FindFirstObjectByType<ClayClickHandler>();
        environmentParent = GameObject.Find("Environment").transform;
        soilDropEffect = Resources.Load("Soil Removing Effect") as GameObject;
        itemIdentification = clayClickHandler.itemIdentification; // Pøiøazení itemIdentification z ClayClickHandler
    }

    private void OnMouseDown()
    {
        if (!hasBeenClicked)
        {
            hasBeenClicked = true;

            if (Random.Range(0, 2) == 1)
            {
                AudioManager.Instance.PlaySFX("SoilRemoving01");
            }
            else
            {
                AudioManager.Instance.PlaySFX("SoilRemoving02");
            }

            itemIdentification.CompareDropdownValues(); // Zavolání funkce CompareDropdownValues pøes itemIdentification

            GameObject newParticles = Instantiate(soilDropEffect, transform.position, Quaternion.identity);
            newParticles.transform.SetParent(environmentParent);
            Destroy(newParticles, 5f);

            Destroy(gameObject);
        }
    }
}
