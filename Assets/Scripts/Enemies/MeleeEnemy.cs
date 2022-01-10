using Combat;
using UnityEngine;
using UnityEngine.Events;

namespace Enemies
{
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
        // сombat variables
        [System.Serializable]
        public class EnemyWanderingFields
        {
            [Range(0f, 3f)]
            public float speedMultiplier = .5f;
            public float basicDuration = 2f;
            public float durationMaxRandIncrease = 1f;
        }
        public EnemyWanderingFields wandering;
    
        private Vector3 wanderToPoint;
        private float lastTimeWanderPointChanged;
        private float lastWanderingDurationRandIncrease;

        private void Awake()
        {
            TryGetComponent<MeleeAttacker>(out meleeAttacker);
            TryGetComponent<EnemyController>(out enemyController);
            // if player is only possible victim
            victim = GameObject.FindGameObjectWithTag("Player");
        }

        void Start()
        {
            // initial values
            lastAttackTime = Time.time;
            lastTimeWanderPointChanged = Time.time;
            if (events.OnDestinationChangeEvent == null) events.OnDestinationChangeEvent = new UnityEvent<Vector3>();
            state = defaultState;
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
                        if (enemyController.navMeshAgent.speed == enemyController.normalMovementSpeed)
                        {
                            enemyController.navMeshAgent.speed = enemyController.normalMovementSpeed * wandering.speedMultiplier;
                        }

                        // calculate new wander point
                        if (Time.time - lastTimeWanderPointChanged >= wandering.basicDuration + lastWanderingDurationRandIncrease)
                        {
                            float multiplier = Random.Range(2, 5);

                            Vector3 randomDirection = Random.insideUnitCircle.normalized * Mathf.Clamp(multiplier, 1, multiplier < 1 ? 1 : multiplier);
                            wanderToPoint = transform.position + new Vector3(randomDirection.x, transform.position.y, randomDirection.z);
                            events.OnDestinationChangeEvent?.Invoke(wanderToPoint);

                            lastTimeWanderPointChanged = Time.time;
                            lastWanderingDurationRandIncrease = Random.Range(0, wandering.durationMaxRandIncrease);
                        }
                    }
                    break;
                case MeleeEnemyState.chasingVictim:
                    // condition to start wandering
                    if (!enemyController.isPlayerNoticed)
                    {
                        state = MeleeEnemyState.wandering;
                    }
                    else
                    {
                        // speed
                        if (enemyController.navMeshAgent.speed != enemyController.normalMovementSpeed)
                        {
                            enemyController.navMeshAgent.speed = enemyController.normalMovementSpeed;
                        }
                        // change destination
                        events.OnDestinationChangeEvent?.Invoke(victimPosition);

                        // condition to start attacking
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
            if (!enemyController || enemyController.isDead) return;

            // attack radius
            if (state == MeleeEnemyState.attacking)
            {
                Gizmos.color = Color.red / 3;
                Gizmos.DrawSphere(transform.position, combat.attackRadius);
            }

            // chasing player
            if (enemyController.isPlayerNoticed)
            {

                if (victim?.transform?.position != null)
                {
                    Gizmos.DrawLine(transform.position, victim.transform.position);
                }
            }

            // wandering point
            if (state == MeleeEnemyState.wandering)
            {
                Gizmos.DrawLine(transform.position, wanderToPoint);
            }
        }
#endif
    }
}
