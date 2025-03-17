using UnityEngine;

public interface IPersistData
{
    public void LoadData(gameData data);
    public void SaveData(ref gameData data);
}

