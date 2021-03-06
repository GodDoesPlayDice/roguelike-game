using System;
using Actors.Enemies;
using Combat;
using Combat.Weapon;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Enemies
{
    public class MeleeEnemy : MonoBehaviour
    {
        // some common vars
        public enum MeleeEnemyState
        {
            Idle,
            Attacking,
            AfterAttackAttemp,
            Wandering,
            ChasingVictim,

        };
        public MeleeEnemyState state;
        public MeleeEnemyState defaultState;

        [HideInInspector]
        public GameObject victim;
        private EnemyController _enemyController;

        // сombat variables
        [System.Serializable]
        public class EnemyCombatFields
        {
            public float attackRadius = 1f;
            public float attackDamage = 20f;
            public float attackRate;

        }
        public EnemyCombatFields combat;

        private float _lastAttackTime = 0;
        private MeleeWeapon _meleeWeapon;


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
    
        private Vector3 _wanderToPoint;
        private float _lastTimeWanderPointChanged;
        private float _lastWanderingDurationRandIncrease;

        private void Awake()
        {
            TryGetComponent<MeleeWeapon>(out _meleeWeapon);
            TryGetComponent<EnemyController>(out _enemyController);
            // if player is only possible victim
            victim = GameObject.FindGameObjectWithTag("Player");
        }

        private void Start()
        {
            // initial values
            _lastAttackTime = Time.time;
            _lastTimeWanderPointChanged = Time.time;
            if (events.OnDestinationChangeEvent == null) events.OnDestinationChangeEvent = new UnityEvent<Vector3>();
            state = defaultState;
        }

        // Update is called once per frame
        private void Update()
        {
            // check dist to player
            // check if attack is possible
            var victimPosition = victim.transform.position;
            var distToVictim = Vector3.Distance(transform.position, victimPosition);

            switch (state)
            {
                case MeleeEnemyState.Wandering:
                    // condition to start chasing
                    if (_enemyController.isPlayerNoticed)
                    {
                        state = MeleeEnemyState.ChasingVictim;
                    }
                    else
                    {
                        // speed
                        if (Math.Abs(_enemyController.navMeshAgent.speed - _enemyController.normalMovementSpeed) < 0.01f)
                        {
                            _enemyController.navMeshAgent.speed = _enemyController.normalMovementSpeed * wandering.speedMultiplier;
                        }

                        // calculate new wander point
                        if (Time.time - _lastTimeWanderPointChanged >= wandering.basicDuration + _lastWanderingDurationRandIncrease)
                        {
                            float multiplier = Random.Range(2, 5);

                            Vector3 randomDirection = Random.insideUnitCircle.normalized * Mathf.Clamp(multiplier, 1, multiplier < 1 ? 1 : multiplier);
                            _wanderToPoint = transform.position + new Vector3(randomDirection.x, transform.position.y, randomDirection.z);
                            events.OnDestinationChangeEvent?.Invoke(_wanderToPoint);

                            _lastTimeWanderPointChanged = Time.time;
                            _lastWanderingDurationRandIncrease = Random.Range(0, wandering.durationMaxRandIncrease);
                        }
                    }
                    break;
                case MeleeEnemyState.ChasingVictim:
                    // condition to start wandering
                    if (!_enemyController.isPlayerNoticed)
                    {
                        state = MeleeEnemyState.Wandering;
                    }
                    else
                    {
                        // speed
                        if (_enemyController.navMeshAgent.speed != _enemyController.normalMovementSpeed)
                        {
                            _enemyController.navMeshAgent.speed = _enemyController.normalMovementSpeed;
                        }
                        // change destination
                        events.OnDestinationChangeEvent?.Invoke(victimPosition);

                        // condition to start attacking
                        if (distToVictim <= combat.attackRadius)
                        {
                            state = MeleeEnemyState.Attacking;
                        }
                    }
                    break;
                case MeleeEnemyState.Attacking:
                    // condition to start wandering
                    if (distToVictim > combat.attackRadius)
                    {
                        state = MeleeEnemyState.Wandering;
                    } else
                    {
                        // change destination to self
                        events.OnDestinationChangeEvent?.Invoke(transform.position);
                        // attack rate and attack part
                        if (Time.time - _lastAttackTime >= combat.attackRate)
                        {
                            if (_meleeWeapon != null && _meleeWeapon.enabled)
                            {
                                _meleeWeapon.Attack(victim);
                                _lastAttackTime = Time.time;
                            }
                        }
                    }
                    break;
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_enemyController || _enemyController.isDead) return;

            // attack radius
            if (state == MeleeEnemyState.Attacking)
            {
                Gizmos.color = Color.red / 3;
                Gizmos.DrawSphere(transform.position, combat.attackRadius);
            }

            // chasing player
            if (_enemyController.isPlayerNoticed)
            {

                if (victim?.transform?.position != null)
                {
                    Gizmos.DrawLine(transform.position, victim.transform.position);
                }
            }

            // wandering point
            if (state == MeleeEnemyState.Wandering)
            {
                Gizmos.DrawLine(transform.position, _wanderToPoint);
            }
        }
#endif
    }
}
