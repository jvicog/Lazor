using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishGame : MonoBehaviour {
    private WorldManager world;
    [SerializeField] private GameObject screen;

    private void Awake() {
        world = FindObjectOfType<WorldManager>();
    }

    private void OnTriggerEnter(Collider other) {
        if (world.GuardsChasing) return;

        if (other.CompareTag("Player")) {
            print("Olee");
            screen.SetActive(true);
            Time.timeScale = 0;
        }
        
    }
}
