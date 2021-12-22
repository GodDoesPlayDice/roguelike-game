using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable<PlayerController>
{
    public bool neverOpened = true;
    public bool cannotBeOpened = false;
    public bool opened = false;

    public void Interact(PlayerController playerController)
    {
        if (cannotBeOpened) return;
        if (!opened)
        {
            transform.position = transform.position + Vector3.up * 3;
            opened = true;
        }
        else if (opened)
        {
            transform.position = transform.position + Vector3.down * 3;
            opened = false;
        }
    }
}
