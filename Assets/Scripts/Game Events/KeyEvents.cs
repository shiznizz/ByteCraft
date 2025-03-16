using System;
using UnityEngine;

public class KeyEvents
{
    public event Action<int> onKeyGained;
    public void KeyGained(int key)
    {
        if (onKeyGained != null)
        {
            onKeyGained(key);
        }
    }

    public event Action<int> onKeyChange;
    public void KeyChange(int key) 
    {
        if (onKeyChange != null)
        {
            onKeyChange(key); 
        }
    }
}
