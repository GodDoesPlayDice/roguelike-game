using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingAtObjects : MonoBehaviour
{
    public Color highlightColor;
    [Range(0f, 5f)]
    public float emission = 1f;

    private Dictionary<int, Color> matDictionary = new Dictionary<int, Color>();

    private void OnTriggerEnter(Collider other)
    {
        int instanceID = other.gameObject.GetInstanceID();
        Material mat = other.gameObject.GetComponent<MeshRenderer>().material;
        matDictionary[instanceID] = mat.color;
        
        mat.color = highlightColor*emission;
    }

    private void OnTriggerExit(Collider other)
    {
        int instanceID = other.gameObject.GetInstanceID();
        Material mat = other.gameObject.GetComponent<MeshRenderer>().material;
        mat.color = matDictionary[instanceID];
    }
}
