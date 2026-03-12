using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MonsterAnimationManager), typeof(MonsterAI))]
public class ChaserAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Source for footsteps.")]
    [SerializeField] private AudioSource footstepSource;
    [Tooltip("Source for ambient monster sounds (idle).")]
    [SerializeField] private AudioSource ambientSource;
    [Tooltip("Source for detection and charging sounds.")]
    [SerializeField] private AudioSource voiceSource;

    [Header("Footstep Settings")]
    [SerializeField] private AudioClip[] walkingFootsteps;
    [SerializeField] private AudioClip[] runningFootsteps;
    [SerializeField] private float walkStepInterval = 0.6f;
    [SerializeField] private float runStepInterval = 0.3f;

    [Header("Footstep Screenshake Settings")]
    [SerializeField] private Unity.Cinemachine.CinemachineImpulseSource impulseFootstepsSource;
    [SerializeField] private float walkShakeIntensity = 0.05f;
    [SerializeField] private float runShakeIntensity = 0.4f;

    [Header("Detection Screenshake settings")]

[SerializeField] private Unity.Cinemachine.CinemachineImpulseSource impulseDetectionSource;
    [SerializeField] private float detectionShakeIntensity = 1.0f;

    [Header("Ambient Sound Settings")]
    [Tooltip("Random sounds played when idle.")]
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private Vector2 ambientSoundIntervalRange = new Vector2(5f, 15f);

    [Header("Voice Sound Settings")]
    [Tooltip("Sound played upon detecting the player.")]
    [SerializeField] private AudioClip detectionSound;
    [Tooltip("Sounds played randomly while charging at the player.")]
    [SerializeField] private AudioClip[] chargingSounds;
    [SerializeField] private Vector2 chargingSoundIntervalRange = new Vector2(1f, 3f);
    private MonsterAnimationManager animManager;
    private MonsterAI monsterAI;
    private Coroutine footstepCoroutine;
    private Coroutine ambientSoundCoroutine;
    private Coroutine chargingSoundCoroutine;

    void Awake()
    {
        animManager = GetComponent<MonsterAnimationManager>();
        monsterAI = GetComponent<MonsterAI>();
    }

    void Start()
    {
        ambientSoundCoroutine = StartCoroutine(PlayRandomSounds(ambientSource, ambientSounds, ambientSoundIntervalRange));
    }

    void Update()
    {
        HandleFootsteps();
        HandleChargingSounds();
    }

    private void HandleFootsteps()
    {
        bool isMoving = animManager.GetIsMoving();
        bool isRunning = animManager.GetIsRunning();

        if (isMoving && footstepCoroutine == null)
        {
            // Monster started moving, start footstep sounds
            footstepCoroutine = StartCoroutine(PlayFootsteps(isRunning));
        }
        else if (!isMoving && footstepCoroutine != null)
        {
            // Monster stopped, stop footstep sounds
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
            footstepSource.Stop();
        }
    }

    private void HandleChargingSounds()
    {
        bool isCharging = monsterAI.isCharging;

        if (isCharging && chargingSoundCoroutine == null)
        {

            if (ambientSoundCoroutine != null)
            {
                StopCoroutine(ambientSoundCoroutine);
                ambientSoundCoroutine = null;
            }
            chargingSoundCoroutine = StartCoroutine(PlayRandomSounds(voiceSource, chargingSounds, chargingSoundIntervalRange));
        }
        else if (!isCharging && chargingSoundCoroutine != null)
        {

            StopCoroutine(chargingSoundCoroutine);
            chargingSoundCoroutine = null;

            if (ambientSoundCoroutine == null)
            {
                ambientSoundCoroutine = StartCoroutine(PlayRandomSounds(ambientSource, ambientSounds, ambientSoundIntervalRange));
            }
        }
    }

    private IEnumerator PlayFootsteps(bool isRunning)
    {
        float interval = isRunning ? runStepInterval : walkStepInterval;
        AudioClip[] clips = isRunning ? runningFootsteps : walkingFootsteps;

        float intensity = isRunning ? runShakeIntensity : walkShakeIntensity;

        while (true)
        {
            if (clips.Length > 0)
            {
                AudioClip clip = clips[Random.Range(0, clips.Length)];
                footstepSource.PlayOneShot(clip);

                if (impulseFootstepsSource != null)
                {
                    impulseFootstepsSource.GenerateImpulseWithVelocity(Vector3.down * intensity);
                }
                
            
            }
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator PlayRandomSounds(AudioSource source, AudioClip[] clips, Vector2 intervalRange)
    {
        while (true)
        {
            float waitTime = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(waitTime);

            if (clips.Length > 0 && source.enabled)
            {
                AudioClip clip = clips[Random.Range(0, clips.Length)];
                source.PlayOneShot(clip);
            }
        }
    }

    public void PlayDetectionSound()
    {
      

        if (detectionSound != null && voiceSource.enabled)
        {
            voiceSource.PlayOneShot(detectionSound);
        }

        if (impulseDetectionSource != null)
                {
                    impulseDetectionSource.GenerateImpulse();
                }
    }
}