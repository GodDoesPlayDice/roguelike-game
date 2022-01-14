using Actors;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(MeshRenderer))]
    public class DoorController : MonoBehaviour, IInteractable<PlayerController>
    {
        public enum DoorState
        {
            Closed,
            Open
        }
        public DoorState state = DoorState.Closed;
        private bool _isLocked = false;
        private bool _isBusy = false;


        [Range(0, 1)]
        public float anotherSideReachThreshold = .1f;
        public float interactTimeRate = 1f;
        private float _prevInteractEndTime = 0f;

        // vars for open and closed state difference
        [SerializeField]
        private Vector3 openDoorPosition;
        private Vector3 _closedDoorPosition;
        private Material _material;
        private Color _defaultColor;

        // these points are needed in order to move the player and so that the room can find doors
        [HideInInspector]
        public Vector3 leftSidePoint;
        [HideInInspector]
        public Vector3 rightSidePoint;


        private PlayerController _playerController;

        private void Awake()
        {
            _material = GetComponent<MeshRenderer>().material;
            var position = transform.position;
            rightSidePoint = (position + transform.InverseTransformDirection(Vector3.right));
            leftSidePoint = (position + transform.InverseTransformDirection(Vector3.left));
        }

        private void Start()
        {
            var position = transform.position;
            openDoorPosition = position + openDoorPosition;
            _closedDoorPosition = position;
            _defaultColor = _material.color;
            state = DoorState.Closed;
            _prevInteractEndTime = Time.time;
        }

        public void Interact(PlayerController player)
        {
            if (Time.time - _prevInteractEndTime < interactTimeRate) return;
            if (_isLocked || _isBusy) return;
            // cache player controller on first interaction
            if (_playerController == null) _playerController = player;
            MakeOpen();
            // move player
            var position = _playerController.gameObject.transform.position;
            Vector2 playerPos = new Vector2(position.x, position.z);
            Vector2 right = new Vector2(rightSidePoint.x, rightSidePoint.z);
            Vector2 left = new Vector2(leftSidePoint.x, leftSidePoint.z);
            float distToRight = Vector2.Distance(playerPos, right);
            float distToLeft = Vector2.Distance(playerPos, left);

            Vector3 destination = distToRight > distToLeft ? rightSidePoint : leftSidePoint;
            _playerController.StopMovement();
            _playerController.SetDestination(destination, anotherSideReachThreshold);
            // subscribe on destination reached event
            _playerController.events.OnDestinationReachedEvent.AddListener(OnPlayerWalkedThrough);
            _isBusy = true;
        }

        private void OnPlayerWalkedThrough()
        {
            _isBusy = false;
            _prevInteractEndTime = Time.time;
            if (!_isLocked) MakeClosed();
            _playerController.UnsetDestination();
            // unsubscribe on destination reached event
            _playerController.events.OnDestinationReachedEvent.RemoveListener(OnPlayerWalkedThrough);
        }

        private void MakeClosed()
        {
            state = DoorState.Closed;
            //maintain closed position and look
            if (transform.position != _closedDoorPosition) transform.position = _closedDoorPosition;
            _material.color = _defaultColor * 2;
        }

        private void MakeOpen()
        {
            if (state == DoorState.Open || _isLocked) return;
            state = DoorState.Open;
            //maintain closed position and look
            if (transform.position != openDoorPosition) transform.position = openDoorPosition;
            _material.color = Color.green * 1.3f;
        }
        public void LockDoor()
        {
            MakeClosed();
            _isLocked = true;
            _material.color = Color.red * 1.3f;
            if (transform.position != _closedDoorPosition) transform.position = _closedDoorPosition;
        }

        public void UnlockDoor()
        {
            MakeClosed();
            _isLocked = false;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var position = transform.position;
            Gizmos.DrawLine(position, leftSidePoint);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(position, rightSidePoint);
        }
#endif
    }
}
