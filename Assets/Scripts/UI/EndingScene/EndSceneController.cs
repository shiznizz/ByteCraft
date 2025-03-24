using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class EndSceneController : MonoBehaviour
{
    public PlayableDirector timelineDirector;  // The single timeline
    public string creditsSceneName = "CreditsScene"; // The credits scene name

    private bool canSkip = false; // Flag to prevent multiple skips

    void Start()
    {
        // Start playing the timeline
        timelineDirector.stopped += OnTimelineFinished; // Listen for the timeline to finish
        timelineDirector.Play(); // Start the timeline
    }

    private void Update()
    {
        if (Input.anyKeyDown && !canSkip)
        {
            SkipCutscene();
        }
    }

    // This is called after the timeline finishes
    private void OnTimelineFinished(PlayableDirector director)
    {
        LoadCreditsScene(); // Load the credits scene
    }

    private void SkipCutscene()
    {
        canSkip = true;

        // Load the credits scene early
        LoadCreditsScene();
    }

    private void LoadCreditsScene()
    {
        SceneManager.LoadScene(creditsSceneName); // Load the credits scene by name
    }
}
