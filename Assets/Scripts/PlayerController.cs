using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // some basic vars
    public bool disableControls = false;
    public GameObject cam;
    public float interactionDistance = 0.5f;

    private Rigidbody rb;


    // interaction variables
    public float interactionRadius = 0.5f;
    private GameObject closestObjectToInteract;

    // movement variables
    [System.Serializable]
    public class PlayerMovement
    {
        public float movementSpeed = 1f;
        public float jumpForce = 1f;

        [Range(0.01f, 5)]
        public float accelerationTime = 1f;
        public float turnSmoothTime = 0.1f;
    }
    public PlayerMovement movement;

    // fighting system variables
    [System.Serializable]
    public class PlayerCombat
    {
        public float meleeAttackRadius = 1f;
    }
    public PlayerCombat combat;

    private MeleeAttacker meleeAttackerController;
    private bool isCloseToEnemies = false;


    // private variables for movement
    private Vector2 movementInput;
    private NavMeshAgent navMeshAgent;
    // acceleration
    private float currentMovementDuration = 0;
    // jumping
    private bool jumpNeeded = false;
    private bool isOnJumpSurface = false;
    private string jumpSurfaceTagName = "JumpSurface";
    // privates for rotation
    private float turnSmoothVelocity;
    private float targetAngleY;


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
        movementInput = new Vector2();
        TryGetComponent<Rigidbody>(out rb);
        TryGetComponent<MeleeAttacker>(out meleeAttackerController);
        TryGetComponent<NavMeshAgent>(out navMeshAgent);
    }

    // Update is called once per frame
    void Update()
    {
        // writitng acceleration variables
        if (movementInput.magnitude >= 0.1f)
        {
            currentMovementDuration += Time.deltaTime;
        }
        else if (movementInput.magnitude == 0f)
        {
            currentMovementDuration = 0;
        }
    }

    private void FixedUpdate()
    {
        if (!disableControls)
        {
            Move();
            Rotation();
        }


        // TEMP: define if there are enemies inside melee attack radius
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

        // TEMP: define if there are interaction objects inside interaction radius
        GameObject[] interactives = GameObject.FindGameObjectsWithTag("Interactive");
        closestObjectToInteract = null;
        float minFoundDist = Mathf.Infinity;
        foreach (GameObject interactive in interactives)
        {
            float dist = Vector3.Distance(interactive.transform.position, transform.position);
            if (dist < minFoundDist && dist <= interactionRadius) {
                minFoundDist = dist;
                closestObjectToInteract = interactive;
            }
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

    private void Jump()
    {
        if (jumpNeeded && isOnJumpSurface) rb.AddForce(0f, movement.jumpForce, 0f, ForceMode.Impulse);
        jumpNeeded = false;
    }

    private void MeleeAttack()
    {
        if (meleeAttackerController != null)
        {

            Collider[] enemies = Physics.OverlapSphere(transform.position, combat.meleeAttackRadius);

            foreach (Collider enemy in enemies)
            {
                meleeAttackerController.Attack(enemy.gameObject);
            }

        }
    }

    private void Interact()
    {
        if (closestObjectToInteract != null) {
            IInteractable<PlayerController> interactableController;
            closestObjectToInteract.TryGetComponent<IInteractable<PlayerController>>(out interactableController);

            if (interactableController != null)
            {
                interactableController.Interact(this);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == jumpSurfaceTagName)
        {
            isOnJumpSurface = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == jumpSurfaceTagName)
        {
            isOnJumpSurface = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (isCloseToEnemies)
        {
            Gizmos.color = Color.yellow / 4;
        }
        else
        {
            Gizmos.color = Color.red / 4;
        }
        Gizmos.DrawSphere(transform.position, combat.meleeAttackRadius);
    }
}
