using UnityEngine;
using UnityEngine.InputSystem;

public class SonarScript : MonoBehaviour
{
    public GameObject sonarPrefab;
    public AudioClip sonarSound;

    void Update()
    {
    }
    void SpawnSonar()
    {
        Instantiate(sonarPrefab, transform.position, transform.rotation);

        if (AudioManager.instance != null && sonarSound != null)
        {
            AudioManager.instance.PlaySFXAtPoint(sonarSound, transform.position, 1.0f);
        }
        MonsterAI monster = Object.FindAnyObjectByType<MonsterAI>();
    if (monster != null)
    {
        monster.RegisterSonarPing(transform.position, 1.2f);
    }
    }
}
