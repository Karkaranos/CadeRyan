/*****************************************************************************
// File Name :         SheriffBehavor.cs
// Author :            Cade R. Naylor
// Creation Date :     March 19, 2023
//
// Brief Description : Creates the Sheriff Behavior, links them to PlayerActions, 
                        handles attacks, movement, power up/weapon switching
*****************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SheriffBehavior : MonoBehaviour
{
    #region Variables

    //Create an instance of input
    InputActionAsset inputAsset;
    InputActionMap inputMap;
    PlayerInput pInput;

    //Create a reference for each inputAction
    InputAction playerMovement;
    InputAction scopeMovement;
    InputAction quickAttack;
    InputAction chargeAttack;
    InputAction switchWeapon;
    InputAction switchPowerUp;
    InputAction pauseMenu;

    //Temporary Variables
    Vector2 movement;
    Vector2 scopePos;

    //Variables for Attacks
    [SerializeField] private GameObject scope;
    private int scopeRange = 100;
    [SerializeField] private WeaponData weapon;
    private SpriteRenderer gunImage;
    [SerializeField] private GameObject gun;
    private bool chgAtkAvailable = true;
    private bool atkAvailable = true;
    public float scopeDistance;
    public float dmgShot;
    private int ammo;
    private int maxAmmo;

    //Other Variables
    [SerializeField] private GameObject sheriff;
    [SerializeField] private Sprite revolver;
    [SerializeField] private Sprite shotgun;
    [SerializeField] private Sprite pistol;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject atkPoint;
    private int playerhealth = 200;
    private bool weaponChanged = false;
    private int weaponNumber = 1;
    Coroutine stopMe;

    private UIManagerBehavior uim;

    public int Playerhealth { get => playerhealth; set => playerhealth = value; }

    public int Ammo { get => ammo; set => ammo = value; }

    public bool Weaponchanged { get => weaponChanged; set => weaponChanged = value; }
    public int MaxAmmo { get => maxAmmo; set => maxAmmo = value; }
    public int WeaponNumber { get => weaponNumber; set => weaponNumber = value; }
    #endregion

    #region Functions

    //Sets up control references
    #region Set Up
    /// <summary>
    /// Awake is called before start. Gets references to Player Controls.
    /// </summary>
    private void Awake()
    {
        inputAsset = this.GetComponent<PlayerInput>().actions;
        inputMap = inputAsset.FindActionMap("Player1Actions");
        playerMovement = inputMap.FindAction("Movement");
        scopeMovement = inputMap.FindAction("MoveScope");
        switchWeapon = inputMap.FindAction("SwitchWeapon");
        quickAttack = inputMap.FindAction("QuickAttack");
        chargeAttack = inputMap.FindAction("ImpactAttack");
        switchPowerUp = inputMap.FindAction("SwitchPowerup");
        pauseMenu = inputMap.FindAction("PauseMenu");

        Ammo = weapon.Ammo;
        maxAmmo = weapon.MaxAmmo;
        uim = GameObject.Find("UIManager").GetComponent<UIManagerBehavior>();
        pInput = GetComponent<PlayerInput>();
        pInput.camera = Camera.current;

        gunImage = gun.GetComponent<SpriteRenderer>();
        gunImage.sprite = revolver;

        //Movement - Left Stick
        //Reads in input from the Left Stick and saves it to a temporary variable
        playerMovement.performed += contx => movement = contx.ReadValue<Vector2>();
        //When the Left Stick is not being pressed, set the temp variable to 0
        playerMovement.canceled += contx => movement = Vector2.zero;


        //Scope Movement - Right Stick
        //Reads in input from the Right Stick and saves it to a temporary variable
        scopeMovement.performed += contx => scopePos = contx.ReadValue<Vector2>();
        //When the Right Stick is not being pressed, set the temp variable to 0
        scopeMovement.canceled += contx => scopePos = Vector2.zero;

        //Weapon Switching - Left Trigger
        switchWeapon.performed += contx => SwitchWeapon();

        //Quick Attack - A button
        quickAttack.started += contx => stopMe=StartCoroutine(QuickAtk());
        quickAttack.canceled += contx => StopShooting();

        //Charged Attack - B Button
        chargeAttack.performed += contx => stopMe=StartCoroutine(ChargeAtk());
        chargeAttack.canceled += contx => StopShooting();

        //Powerup Switching - Right Trigger
        switchPowerUp.performed += contx => SwitchPowerUp();

        //Pause Menu - Start Button
        pauseMenu.performed += contx => uim.PauseMenu();

    }

    private void OnEnable()
    {
        //Turn on Action Maps; Implicitly called
        inputMap.Enable();
    }

    private void OnDisable()
    {
        //Turn off action maps
        inputMap.Disable();
    }
    #endregion Set Up

    //Handles player attacks and switching weapons
    #region Attacks and Weapons
    /// <summary>
    /// Attacks using the player's Charged Attack, if available
    /// </summary>
    IEnumerator  ChargeAtk()
    {
        for(; ; ) 
        {
            if (weapon.Ammo == 0)
            {
                print("Out of Ammo");
            }
            else
            {
                if (chgAtkAvailable && weapon)
                {
                    GameObject temp;
                    //Attack, then start the cooldown timer
                    print(weapon.Weapon + " deals " + weapon.ChargeDmg + " damage. " + weapon.Ammo + " shots remaining.");
                    temp = Instantiate(bullet, transform.position, Quaternion.identity);
                    temp.GetComponent<SheriffBulletBehavior>().damageDealt =
                        weapon.ChargeDmg;
                    chgAtkAvailable = false;
                    StartCoroutine(ChargeWeaponCoolDown());
                    weapon.Ammo--;
                    Ammo = weapon.Ammo;
                }
                else
                {
                    print(weapon.Weapon + " is on cooldown.");
                }
            }
            yield return new WaitForSeconds(weapon.ChargeCD);
        }
    }

    /// <summary>
    /// The cooldown timer for a charged attack
    /// </summary>
    /// <returns>How long before charged attack can occur again</returns>
    IEnumerator ChargeWeaponCoolDown()
    {
        yield return new WaitForSeconds(weapon.ChargeCD);
        chgAtkAvailable = true;
    }


    /// <summary>
    /// Attacks using the player's standard attack, if available
    /// </summary>
    IEnumerator QuickAtk()
    {
        for(; ; )
        {
            if (weapon.Ammo == 0)
            {
                print("Out of Ammo");
            }
            else
            {
                if (atkAvailable && weapon)
                {
                    GameObject temp;
                    //Attack, then start the cooldown timer
                    print(weapon.Weapon + " deals " + weapon.Dmg + " damage. " + weapon.Ammo + " shots remaining.");
                    temp = Instantiate(bullet, transform.position, Quaternion.identity);
                    temp.GetComponent<SheriffBulletBehavior>().damageDealt =
                        weapon.Dmg;
                    atkAvailable = false;
                    StartCoroutine(WeaponCoolDown());
                    weapon.Ammo--;
                    Ammo = weapon.Ammo;
                }
                else
                {
                    print(weapon.Weapon + " is on cooldown.");
                }
            }
            yield return new WaitForSeconds(weapon.StandardCD);
        }
    }

    /// <summary>
    /// The cooldown timer for an attack
    /// </summary>
    /// <returns>How long before attack can occur again</returns>
    IEnumerator WeaponCoolDown()
    {
        yield return new WaitForSeconds(weapon.StandardCD);
        atkAvailable = true;
    }


    /// <summary>
    /// Stops the Coroutine currently making the player shoot
    /// </summary>
    private void StopShooting()
    {
        StopCoroutine(stopMe);
        print("stop");
    }

    /// <summary>
    /// Switches the WeaponData the player is currently using
    /// </summary>
    private void SwitchWeapon()
    {
        string fileName = "";
        if (weapon.Weapon == WeaponData.WeaponID.REVOLVER)
        {
            fileName = "SHOTGUN_DATA";
            print("Weapon switched to Shotgun");
            gunImage.sprite = shotgun;
            weaponNumber = 2;
        }
        else if (weapon.Weapon == WeaponData.WeaponID.SHOTGUN)
        {
            fileName = "PISTOL_DATA";
            print("Weapon switched to Pistol");
            gunImage.sprite = pistol;
            weaponNumber = 3;
        }
        else if (weapon.Weapon == WeaponData.WeaponID.PISTOL)
        {
            fileName = "REVOLVER_DATA";
            print("Weapon switched to Revolver");
            gunImage.sprite = revolver;
            weaponNumber = 1;
        }
        weapon = Resources.Load<WeaponData>(fileName);

        //Reset the attack cooldowns
        chgAtkAvailable = true;
        atkAvailable = true;
        Ammo = weapon.Ammo;
        maxAmmo = weapon.MaxAmmo;
        weaponChanged = true;
        StartCoroutine(WeaponChange());
    }

    /// <summary>
    /// Resets weaponChanged after a brief pause
    /// </summary>
    /// <returns>Time paused for</returns>
    IEnumerator WeaponChange()
    {
        yield return new WaitForSeconds(.1f);
        weaponChanged = false;
    }

    /// <summary>
    /// Switches the PowerUp Data the player has access to
    /// </summary>
    private void SwitchPowerUp()
    {
        //code here to get list of power ups and move to next index
    }
    

    #endregion

    //Handles player and scope movement
    #region Movement

    /// <summary>
    /// Handles player and scope movement
    /// </summary>
    private void FixedUpdate()
    {
        SheriffArt sherArt = GetComponent<SheriffArt>();
        //Create a reference to the player's position
        Vector2 playerPos = transform.position;
        Vector2 newScopePos;
        Vector2 movementVelocity = new Vector2(movement.x, movement.y) * 5 *
            Time.deltaTime;
        float fAngle;
        float scopeDistance;
        Quaternion playerRot = transform.rotation;

        //Translate is a movement function
        transform.Translate(movementVelocity, Space.Self);

        //Clamp the player's position to stay on screen
        //ClampPlayer(transform.position);

        //Set the scope's position to the new value while ensuring it revolves
        //around the player

        fAngle = Mathf.Atan(scopePos.y / scopePos.x);
        scopeDistance = Mathf.Sqrt((Mathf.Pow(scopePos.x, 2)) +
            (Mathf.Pow(scopePos.y, 2)));


        newScopePos.x = playerPos.x + (scopePos.x * scopeDistance * scopeRange *
            Time.deltaTime);
        newScopePos.y = playerPos.y + (scopePos.y * scopeDistance * scopeRange *
            Time.deltaTime);

        scope.transform.position = newScopePos;

        SheriffArt sherA = sheriff.GetComponent<SheriffArt>();

        //Sets the animation based on the direction the player is walking in
        sherA.SetDirection(movementVelocity, playerRot);

        scopeDistance = Mathf.Sqrt(Mathf.Pow(newScopePos.x - playerPos.x, 2) + Mathf.Pow(newScopePos.y - playerPos.y, 2));

    }



    /// <summary>
    /// Clamps the player's position to remain onscreen
    /// </summary>
    /// <param name="pos">The player's position</param>
    private void ClampPlayer(Vector2 pos)
    {
        Vector2 playerBind = pos;

        if (pos.x > 8.4f)
        {
            playerBind.x = 8.4f;
        }
        if (pos.x < -8.4f)
        {
            playerBind.x = -8.4f;
        }
        if (pos.y > 4.5f)
        {
            playerBind.y = 4.5f;
        }
        if (pos.y < -4.5f)
        {
            playerBind.y = -4.5f;
        }
        transform.position = playerBind;
    }

    #endregion

    //Handles collisions with Enemies
    #region Collisions

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "Large TumbleFiend(Clone)" || collision.gameObject.name == "Large TumbleFiend")
        {
            //take large tumble damage
            Playerhealth -= 5;
            print("Hit by Large Tumble");
        }
        if (collision.gameObject.name == "Small TumbleFiend(Clone)")
        {
            //take large tumble damage
            print("Hit by Small Tumble");
            Playerhealth -= 3;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "explosion")
        {
            //Take explosion Damage
            print("Hit by Explosion");
            Playerhealth -= 10;
        }
        if (collision.gameObject.tag == "Spike")
        {
            //Take turret damage
            print("Hit by Cactus Spike");
            Playerhealth -= 1;
        }
    }

    #endregion Collisions


    #endregion Functions
}