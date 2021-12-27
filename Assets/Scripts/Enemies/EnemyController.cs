using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using Target;

public class EnemyController : MonoBehaviour
{
    public float NormalMovementSpeed { get; private set; }
    public bool IsDead { get; private set; } = false;

    private GameObject player;
    [HideInInspector]
    public NavMeshAgent navMeshAgent;

    public float distToNoticePlayer = 3f;
    public float distToUnnoticePlayer = 5f;

    private bool lastIsPlayerNoticedState;
    public bool isPlayerNoticed
    {
        get
        {
            float dist = Vector3.Distance(player.transform.position, transform.position);
            bool result;

            if (dist <= distToNoticePlayer ||
                lastIsPlayerNoticedState == true && dist <= distToUnnoticePlayer)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            lastIsPlayerNoticedState = result;
            return result;
        }
        set { }
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (navMeshAgent != null)
        {
            NormalMovementSpeed = navMeshAgent.speed;
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
        transform.position = transform.position + Vector3.down / 2;
        IsDead = true;
    }
}