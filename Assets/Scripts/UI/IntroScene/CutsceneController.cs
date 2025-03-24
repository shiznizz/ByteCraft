using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    public PlayableDirector firstTimelineDirector;  // The first timeline
    public PlayableDirector secondTimelineDirector; // The second timeline
    public string nextSceneName = "Level 1 - Hanger"; // The first level

    private bool canSkip = false; // Flag to prevent multiple skips

    void Start()
    {
        //Start playing the first timeline
        firstTimelineDirector.stopped += OnFirstTimelineFinished; // Listen for the first timeline to finish
        firstTimelineDirector.Play(); // Start the first timeline
    }

    private void Update()
    {
        if (Input.anyKeyDown && !canSkip)
        {
            SkipCutscene();
        }
    }

    //This is call after first timeline finishes
    private void OnFirstTimelineFinished(PlayableDirector director)
    {
        ////Play the second timeline after the first one finishes
        //secondTimelineDirector.stopped += OnSecondTimelineFinished; // Listen for the second timeline to finish
        //secondTimelineDirector.Play(); // Start the second timeline

        // Make sure the second timeline director is not null before playing it
        if (secondTimelineDirector != null)
        {
            secondTimelineDirector.stopped += OnSecondTimelineFinished;
            secondTimelineDirector.Play(); // Start the second timeline
        }
    }

    //This is called when the second timeline finishes
    private void OnSecondTimelineFinished(PlayableDirector director)
    {
        LoadNextScene();
    }

    private void SkipCutscene()
    {
        canSkip = true;

        //Loads the next scene early
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
