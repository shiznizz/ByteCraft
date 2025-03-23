using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;

    [Header("Music Settings")]
    [SerializeField] public AudioClip defaultMusic;
    public float fadeDuration = 2f; // time taken to fade in/out

    private bool isPlayingA = true;
    private AudioClip currentTrack;
    private Coroutine fadeCoroutine;
    private bool isDefaultMusicPlaying = true;

    void Start()
    {
        instance = this;
        if (musicSourceA == null || musicSourceB == null)
        {
            Debug.LogError("MusicManager requires two AudioSources.");
            return;
        }

        PlayMusic(defaultMusic, true);
/*        if (musicSourceA.clip == null) musicSourceA.clip = defaultMusic;
        musicSourceA.loop = true;
        musicSourceA.Play();*/
    }

    public void PlayMusic(AudioClip newTrack, bool isDefault = false)
    {
        if (newTrack == currentTrack) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(CrossfadeMusic(newTrack));

        currentTrack = newTrack;
        isDefaultMusicPlaying = isDefault;
    }

    private IEnumerator CrossfadeMusic(AudioClip newTrack)
    {
        AudioSource fadingOutSource = isPlayingA ? musicSourceA : musicSourceB;
        AudioSource fadingInSource = isPlayingA ? musicSourceB : musicSourceA;
        isPlayingA = !isPlayingA;

        fadingInSource.clip = newTrack;
        fadingInSource.volume = 0f;
        fadingInSource.loop = true;
        fadingInSource.Play();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            fadingOutSource.volume = Mathf.Lerp(1f, 0f, progress);
            fadingInSource.volume = Mathf.Lerp(0f, 1f, progress);
            yield return null;
        }

        fadingOutSource.Stop();
    }

    public void ReturnToDefaultMusic()
    {
        if (isDefaultMusicPlaying)
        {
            PlayMusic(defaultMusic, true);
        }
    }
}
