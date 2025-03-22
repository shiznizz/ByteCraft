using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    public PlayableDirector firstTimelineDirector;  // The first timeline's Playable Director
    public PlayableDirector secondTimelineDirector; // The second timeline's Playable Director
    public string nextSceneName = "Level 1 - Hanger"; // The name of the first playable level (Scene 3)

    void Start()
    {
        // Start playing the first timeline
        firstTimelineDirector.stopped += OnFirstTimelineFinished; // Listen for the first timeline to finish
        firstTimelineDirector.Play(); // Start the first timeline
    }

    // Called when the first timeline finishes
    private void OnFirstTimelineFinished(PlayableDirector director)
    {
        // Play the second timeline after the first one finishes
        secondTimelineDirector.stopped += OnSecondTimelineFinished; // Listen for the second timeline to finish
        secondTimelineDirector.Play(); // Start the second timeline
    }

    // Called when the second timeline finishes
    private void OnSecondTimelineFinished(PlayableDirector director)
    {
        // Once both timelines are finished, load the next scene (Scene 3, your first level)
        //SceneManager.LoadScene(nextSceneName); // Load the first playable level (Scene 3)

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
