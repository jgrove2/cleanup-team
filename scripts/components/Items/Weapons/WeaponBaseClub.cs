public class WeaponBasicClub : Weapon
{
    public WeaponBasicClub() : 
        base(
            "Basic Club", 
            "A simple wooden club, effective for close combat.", 
            ItemRarity.Common, 
            new DamageEffect("Blunt Damage", 10, DamageType.Physical)
        )
    {
    }
}