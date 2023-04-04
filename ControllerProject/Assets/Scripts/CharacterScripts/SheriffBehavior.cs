using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheriffBehavior : MonoBehaviour
{
    #region Variables

    //Create an instance of input
    PlayerActions controls;

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
    [SerializeField] private GameObject atkPoint;

    //Other Variables
    [SerializeField] private GameObject sheriff;
    [SerializeField] private Sprite revolver;
    [SerializeField] private Sprite shotgun;
    [SerializeField] private Sprite pistol;
    [SerializeField] private GameObject bullet;

    //Power Up and Bullet Storage
    private List<PowerUpData> currPowerUps;
    private List<SheriffBulletBehavior> currBullets;
    #endregion

    #region Functions

    //Sets up control references
    #region Set Up
    /// <summary>
    /// Awake is called before start. Gets references to Player Controls.
    /// </summary>
    private void Awake()
    {
        controls = new PlayerActions();

        gunImage = gun.GetComponent<SpriteRenderer>();
        gunImage.sprite = revolver;

        //Movement - Left Stick
        //Reads in input from the Left Stick and saves it to a temporary variable
        controls.Player1Actions.Movement.performed += contx => movement =
        contx.ReadValue<Vector2>();
        //When the Left Stick is not being pressed, set the temp variable to 0
        controls.Player1Actions.Movement.canceled += contx => movement =
        Vector2.zero;


        //Scope Movement - Right Stick
        //Reads in input from the Right Stick and saves it to a temporary variable
        controls.Player1Actions.MoveScope.performed += contx => scopePos =
        contx.ReadValue<Vector2>();
        //When the Right Stick is not being pressed, set the temp variable to 0
        controls.Player1Actions.MoveScope.canceled += contx => scopePos =
        Vector2.zero;

        //Weapon Switching - Left Trigger
        controls.Player1Actions.SwitchWeapon.performed += contx => SwitchWeapon();

        //Quick Attack - A button
        controls.Player1Actions.QuickAttack.performed += contx => quickAtk();

        //Charged Attack - B Button
        controls.Player1Actions.ImpactAttack.performed += contx => chargeAtk();
    }

    private void OnEnable()
    {
        //Turn on Action Maps; Implicitly called
        controls.Player1Actions.Enable();
    }

    private void OnDisable()
    {
        //Turn off action maps
        controls.Player1Actions.Disable();
    }
    #endregion Set Up

    //Handles player attacks and switching weapons
    #region Attacks and Weapons
    /// <summary>
    /// Attacks using the player's Charged Attack, if available
    /// </summary>
    private void chargeAtk()
    {
        if (weapon.Ammo == 0)
        {
            print("Out of Ammo");
        }
        else
        {
            if (chgAtkAvailable && weapon)
            {
                //Attack, then start the cooldown timer
                print(weapon.Weapon + " deals " + weapon.ChargeDmg + " damage. " + weapon.Ammo + " shots remaining.");
                currBullets.Add(Instantiate(bullet, atkPoint.transform.position, Quaternion.identity).GetComponent<SheriffBulletBehavior>());
                chgAtkAvailable = false;
                StartCoroutine(ChargeWeaponCoolDown());
                weapon.Ammo--;
            }
            else
            {
                print(weapon.Weapon + " is on cooldown.");
            }
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
    private void quickAtk()
    {
        if (weapon.Ammo == 0)
        {
            print("Out of Ammo");
        }
        else
        {
            if (atkAvailable && weapon)
            {
                //Attack, then start the cooldown timer
                print(weapon.Weapon + " deals " + weapon.Dmg + " damage. " + weapon.Ammo + " shots remaining.");
                currBullets.Add(Instantiate(bullet, atkPoint.transform.position, Quaternion.identity).GetComponent<SheriffBulletBehavior>());
                atkAvailable = false;
                StartCoroutine(WeaponCoolDown());
                weapon.Ammo--;
            }
            else
            {
                print(weapon.Weapon + " is on cooldown.");
            }
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
        }
        else if (weapon.Weapon == WeaponData.WeaponID.SHOTGUN)
        {
            fileName = "PISTOL_DATA";
            print("Weapon switched to Pistol");
            gunImage.sprite = pistol;
        }
        else if (weapon.Weapon == WeaponData.WeaponID.PISTOL)
        {
            fileName = "REVOLVER_DATA";
            print("Weapon switched to Revolver");
            gunImage.sprite = revolver;
        }
        weapon = Resources.Load<WeaponData>(fileName);

        //Reset the attack cooldowns
        chgAtkAvailable = true;
        atkAvailable = true;


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
        ClampPlayer(transform.position);

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
    #endregion Functions
}
