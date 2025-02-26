using UnityEngine;

[CreateAssetMenu]

public class weaponStats : itemSO
{
    public enum weaponType { Gun, Melee, Magic }
    public weaponType type;
    public gunStats gun;
    public meleeWepStats meleeWep;
    public magicWepStats magicWep;

    public override itemSO GetItem()
    {
        return this;
    }

    public override ArmorSO GetArmor()
    {
        return null;
    }

    public override weaponStats GetWeapon()
    {
        return this;
    }
}
