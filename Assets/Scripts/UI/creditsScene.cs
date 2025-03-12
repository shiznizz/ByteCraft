using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class creditsScene : MonoBehaviour
{
    public float scrollSpeed = 40f; // Adjust the speed as needed
    private RectTransform rectTransform;
    public AudioClip creditsMusic;
    private AudioSource audioSource;

    public Button backToMainMenu;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        // Get or add an AudioSource component to the GameObject
        audioSource = GetComponent<AudioSource>();

        // If no AudioSource, we can add one manually
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Play the music if it's assigned
        if (creditsMusic != null)
        {
            audioSource.clip = creditsMusic;
            audioSource.loop = true;  // Optional: loop the music
            audioSource.Play();  // Start playing the music
        }

        if (backToMainMenu != null)
        {
            backToMainMenu.onClick.AddListener(goToMainMenu);
        }
    }

    void Update()
    {
        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);
    }

    void goToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
