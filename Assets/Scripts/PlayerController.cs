using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Target;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // some basic vars
    public bool disableControls = false;
    private Rigidbody rb;
    private GameObject cam;

    // interaction variables
    public float interactionRadius = 0.5f;
    private GameObject nearestInteractable;

    // movement variables
    [System.Serializable]
    public class PlayerMovement
    {
        public float movementSpeed = 1f;
        [Range(0.01f, 5)]
        public float accelerationTime = 1f;
        public float turnSmoothTime = 0.1f;
    }
    public PlayerMovement movement;


    // private variables for movement
    private Vector2 movementInput;

    // nav mesh part
    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    [HideInInspector]
    public float destinationThreshold = 0f;


    // acceleration
    private float currentMovementDuration = 0;
    // privates for rotation
    private float turnSmoothVelocity;
    private float targetAngleY;


    // fighting system variables
    [System.Serializable]
    public class PlayerCombat
    {
        public float meleeAttackRadius = 1f;
        public float meleeAttackDamage = 20f;
    }
    public PlayerCombat combat;
    private MeleeAttacker meleeAttackerController;
    private bool isCloseToEnemies = false;


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
            movementInput = value.ReadValue<Vector2>();
        }
        else if (value.canceled)
        {
            movementInput = Vector2.zero;
        }
    }
    public void OnAttack(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            MeleeAttack();
        }
    }
    public void OnInteract(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            Interact();
        }
    }

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
        movementInput = new Vector2();
        TryGetComponent<Rigidbody>(out rb);
        TryGetComponent<MeleeAttacker>(out meleeAttackerController);
        TryGetComponent<NavMeshAgent>(out navMeshAgent);
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = false;
        }

        // events
        if (events.OnDestinationReachedEvent == null) events.OnDestinationReachedEvent = new UnityEvent();

        StartCoroutine(UpdateNearestInteractable());
        StartCoroutine(UpdateNearestEnemies());
    }

    // Update is called once per frame
    void Update()
    {
        // updating acceleration variables
        if (movementInput.magnitude >= 0.1f)
        {
            currentMovementDuration += Time.deltaTime;
        }
        else if (movementInput.magnitude == 0f)
        {
            currentMovementDuration = 0;
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

    public void OnHealthChanged(TargetController.OnHealthChangeEventArgs onHealthChangeEventArgs)
    {
        Debug.Log("Player health changed: " + onHealthChangeEventArgs.gameObject.name);
    }
    public void OnDeath(TargetController.OnDeathEventArgs onDeathEventArgs)
    {
        Debug.Log("Player death: ");
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
        targetAngleY = Mathf.Atan2(movementInput.x, movementInput.y) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
        // smooth
        float tempAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity, movement.turnSmoothTime);

        transform.rotation = Quaternion.Euler(0f, tempAngle, 0f);
    }

    private void Move()
    {
        // caclulate acceleration part
        float acceleration = Mathf.Clamp01(currentMovementDuration / movement.accelerationTime);
        float speed = Time.deltaTime * movement.movementSpeed * acceleration;

        // camera aim part
        Vector3 movementVector = Quaternion.Euler(0f, targetAngleY, 0f) * Vector3.forward;

        if (movementInput.magnitude >= 0.1f)
        {
            // add velocity
            rb.velocity = new Vector3(movementVector.x * speed, rb.velocity.y, movementVector.z * speed);
        }
        else
        {
            // add velocity
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    public void StopMovement()
    {
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
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
        } else
        {
            return false;
        }   
    }

    private void MeleeAttack()
    {
        if (meleeAttackerController != null)
        {

            Collider[] enemies = Physics.OverlapSphere(transform.position, combat.meleeAttackRadius);

            foreach (Collider enemy in enemies)
            {
                meleeAttackerController.Attack(enemy.gameObject, combat.meleeAttackDamage);
            }

        }
    }

    private void Interact()
    {
        if (nearestInteractable != null)
        {
            IInteractable<PlayerController> interactableController;
            nearestInteractable.TryGetComponent<IInteractable<PlayerController>>(out interactableController);

            if (interactableController != null)
            {
                interactableController.Interact(this);
            }
        }
    }

    private IEnumerator UpdateNearestInteractable()
    {
        for (; ;)
        {
            nearestInteractable = null;
            // define if there are interaction objects inside interaction radius
            GameObject[] interactables = GameObject.FindGameObjectsWithTag("Interactive");
            float minFoundDist = Mathf.Infinity;
            foreach (GameObject interactable in interactables)
            {
                Vector2 interactablePos2D = new Vector2(interactable.transform.position.x, interactable.transform.position.z);
                Vector2 playerPos2D = new Vector2(transform.position.x, transform.position.z);
                float dist = Vector2.Distance(interactablePos2D, playerPos2D);
                if (dist < minFoundDist && dist <= interactionRadius)
                {
                    minFoundDist = dist;
                    nearestInteractable = interactable;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator UpdateNearestEnemies()
    {
        for (; ; )
        {
            // define if there are enemies inside melee attack radius
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            isCloseToEnemies = false;
            foreach (GameObject enemy in enemies)
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) <= combat.meleeAttackRadius)
                {
                    isCloseToEnemies = true;
                    break;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // melee attack range 
        if (isCloseToEnemies)
        {
            Gizmos.color = Color.red / 3;
        }
        else
        {
            Gizmos.color = Color.yellow / 4;
        }
        Gizmos.DrawSphere(transform.position, combat.meleeAttackRadius);

        // interaction range 
        if (nearestInteractable)
        {
            Gizmos.color = Color.yellow / 3;
            Gizmos.DrawLine(transform.position, nearestInteractable.transform.position);
        }
    }
#endif
}
