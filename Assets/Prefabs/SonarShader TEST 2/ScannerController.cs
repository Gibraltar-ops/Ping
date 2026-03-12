using System.Collections.Generic;
using UnityEngine;

public class ScannerController : MonoBehaviour
{

    //Detectable object list
    public static List<ScannerController> active_sonar = new List<ScannerController>();

    [Header("Speed & Fade")]
    public float Speed = 10f;
    public float fadeDuration = 5f;

     [Header("Audio ")]
    [Range(0.1f, 2.0f)] public float minPitch = 0.8f;
    [Range(0.1f, 2.0f)] public float maxPitch = 1.3f;
    [Range(0f, 1f)] public float minVolume = 0.5f;
    [Range(0f, 1f)] public float maxVolume = 1.0f;

    private float currentFade = 2.0f;
    private bool initialized = false;
    private Material mat;

    public Renderer innerScannerRenderer;
private Material innerMat;
    private AudioSource audioSource;

    // Optimized distance check
    public float CurrentRadiusSqr { get; private set; }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable() 
    { 
        active_sonar.Add(this); 
    }

    void OnDisable() 
    { 
        active_sonar.Remove(this);
    }

    public void Setup(float strength, float chargePercent)
    {
        mat = GetComponent<Renderer>().material;

        if (innerScannerRenderer != null)
    {
        innerMat = innerScannerRenderer.material;
        if (innerMat != null) innerMat.SetFloat("_Fade", 2.0f);
    }
        if (mat != null) mat.SetFloat("_Fade", 2.0f);

        if (audioSource != null)
        {
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, chargePercent);
            audioSource.volume = Mathf.Lerp(minVolume, maxVolume, chargePercent);
        }
        initialized = true;
    }

    void Update()
    {
        
        if (!initialized) return;

        float growing = Speed * Time.deltaTime;
        transform.localScale += new Vector3(growing, growing, growing);
        
        float radius = transform.localScale.x * 0.5f;
        CurrentRadiusSqr = radius * radius;

        currentFade -= Time.deltaTime / fadeDuration;
        float finalFade = Mathf.Max(0, currentFade);

        if (mat != null)
        {
            mat.SetFloat("_Radius", transform.localScale.x);
            mat.SetFloat("_Fade", finalFade);
        }

float currentRadius = transform.localScale.x * 0.5f;

        if (innerMat != null)
        {
            innerMat.SetVector("_PulseOrigin", transform.position);
            innerMat.SetFloat("_Radius", currentRadius);
            innerMat.SetFloat("_Fade", finalFade);
        }

        if (currentFade <= 0)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                return;
            }
            
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
{
    GhostEffect ghost = other.GetComponent<GhostEffect>();
   
    if (ghost != null)
    {
        ghost.CreateGhostSnapshot();
    }
}
}