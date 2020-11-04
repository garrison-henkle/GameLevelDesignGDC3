using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponManager : MonoBehaviour
{
    //game manager
    private GameManager manager;

    //weapons and weapon states
    public enum WeaponType
    {
        Pistol,
        Automatic,
        Shotgun
    }
    private Dictionary<WeaponType, bool> unlocked = new Dictionary<WeaponType, bool>();
    private Dictionary<WeaponType, bool> cooldown = new Dictionary<WeaponType, bool>();
    private Dictionary<WeaponType, int> ammo = new Dictionary<WeaponType, int>();
    private Dictionary<WeaponType, int> magSize = new Dictionary<WeaponType, int>();
    private Dictionary<WeaponType, int> bulletsInMag = new Dictionary<WeaponType, int>();
    private Dictionary<WeaponType, float> reloadTime = new Dictionary<WeaponType, float>();

    //shoot delegate
    public delegate void ShootWeapon(Vector3 position, Vector3 direction);
    private ShootWeapon Shoot;

    [Header("Player")]
    public Camera playerCamera = null;
    public GameObject playerHands = null;

    [Header("Enemy")]
    public string enemyTag = "Enemy";

    [Header("Audio")]
    public AudioClip machineGun;
    private AudioSource audioPlayer;

    //state
    [Header("Equipped Weapon")]
    public WeaponType initialWeapon = WeaponType.Pistol;
    public int initialAmmo = 8;
    private WeaponType currentWeapon = WeaponType.Pistol;

    //weapon attributes
    [Header("Weapon Mag Sizes")]
    public int pistolMagSize = 8;
    public int automaticMagSize = 30;
    public int shotgunMagSize = 2;
    [Header("Weapon Reload Times")]
    public float pistolReloadTime = 1.5f;
    public float automaticReloadTime = 3f;
    public float shotgunReloadTime = 6f;
    [Header("Misc Weapon Attributes")]
    public float automaticIntershotTime = 0.1f;
    private bool automaticReloading = false;

    //bullets
    [Header("Bullets")]
    public GameObject bulletObject = null;
    public float baseProjectileSpeed = 15f;
    public int baseProjectileDamage = 20;
    public float baseMaxEffectiveRange = 25f;
    private float currentEffectiveRange;

    //animation
    [Header("Animation")]
    public GameObject pistol;
    public GameObject automatic;
    public GameObject shotgun;
    private GameObject currentWeaponSprite;

    //powerups
    [Header("Powerups")]
    public float doubleDamageTime = 10f;
    public float freeAmmoTime = 5f;
    private bool doubleDamage = false;
    private bool freeAmmo = false;

    private void Start()
    {
        //get the game manager
        var managerObj = GameObject.Find("GameManager");
        if (managerObj != null)
            manager = managerObj.GetComponent<GameManager>();

        //audio
        audioPlayer = GetComponent<AudioSource>();
        audioPlayer.loop = true;
        audioPlayer.clip = machineGun;

        //populate the unlocked dictionary
        unlocked.Add(WeaponType.Pistol, false);
        unlocked.Add(WeaponType.Automatic, false);
        unlocked.Add(WeaponType.Shotgun, false);

        //set all weapons off cooldown
        cooldown.Add(WeaponType.Pistol, false);
        cooldown.Add(WeaponType.Automatic, false);
        cooldown.Add(WeaponType.Shotgun, false);

        //set the starting ammo to 0 for all
        ammo.Add(WeaponType.Pistol, 0);
        ammo.Add(WeaponType.Automatic, 0);
        ammo.Add(WeaponType.Shotgun, 0);

        //set the mag size
        magSize.Add(WeaponType.Pistol, pistolMagSize);
        magSize.Add(WeaponType.Automatic, automaticMagSize);
        magSize.Add(WeaponType.Shotgun, shotgunMagSize);

        //fill the mags of all weapons
        bulletsInMag.Add(WeaponType.Pistol, pistolMagSize);
        bulletsInMag.Add(WeaponType.Automatic, automaticMagSize);
        bulletsInMag.Add(WeaponType.Shotgun, shotgunMagSize);

        //set the reload times
        reloadTime.Add(WeaponType.Pistol, pistolReloadTime);
        reloadTime.Add(WeaponType.Automatic, automaticReloadTime);
        reloadTime.Add(WeaponType.Shotgun, shotgunReloadTime);

        //unlock, give ammo for, and equip initial weapon
        UnlockWeapon(WeaponType.Pistol);
        currentWeapon = initialWeapon;
        ammo[currentWeapon] = initialAmmo;
        switch (currentWeapon)
        {
            case WeaponType.Pistol:
                EquipPistol();
                break;
            case WeaponType.Automatic:
                EquipAutomatic();
                break;
            case WeaponType.Shotgun:
                EquipShotgun();
                break;
        }
    }

    private void Update()
    {
        var scale = transform.localScale;

        //rotate the current equipped weapon
        var direction = GetDirectionToMouse();
        var rotation = Quaternion.LookRotation(direction);
        rotation.y = rotation.x = 0;
        currentWeaponSprite.transform.rotation = rotation;

        //1 switches to pistol
        if (Input.GetKeyDown(KeyCode.Alpha1) && unlocked[WeaponType.Pistol])
            EquipPistol();

        //2 switches to automatic
        else if (Input.GetKeyDown(KeyCode.Alpha2) && unlocked[WeaponType.Automatic])
            EquipAutomatic();

        //3 switches to shotgun
        else if (Input.GetKeyDown(KeyCode.Alpha3) && unlocked[WeaponType.Shotgun])
            EquipShotgun();

        //handle weapon shots if the gun is not on cooldown and handle the special case for the automatic gun (start firing after reload while still holding the mouse down)
        //also prevent shooting the weapon backwards
        if (((!cooldown[currentWeapon] && Input.GetMouseButtonDown((int)MouseButton.LeftMouse)) ||
             (!cooldown[currentWeapon] && Input.GetMouseButton((int)MouseButton.LeftMouse) && automaticReloading && currentWeapon == WeaponType.Automatic)) &&
            ((direction.x >= 0 && scale.x > 0) ||
             (direction.x <= 0 && scale.x < 0)))
        {
            Shoot(playerHands.transform.position, direction);
            if (currentWeapon == WeaponType.Automatic && manager.state != GameManager.GameState.Death)
            {
                audioPlayer.clip = machineGun;
                audioPlayer.loop = true;
                audioPlayer.Play();
            }  
        }
            
                

        //manual reloading
        if (!cooldown[currentWeapon] && Input.GetKeyDown(KeyCode.R))
            Reload();

        //handle releasing the mouse for the automatic
        if (Input.GetMouseButtonUp((int)MouseButton.LeftMouse))
        {
            CancelInvoke(nameof(AutomaticHelper));
            if(audioPlayer.clip == machineGun)
                audioPlayer.Stop();
        }
            

        //update ammo UI
        if(manager != null)
        {
            manager.ammo1Text = $"{bulletsInMag[WeaponType.Pistol]} / {ammo[WeaponType.Pistol]}";
            manager.ammo2Text = $"{bulletsInMag[WeaponType.Automatic]} / {ammo[WeaponType.Automatic]}";
            manager.ammo3Text = $"{bulletsInMag[WeaponType.Shotgun]} / {ammo[WeaponType.Shotgun]}";
        }
    }

    /*Weapon Management*/

    private void EquipPistol()
    {
        //show the weapon, assign the shoot function and sprite
        ShowWeapon(true, false, false);
        Shoot = ShootPistol;
        currentWeapon = WeaponType.Pistol;
        currentWeaponSprite = pistol;
        currentEffectiveRange = baseMaxEffectiveRange * 1.5f; //the range at which bullets do no damage
    }

    private void EquipAutomatic()
    {
        //show the weapon, assign the shoot function and sprite
        ShowWeapon(false, true, false);
        Shoot = ShootAutomatic;
        currentWeapon = WeaponType.Automatic;
        currentWeaponSprite = automatic;
        currentEffectiveRange = baseMaxEffectiveRange; //the range at which bullets do no damage
    }

    private void EquipShotgun()
    {
        //show the weapon, assign the shoot function and sprite
        ShowWeapon(false, false, true);
        Shoot = ShootShotgun;
        currentWeapon = WeaponType.Shotgun;
        currentWeaponSprite = shotgun;
        currentEffectiveRange = baseMaxEffectiveRange * 0.5f; //the range at which bullets do no damage
    }

    /*Shooting*/

    public void ShootPistol(Vector3 position, Vector3 direction)
    {
        if (bulletsInMag[currentWeapon] <= 0)
            Reload();
        else
        {
            //update the ammo
            if(!freeAmmo) bulletsInMag[currentWeapon] -= 1;

            //create the bullet
            CreateBullet(position, direction);
        }
    }

    private void ShootAutomatic(Vector3 position, Vector3 direction)
    {
        automaticReloading = false;
        InvokeRepeating(nameof(AutomaticHelper), 0, automaticIntershotTime);
    }

    private void AutomaticHelper()
    {
        if (bulletsInMag[currentWeapon] <= 0)
        {
            Reload();
            CancelInvoke(nameof(AutomaticHelper));
            audioPlayer.Stop();
        }
        else
        {
            //update the ammo
            if (!freeAmmo) bulletsInMag[currentWeapon] -= 1;

            //create the bullet
            CreateBullet(playerHands.transform.position, GetDirectionToMouse());
        }
    }

    private void ShootShotgun(Vector3 position, Vector3 direction)
    {
        if (bulletsInMag[currentWeapon] <= 0)
            Reload();
        else
        {
            //update the ammo
            if(!freeAmmo) bulletsInMag[currentWeapon] -= 1;

            //create the 9 bullets from -10 to +10 degrees around the aiming direction
            for(float f = -10; f <= 10; f += 2.5f)
                CreateBullet(position, Quaternion.Euler(0, 0, f) * direction);
        }
    }

    /*Helpers*/
    
    public void UnlockWeapon(WeaponType weapon)
    {
        unlocked[weapon] = true;

        //update UI
        if (manager != null)
            switch (weapon)
            {
                case WeaponType.Pistol:
                    manager.ShowGun1(true);
                    break;
                case WeaponType.Automatic:
                    manager.ShowGun2(true);
                    break;
                case WeaponType.Shotgun:
                    manager.ShowGun3(true);
                    break;
            }
    }

    public void GiveAmmo(WeaponType weapon, int amount)
    {
        ammo[weapon] += amount;
    }

    private void ShowWeapon(bool pistol, bool automatic, bool shotgun)
    {
        //display only the active weapon
        this.pistol.SetActive(pistol);
        this.automatic.SetActive(automatic);
        this.shotgun.SetActive(shotgun);
    }

    private void CreateBullet(Vector3 position, Vector3 direction)
    {
        //create the bullet at the player hands
        var bullet = Instantiate(bulletObject);

        //activate the new bullet
        var attr = bullet.GetComponent<Bullet>();
        attr.Activate(position, direction, baseProjectileSpeed, baseProjectileDamage * (doubleDamage ? 2 : 1), currentEffectiveRange, enemyTag);
    }

    private void Reload()
    {
        //if the mnag is not empty, add the bullets to ammo
        if (bulletsInMag[currentWeapon] > 0)
            ammo[currentWeapon] += bulletsInMag[currentWeapon];

        //if there is too few bullets, use all available to fill mag
        if(ammo[currentWeapon] < magSize[currentWeapon])
        {
            bulletsInMag[currentWeapon] = ammo[currentWeapon];
            ammo[currentWeapon] = 0;
        }

        //if there is enough bullets, fill an entire mag
        else
        {
            bulletsInMag[currentWeapon] = magSize[currentWeapon];
            ammo[currentWeapon] -= magSize[currentWeapon];
        }

        //allow automatic to continue to hold down fire during reload so that shooting can begin immediately
        if (currentWeapon == WeaponType.Automatic)
            automaticReloading = true;

        //set reload cooldown on the current gun
        StartCooldown(currentWeapon);   
    }

    private Vector3 GetDirectionToMouse()
    {
        //get direction the gun is facing
        var target = playerCamera.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0;

        return target - playerHands.transform.position;
    }

    /*Cooldowns*/

    private void StartCooldown(WeaponType weapon)
    {
        cooldown[weapon] = true;
        if (manager != null) manager.ShowReload(true);

        //change cooldown back to false after the reload time
        switch (weapon)
        {
            case WeaponType.Pistol:
                Invoke(nameof(EndCooldownPistol), reloadTime[WeaponType.Pistol]);
                break;
            case WeaponType.Automatic:
                Invoke(nameof(EndCooldownAutomatic), reloadTime[WeaponType.Automatic]);
                break;
            case WeaponType.Shotgun:
                Invoke(nameof(EndCooldownShotgun), reloadTime[WeaponType.Shotgun]);
                break;
        }
    }

    private void EndCooldownPistol()
    {
        cooldown[WeaponType.Pistol] = false;
        if (manager != null) manager.ShowReload(false);
    }

    private void EndCooldownAutomatic()
    {
        cooldown[WeaponType.Automatic] = false;
        if (manager != null) manager.ShowReload(false);
    }

    private void EndCooldownShotgun()
    {
        cooldown[WeaponType.Shotgun] = false;
        if (manager != null) manager.ShowReload(false);
    }

    /*Powerups*/

    public void DoubleDamage()
    {
        doubleDamage = true;
        if(manager != null) manager.ShowDoubleDamage(true);
        Invoke(nameof(RemoveDoubleDamage), doubleDamageTime);
    }

    public void FreeAmmo()
    {
        freeAmmo = true;
        if (manager != null) manager.ShowFreeAmmo(true);
        Invoke(nameof(RemoveFreeAmmo), freeAmmoTime);
    }

    public void RemoveDoubleDamage()
    {
        doubleDamage = false;
        if (manager != null) manager.ShowDoubleDamage(false);
    }

    public void RemoveFreeAmmo()
    {
        freeAmmo = false;
        if (manager != null) manager.ShowFreeAmmo(false);
    }
}
