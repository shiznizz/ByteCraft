using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

[RequireComponent(typeof(Image))]

public class MenuTabButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public MenuTabGroup tabGroup;
    public Image background;

    void Awake()
    { 
        background = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelect(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    
}
