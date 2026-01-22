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
            if (movement != null && movement.IsWalking)
            {
                if (footStepSFX.Length > 0)
                {
                    AudioClip randomClip = footStepSFX[Random.Range(0, footStepSFX.Length)];

                    AudioManager.instance.PlaySFXOnTransform(randomClip, transform, 0.5f, Random.Range(0.9f, 1.1f));
                }
            }
            yield return new WaitForSeconds(0.6f);
        }
    
    }
}
