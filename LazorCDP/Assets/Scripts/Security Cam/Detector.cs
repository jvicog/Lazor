using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour {
    [SerializeField] private SecurityCamBehaviour securityCamBehaviour;
    private void OnTriggerEnter(Collider other) {
        print(other.tag);
        if (other.CompareTag("Player")) {
            securityCamBehaviour.PlayerDetection(other.transform);
        }
    }
}
