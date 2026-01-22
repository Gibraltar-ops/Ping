using UnityEngine;

public class MonsterAnimationManager : MonoBehaviour
{
    private Animator anim;
    private float targetSpeed;

    [Header("Settings")]
    public float smoothing = 8f;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        bool isMoving = anim.GetBool("IsMoving");
        bool isRunning = anim.GetBool("IsRunning");


        if (!isMoving)
        {
            targetSpeed = 0f;
        }
        else
        {
            targetSpeed = isRunning ? 2f : 1f;
        }
        float currentX = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(currentX, targetSpeed, Time.deltaTime * smoothing));
    }

    public bool GetIsMoving()
    {
        return anim.GetBool("IsMoving");
    }
    public bool GetIsRunning()
    {
        return anim.GetBool("IsRunning");
    }

    public void TriggerDetection()
    {
        anim.SetInteger("DetectionIndex", 1);
        Invoke("ResetDetectionIndex", 0.2f);
    }

    void ResetDetectionIndex()
    {
        anim.SetInteger("DetectionIndex", 0);
    }
}