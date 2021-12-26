using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeEnemy : MonoBehaviour
{
    public enum MeleeEnemyState
    {
        idle,
        attacking,
        wandering,
        chasingVictim,
    };
    [HideInInspector]
    public MeleeEnemyState state;
    public MeleeEnemyState defaultState;
    [HideInInspector]
    public GameObject victim;

    // сombat variables
    [System.Serializable]
    public class EnemyCombatFields
    {
        public float attackRadius = 1f;
        public float attackDamage = 20f;
        public float attackRate;
        
    }
    public EnemyCombatFields combat;

    private float lastAttackTime = 0;
    private MeleeAttacker meleeAttacker;


    // events part
    [System.Serializable]
    public class EnemyEventsFields
    {
        public UnityEvent<Vector3> OnDestinationChangeEvent;
    }
    public EnemyEventsFields events;
    


    void Start()
    {
        TryGetComponent<MeleeAttacker>(out meleeAttacker);
        lastAttackTime = Time.time;

        if (events.OnDestinationChangeEvent == null) events.OnDestinationChangeEvent = new UnityEvent<Vector3>();

        state = defaultState;
        // TEMP
        victim = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case MeleeEnemyState.chasingVictim:
                events.OnDestinationChangeEvent?.Invoke(victim.transform.position);
                break;
            case MeleeEnemyState.attacking:
                if (Time.time - lastAttackTime >= combat.attackRate)
                {
                    meleeAttacker?.Attack(victim, combat.attackDamage);
                    lastAttackTime = Time.time;
                }
                break;
        }
    }
}
