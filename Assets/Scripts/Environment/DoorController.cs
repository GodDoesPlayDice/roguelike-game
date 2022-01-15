using Actors;
using UnityEngine;

namespace Environment
{
    [RequireComponent(typeof(MeshRenderer))]
    public class DoorController : MonoBehaviour, IInteractable<ActorController>
    {
        public enum DoorState
        {
            Closed,
            Open
        }

        public DoorState state = DoorState.Closed;
        private bool _isLocked = false;
        private bool _isBusy = false;


        [Range(0, 1)] public float anotherSideReachThreshold = .1f;
        public float interactTimeRate = 1f;
        private float _prevInteractEndTime = 0f;

        // vars for open and closed state difference
        [SerializeField] private Vector3 openDoorPosition;
        private Vector3 _closedDoorPosition;
        private Material _material;
        private Color _defaultColor;

        // these points are needed in order to move the player and so that the room can find doors
        [HideInInspector] public Vector3 leftSidePoint;
        [HideInInspector] public Vector3 rightSidePoint;


        private ActorController _actorController;

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

        public void Interact(ActorController actor)
        {
            if (Time.time - _prevInteractEndTime < interactTimeRate) return;
            if (_isLocked || _isBusy) return;
            _actorController = actor;
            MakeOpen();
            // move actor
            var position = _actorController.gameObject.transform.position;
            var playerPos = new Vector2(position.x, position.z);
            var rightPoint = new Vector2(rightSidePoint.x, rightSidePoint.z);
            var leftPoint = new Vector2(leftSidePoint.x, leftSidePoint.z);
            var distToRight = Vector2.Distance(playerPos, rightPoint);
            var distToLeft = Vector2.Distance(playerPos, leftPoint);

            var destination = distToRight > distToLeft ? rightSidePoint : leftSidePoint;
            _actorController.SetDestination(destination, anotherSideReachThreshold);
            // subscribe on destination reached event
            _actorController.onDestinationReachedEvent.AddListener(OnActorWalkedThrough);
            _isBusy = true;
        }

        private void OnActorWalkedThrough()
        {
            _isBusy = false;
            _prevInteractEndTime = Time.time;
            if (!_isLocked) MakeClosed();
            _actorController.UnsetDestination();
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