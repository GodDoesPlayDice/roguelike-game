using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Actors
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class ActorController : MonoBehaviour
    {
        public UnityEvent onDestinationReachedEvent;
        public bool navMeshActive { get; private set; } = false;

        // nav mesh part
        public float destinationThreshold = .1f;
        private NavMeshAgent _navMeshAgent;


        private void Awake()
        {
            TryGetComponent(out _navMeshAgent);
        }

        private void Start()
        {
            onDestinationReachedEvent ??= new UnityEvent();
        }

        public void SetDestination(Vector3 destination, float threshold)
        {
            destinationThreshold = threshold;
            navMeshActive = true;
            _navMeshAgent.enabled = true;
            _navMeshAgent.destination = destination;
            StartCoroutine(CheckDestinationReachRoutine());
        }

        public void UnsetDestination()
        {
            destinationThreshold = 0f;
            _navMeshAgent.destination = Vector3.zero;
            _navMeshAgent.enabled = false;
            navMeshActive = false;
            StopCoroutine(CheckDestinationReachRoutine());
        }

        private bool DidReachLastDestination()
        {
            var destination = new Vector2(_navMeshAgent.destination.x, _navMeshAgent.destination.z);
            var playerPos = new Vector2(transform.position.x, transform.position.z);
            var dist = Vector2.Distance(destination, playerPos);
            return dist <= destinationThreshold;
        }

        private IEnumerator CheckDestinationReachRoutine()
        {
            for (;;)
            {
                if (DidReachLastDestination())
                {
                    onDestinationReachedEvent.Invoke();
                    UnsetDestination();
                    onDestinationReachedEvent.RemoveAllListeners();
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}