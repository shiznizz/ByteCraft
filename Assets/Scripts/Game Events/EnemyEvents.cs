using UnityEngine;
using System;

public class EnemyEvents
{
    public event Action<int> onEnemyGained;
    public void EnemyGained(int enemy)
    {
        if (onEnemyGained != null)
        {
            onEnemyGained(enemy);
        }
    }

    public event Action<int> onEnemyChange;
    public void EnemyChange(int enemy)
    {
        if (onEnemyChange != null)
        {
            onEnemyChange(enemy);
        }
    }
}
