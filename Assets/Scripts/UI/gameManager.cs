using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System.Diagnostics.Contracts;
using UnityEngine.Audio;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;



public class gameManager : MonoBehaviour
{
    public static gameManager instance;

    [Header("Menus")]
    [SerializeField] GameObject menuInventory;
    public GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuDeath;
    [SerializeField] public GameObject menuObjectiveFail;
    [SerializeField] public TextMeshProUGUI objectiveText;


    [Header("UI Elements to Toggle Visibility")]
    [SerializeField] GameObject ammoHUD;
    [SerializeField] GameObject jetpackHUD;
    [SerializeField] public GameObject enemyHealthbar;
    [SerializeField] GameObject overShieldHUD;
    public Image playerHPBar;
    public Image enemyHPBar;
    public Image JPFuelGauge;
    public Image grappleGauge;
    public Image shieldBar;
    public Image overShieldBar;
    public GameObject playerDamageScreen;
    public GameObject checkpointPopup;

    [Header("Text Fields to Update")]
    [SerializeField] public TMP_Text ammoCurText;
    [SerializeField] public TMP_Text ammoMaxText;
    [SerializeField] public TMP_Text ammoReserveText;

    [Header("State Monitoring Values")]
    public bool isPaused;
    public GameObject player;
    public playerController playerScript;
    public GameObject playerSpawnPos;

    int goalCount;

    [Header("Inventory Options")]
    [SerializeField] GameObject inventorySlot;
    public GameObject selectedEquipSlot;
    public GameObject selectedInventorySlot;

    public GameObject[] slots;

    public Image itemIcon;

    public TMP_Text deleteNotifaction;
    public TMP_Text itemDescription;
    public TMP_Text itemName;

    public GameObject displaySlot;

    [Header("Low Health Screen Indicator")]
    public Image lowHealthIndicator;

    [SerializeField] float lowHealthThreshold = 0.25f;
    [SerializeField] float heartbeatSpeed = 2f;
    [SerializeField] float heartbeatMagnitude = 0.2f;
    [SerializeField] float baseAlpha = 0.5f;

    [SerializeField] AudioMixer mixer;
    public bool inventoryOpen = false;
    private bool keepMenu;

