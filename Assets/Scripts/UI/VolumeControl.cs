using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeControl : MonoBehaviour
{
    [SerializeField] string volumeParameter;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Slider slider;
    [SerializeField] Toggle toggle;

    float lastVolume;
    bool disableToggleEvent;

    private void Awake()
    {
        slider.onValueChanged.AddListener(SliderValueChanged);
        toggle.onValueChanged.AddListener(ToggleValueChanged);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(volumeParameter, slider.value);
        PlayerPrefs.SetFloat(volumeParameter + "lastVolume", lastVolume);
    }

    private void SliderValueChanged(float value)
    {
        mixer.SetFloat(volumeParameter, Mathf.Log10(value)*20);

        disableToggleEvent = true;
        toggle.isOn = slider.value > slider.minValue;
        disableToggleEvent = false;
    }

    private void ToggleValueChanged(bool enableSound)
    {
        if (disableToggleEvent)
            return;

        if (enableSound)
        {
            slider.value = lastVolume;
        }
        else
        {
            lastVolume = slider.value;
            slider.value = slider.minValue;
        }
    }

    void Start()
    {
        slider.value = PlayerPrefs.GetFloat(volumeParameter, slider.value);
        lastVolume = PlayerPrefs.GetFloat(volumeParameter + "lastVolume", lastVolume);
    }

}
