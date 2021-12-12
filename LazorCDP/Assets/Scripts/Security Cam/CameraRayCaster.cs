using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRayCaster : MonoBehaviour {


    public bool LaunchRays(Transform p, float distance) {
        Ray ray = new Ray(transform.position, p.position - transform.position);
        
        RaycastHit hit;
            
        if (Physics.Raycast(ray, out hit, distance)){
            if (hit.collider.CompareTag("Player")) {
                return true;
            }
               
        }
        
        return false;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.green;
        Vector3 direction = transform.forward * 5;
        Gizmos.DrawRay(transform.position, direction);

    }
}
