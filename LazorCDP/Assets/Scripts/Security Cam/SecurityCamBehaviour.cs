using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SecurityCamBehaviour : MonoBehaviour, IDamageable<float>
{
    // States Updates
    delegate void FsmUpdate();
    private FsmUpdate _fsmUpdate;
    
    // State Machine Parameters
    #region StateMachineSetUp
        private StateMachineEngine fsm;

        private State watch;
        private State lookingAtPlayer;
        private State destroyed;

        private Perception playerFoundPerception;
        private Perception playerLostPerception;
        private Perception cameraDestroyedPerception;
    #endregion
    
    // Camera Rotation
    private Vector3 initialRotation;
    [SerializeField] private float rotationAngle = 45;
    [SerializeField] private float rotationTime;
    [SerializeField] private float waitingTime;
    private bool isRotating;
    private bool rotatingLeft;
    private bool rotationInProgress;

    private Vector2 targets;
    
    // Player Detection

    [SerializeField] private Transform playerGO;
    [SerializeField] private CameraRayCaster cameraRayCaster;
    [SerializeField] private Light light;

    private WorldManager worldManager;
    
    
    // Camera Status
    private int health = 3;

    private void Awake() {
        initialRotation = transform.rotation.eulerAngles;
        playerGO = FindObjectOfType<PlayerController>().transform;
        InitFsm();
        InitCam();

        worldManager = FindObjectOfType<WorldManager>();
    }

    private void Update() {
        _fsmUpdate();
        fsm.Update();
    }

    void WatchUpdate() {
        CameraRotation();

    }

    void CameraRotation() {
        if (isRotating) {
            if (rotationInProgress) return;
            
            rotationInProgress = true;
            
            if(rotatingLeft) {
                transform.DORotate(new Vector3(initialRotation.x, targets.x, initialRotation.z), rotationTime)
                    .OnComplete(() => StartCoroutine(WaitingTime(waitingTime)));
            }
            else {
                transform.DORotate(new Vector3(initialRotation.x, targets.y, initialRotation.z), rotationTime)
                    .OnComplete(() => StartCoroutine(WaitingTime(waitingTime)));
            }
            
        }

        
    }

    IEnumerator WaitingTime(float t) {
        isRotating = false;
        rotatingLeft = !rotatingLeft;
        yield return new WaitForSeconds(t);
        isRotating = true;
        
        rotationInProgress = false;
        
    }

    public void PlayerDetection(Transform player) {
        if (cameraRayCaster.LaunchRays(player, 30)) {
            playerFoundPerception.Fire();
        }
    }

    void LookingUpdate() {

        transform.DOLookAt(playerGO.position, 0, AxisConstraint.Y);
        
        if (!cameraRayCaster.LaunchRays(playerGO, 30)) {
            playerLostPerception.Fire();
        }
    }

    void DestroyUpdate() {
        print("Destroyed");
    }

    void InitFsm() {
        fsm = new StateMachineEngine();

        watch = fsm.CreateEntryState("Watch", () => {
            _fsmUpdate = WatchUpdate;
            light.color = Color.green;
        });

        lookingAtPlayer = fsm.CreateState("Looking", () => {
            _fsmUpdate = LookingUpdate;
            worldManager.onPlayerSeen.Invoke();
            light.color = Color.red;
        });

        destroyed = fsm.CreateState("Destroyed", (() => {
            Die();
        }));

        playerFoundPerception = fsm.CreatePerception<PushPerception>();
        playerLostPerception = fsm.CreatePerception<PushPerception>();
        cameraDestroyedPerception = fsm.CreatePerception<PushPerception>();
        

        fsm.CreateTransition("PlayerFound", watch, playerFoundPerception, lookingAtPlayer);
        fsm.CreateTransition("PlayerLost", lookingAtPlayer, playerLostPerception, watch);
        fsm.CreateTransition("DestroyedFromWatch", watch, cameraDestroyedPerception, destroyed);
        fsm.CreateTransition("DestroyedFromLooking", lookingAtPlayer, cameraDestroyedPerception, destroyed);
    }

    void InitCam() {
        targets.x = initialRotation.y - rotationAngle;
        targets.y = initialRotation.y + rotationAngle;
        isRotating = true;
    }


    public void TakeDamage(float damage) {
        health -= 1;

        if (health <= 0) {
            cameraDestroyedPerception.Fire();
        }
    }

    public void Die() {
        Destroy(gameObject);
    }
}
