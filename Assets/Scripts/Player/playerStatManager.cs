using UnityEngine;

public class playerStatManager : MonoBehaviour
{
    static public playerStatManager instance;

    [Header("Player Base Stat")]

    public int playerHP;
    public int playerHPMax;

    public int playerArmor;
    public int playerArmorMax;

    public float playerHeight;
    public float standingHeight = 2f;
    public float crouchHeight = 0.5f;

    [Header("Player Base Movement")]

    public float playerSpeed;
    public float playerWalkSpeed;
    public float playerSprintSpeed;
    public float playerCrouchSpeed;
    public float playerSlideSpeed;
    public float playerWallRunSpeed;

    [Header("Player Base Physics/gravity")]

    public float playerDrag;
    public float playergravity;

    [Header("Player Base Jump")]

    public float playerJumpSpeed;
    public float playerJumpMax;
    public float playerJumpCount;

    public void Awake()
    {
        instance = this;
    }

    [Header("JetPack Options")]
    public bool hasJetpack;
    public int jetpackFuelMax;
    public float jetpackFuel;
    public float jetpackFuelUse;
    public float jetpackFuelRegen;
    public float jetpackFuelRegenDelay;
    public int jetpackSpeed;
    public float jetpackHoldTimer = 0.01f;

    public float maxSlideTime;

    public float wallRunForce;
    public float maxWallRunTime;
    public float exitWallTime;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
}
