using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class loadingScreen : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    public void loadLevelButton(string levelToLoad)
    {
        mainMenu.SetActive(false);
        loadScreen.SetActive(true);

        StartCoroutine(loadLevelASync(levelToLoad));
    }

    //IEnumerator loadLevelASync(string levelToLoad)
    //{
    //    AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

    //    while (!loadOperation.isDone)
    //    {
    //        float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
    //        loadingSlider.value = progressValue;
    //        yield return null;
    //    }
    //}

    IEnumerator loadLevelASync(string levelToLoad)
    {
        // Start loading the scene asynchronously
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        // Disable scene activation until after the artificial delay
        loadOperation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadingTime = 5f; // Minimum loading time in seconds
        float progressValue = 0f;

        // Loop until the loading is complete
        while (!loadOperation.isDone)
        {
            // Calculate progress (loadOperation.progress maxes out at 0.9)
            progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);

            // Update the slider with the current progress value
            loadingSlider.value = progressValue;

            // Increment the timer by the deltaTime to keep track of elapsed time
            timer += Time.deltaTime;

            // If the loading reaches 90% and the timer exceeds the minimum loading time, allow scene activation
            if (timer >= minLoadingTime && loadOperation.progress >= 0.9f)
            {
                loadOperation.allowSceneActivation = true;
            }

            // Continue until the loading is complete and we've waited the minimum time
            yield return null;
        }

        // Ensure scene is activated after the minimum loading time and progress reaches 100%
        if (loadOperation.progress >= 0.9f && timer >= minLoadingTime)
        {
            loadOperation.allowSceneActivation = true;
        }
    }
}
