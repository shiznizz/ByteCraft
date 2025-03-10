using UnityEngine;

public class playerStatManager : MonoBehaviour
{
    static public playerStatManager instance;

    [Header("Player Base Stat")]

    public int playerHP;
    public int playerHPMax;

    public int playerArmor;
    public int playerArmorMax;

    [Header("Player Base Movement")]

    public float playerSpeed;
    public float playerWalkSpeed;
    public float playerSprintSpeed;
    public float playerCrouchSpeed;
    public float playerSlideSpeed;

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





}
