using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    Rigidbody2D rb2d;
    float arrowSpeed = 10;
    public float damage;

    void Start()
    {
        damage = EnemyBase.attackDamage;
        rb2d = GetComponent<Rigidbody2D>();

        if (transform.eulerAngles.z > -20 && transform.eulerAngles.z < 20) {
            Vector2 arrowVelocity = new Vector2(arrowSpeed, 0);
            rb2d.velocity = arrowVelocity;
        }
        else if (transform.eulerAngles.z > 160 && transform.eulerAngles.z < 200){
            Vector2 arrowVelocity = new Vector2(-arrowSpeed, 0);
            rb2d.velocity = arrowVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {

        }
        else
        {
            Destroy(gameObject);
        }
    }

}
