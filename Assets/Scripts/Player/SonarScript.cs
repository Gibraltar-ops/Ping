using UnityEngine;
using UnityEngine.InputSystem;

public class SonarScript : MonoBehaviour
{
    public GameObject sonarPrefab;
    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            SpawnSonar();
        }
    }    void SpawnSonar()
    {
        Instantiate(sonarPrefab, transform.position, transform.rotation);
    }
}
