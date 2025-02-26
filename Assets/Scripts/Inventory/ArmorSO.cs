using UnityEngine;



[CreateAssetMenu(fileName = "New Armor", menuName = "Armor")]
public class ArmorSO : itemSO
{

    
    


    public override ArmorSO GetArmor()
    {
        return this;
    }

    public override itemSO GetItem()
    {
        return this;
    }

    public override weaponStats GetWeapon()
    {
        return null;
    }


}
