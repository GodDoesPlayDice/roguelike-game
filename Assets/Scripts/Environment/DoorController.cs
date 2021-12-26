using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MeshRenderer))]
public class DoorController : MonoBehaviour, IInteractable<PlayerController>
{
    public enum DoorState
    {
        locked,
        open,
        closed
    }
    public DoorState state;

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


    private PlayerController playerControllerRef;
    private Vector3 currentPlayerDestination;
    private bool lastPlayerGoingState = false;

    private void Awake()
    {
        state = DoorState.closed;

        rightSidePoint = (transform.position + transform.InverseTransformDirection(Vector3.right));
        leftSidePoint = (transform.position + transform.InverseTransformDirection(Vector3.left));

        openDoorPosition = transform.position + openDoorPosition;
        closedDoorPosition = transform.position;
        material = GetComponent<MeshRenderer>().material;
        defaultColor = material.color;
    }

    private void Update()
    {
        switch (state)
        {
            case DoorState.closed:
                if (transform.position != closedDoorPosition) transform.position = closedDoorPosition;
                material.color = defaultColor * 2;
                break;
            case DoorState.open:
                if (transform.position != openDoorPosition) transform.position = openDoorPosition;

                // if the player first went to the destination point, and then reached it, close the door
                if (playerControllerRef != null)
                {
                    bool currentPlayerGoingState = playerControllerRef.isGoingToDestination;
                    if (lastPlayerGoingState == true && currentPlayerGoingState == false)
                    {
                        CloseDoor();
                    }
                    lastPlayerGoingState = playerControllerRef.isGoingToDestination;
                }
                break;
            case DoorState.locked:
                material.color = Color.red * 1.3f;
                if (transform.position != closedDoorPosition) transform.position = closedDoorPosition;

                break;
        }

    }

    public void Interact(PlayerController playerController)
    {
        if (playerControllerRef == null) playerControllerRef = playerController;

        if (state == DoorState.closed)
        {
            OpenDoor();
            // move player
            Vector3 playerPos = playerController.gameObject.transform.position;
            currentPlayerDestination = Vector3.Distance(playerPos, rightSidePoint) > Vector3.Distance(playerPos, leftSidePoint) ? rightSidePoint : leftSidePoint;
            playerController.SetDestination(currentPlayerDestination);
        }
    }

    private void CloseDoor()
    {
        state = DoorState.closed;
    }
    private void OpenDoor()
    {
        state = DoorState.open;
    }
    public void LockDoor()
    {
        state = DoorState.locked;
    }
    public void UnlockDoor()
    {
        state = DoorState.closed;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, leftSidePoint);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, rightSidePoint);
    }
}
