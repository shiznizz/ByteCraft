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
        if (instance != null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if (subtitleText != null) 
        {
            subtitleText.gameObject.SetActive(false);
        }
    }

    public void ShowSubtitle(string text, float duration)
    {
        if (currentSubtitleRoutine != null) 
        {
            StopCoroutine(currentSubtitleRoutine);
        }

        currentSubtitleRoutine = StartCoroutine(ShowSubtitleRoutine(text, duration));
    }

    private IEnumerator ShowSubtitleRoutine(string text, float duration)
    {
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        subtitleText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
