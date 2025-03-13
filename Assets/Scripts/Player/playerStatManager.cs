using UnityEngine;

public class playerStatManager : MonoBehaviour
{
    static public playerStatManager instance;

    [Header("Player Base Stat")]

    public int HP;
    public int HPMax;

    public int Armor;
    public int ArmorMax;

    public float playerHeight;
    public float standingHeight = 2f;
    public float crouchHeight = 0.5f;
    public int playerHPMax = 100;
    public int playerHP;

    [Header("Player Base Movement")]

    public float currSpeed;
    public float speedLimit;
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float slideSpeed;
    public float wallRunSpeed;

    [Header("Player Base Physics/gravity")]

    public float groundDrag;
    public float drag;
    public float gravity;

    [Header("Player Base Jump")]

    public float jumpForce;
    public float jumpMax;
    public float jumpCount;

    [Header("JetPack Stats")]
    public bool hasJetpack;
    public int jetpackFuelMax;
    public float jetpackFuel;
    public float jetpackFuelUse;
    public float jetpackFuelRegen;
    public float jetpackFuelRegenDelay;
    public int jetpackSpeed;
    public float jetpackHoldTimer = 0.01f;

    [Header("Slide Stats")]
    public float maxSlideTime;

    [Header("Wall Run Stats")]
    public float wallRunForce;
    public float maxWallRunTime;
    public float exitWallTime;
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    [Header("Starting Level bool")]
    public bool isPlayerInStartingLevel;

    public void Awake()
    {
        instance = this;
    }

}
