using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public AudioClip menuMusic;
    private AudioSource audioSource;

    public GameObject optionsPanel;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource= gameObject.AddComponent<AudioSource>();
        }

        if (menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void loadCredits()
    {
        SceneManager.LoadScene("10 Credits");
    }

    //Toggle mute on and off
    public void toggleMute()
    {
        audioSource.mute = !audioSource.mute;
    }

    // Toggle the visibility of the options panel
    public void ToggleOptions()
    {
        if (optionsPanel != null)
        {
            bool isActive = optionsPanel.activeSelf;
            optionsPanel.SetActive(!isActive);  // Toggle the panel's visibility
        }
    }

    public void SetDifficulty(int difficultyIndex)
    {
        // cast int to GameDifficulty enum
        if (DifficultyManager.instance != null)
        {
            DifficultyManager.instance.SetDifficulty((GameDifficulty)difficultyIndex);
        }
    }
}
