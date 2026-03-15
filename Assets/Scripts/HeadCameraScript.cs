using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadCameraScript : MonoBehaviour
{
    public void PlayFootstep()
    {
        if (Random.Range(0, 2) == 0)
        {
            AudioManager.Instance.PlaySFX("Footstep01");
        }
        else
        {
            AudioManager.Instance.PlaySFX("Footstep02");
        }
    }

    public IEnumerator ShakeWithCamera(float duration, float magnitude)
    {
        Vector3 originalPosition = transform.localPosition;
        Quaternion originalRotation = transform.localRotation;

        GetComponent<Animator>().enabled = false;//vypnu animace, aby se mohla klamera trast, protoze animace jsou nadrazene

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPosition.y + Random.Range(-1f, 1f) * magnitude;

            float percentageComplete = elapsed / duration;

            // Plynulý pøechod pozice
            Vector3 newPosition = new Vector3(x, y, originalPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, percentageComplete);

            // Plynulý pøechod rotace
            Quaternion newRotation = originalRotation * Quaternion.Euler(Random.Range(-1f, 1f) * magnitude, Random.Range(-1f, 1f) * magnitude, 0);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, newRotation, percentageComplete);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        GetComponent<Animator>().enabled = true;//zapnu animace kamery
    }
}
