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
    private Rigidbody _rb;

    public bool IsPlayerNoticed
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

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();
        TryGetComponent<Rigidbody>(out _rb);
    }

    // Start is called before the first frame update
    void Start()
    {
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
        if (_rb != null)
        {
            //_rb.AddForce(Vector3.up * 20, ForceMode.Acceleration);
        }
        transform.position = transform.position + Vector3.down / 1.2f;
        IsDead = true;
    }
}