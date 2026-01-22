using System.Collections.Generic;
using UnityEngine;

public class ScannerController : MonoBehaviour
{

    //Detectable object list
    public static List<ScannerController> active_sonar = new List<ScannerController>();

    [Header("Speed & Fade")]
    public float Speed = 10f;
    public float fadeDuration = 0.8f;

     [Header("Audio ")]
    [Range(0.1f, 2.0f)] public float minPitch = 0.8f;
    [Range(0.1f, 2.0f)] public float maxPitch = 1.3f;
    [Range(0f, 1f)] public float minVolume = 0.5f;
    [Range(0f, 1f)] public float maxVolume = 1.0f;

    private float currentFade = 1.0f;
    private bool initialized = false;
    private Material mat;
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
        if (mat != null) mat.SetFloat("_Fade", 1.0f);

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

    private void OnTriggerEnter(Collider other)
{
    GhostEffect ghost = other.GetComponent<GhostEffect>();
   
    if (ghost != null)
    {
        ghost.CreateGhostSnapshot();
    }
}
}