using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSoundEffects : MonoBehaviour
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
}
