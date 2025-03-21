using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class TitleScreenController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string mainSceneName;

    void Start()
    {
        videoPlayer.Play();
    }

    void Update()
    {
        //Checks if the Escape key is pressed to skip the video
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadMainScene();
        }

        // If the video has finished playing, load the main scene
        if (!videoPlayer.isPlaying && videoPlayer.time >= videoPlayer.length - 0.1f)
        {
            LoadMainScene();
        }
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}
