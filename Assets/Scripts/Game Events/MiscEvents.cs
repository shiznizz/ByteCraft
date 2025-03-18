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

    public event Action onEnemyKilled;
    public void EnemyKilled()
    {
        if (onEnemyKilled != null)
        {
            onEnemyKilled();
        }
    }

    public event Action onButtonPressed;
    public void ButtonPressed()
    {
        if (onButtonPressed != null)
        {
            onButtonPressed();
        }
    }
}
