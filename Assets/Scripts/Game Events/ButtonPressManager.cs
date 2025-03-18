using UnityEngine;

public class ButtonPressManager : MonoBehaviour
{
    [Header("Configuration")]
    private int startingButtons = 0;
    public int currentButtons { get; private set; }

    private void Awake()
    {
        currentButtons = startingButtons;
    }

    void OnEnable()
    {
        GameEventsManager.instance.buttonPressEvents.onButtonGained += ButtonGained;
    }

    private void OnDisable()
    {
        GameEventsManager.instance.buttonPressEvents.onButtonGained -= ButtonGained;
    }

    private void Start()
    {
        GameEventsManager.instance.buttonPressEvents.ButtonChange(currentButtons);
    }

    private void ButtonGained(int button)
    {
        currentButtons++;
        GameEventsManager.instance.buttonPressEvents.ButtonChange(currentButtons);
    }
}
