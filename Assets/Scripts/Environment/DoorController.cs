using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshRenderer))]
public class DoorController : MonoBehaviour, IInteractable<PlayerController>
{
    public enum DoorState
    {
        closed,
        open
    }
    public DoorState state = DoorState.closed;
    private bool isLocked = false;
    private bool isBusy = false;


    [Range(0, 1)]
    public float anotherSideReachThreshold = .1f;
    public float interactTimeRate = 1f;
    private float prevInteractEndTime = 0f;

    // vars for open and closed state difference
    [SerializeField]
    private Vector3 openDoorPosition;
    private Vector3 closedDoorPosition;
    private Material material;
    private Color defaultColor;

    // these points are needed in order to move the player and so that the room can find doors
    [HideInInspector]
    public Vector3 leftSidePoint;
    [HideInInspector]
    public Vector3 rightSidePoint;


    private PlayerController playerController;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        rightSidePoint = (transform.position + transform.InverseTransformDirection(Vector3.right));
        leftSidePoint = (transform.position + transform.InverseTransformDirection(Vector3.left));
    }

    private void Start()
    {
        openDoorPosition = transform.position + openDoorPosition;
        closedDoorPosition = transform.position;
        defaultColor = material.color;
        state = DoorState.closed;
        prevInteractEndTime = Time.time;
    }

    public void Interact(PlayerController player)
    {
        if (Time.time - prevInteractEndTime < interactTimeRate) return;
        if (isLocked || isBusy) return;
        // cache player controller on first interaction
        if (playerController == null) playerController = player;
        MakeOpen();
        // move player
        Vector2 playerPos = new Vector2(playerController.gameObject.transform.position.x, playerController.gameObject.transform.position.z);
        Vector2 right = new Vector2(rightSidePoint.x, rightSidePoint.z);
        Vector2 left = new Vector2(leftSidePoint.x, leftSidePoint.z);
        float distToRight = Vector2.Distance(playerPos, right);
        float distToLeft = Vector2.Distance(playerPos, left);

        Vector3 destination = distToRight > distToLeft ? rightSidePoint : leftSidePoint;
        playerController.StopMovement();
        playerController.SetDestination(destination, anotherSideReachThreshold);
        // subscribe on destination reached event
        playerController.events.OnDestinationReachedEvent.AddListener(OnPlayerWalkedThrough);
        isBusy = true;
    }

    private void OnPlayerWalkedThrough()
    {
        isBusy = false;
        prevInteractEndTime = Time.time;
        if (!isLocked) MakeClosed();
        playerController.UnsetDestination();
        // unsubscribe on destination reached event
        playerController.events.OnDestinationReachedEvent.RemoveListener(OnPlayerWalkedThrough);
    }

    private void MakeClosed()
    {
        state = DoorState.closed;
        //maintain closed position and look
        if (transform.position != closedDoorPosition) transform.position = closedDoorPosition;
        material.color = defaultColor * 2;
    }

    private void MakeOpen()
    {
        if (state == DoorState.open || isLocked) return;
        state = DoorState.open;
        //maintain closed position and look
        if (transform.position != openDoorPosition) transform.position = openDoorPosition;
        material.color = Color.green * 1.3f;
    }
    public void LockDoor()
    {
        MakeClosed();
        isLocked = true;
        material.color = Color.red * 1.3f;
        if (transform.position != closedDoorPosition) transform.position = closedDoorPosition;
    }

    public void UnlockDoor()
    {
        MakeClosed();
        isLocked = false;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, leftSidePoint);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, rightSidePoint);
    }
#endif
}
