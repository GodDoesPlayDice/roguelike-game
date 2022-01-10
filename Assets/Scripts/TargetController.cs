using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetController : MonoBehaviour
{
    public float maxHealth = 100f;
    public float startHealth = 100f;
    public bool isDead { get; private set; }
    public float health { get; private set; }


    // events part
    [System.Serializable]
    public class TargetEventsFields
    {
        public UnityEvent<GameObject> onDeathEvent;
        public UnityEvent<GameObject, float> onHealthChangeEvent;
    }

    public TargetEventsFields events;

    private void OnValidate()
    {
        if (startHealth < -1)
        {
            startHealth = -1;
        }
    }

    void Start()
    {
        health = startHealth;
        isDead = false; // born alive babyy!
        if (events.onDeathEvent == null) events.onDeathEvent = new UnityEvent<GameObject>();
        if (events.onHealthChangeEvent == null) events.onHealthChangeEvent = new UnityEvent<GameObject, float>();
        events.onHealthChangeEvent.Invoke(gameObject, health);
    }

    public void TakeDamage(float takenDamage)
    {
        // if invincible
        if (Math.Abs(health - (-1f)) < 0.001f) return;

        float result = health - Mathf.Abs(takenDamage);

        if (result <= 0)
        {
            // death event
            if (!isDead)
            {
                health = 0;
                // health change  event
                events.onHealthChangeEvent.Invoke(gameObject, health);
                events.onDeathEvent.Invoke(gameObject);
                isDead = true;
            }
        }
        else
        {
            health = result;
            // health change  event
            events.onHealthChangeEvent.Invoke(gameObject, result);
        }
    }

    public void Heal(float healAmount)
    {
        // if invincible
        if (Math.Abs(health - (-1f)) < 0.001f || isDead) return;

        float result = health + Mathf.Abs(healAmount);
        if (result > maxHealth)
        {
            result = maxHealth;
        }

        health = result;
        // health change event
        // health change  event
        events.onHealthChangeEvent.Invoke(gameObject, result);
    }
}