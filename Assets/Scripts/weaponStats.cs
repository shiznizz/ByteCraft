using UnityEngine;

[CreateAssetMenu]

public class weaponStats : ScriptableObject
{
    public enum weaponType { Gun, Melee, Magic }
    public weaponType type;
    public gunStats gun;
    public meleeWepStats meleeWep;
    public magicWepStats magicWep;
}
