using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using Target;

public class EnemyController : MonoBehaviour
{
    private GameObject player;
    private NavMeshAgent navMeshAgent;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();
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
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
        transform.position = transform.position + Vector3.down / 2;
    }
}