    private int selectedButtonIndex = 0;
    private Button[] menuButtons;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
    }

    private void Start()
    {
        // Assign a default value to menuActive if it's not already assigned
        if (menuActive == null)
        {
            menuActive = menuPause;  // Or any other menu GameObject you want to set as the default
        }

        updateInventory();
        getSavedAudioSettings();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
                switchMenu(menuPause);
            else if (!keepMenu)
            {
                stateUnpause();
            }
            inventoryOpen = false;
        }
        if (Input.GetButtonDown("Inventory"))
        {
            inventoryOpen = true;
            switchMenu(menuInventory);
        }
        // handle Keyboard Menu Navigation
        if (menuActive != null)
        {
            HandleMenuNavigation();
        }

        CheckLowHealth();
    }
    #region Menus

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        // find and highlight buttons
        menuButtons = menuActive.GetComponentsInChildren<Button>();

        if (menuButtons.Length > 0)
        {
            selectedButtonIndex = 0;
            HighlightButton(selectedButtonIndex);
        }
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
        keepMenu = false;
    }

    public void mainMenu()
    {
        isPaused = !isPaused;
        Time.timeScale = 1;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void switchMenu(GameObject menuToOpen, bool closeMenu = true)
    {
        if (menuActive == null)
        {
            Debug.Log(menuToOpen);           
            menuActive = menuToOpen;
            menuActive.SetActive(true);
            statePause();
        }
        else if (closeMenu && menuActive == menuToOpen)
        {
            stateUnpause();
        }
        else
        {
            menuActive.SetActive(false);
            menuActive = menuToOpen;
            menuActive.SetActive(true);
        }

        // update button navigation
        menuButtons = menuActive.GetComponentsInChildren<Button>();

        if (menuButtons.Length > 0)
        {
            selectedButtonIndex = 0;
            HighlightButton(selectedButtonIndex);
        }
    }

    public void youLose()
    {
        switchMenu(menuDeath);
        keepMenu = true;
    }

    public void objectiveFailed(string failedObj)
    {
        keepMenu = true;
        switchMenu(menuObjectiveFail);
        objectiveText.SetText(failedObj);
    }

    public void youWin()
    {
        keepMenu = true;
        switchMenu(menuWin);
    }

    #endregion Menus

    #region Menu Navigation (Keyboard)
    void HandleMenuNavigation()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

        // down Arrow / s
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedButtonIndex = (selectedButtonIndex + 1) % menuButtons.Length;
            HighlightButton(selectedButtonIndex);
        }

        // up Arrow / w
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedButtonIndex = (selectedButtonIndex - 1 + menuButtons.Length) % menuButtons.Length;
            HighlightButton(selectedButtonIndex);
        }

        // right Arrow / d
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedButtonIndex = (selectedButtonIndex + 1) % menuButtons.Length;
            HighlightButton(selectedButtonIndex);
        }

        // left Arrow / a
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedButtonIndex = (selectedButtonIndex - 1 + menuButtons.Length) % menuButtons.Length;
            HighlightButton(selectedButtonIndex);
        }

        // enter / select
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            menuButtons[selectedButtonIndex].onClick.Invoke();
        }

    }

    void HighlightButton(int index)
    {
        if (menuButtons == null || index < 0 || index >= menuButtons.Length) return;

        EventSystem.current.SetSelectedGameObject(menuButtons[index].gameObject);
        ColorBlock cb = menuButtons[index].colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.yellow;
        cb.selectedColor = Color.yellow;
        cb.pressedColor = Color.red;
        cb.colorMultiplier = 1.2f;

        menuButtons[index].colors = cb;
    }
    #endregion

    #region UI Element Updates

    public void updateAmmo()
    {
        weaponStats gun = inventoryManager.instance.returnCurrentWeapon();
        ammoCurText.text = gun.ammoCur.ToString("D3");
        ammoMaxText.text = gun.ammoMax.ToString("D3");
        ammoReserveText.text = gun.ammoReserve.ToString("D3");
    }

    public void hideAmmo()
    {
        ammoHUD.SetActive(false);
    }

    public void showAmmo()
    {
        ammoHUD.SetActive(true);
    }

    public void showJetpack()
    {
        jetpackHUD.SetActive(true);
    }

    public void hideJetpack()
    {
        jetpackHUD.SetActive(false);
    }

    public void showOverShield()
    {
        overShieldHUD.SetActive(true);
    }

    public void hideOverShield()
    {
        overShieldHUD.SetActive(false);
    }

    private void CheckLowHealth()
    {
        // Check if playerStatManager or lowHealthIndicator is null to avoid NullReferenceException
        if (playerStatManager.instance == null || lowHealthIndicator == null) return;

        if (playerStatManager.instance.HPMax <= 0) return;

        float hpRatio = (float)playerStatManager.instance.playerHP / playerStatManager.instance.playerHPMax;

        if (hpRatio <= lowHealthThreshold)
        {
            float alpha = baseAlpha + Mathf.Sin(Time.time * heartbeatSpeed) * heartbeatMagnitude;
            //alpha = Mathf.Clamp01(alpha);

            Color c = lowHealthIndicator.color;
            c.a = alpha;
            lowHealthIndicator.color = c;  
        }
        else
        {
            Color c = lowHealthIndicator.color;
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime);
            lowHealthIndicator.color = c;
        }
    }

    #endregion UI Element Updates

    #region Inventory
    public void updateInventory()
    {

        for (int i = 0; i < slots.Length; i++)
        {

            if (inventoryManager.instance.inventory.Count == 0)
            {
                slots[i].transform.GetChild(1).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(1).GetComponent<Image>().enabled = false;
                slots[i].GetComponent<SlotBoss>().isFull = false;
                slots[i].GetComponent<SlotBoss>().item = null;
            }

            try
            {
                
                slots[i].transform.GetChild(1).GetComponent<Image>().enabled = true;
                slots[i].transform.GetChild(1).GetComponent<Image>().sprite = inventoryManager.instance.inventory[i].itemIcon;
                slots[i].GetComponent<SlotBoss>().item = inventoryManager.instance.inventory[i];
                slots[i].GetComponent<SlotBoss>().isFull = true;

            }
            catch
            {
                slots[i].transform.GetChild(1).GetComponent<Image>().sprite = null;
                slots[i].transform.GetChild(1).GetComponent<Image>().enabled = false;
                slots[i].GetComponent<SlotBoss>().isFull = false;
                slots[i].GetComponent<SlotBoss>().item = null;

            }
        }
    }

    public void deselectSlot()
    {
        if (selectedEquipSlot != null)
        {
            selectedEquipSlot.GetComponent<equipSlot>().isSelected = false;
            selectedEquipSlot.transform.GetChild(1).gameObject.SetActive(false);
        }

        if(selectedInventorySlot != null)
        {
            selectedInventorySlot.GetComponent<SlotBoss>().isSelected = false;
            selectedInventorySlot.transform.GetChild(2).gameObject.SetActive(false);
        }

        itemDescription.text = "";
        itemName.text = "";
        displaySlot.SetActive(false);
    }
    #endregion Inventory


    private void getSavedAudioSettings()
    {
        float value;
        foreach (AudioMixerGroup group in mixer.FindMatchingGroups(""))
        {
            value = PlayerPrefs.GetFloat(group.name);
            if (value == 0)
            {
                mixer.SetFloat(group.name, -80);
            }
            else
            {
                mixer.SetFloat(group.name, Mathf.Log10(value) * 20);
            }
        }
    }
}
