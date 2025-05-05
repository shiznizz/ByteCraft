using Ink.Parsed;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class gameData
{
    public List<weaponStats> playerWeapons;
    public List<itemSO> playerInventory;
    public int  playerHP;
    public float playerShield;
    public int weaponPos;
    

    public gameData()
    {
        this.playerWeapons = new();
       

        this.weaponPos = 0;

        this.playerHP = 100;
        this.playerShield = 100;
        this.playerInventory = new();
    }
}
