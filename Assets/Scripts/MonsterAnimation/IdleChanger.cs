using UnityEngine;

public class IdleChanger : MonoBehaviour
{
    private Animator anim;
    private float timer;
    public float minWaitTime = 3f;
    public float maxWaitTime = 7f;

    void Start()
    {
        anim = GetComponent<Animator>();
        ResetTimer();
    }

    void Update()
    {
        bool isMoving = anim.GetBool("IsMoving");

        if (!isMoving)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                CycleIdle();
                ResetTimer();
            }
        }
        else
        {
            ResetTimer();
        }
    }

    void CycleIdle()
    {
        int currentIndex = anim.GetInteger("IdleIndex");
        int nextIndex;

        do {
            nextIndex = Random.Range(0, 3); 
        } while (nextIndex == currentIndex);

        anim.SetInteger("IdleIndex", nextIndex);
    }

    void ResetTimer()
    {
        timer = Random.Range(minWaitTime, maxWaitTime);
    }
}