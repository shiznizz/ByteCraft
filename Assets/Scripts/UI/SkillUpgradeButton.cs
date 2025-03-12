using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]

public class SkillUpgradeButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum PlayerStat { Sprint }
    [SerializeField] PlayerStat skillToIncrease;
    [SerializeField] int percentToIncrease;
    [SerializeField] int upgradeCost;
    [SerializeField] SkillUpgradeButton skillDependence;

    [SerializeField] Sprite skillSelected;
    [SerializeField] Sprite skillUnselected;
    [SerializeField] Sprite skillPurchasable;
    [SerializeField] Sprite skillNotPurchasable;

    bool isSelected;
    Image background;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        background = GetComponent<Image>();
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
                    playerStatManager.instance.upgradeCurrency -= upgradeCost;
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
            case PlayerStat.Sprint:
                playerStatManager.instance.sprintSpeed *= (1 + (percentToIncrease/100.0F));
                break;
        }
    }
}
