using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashEffect : MonoBehaviour
{
    public Image targetImage; // Odkaz na Image komponentu
    public Color flashColor = Color.white; // Barva vzplanutí
    public float flashDuration = 0.5f; // Doba trvání vzplanutí
    public float flashScale = 1.2f; // Faktor zvìtšení

    private Color originalColor;
    private Vector3 originalScale;

    void Start()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
        originalColor = targetImage.color;
        originalScale = targetImage.transform.localScale;
    }

    public void Flash()
    {
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            float t = elapsedTime / (flashDuration / 2);
            if (t > 1) t = 2 - t; // Zajištìní plynulého návratu

            // Lerp mezi pùvodní a flash barvou
            targetImage.color = Color.Lerp(originalColor, flashColor, t);

            // Lerp mezi pùvodní a zvìtšenou velikostí
            targetImage.transform.localScale = Vector3.Lerp(originalScale, originalScale * flashScale, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Obnovit pùvodní barvu a velikost
        targetImage.color = originalColor;
        targetImage.transform.localScale = originalScale;
    }
}
