using UnityEngine;

public class playerStatManager : MonoBehaviour
{
    static public playerStatManager instance;

    public int playerHP;
    public int playerArmor;
    public int playerSpeed;

    public void Awake()
    {
        instance = this;
    }





}
