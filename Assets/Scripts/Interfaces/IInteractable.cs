using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable<T>
{
    void Interact(T controllerComponent);
}
