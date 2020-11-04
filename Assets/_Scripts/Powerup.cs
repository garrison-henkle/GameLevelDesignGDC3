using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum PowerupType
    {
        DoubleDamage,
        FreeAmmo,
        UnlockAutomatic,
        UnlockShotgun,
        Ammo
    }

    //powerup
    [Header("Powerup")]
    public PowerupType powerup;

    [Header("Powerup Properties")]
    public bool overrideDuration = false;
    public float duration = 5f;

    [Header("Ammo Properties")]
    public WeaponManager.WeaponType weapon;
    public int ammoAmount = 8;

    //player
    [Header("Player")]
    public GameObject player;
    public string playerTag = "Player";
    private WeaponManager manager;

    private void Start()
    {
        if (player != null)
            manager = player.GetComponent<WeaponManager>();       
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag) && manager != null)
        {
            switch (powerup)
            {
                //handle powerups
                case PowerupType.DoubleDamage:
                    //override duration
                    if (overrideDuration) manager.doubleDamageTime = duration;
                    //activate powerup
                    manager.DoubleDamage();
                    break;
                case PowerupType.FreeAmmo:
                    //override duration
                    if (overrideDuration) manager.freeAmmoTime = duration;
                    //activate powerup
                    manager.FreeAmmo();
                    break;

                //handle weapon unlocks
                case PowerupType.UnlockAutomatic:
                    manager.UnlockWeapon(WeaponManager.WeaponType.Automatic);
                    break;
                case PowerupType.UnlockShotgun:
                    manager.UnlockWeapon(WeaponManager.WeaponType.Shotgun);
                    break;

                //handle ammo pickups
                case PowerupType.Ammo:
                    manager.GiveAmmo(weapon, ammoAmount);
                    break;
            }
            Destroy(gameObject);
        }
    }
}
