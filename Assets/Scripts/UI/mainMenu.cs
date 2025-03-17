using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public AudioMixer mixer;
    private AudioSource audioSource;

    public GameObject optionsPanel;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        getSavedAudioSettings();

        if (audioSource == null)
        {
            audioSource= gameObject.AddComponent<AudioSource>();
        }
    }

    public void newGame()
    {
        dataManager.instance.NewGame();
    }

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        newGame();
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

    private void getSavedAudioSettings()
    {
        float value;
        foreach (AudioMixerGroup group in mixer.FindMatchingGroups(""))
        {
            value = PlayerPrefs.GetFloat(group.name);
            if (value == 0)
            {
                mixer.SetFloat(group.name, -80);
            }
            else
            {
                mixer.SetFloat(group.name, Mathf.Log10(value) * 20);
            }
        }
    }
}
