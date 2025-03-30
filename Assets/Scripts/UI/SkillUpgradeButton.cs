using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class SkillUpgradeButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum PlayerStat { MaxHealth, MaxShield, SprintSpeed, JetpackRegen }
    [SerializeField] PlayerStat skillToIncrease;
    [SerializeField] int percentToIncrease;
    [SerializeField] int upgradeCost;
    [SerializeField] SkillUpgradeButton skillDependence;

    [SerializeField] public Sprite skillSelected;
    [SerializeField] Sprite skillUnselected;
    [SerializeField] Sprite skillPurchasable;
    [SerializeField] Sprite skillNotPurchasable;

    

    public bool isSelected;
    public Image background;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        background = GetComponent<Image>();
        if (isSelected )
        {
            background.sprite = skillSelected;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelected)
        {
            if (skillDependence == null || skillDependence.isSelected)
            {
                if (canUpgrade())
                {
                    background.sprite = skillSelected;
                    playerStatManager.instance.IncrementUpgradeCurrency(-upgradeCost);
                    isSelected = true;
                    UpgradePlayerStat();
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
        {
            if (skillDependence == null || skillDependence.isSelected)
            {
                if (canUpgrade())
                    background.sprite = skillPurchasable;
                else
                    background.sprite = skillNotPurchasable;
            }
        }    
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
        {
            background.sprite = skillUnselected;
        }
    }

    bool canUpgrade()
    {
        return upgradeCost <= playerStatManager.instance.upgradeCurrency;
    }

    private void UpgradePlayerStat()
    {
        switch (skillToIncrease)
        {
            case PlayerStat.MaxHealth:
                playerStatManager.instance.increaseMaxHealth(percentToIncrease);
                break;
            case PlayerStat.MaxShield:
                playerStatManager.instance.increaseMaxShield(percentToIncrease);
                break;
            case PlayerStat.SprintSpeed:
                playerStatManager.instance.increaseSprintSpeed(percentToIncrease);
                break;
            case PlayerStat.JetpackRegen:
                playerStatManager.instance.increaseJetpackRegen(percentToIncrease);
                break;
        }
    }

    void onLoad()
    {
        
    }

    
}
