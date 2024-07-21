using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int health = 100;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            Death();
        }
    }

    public void TakeDamage(int damage)
    {
        health-=damage;
        //print("Enemy health is " + health);
    }

	private void Death()
	{
        print("Enemy dead");
        health = 0;
	}
}
