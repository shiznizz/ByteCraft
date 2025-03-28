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
    string toggleStatus = "";

    private void Awake()
    {
        slider.onValueChanged.AddListener(SliderValueChanged);
        toggle.onValueChanged.AddListener(ToggleValueChanged);
    }

    void Start()
    {
        Debug.Log("Start");
        slider.value = PlayerPrefs.GetFloat(volumeParameter, slider.value);
        lastVolume = PlayerPrefs.GetFloat(volumeParameter + "lastVolume", lastVolume);
        toggleStatus = PlayerPrefs.GetString(volumeParameter + "lastToggle", toggleStatus);

        Debug.Log("ToggleStatus = " + toggleStatus);

        if (toggleStatus == "False")
        {
            toggle.isOn = false;
        }
        else
        {
            toggle.isOn = true;
        }
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(volumeParameter, slider.value);
        PlayerPrefs.SetFloat(volumeParameter + "lastVolume", lastVolume);
        PlayerPrefs.SetString(volumeParameter + "lastToggle", toggle.isOn.ToString());
        PlayerPrefs.Save();
    }

    private void SliderValueChanged(float value)
    {
        if (slider.value <= 0.01)
        {
            mixer.SetFloat(volumeParameter, -80);
        }
        else
        {
            mixer.SetFloat(volumeParameter, Mathf.Log10(value) * 20);
        }

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


}
