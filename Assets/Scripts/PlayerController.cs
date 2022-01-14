using System.Collections;
using System.Collections.Generic;
using Combat;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // some basic vars 
    public bool disableControls = false;
    private Rigidbody _rb;
    private GameObject _cam;

    // interaction variables
    public float interactionRadius = 0.5f;
    private GameObject _nearestInteractable;

    // movement variables
    [System.Serializable]
    public class PlayerMovement
    {
        public float movementSpeed = 1f;
        [Range(0.01f, 5)] public float accelerationTime = 1f;
        public float turnSmoothTime = 0.1f;
    }

    public PlayerMovement movement;


    // private variables for movement
    private Vector2 _movementInput;

    // nav mesh part
    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public float destinationThreshold = 0f;


    // acceleration
    private float _currentMovementDuration = 0;

    // privates for rotation
    private float _turnSmoothVelocity;
    private float _targetAngleY;
    
    
    private ActorCombatController _combatController;
    private bool _isCloseToEnemies = false;


    // events part
    [System.Serializable]
    public class PlayerEventsFields
    {
        public UnityEvent OnDestinationReachedEvent;
    }

    public PlayerEventsFields events;

    // input system part
    public void OnMovement(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            _movementInput = value.ReadValue<Vector2>();
        }
        else if (value.canceled)
        {
            _movementInput = Vector2.zero;
        }
    }

    public void OnAttack(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            _combatController.MeleeAttack();
        }
    }

    public void OnInteract(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            Interact();
        }
    }

    private void Awake()
    {
        _cam = GameObject.FindGameObjectWithTag("MainCamera");
        TryGetComponent(out _rb);
        TryGetComponent(out navMeshAgent);
    }

    private void Start()
    {
        _movementInput = new Vector2();
        if (navMeshAgent != null) navMeshAgent.enabled = false;
        if (events.OnDestinationReachedEvent == null) events.OnDestinationReachedEvent = new UnityEvent();

        StartCoroutine(UpdateNearestInteractable());
        StartCoroutine(UpdateNearestEnemies());
    }

    // Update is called once per frame
    void Update()
    {
        // updating acceleration variables
        if (_movementInput.magnitude >= 0.1f)
        {
            _currentMovementDuration += Time.deltaTime;
        }
        else if (_movementInput.magnitude == 0f)
        {
            _currentMovementDuration = 0;
        }


        // nav mesh agent movement part
        if (navMeshAgent.enabled)
        {
            // if the player has reached the destination
            if (DidReachDestination())
            {
                events.OnDestinationReachedEvent.Invoke();
            }
        }
    }

    public void OnHealthChanged(GameObject gameObj, float health)
    {
        //Debug.Log("Player health changed: " + onHealthChangeEventArgs.currentHealth);
    }

    public void OnDeath(GameObject gameObj)
    {
        //Debug.Log("Player death: ");
    }


    private void FixedUpdate()
    {
        if (!disableControls)
        {
            Move();
            Rotation();
        }
    }

    private void Rotation()
    {
        // calculate camera look angle to rotate
        _targetAngleY = Mathf.Atan2(_movementInput.x, _movementInput.y) * Mathf.Rad2Deg + _cam.transform.eulerAngles.y;
        // smooth
        float tempAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngleY, ref _turnSmoothVelocity,
            movement.turnSmoothTime);

        transform.rotation = Quaternion.Euler(0f, tempAngle, 0f);
    }

    private void Move()
    {
        // caclulate acceleration part
        float acceleration = Mathf.Clamp01(_currentMovementDuration / movement.accelerationTime);
        float speed = Time.fixedDeltaTime * movement.movementSpeed * acceleration;

        // camera aim part
        Vector3 movementVector = Quaternion.Euler(0f, _targetAngleY, 0f) * Vector3.forward;

        if (_movementInput.magnitude >= 0.1f)
        {
            // add velocity
            _rb.velocity = new Vector3(movementVector.x * speed, _rb.velocity.y, movementVector.z * speed);
        }
        else
        {
            // add velocity
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        }
    }

    public void StopMovement()
    {
        _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
    }

    public void SetDestination(Vector3 destination, float threshold)
    {
        destinationThreshold = threshold;
        disableControls = true;
        navMeshAgent.enabled = true;
        navMeshAgent.destination = destination;
    }

    public void UnsetDestination()
    {
        destinationThreshold = 0f;
        navMeshAgent.destination = Vector3.zero;
        navMeshAgent.enabled = false;
        disableControls = false;
        StopMovement();
    }

    public bool DidReachDestination()
    {
        Vector2 destination = new Vector2(navMeshAgent.destination.x, navMeshAgent.destination.z);
        Vector2 playerPos = new Vector2(transform.position.x, transform.position.z);

        float dist = Vector2.Distance(destination, playerPos);

        if (dist <= destinationThreshold)
        {
            //Debug.Log("player pos: " + playerPos);
            //Debug.Log("destination: " + destination);
            //Debug.Log("distance: " + dist);
            //Debug.Log("threshold: " + destinationThreshold);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Interact()
    {
        if (_nearestInteractable != null)
        {
            _nearestInteractable.TryGetComponent<IInteractable<PlayerController>>(out var interactableController);
            if (interactableController != null)
            {
                interactableController.Interact(this);
            }
        }
    }

    private IEnumerator UpdateNearestInteractable()
    {
        for (;;)
        {
            _nearestInteractable = null;
            // define if there are interaction objects inside interaction radius
            GameObject[] interactables = GameObject.FindGameObjectsWithTag("Interactive");
            float minFoundDist = Mathf.Infinity;
            foreach (GameObject interactable in interactables)
            {
                var position = interactable.transform.position;
                Vector2 interactablePos2D = new Vector2(position.x, position.z);
                Vector2 playerPos2D = new Vector2(transform.position.x, transform.position.z);
                float dist = Vector2.Distance(interactablePos2D, playerPos2D);
                if (dist < minFoundDist && dist <= interactionRadius)
                {
                    minFoundDist = dist;
                    _nearestInteractable = interactable;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator UpdateNearestEnemies()
    {
        for (;;)
        {
            // define if there are enemies inside melee attack radius
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            _isCloseToEnemies = false;
            foreach (GameObject enemy in enemies)
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) <= _combatController.meleeAttackRadius)
                {
                    _isCloseToEnemies = true;
                    break;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // melee attack range 
        if (_isCloseToEnemies)
        {
            Gizmos.color = Color.red / 3;
        }
        else
        {
            Gizmos.color = Color.yellow / 4;
        }

        Gizmos.DrawSphere(transform.position, _combatController.meleeAttackRadius);

        // interaction range 
        if (_nearestInteractable)
        {
            Gizmos.color = Color.yellow / 3;
            Gizmos.DrawLine(transform.position, _nearestInteractable.transform.position);
        }
    }
#endif
}