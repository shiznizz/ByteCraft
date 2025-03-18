using UnityEngine;
using System;

public class ButtonPressEvents
{
    public event Action<int> onButtonGained;
    public void ButtonGained(int button)
    {
        if (onButtonGained != null)
        {
            onButtonGained(button);
        }
    }

    public event Action<int> onButtonChange;
    public void ButtonChange(int button)
    {
        if (onButtonChange != null)
        {
            onButtonChange(button);
        }
    }
}
