using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayViewer : MonoBehaviour
{
    public float weaponRange = 50f;
    public Camera fpsCam;

    
    // Start is called before the first frame update
    void Start()
    {
        fpsCam = GetComponentInChildren<Camera>();

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 lineOrigin = fpsCam.ViewportToWorldPoint(new Vector3(.5f, .5f, .0f));
        Debug.DrawRay(lineOrigin, fpsCam.transform.forward * weaponRange, Color.green);
    }
}
