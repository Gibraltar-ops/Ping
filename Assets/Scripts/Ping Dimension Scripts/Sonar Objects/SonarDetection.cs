using System.Collections.Generic;
using UnityEngine;

public class SonarDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    [Range(0f, 1f)] public float minIntensity = 0f;

    [Header("Fade settings")]
    public float visibilityDuration = 5.0f;
    public float maxTimeHold = 1f;

    private float timer = 0f;
    private ScannerController lastHitSonar; 

    private Renderer objectRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    } 

    void Start()
    {
        ApplyIntensity(minIntensity);
    }

    void Update()
    {
        for (int i = 0; i < ScannerController.active_sonar.Count; i++)
        {
            ScannerController sonar = ScannerController.active_sonar[i];
            if (sonar == null) continue;

            float sqrDistance = (transform.position - sonar.transform.position).sqrMagnitude;

            if (sqrDistance < sonar.CurrentRadiusSqr)
            {
                if (lastHitSonar != sonar)
                {
                    timer = visibilityDuration; 
                    lastHitSonar = sonar;
                }
            }
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;

            float intensityToApply = 1.0f;

            float fadeThreshold = visibilityDuration - maxTimeHold;

            if (timer < fadeThreshold)
            {
                intensityToApply = timer / fadeThreshold;
            }
            ApplyIntensity(Mathf.Clamp(intensityToApply, minIntensity, 1.0f));
        }
        else if (lastHitSonar != null)
        {
            ApplyIntensity(minIntensity);
            lastHitSonar = null;
        }
    }

    private void ApplyIntensity(float val)
    {
        if (objectRenderer != null)
        {
            objectRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_Intensity", val);
            objectRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}