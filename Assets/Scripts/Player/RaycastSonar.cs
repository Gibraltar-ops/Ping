using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RaycastSonarCharger : MonoBehaviour
{


    public GameObject sonarPrefab;
    public LayerMask walkableLayer;

    [Header("Audio Settings")]
    public AudioClip sonarSound;
    [Range(0.1f, 2.0f)] public float minPitch = 0.8f;
    [Range(0.1f, 2.0f)] public float maxPitch = 1.3f;
    [Range(0f, 1f)] public float minVolume = 0.5f;
    [Range(0f, 1f)] public float maxVolume = 1.0f;

    [Header("UI references")]
    public Image chargeIndicatorUI;

    [Header("settings")]
    public float maxChargeTime = 3.0f;
    public float minStrength = 0.5f;
    public float maxStrength = 3.0f;

    [Header("Sonar cooldown")]
    [SerializeField] private float cooldownDuration = 1.0f;
    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;

    private float chargeTimer = 0;
    private bool isCharging = false;
    private Vector3 hitPoint;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        if (chargeIndicatorUI != null)
        {
            chargeIndicatorUI.gameObject.SetActive(true);
            chargeIndicatorUI.fillAmount = 0;
            }
    }

    void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (chargeIndicatorUI != null)
            {
                chargeIndicatorUI.fillAmount = cooldownTimer / cooldownDuration;
            }

            if (cooldownTimer <= 0)
            {
                isOnCooldown = false;
                if (chargeIndicatorUI != null)
                {
                    chargeIndicatorUI.fillAmount = 0;
                }
            }
            return;
        }
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
        if (EventSystem.current == null)
            return false;
        if (Pointer.current is Touchscreen)
            return EventSystem.current.IsPointerOverGameObject(Pointer.current.deviceId);
        return EventSystem.current.IsPointerOverGameObject();
    }

    void StartCharging(Vector2 screenPosition)
    {
        if (isOnCooldown) return;
        
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
        isOnCooldown = true;
        cooldownTimer = cooldownDuration;

        float chargePercent = chargeTimer / maxChargeTime;
        float finalStrength = Mathf.Lerp(minStrength, maxStrength, chargePercent);

        GameObject go = Instantiate(sonarPrefab, hitPoint, Quaternion.identity);
        ScannerController controller = go.GetComponent<ScannerController>();
        if (controller != null) controller.Setup(finalStrength, chargePercent);

        MonsterAI monster = Object.FindAnyObjectByType<MonsterAI>();
        if (monster != null)
        {

            monster.RegisterSonarPing(hitPoint, finalStrength);
        }
    }
}
