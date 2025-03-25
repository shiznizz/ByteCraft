using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ModulatedSoundBank : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private float minPitchVariation = 0.8f;
    [SerializeField] private float maxPitchVariation = 1.2f;
    [SerializeField][Range(1, 5)] private int maxClipsPerEvent = 1;
    [SerializeField] private float pitchModifier = 2f;

    private int currentClipIndex;
    public bool isPlaying = false;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a random clip with pitch modulation.
    /// </summary>

    public void PlayRandomSound()
    {
        if (audioClips == null || audioClips.Length == 0) return;

        int clipCount = Mathf.Min(maxClipsPerEvent, audioClips.Length);

        for (int i = 0; i < clipCount; i++)
        {
            AudioClip clip = audioClips[Random.Range(0, audioClips.Length)];
            audioSource.pitch = pitchModifier * Random.Range(minPitchVariation, maxPitchVariation);
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Plays a specific clip by index with pitch modulation.
    /// </summary>

    public void PlaySpecificSound(int index)
    {
        if (audioClips == null || audioClips.Length == 0) return;
        if (index < 0 || index >= audioClips.Length) return;

        audioSource.pitch = pitchModifier * Random.Range(minPitchVariation, maxPitchVariation);
        audioSource.PlayOneShot(audioClips[index]);

    }

    /// <summary>
    /// Play a specific sound on assigned AudioSource
    /// </summary>

    public void PlaySpecificExternal(AudioSource externalAudioSource, AudioClip clip)
    {
        audioSource.pitch = pitchModifier * Random.Range(minPitchVariation, maxPitchVariation); Random.Range(minPitchVariation, maxPitchVariation);
        externalAudioSource.PlayOneShot(clip, 2);
    }

    public float GetClipDuration()
    {
        SetCurrentClipIndex();
        AudioClip clip = audioClips[currentClipIndex];
        float clipDuration = clip.length;
        return clipDuration;
    }

    public void SetCurrentClipIndex()
    {
        int clipIndex = Random.Range(0, audioClips.Length);
    }

    public void PlayCurrentClip()
    {

        audioSource.pitch = pitchModifier * Random.Range(minPitchVariation, maxPitchVariation); Random.Range(minPitchVariation, maxPitchVariation);
        audioSource.PlayOneShot(audioClips[currentClipIndex]);
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying; 
    }
}
