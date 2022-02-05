using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float BulletSpeed;
    public float BulletLife;
    public int Damage = 1;
    public Rigidbody RB;
    public GameObject ImpactEffect;
    public bool DamageEnemy;
    public bool DamagePlayer;



    void Update()
    {
        RB.velocity = transform.forward * BulletSpeed;

        BulletLife -= Time.deltaTime;
        if(BulletLife <= 0)
        {
            Destroy(gameObject);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Enemy" && DamageEnemy)
        {
            //Destroy(other.gameObject);
            //other.gameObject.GetComponent<EnemyHealthController>().DamageEnemy(Damage);
        }

        if(other.gameObject.tag == "Player" && DamagePlayer)
        {
            Debug.Log("hit Player");
        }
        
        Destroy(gameObject);
        //Instantiate(ImpactEffect, transform.position + (transform.forward * (-MoveSpeed * Time.deltaTime)), transform.rotation);
    }
}
