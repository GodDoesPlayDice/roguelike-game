using System.Collections;
using Combat;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Actors
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(TargetController))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(ActorController))]
    public class PlayerController : MonoBehaviour, IActor
    {
        public GameObject thisObject { get; set; }

        // some basic vars 
        private ActorController _actorController;
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

        // acceleration
        private float _currentMovementDuration = 0;

        // privates for rotation
        private float _turnSmoothVelocity;
        private float _targetAngleY;

        private ActorCombatController _combatController;
        private bool _isCloseToEnemies = false;

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
                _combatController.ShootAttack();
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
            TryGetComponent(out _combatController);
            TryGetComponent(out _actorController);

            thisObject = gameObject;
        }

        private void Start()
        {
            _movementInput = new Vector2();
            StartCoroutine(UpdateNearestInteractable());
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
            Movement();
            Rotation();
        }

        private void Rotation()
        {
            // calculate camera look angle to rotate
            _targetAngleY = Mathf.Atan2(_movementInput.x, _movementInput.y) * Mathf.Rad2Deg +
                            _cam.transform.eulerAngles.y;
            // smooth
            float tempAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngleY, ref _turnSmoothVelocity,
                movement.turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, tempAngle, 0f);
        }

        private void Movement()
        {
            if (_actorController.navMeshActive)
            {
                if (_rb.velocity != Vector3.zero) _rb.velocity = Vector3.zero;
                return;
            }

            // updating acceleration variables
            if (_movementInput.magnitude >= 0.1f)
            {
                _currentMovementDuration += Time.fixedDeltaTime;
            }
            else
            {
                _currentMovementDuration = 0;
            }

            // calculate acceleration part
            var acceleration = Mathf.Clamp01(_currentMovementDuration / movement.accelerationTime);
            var speed = Time.fixedDeltaTime * movement.movementSpeed * acceleration;

            // camera aim part
            var movementVector = Quaternion.Euler(0f, _targetAngleY, 0f) * Vector3.forward;

            _rb.velocity = _movementInput.magnitude >= 0.1f
                ? new Vector3(movementVector.x * speed, _rb.velocity.y, movementVector.z * speed)
                : new Vector3(0f, _rb.velocity.y, 0f);
        }

        public void StopMovement()
        {
            _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
        }

        private void Interact()
        {
            if (_nearestInteractable == null) return;
            _nearestInteractable.TryGetComponent<IInteractable<ActorController>>(out var interactableController);
            interactableController?.Interact(_actorController);
        }

        private IEnumerator UpdateNearestInteractable()
        {
            for (;;)
            {
                _nearestInteractable = null;
                // define if there are interaction objects inside interaction radius
                var interactables = GameObject.FindGameObjectsWithTag("Interactive");
                var minFoundDist = Mathf.Infinity;
                foreach (var interactable in interactables)
                {
                    var position = interactable.transform.position;
                    var interactablePos2D = new Vector2(position.x, position.z);
                    var playerPos2D = new Vector2(transform.position.x, transform.position.z);
                    var dist = Vector2.Distance(interactablePos2D, playerPos2D);
                    if (!(dist < minFoundDist) || !(dist <= interactionRadius)) continue;
                    minFoundDist = dist;
                    _nearestInteractable = interactable;
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

            // interaction range 
            if (_nearestInteractable)
            {
                Gizmos.color = Color.yellow / 3;
                Gizmos.DrawLine(transform.position, _nearestInteractable.transform.position);
            }
        }
#endif
    }
}