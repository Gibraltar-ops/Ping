using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaycastSonarCharger : MonoBehaviour
{
    public GameObject sonarPrefab;
    public LayerMask walkableLayer;

    [Header("UI references")]
    public Image chargeIndicatorUI;

    [Header("settings")]
    public float maxChargeTime = 3.0f;
    public float minStrength = 0.5f;
    public float maxStrength = 3.0f;

    private float chargeTimer = 0;
    private bool isCharging = false;
    private Vector3 hitPoint; 
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (chargeIndicatorUI != null)
            chargeIndicatorUI.gameObject.SetActive(false);
    }

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;

        if (pointer.press.wasPressedThisFrame)
        {
            if (IsPointerOverUI()) return;
            StartCharging(pointer.position.ReadValue());
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            chargeTimer = Mathf.Clamp(chargeTimer, 0, maxChargeTime);

            if (chargeIndicatorUI != null)
            {
                chargeIndicatorUI.fillAmount = chargeTimer / maxChargeTime;
            }
        }

        if (pointer.press.wasReleasedThisFrame && isCharging)
        {
            ReleaseSonar();
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (Pointer.current is Touchscreen)
            return EventSystem.current.IsPointerOverGameObject(Pointer.current.deviceId);
        return EventSystem.current.IsPointerOverGameObject();
    }

    void StartCharging(Vector2 screenPosition)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, walkableLayer))
        {
            hitPoint = hit.point; 
            isCharging = true;
            chargeTimer = 0;

            if (chargeIndicatorUI != null)
            {
                chargeIndicatorUI.gameObject.SetActive(true);
                chargeIndicatorUI.fillAmount = 0;
            }
        }
    }

    void ReleaseSonar()
    {
        isCharging = false;

        if (chargeIndicatorUI != null)
            chargeIndicatorUI.gameObject.SetActive(false);

        float chargePercent = chargeTimer / maxChargeTime;
        float finalStrength = Mathf.Lerp(minStrength, maxStrength, chargePercent);

        GameObject go = Instantiate(sonarPrefab, hitPoint, Quaternion.identity);
        ScannerController controller = go.GetComponent<ScannerController>();
        if (controller != null)
        {
            controller.Setup(finalStrength);
        }
    }
}