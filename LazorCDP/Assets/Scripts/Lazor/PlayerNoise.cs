using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Guard")) {
            other.GetComponent<GuardBehaviour>().CorpseSeen();
            return;
        }

        if (other.CompareTag("Worker")) {
            var b = other.GetComponent<WorkerFinal>();

            if (b != null) {
                b.Oido();
            }
            else {
                other.GetComponent<CleanerFinal>().Oido();
            }
        }

    }
}
