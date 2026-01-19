using System.Collections.Generic;
using UnityEngine;

public class ScannerController : MonoBehaviour
{

    //Detectable object list
    public static List<ScannerController> active_sonar = new List<ScannerController>();

    [Header("Speed & Fade")]
    public float Speed = 10f;
    public float fadeDuration = 0.8f;

    private float currentFade = 1.0f;
    private bool initialized = false;
    private Material mat;

    // Optimized distance check
    public float CurrentRadiusSqr { get; private set; }

    void OnEnable() 
    { 
        active_sonar.Add(this); 
    }

    void OnDisable() 
    { 
        active_sonar.Remove(this);
    }

    public void Setup(float strength)
    {
        mat = GetComponent<Renderer>().material;
        if (mat != null) mat.SetFloat("_Fade", 1.0f);
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        float growing = Speed * Time.deltaTime;
        transform.localScale += new Vector3(growing, growing, growing);
        float radius = transform.localScale.x * 0.5f;
        CurrentRadiusSqr = radius * radius;

        if (mat != null)
            mat.SetFloat("_Radius", transform.localScale.x);

        currentFade -= Time.deltaTime / fadeDuration;

        if (mat != null)
        {
            mat.SetFloat("_Fade", Mathf.Max(0, currentFade));
        }

        if (currentFade <= 0)
        {
            Destroy(gameObject);
        }
    }
}