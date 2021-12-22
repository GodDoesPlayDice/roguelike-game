using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    public float maxHealth = 100f;
    public bool isDead = false;

    [SerializeField]
    private float health = -1f;
    

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(float takenDamage)
    {
        // if invincible
        if (health == -1f) return;

        float result = health - Mathf.Abs(takenDamage);

        if (result <= 0)
        {
            isDead = true;
        }
        else
        {
            health = result;
        }
    }

    public void Heal(float healAmount)
    {
        // if invincible
        if (health == -1f || isDead) return;

        float result = health + Mathf.Abs(healAmount);
        if (result > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health = result;
        }

    }
}
