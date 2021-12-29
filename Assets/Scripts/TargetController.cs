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
        public float startHealth = 100f;
        public bool IsDead { get; private set; }
        public float Health
        {
            get; private set;
        }


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

        private void OnValidate()
        {
            if (startHealth < -1)
            {
                startHealth = -1;
            }
        }

        void Start()
        {
            Health = startHealth;
            IsDead = false; // born alive babyy!
            if (events.onDeathEvent == null) events.onDeathEvent = new UnityEvent<OnDeathEventArgs>();
            if (events.onHealthChangeEvent == null) events.onHealthChangeEvent = new UnityEvent<OnHealthChangeEventArgs>();
        }

        public void TakeDamage(float takenDamage)
        {
            // if invincible
            if (Health == -1f) return;

            float result = Health - Mathf.Abs(takenDamage);

            if (result <= 0)
            {
                // death event
                if (!IsDead)
                {
                    events.onDeathEvent.Invoke(new OnDeathEventArgs { gameObject = gameObject });
                    IsDead = true;
                }
            }
            else
            {
                Health = result;
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
            if (Health == -1f || IsDead) return;

            float result = Health + Mathf.Abs(healAmount);
            if (result > maxHealth)
            {
                result = maxHealth;
            }

            Health = result;
            // health change event
            events.onHealthChangeEvent.Invoke(new OnHealthChangeEventArgs
            {
                gameObject = gameObject,
                currentHealth = result
            });
        }
    }
}

