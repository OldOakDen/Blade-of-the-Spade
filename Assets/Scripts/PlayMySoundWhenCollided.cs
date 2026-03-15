using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMySoundWhenCollided : MonoBehaviour
{
    private bool canPlaySound = false;
    private float delayTime = 3f; // Èasové zpoždìní v sekundách

    // Start is called before the first frame update
    void Start()
    {
        // Spustíme èasovaè pro zpoždìní zvuku
        StartCoroutine(PlayDelayedSound());
    }

    private void OnCollisionEnter(Collision collision)
    {
        //print("list koliduje!");
        if (Random.Range(1,5)==1 && canPlaySound) //pust zvuk susteni jen obcas
            {
            AudioManager.Instance.PlaySFX("LeafSound");
            }
    }

    private IEnumerator PlayDelayedSound()
    {
        yield return new WaitForSeconds(delayTime);

        if (!canPlaySound)
        {
            canPlaySound = true;
        }
    }
}
