using System.Collections;
using UnityEngine;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioClip[] footStepSFX;

    private MobileFPSController movement;

    void Start()
    {
        movement = GetComponent<MobileFPSController>();
        StartCoroutine(PlayFootSteps());
    }
    IEnumerator PlayFootSteps()
    {
        while (true)
        {
            if (movement.IsWalking)
            {
                if (footStepSFX.Length > 0)
                {

                    AudioClip randomClip = footStepSFX[Random.Range(0, footStepSFX.Length)];
                    AudioManager.instance.PlaySFX(randomClip);
                }
            }
            yield return new WaitForSeconds(0.6f);
        }
    }
}
