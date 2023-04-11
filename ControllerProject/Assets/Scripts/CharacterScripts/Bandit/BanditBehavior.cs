/*****************************************************************************
// File Name :         BanditBehavior.cs
// Author :            Cade R. Naylor
// Creation Date :     April 8, 2023
//
// Brief Description : Creates the Bandit's Behavior, links them to PlayerActions, 
                        handles attacks, movement, power up/weapon switching
*****************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BanditBehavior : MonoBehaviour
{
    #region Variables

    //Create an instance of input
    InputActionAsset inputAsset;
    InputActionMap inputMap;

    //Create a reference for each inputAction
    InputAction playerMovement;
    InputAction scopeMovement;
    InputAction quickAttack;
    InputAction chargeAttack;
    InputAction switchWeapon;
    InputAction switchPowerUp;

    //Temporary Variables
    Vector2 movement;
    Vector2 scopePos;

    //Variables for Attacks
    [SerializeField] private GameObject scope;
    private int scopeRange = 100;
    [SerializeField] private WeaponData weapon;
    private SpriteRenderer explodeImage;
    [SerializeField] private GameObject arm;
    private bool chgAtkAvailable = true;
    private bool atkAvailable = true;
    public float scopeDistance;

    //Other Variables
    [SerializeField] private GameObject bandit;
    [SerializeField] private Sprite dynamite;
    [SerializeField] private Sprite cocktails;
    [SerializeField] private Sprite firecrackers;
    [SerializeField] private GameObject playerExplosion;
    [SerializeField] private GameObject atkPoint;
    [SerializeField] private int playerhealth = 100;
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
        inputMap = inputAsset.FindActionMap("Player1Actions1");
        playerMovement = inputMap.FindAction("Movement");
        scopeMovement = inputMap.FindAction("MoveScope");
        switchWeapon = inputMap.FindAction("SwitchWeapon");
        quickAttack = inputMap.FindAction("QuickAttack");
        chargeAttack = inputMap.FindAction("ImpactAttack");
        switchPowerUp = inputMap.FindAction("SwitchPowerup");


        explodeImage = arm.GetComponent<SpriteRenderer>();
        explodeImage.sprite = dynamite;

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
        quickAttack.performed += contx => quickAtk();

        //Charged Attack - B Button
        chargeAttack.performed += contx => chargeAtk();

        //Powerup Switching - Right Trigger
        switchPowerUp.performed += contx => SwitchPowerUp();
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
        if (weapon.Weapon == WeaponData.WeaponID.DYNAMITE)
        {
            fileName = "COCKTAILS_DATA";
            print("Weapon switched to Molotov Cocktails");
            explodeImage.sprite = cocktails;
        }
        else if (weapon.Weapon == WeaponData.WeaponID.COCKTAILS)
        {
            fileName = "FIRECRACKERS_DATA";
            print("Weapon switched to Firecrackers");
            explodeImage.sprite = firecrackers;
        }
        else if (weapon.Weapon == WeaponData.WeaponID.FIRECRACKERS)
        {
            fileName = "DYNAMITE_DATA";
            print("Weapon switched to Dynamite");
            explodeImage.sprite = dynamite;
        }
        weapon = Resources.Load<WeaponData>(fileName);

        //Reset the attack cooldowns
        chgAtkAvailable = true;
        atkAvailable = true;


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
        BanditArt bandArt = GetComponent<BanditArt>();
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

        BanditArt bandA = bandit.GetComponent<BanditArt>();

        //Sets the animation based on the direction the player is walking in
        bandA.SetDirection(movementVelocity, playerRot);

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
        if (collision.gameObject.name == "Large TumbleFiend(clone)")
        {
            //take large tumble damage!
        }
        if (collision.gameObject.name == "Small TumbleFiend(clone)")
        {
            //take large tumble damage!
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "explosion")
        {
            //Take explosion Damage
        }
    }

    #endregion Collisions



    #endregion
}
