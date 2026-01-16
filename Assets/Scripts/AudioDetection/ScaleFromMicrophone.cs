using UnityEngine;

public class ScaleFromMicrophone : MonoBehaviour
{
    public AudioDetection detector;
    public GameObject sonarPrefab; 

    public float loudnessSensibility = 100;
    public float threshold = 0.1f;
    public float cooldown = 0.5f; 
    private float nextSpawnTime;

    void Update()
    {
        float loudness = detector.GetLoudnessFromMicrophone() * loudnessSensibility;

 
        if (loudness > threshold && Time.time >= nextSpawnTime)
        {
            SpawnSonar(loudness);
            nextSpawnTime = Time.time + cooldown;
        }
    }

    void SpawnSonar(float strength)
    {
        GameObject go = Instantiate(sonarPrefab, transform.position, Quaternion.identity);
        ScannerController controller = go.GetComponent<ScannerController>();
        if (controller != null)
        {
            controller.Setup(strength);
        }
    }
}