using UnityEngine;
using System;

public class MiscEvents
{
    public event Action onKeyCollected;
    public void KeyCollected()
    {
        if (onKeyCollected != null)
        {
            onKeyCollected();
        }
    }
}
