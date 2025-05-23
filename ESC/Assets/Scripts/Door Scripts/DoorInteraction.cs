using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    private Animator animator;
    private bool isDoorOpen = false;

    [Header("Door Sounds")]
    public AudioClip openSound;
    public AudioClip closeSound;
    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure AudioSource is set up for 3D sound
        audioSource.spatialBlend = 1.0f; // 1 = fully 3D
        audioSource.minDistance = 1f;    // Distance where sound plays at full volume
        audioSource.maxDistance = 10f;   // Beyond this, volume fades to 0
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.playOnAwake = false;
    }

    public void ToggleDoor()
    {
        if (!isDoorOpen)
        {
            animator.SetBool("isOpen", true);
            animator.SetBool("isClose", false);
            PlaySound(openSound);
        }
        else
        {
            animator.SetBool("isOpen", false);
            animator.SetBool("isClose", true);
            PlaySound(closeSound);
        }

        isDoorOpen = !isDoorOpen;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
