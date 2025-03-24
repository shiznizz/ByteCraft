using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettings : MonoBehaviour
{
    [SerializeField] string sensitivityParameter; 
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] TextMeshProUGUI sensitivityValueText;
    [SerializeField] cameraController camController;

    //This will be the last saved sensitivity value
    float lastSensitivity;
    bool disableSliderEvent;

    private void Awake()
    {
        // Add listeners for changes in the slider value
        sensitivitySlider.onValueChanged.AddListener(SensitivitySliderValueChanged);
    }

    private void OnDisable()
    {
        // Save the current sensitivity value and the last sensitivity value when the script is disabled
        PlayerPrefs.SetFloat(sensitivityParameter, sensitivitySlider.value);
        //PlayerPrefs.SetFloat(sensitivityParameter + "lastSensitivity", lastSensitivity);
        PlayerPrefs.Save();
    }

    void Start()
    {
        // Set the slider to the saved value
        sensitivitySlider.value = PlayerPrefs.GetFloat(sensitivityParameter, sensitivitySlider.value);
        Debug.Log(PlayerPrefs.GetFloat(sensitivityParameter, sensitivitySlider.value));
        // Load the last sensitivity value if needed for reset logic
        //lastSensitivity = PlayerPrefs.GetFloat(sensitivityParameter + "lastSensitivity", lastSensitivity);

        // Update the camera sensitivity to match the loaded value
        //camController.SetSensitivity(lastSensitivity);
    }

    private void SensitivitySliderValueChanged(float value)
    {
        // Update the camera controller with the new sensitivity value
        camController.SetSensitivity(value);
    }
}
