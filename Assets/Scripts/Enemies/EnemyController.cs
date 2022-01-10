using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class EnemyController : MonoBehaviour
    {
        public float normalMovementSpeed { get; private set; }
        public bool isDead { get; private set; } = false;

        private GameObject _player;
        [HideInInspector]
        public NavMeshAgent navMeshAgent;

        public float distToNoticePlayer = 3f;
        public float distToUnnoticePlayer = 5f;

        private bool _lastIsPlayerNoticedState;
        private Rigidbody _rb;

        public bool isPlayerNoticed
        {
            get
            {
                float dist = Vector3.Distance(_player.transform.position, transform.position);
                bool result;

                if (dist <= distToNoticePlayer ||
                    _lastIsPlayerNoticedState == true && dist <= distToUnnoticePlayer)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                _lastIsPlayerNoticedState = result;
                return result;
            }
            set { }
        }

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            navMeshAgent = GetComponent<NavMeshAgent>();
            TryGetComponent<Rigidbody>(out _rb);
        }

        // Start is called before the first frame update
        void Start()
        {
            if (navMeshAgent != null)
            {
                normalMovementSpeed = navMeshAgent.speed;
            }
        }

        public void OnDestinationChange(Vector3 newDestination)
        {
            if (navMeshAgent.enabled) navMeshAgent.destination = newDestination;
        }

        public void OnHealthChanged(TargetController.OnHealthChangeEventArgs onHealthChangeEventArgs)
        {

        }

        public void OnDeath(TargetController.OnDeathEventArgs onDeathEventArgs)
        {
            if (_rb != null)
            {
                //_rb.AddForce(Vector3.up * 20, ForceMode.Acceleration);
            }
            transform.position += Vector3.down / 1.2f;
            isDead = true;
        }
    }
}