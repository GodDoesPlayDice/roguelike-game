using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Target
{
    public class TargetController : MonoBehaviour
    {
        public float maxHealth = 100f;
        public bool isDead = false;

        [SerializeField]
        private float health = -1f;


        // events part
        [System.Serializable]
        public class TargetEventsFields
        {
            public UnityEvent<OnDeathEventArgs> onDeathEvent;
            public UnityEvent<OnHealthChangeEventArgs> onHealthChangeEvent;
        }
        public class OnDeathEventArgs
        {
            public GameObject gameObject;
        }
        public class OnHealthChangeEventArgs
        {
            public GameObject gameObject;
            public float currentHealth;
        }

        public TargetEventsFields events;


        // Start is called before the first frame update
        void Start()
        {
            health = maxHealth;

            if (events.onDeathEvent == null) events.onDeathEvent = new UnityEvent<OnDeathEventArgs>();
            if (events.onHealthChangeEvent == null) events.onHealthChangeEvent = new UnityEvent<OnHealthChangeEventArgs>();
        }

        public void TakeDamage(float takenDamage)
        {
            // if invincible
            if (health == -1f) return;

            float result = health - Mathf.Abs(takenDamage);

            if (result <= 0)
            {
                // death event
                if (!isDead)
                {
                    events.onDeathEvent.Invoke(new OnDeathEventArgs { gameObject = gameObject });
                    isDead = true;
                }
            }
            else
            {
                health = result;
                // health change  event
                events.onHealthChangeEvent.Invoke(new OnHealthChangeEventArgs
                {
                    gameObject = gameObject,
                    currentHealth = result
                });
            }
        }

        public void Heal(float healAmount)
        {
            // if invincible
            if (health == -1f || isDead) return;

            float result = health + Mathf.Abs(healAmount);
            if (result > maxHealth)
            {
                result = maxHealth;
            }

            health = result;
            // health change event
            events.onHealthChangeEvent.Invoke(new OnHealthChangeEventArgs
            {
                gameObject = gameObject,
                currentHealth = result
            });
        }
    }
}

