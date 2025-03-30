using Ink.Parsed;
using System.Collections.Generic;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class gameData
{
    public List<bool> upgradeSlotsBool;
    public List<weaponStats> playerWeapons;
    public List<itemSO> playerInventory;
    public int  playerHP;
    public int  playerHPMax;
    public float playerShield;
    public int weaponPos;
    public float jetPackRegenDelay;
    public float sprintSpeed;

    public int upgradeCurrency;

    public int curSprintMod;
    public int curJetpackRegenMod;
    public int curHPMaxMod;
    public int curShieldMaxMod;

    public gameData()
    {
        this.playerWeapons = new();
        this.weaponPos = 0;

        this.playerHP = 100;
        this.playerHPMax = 100;
        this.playerShield = 100;
        this.sprintSpeed = 10;
        this.jetPackRegenDelay = 3;
        this.curSprintMod = 0;
        this.curJetpackRegenMod = 0;
        this.curHPMaxMod = 0;
        this.curShieldMaxMod = 0;

        this.upgradeCurrency = 0;

        this.upgradeSlotsBool = new List<bool>();
        this.playerInventory = new();
    }
}
