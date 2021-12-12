using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtEmoji : MonoBehaviour
{
    private Transform camera1;
    // Start is called before the first frame update
    void Start()
    {
        camera1 = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(transform.position + camera1.rotation * Vector3.forward, camera1.rotation * Vector3.up);
    }
}
