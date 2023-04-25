using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootTableAndDropBehavior : MonoBehaviour
{
    [SerializeField] GameObject cell;
    [SerializeField] GameObject health;
    [SerializeField] GameObject ammo;

    private int randomNum;
    private int totalChances=1;
    private int totalPool=20;

    public void DropLoot(Vector2 pos)
    {
        Instantiate(cell, pos, Quaternion.identity);
        for(int i=0; i<totalChances; i++)
        {
            randomNum = Random.Range(1, totalPool);

            //Spawn an extra cell with a 1 in 10 chance
            if(randomNum >= 1 && randomNum <= 2)
            {
                pos.x += Random.Range(-1, 1);
                pos.y += Random.Range(-1, 1);
                Instantiate(cell, pos, Quaternion.identity);
            }

            //Spawn health with a 1 in 10 chance
            if(randomNum >=5 && randomNum <= 6)
            {
                pos.x += Random.Range(-1, 1);
                pos.y += Random.Range(-1, 1);
                Instantiate(health, pos, Quaternion.identity);
            }

            //Spawn health with a 1 in 10 chance
            if(randomNum>=12 && randomNum <= 13)
            {
                pos.x += Random.Range(-1, 1);
                pos.y += Random.Range(-1, 1);
                Instantiate(ammo, pos, Quaternion.identity);
            }
        }
    }
}