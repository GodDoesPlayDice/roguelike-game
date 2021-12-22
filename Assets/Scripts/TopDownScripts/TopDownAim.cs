using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownAim : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject aimPointObject;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        //if (Physics.Raycast(ray, out RaycastHit raycastHit))
        //{
        //    aimPointObject.transform.position = raycastHit.point;
        //}
        
    }
}
