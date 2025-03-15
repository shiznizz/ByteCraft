using UnityEngine;
using System.Collections;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager instance;

    [Header("UI Reference")]
    [SerializeField] private TMP_Text subtitleText;

    private Coroutine currentSubtitleRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        // singleton pattern for global access
        if (instance != null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // ensure subtitle text is initially hidden
        if (subtitleText != null) 
        {
            subtitleText.gameObject.SetActive(false);
        }
    }

    // use to display subtitle line for a set duration
    public void ShowSubtitle(string text, float duration)
    {
        // if another subtitle is showing, stop it so we can show new one immediately
        if (currentSubtitleRoutine != null) 
        {
            StopCoroutine(currentSubtitleRoutine);
        }

        currentSubtitleRoutine = StartCoroutine(ShowSubtitleRoutine(text, duration));
    }

    private IEnumerator ShowSubtitleRoutine(string text, float duration)
    {
        // show text
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);

        // wait for duration
        yield return new WaitForSeconds(duration);

        // hide text
        subtitleText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
