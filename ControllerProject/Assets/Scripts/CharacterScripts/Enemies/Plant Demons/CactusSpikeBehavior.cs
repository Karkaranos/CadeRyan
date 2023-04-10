/*****************************************************************************
// File Name :         CactusSpikeBehavior.cs
// Author :            Cade R. Naylor
// Creation Date :     April 8, 2023
//
// Brief Description : Sets target angle and Adds force to Cactus Spike Attacks
*****************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactusSpikeBehavior : MonoBehaviour
{
    #region Variables
    Vector2 moveForce;
    private float speed = .01f;
    #endregion

    #region Functions

    //Handles getting a target and adding force
    #region Attacks
    /// <summary>
    /// Gets the position of the target and launches towards it
    /// </summary>
    /// <param name="target">The current player being targeted</param>
    public void GetTarget(GameObject target)
    {
        //Look at the target, then send the spike towards the target

        //Code from robertbu on Stack Overflow- it gets the direction of the target
        //Then converts it to an angle and sets it
        Vector3 dir = target.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


        moveForce.x = dir.x;
        moveForce.y = dir.y;
        moveForce *= speed;
        GetComponent<Rigidbody2D>().AddForce(moveForce);
    }

    #endregion Attacks

#endregion Functions
}