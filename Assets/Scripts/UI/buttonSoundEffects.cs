using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class buttonSoundEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AudioClip hoverSound;     // Sound to play on hover
    public AudioClip clickSound;     // Sound to play on click
    private AudioSource audioSource; // AudioSource to play the sounds
    private Button button;           // Button reference

    void Start()
    {
        // Get the AudioSource component, if it doesn't exist, add one
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Make sure the button click sound is played from the script
        Button button = GetComponent<Button>();

        // Ensure the button has an OnClick listener for the click sound
        if (button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    // Play the hover sound when the pointer enters the button area
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound); // Play the hover sound once
        }
    }

    // Optional: Stop the sound when the pointer exits (you can leave it empty if not needed)
    public void OnPointerExit(PointerEventData eventData)
    {
        audioSource.Stop(); // Optionally stop any ongoing hover sound
    }

    // Play the click sound when the button is clicked
    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound); // Play the click sound once
        }
    }
}
