using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeEnemy : MonoBehaviour
{
    // some common vars
    public enum MeleeEnemyState
    {
        idle,
        attacking,
        afterAttackAttemp,
        wandering,
        chasingVictim,

    };
    public MeleeEnemyState state;
    public MeleeEnemyState defaultState;

    [HideInInspector]
    public GameObject victim;
    private EnemyController enemyController;

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

    // wandering wariables
    [Range(0f, 3f)]
    public float wanderingSpeedMultiplier = .5f;
    public float wanderToPointDuration = 2f;
    private Vector3 wanderToPoint;

    void Start()
    {
        // getting components
        TryGetComponent<MeleeAttacker>(out meleeAttacker);
        TryGetComponent<EnemyController>(out enemyController);

        // initial values
        lastAttackTime = Time.time;
        if (events.OnDestinationChangeEvent == null) events.OnDestinationChangeEvent = new UnityEvent<Vector3>();
        state = defaultState;

        // if player is only possible victim
        victim = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // check dist to player
        // check if attack is possible
        Vector3 victimPosition = victim.transform.position;
        float distToVictim = Vector3.Distance(transform.position, victimPosition);

        switch (state)
        {
            case MeleeEnemyState.wandering:
                // condition to start chasing
                if (enemyController.isPlayerNoticed)
                {
                    state = MeleeEnemyState.chasingVictim;
                }
                else
                {
                    // speed
                    if (enemyController.navMeshAgent.speed == enemyController.NormalMovementSpeed)
                    {
                        enemyController.navMeshAgent.speed = enemyController.NormalMovementSpeed * wanderingSpeedMultiplier;
                    }
                    // set random near destination
                    events.OnDestinationChangeEvent?.Invoke(transform.position);
                }
                break;
            case MeleeEnemyState.chasingVictim:
                // condition to continue chasing
                if (!enemyController.isPlayerNoticed)
                {
                    state = MeleeEnemyState.wandering;
                }
                else
                {
                    // speed
                    if (enemyController.navMeshAgent.speed != enemyController.NormalMovementSpeed)
                    {
                        enemyController.navMeshAgent.speed = enemyController.NormalMovementSpeed;
                    }
                    // change destination
                    events.OnDestinationChangeEvent?.Invoke(victimPosition);

                    // change to attacking condition
                    if (distToVictim <= combat.attackRadius)
                    {
                        state = MeleeEnemyState.attacking;
                    }
                }
                break;
            case MeleeEnemyState.attacking:
                // condition to start wandering
                if (distToVictim > combat.attackRadius)
                {
                    state = MeleeEnemyState.wandering;
                } else
                {
                    // change destination to self
                    events.OnDestinationChangeEvent?.Invoke(transform.position);
                    // attack rate and attack part
                    if (Time.time - lastAttackTime >= combat.attackRate)
                    {
                        if (meleeAttacker != null && meleeAttacker.enabled)
                        {
                            meleeAttacker.Attack(victim, combat.attackDamage);
                            lastAttackTime = Time.time;
                        }
                    }
                }
                break;
            case MeleeEnemyState afterAttackAttemp:
                break;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enemyController || enemyController.IsDead) return;

        if (state == MeleeEnemyState.attacking)
        {
            Gizmos.color = Color.red / 3;
            Gizmos.DrawSphere(transform.position, combat.attackRadius);
        }

        if (enemyController.isPlayerNoticed)
        {

            if (victim?.transform?.position != null)
            {
                Gizmos.DrawLine(transform.position, victim.transform.position);
            }
        }
    }
#endif
}
