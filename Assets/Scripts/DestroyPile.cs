using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyPile : MonoBehaviour
{
    // Rychlost pohybu dolù (v jednotkách za sekundu)
    private float rychlostPohybu = 0.01f;
    // Èas, po který se objekt nehybe pøed spuštìním pohybu
    private float dobaNehybu = 60.0f;
    // Èas, po který se objekt pohybuje dolù, než bude znièen
    private float dobaPohybu = 50.0f;

    private float casOdStartu = 0.0f;
    private bool zacalPohyb = false;
    // Maximální úhel rotace (v stupních).
    public float maxRotationAngle = 360f;

    private void Start()
    {
        // Náhodný úhel rotace v rozsahu od -maxRotationAngle do maxRotationAngle.
        float randomAngle = Random.Range(-maxRotationAngle, maxRotationAngle);

        // Vytvoøení náhodného vektoru rotace.
        Vector3 randomRotation = new Vector3(0f, randomAngle, 0f);

        // Aplikace rotace na objekt.
        transform.Rotate(randomRotation);

        // Nastavíme èas od startu na nulu
        casOdStartu = 0.0f;
    }

    private void Update()
    {
        // Inkrementujeme èas od startu
        casOdStartu += Time.deltaTime;

        // Pokud ještì nezaèal pohyb a uplynula doba nehybu, spustíme pohyb
        if (!zacalPohyb && casOdStartu >= dobaNehybu)
        {
            zacalPohyb = true;
            StartCoroutine(DestroyAfterDelay());
        }

        // Pokud zaèal pohyb, pohybujeme objektem dolù
        if (zacalPohyb)
        {
            transform.Translate(Vector3.down * rychlostPohybu * Time.deltaTime);
        }

        IEnumerator DestroyAfterDelay()
        {
            // Poèkáme na zadaný èas.
            yield return new WaitForSeconds(dobaPohybu);

            // Znièíme objekt.
            Destroy(gameObject);
        }
    }
}
