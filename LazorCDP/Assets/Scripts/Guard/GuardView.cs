using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardView : MonoBehaviour
{
    [SerializeField] private GuardBehaviour guardBehaviour;
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            guardBehaviour.PlayerDetection(other.transform);
        }
        
        if(other.CompareTag("Worker"))
        {
            print("F currele");
            if (other.GetComponent<WorkerFinal>().isDead) {
                
                guardBehaviour.CorpseSeen();
            }
        }
    }
}
