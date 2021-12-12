using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IDamageable<float> {
    
    // States Updates
    delegate void FsmUpdate();
    private FsmUpdate _fsmUpdate;
        
    // Player animator
    [SerializeField] private Animator _animator;
    
    // RigidBody
    private Rigidbody _rigidbody;

    // InputController
    private PlayerInput _playerInput;
    private Vector3 inputVector;

    // State Machine Parameters
    #region StateMachineSetUp
        private StateMachineEngine playerStateMachine;

        private State standar;
        private State stealth;
        private State running;
        private State hidden;
        private State aiming;
        private State dead;
        
        
        private Perception crouchPerception;
        private Perception standUpPerception;
        private Perception runningPerception;
        private Perception stopRunningPerception;
        private Perception crouchRunningPerception;
        
        
        
        private Transition toStealth;
    

    #endregion
    
    //Camera Stuff
    [SerializeField] private Transform cameraTransform;
   
    
    // Player parameters
    [SerializeField] private bool isCrouched;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isAiming;
    
    // Rig Builder
   [SerializeField] private Rig aimRig;
   [SerializeField] private RigBuilder rigBuilder;
    
    // Player stats

    [SerializeField] private float health = 5;
    [SerializeField] private int bullets = 12;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float stealthSpeed;
    [SerializeField] private float shootCoolDown;
    private float timeNoShoot;
    
    public bool escondido;

    [SerializeField] private GameObject noise;
    // Aiming
    [SerializeField] private GameObject gun;
    [SerializeField] private CinemachineFreeLook cinemachineFreeLook;
    [SerializeField] private GameObject shootEffect;
    private bool isReloading;


    // UI 
    [SerializeField] private GameObject aimReticle;
    [SerializeField] private Text bulletsDisplay;
    [SerializeField] private RectTransform healthBar;
    
    [SerializeField] private CapsuleCollider standCol;
    [SerializeField] private CapsuleCollider crouchCol;

    [SerializeField] private AudioSource audioSourceReload;
    [SerializeField] private AudioSource audioSourceScream;
    [SerializeField] private GameObject bloodSplatter;
    
    WorldManager worldManager;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;

        rigBuilder = GetComponent<RigBuilder>();
        rigBuilder.enabled = false;
        aimRig.weight = 0;

        crouchCol.enabled = false;
        
        gun.SetActive(false);
        aimReticle.SetActive(false);
        
        _fsmUpdate = StandardUpdate;

        playerStateMachine = new StateMachineEngine();
        InitFSM(playerStateMachine);

        worldManager = FindObjectOfType<WorldManager>();

        Cursor.lockState = CursorLockMode.Locked;
    }
    

    private void Update() {
        playerStateMachine.Update();
        _fsmUpdate();
        HandleCamera();
        if (isAiming) {
            HandleRotation();
        }

        if (health <= 0) {
            Die();
        }
        
        timeNoShoot += Time.deltaTime;
    }

    private void HandleCamera() {
    }

    #region Input Events

    public void OnMove(InputValue value) {
        if (escondido) return;
        var moveInput = value.Get<Vector2>();
        inputVector = new Vector3(moveInput.x, 0, moveInput.y);

        if (moveInput.magnitude > 0) {
            isMoving = true;
            _animator.SetBool("isWalking" ,true);
            HandleRotation();

            if (isAiming) {
                isAiming = false;
                gun.SetActive(false);
            
                aimReticle.SetActive(false);
            
                cinemachineFreeLook.m_Lens.FieldOfView = 30;
                cinemachineFreeLook.m_XAxis.Value = 0;
            
               DOTween.To(() => aimRig.weight, x => aimRig.weight = x, 0, 0.1f)
                   .OnComplete((() => rigBuilder.enabled= false));
            }
        }
        else {
            isMoving = false;
            _animator.SetBool("isWalking" ,false);
        }
    }

    public void OnCrouch(InputValue value) {
        if (escondido) return;
        if (isRunning) {
            isCrouched = true;
            _animator.SetBool("isRunning", false);
        }
        
        if (!isCrouched) {
            crouchCol.enabled = true;
            standCol.enabled = false;
            crouchPerception.Fire();
        }
        else { 
            crouchCol.enabled = false;
            standCol.enabled = true;
            standUpPerception.Fire();
        }
        
        
    }

    public void OnRun(InputValue value) {
        if (escondido) return;
        crouchCol.enabled = false;
        standCol.enabled = true;
        if (!isMoving) {
            stopRunningPerception.Fire();
            return;
        }
        
        
        if (value.isPressed && isMoving) {
            _animator.SetBool("isCrouched", false);
            isRunning = true;
            
            runningPerception.Fire();
            
        }
        else {
            isRunning = false;
            _animator.SetBool("isRunning", false);
        }
        
    }

    public void OnAim(InputValue value) {

        if (escondido) return;
        
        if (value.isPressed && !isMoving) {
            isAiming = true;
            rigBuilder.enabled = true;
            gun.SetActive(true);
            aimReticle.SetActive(true);
            cinemachineFreeLook.m_Lens.FieldOfView = 20;
            cinemachineFreeLook.m_XAxis.Value = 20;
            
            DOTween.To(() => aimRig.weight, x => aimRig.weight = x, 1, 0.1f);
        }
        else {
            isAiming = false;
            gun.SetActive(false);
            
            shootEffect.SetActive(false);
            aimReticle.SetActive(false);
            
            cinemachineFreeLook.m_Lens.FieldOfView = 30;
            cinemachineFreeLook.m_XAxis.Value = 0;
            
           DOTween.To(() => aimRig.weight, x => aimRig.weight = x, 0, 0.1f)
               .OnComplete((() => rigBuilder.enabled= false));
        }

    }

    public void OnShoot() {
        if (escondido) return;
        if (isAiming && timeNoShoot > shootCoolDown && bullets > 0 && !isReloading) {
            timeNoShoot = 0;
            bullets--;
            bulletsDisplay.text = bullets.ToString();
            shootEffect.SetActive(true);
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 30)){
                IDamageable<float> hitted = hit.collider.gameObject.GetComponent<IDamageable<float>>();
                if (hitted != null) {
                    hitted.TakeDamage(1);

                    if (hit.collider.CompareTag("SecurityCam")) return;
                    Instantiate(bloodSplatter, hit.point, Quaternion.identity);
                }
                   
            }
        }
    }

    public void OnReload() {
        if (escondido) return;
        if (!isReloading && isAiming && bullets!=12) {
            
            audioSourceReload.PlayOneShot(audioSourceReload.clip);
            StartCoroutine(ReloadCoroutine());
        }
    }

    IEnumerator ReloadCoroutine() {
        isReloading = true;
        yield return new WaitForSeconds(1);
        isReloading = false;
        bullets = 12;
        bulletsDisplay.text = bullets.ToString();
    }

    public void OnEsconderse() {
        if (worldManager.GuardsChasing) return;
        
        escondido = !escondido;
    }
    
    #endregion
   

    private void InitFSM(StateMachineEngine fsm) {
        standar = fsm.CreateEntryState("Standard", (() => {
            _fsmUpdate = StandardUpdate;
            isRunning = false;
            isCrouched = false;
            _animator.SetBool("isCrouched", false);
            noise.transform.DOScale(Vector3.one*2, 0.3f);
        }));
        
        stealth = fsm.CreateState("Stealth", (() => {
            _fsmUpdate = StealthUpdate;
            isRunning = false;
            isCrouched = true;
            _animator.SetBool("isCrouched", true);
            
            
            noise.transform.DOScale(Vector3.one, 0.3f);
        }));

        running = fsm.CreateState("Running", () => {
            isRunning = true;
            isCrouched = false;
            _fsmUpdate = RunningUpdate;
            _animator.SetBool("isRunning", true);
            
            
            noise.transform.DOScale(Vector3.one * 4, 0.3f);
        });
        
        
        crouchPerception = fsm.CreatePerception<PushPerception>();
        standUpPerception = fsm.CreatePerception<PushPerception>();
        runningPerception = fsm.CreatePerception <PushPerception>();
        stopRunningPerception = fsm.CreatePerception <ValuePerception>(() => !isRunning);
        crouchRunningPerception = fsm.CreatePerception <ValuePerception>(() => isCrouched);
        

        fsm.CreateTransition("ToStealth", standar,
            crouchPerception, stealth);
        fsm.CreateTransition("ToStandard", stealth,
            standUpPerception, standar);
        
        fsm.CreateTransition("ToRunFromStealth", stealth,
            runningPerception, running);
        fsm.CreateTransition("ToRunFromStandard", standar,
            runningPerception, running);
        
        fsm.CreateTransition("ToStandardFromRun", running,
            stopRunningPerception, standar);
        fsm.CreateTransition("ToStealthFromRun", running,
            crouchRunningPerception, stealth);

    }

    private void RunningUpdate() {
        MoveCharacter(runSpeed);
    }


    void StandardUpdate() {
        MoveCharacter(walkSpeed);
    }

    void StealthUpdate() {
        MoveCharacter(stealthSpeed);
    }

    bool CheckIfRunning() {
        return isRunning;
    }


    void MoveCharacter(float speed) {
        if (inputVector.magnitude <= 0) {
            noise.gameObject.SetActive(false);
            return;
        }
        noise.gameObject.SetActive(true);
        HandleRotation();

        Vector3 move = new Vector3(inputVector.x, 0, inputVector.z);
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0;

        _rigidbody.velocity = move.normalized * speed + _rigidbody.velocity.y * Vector3.up ;
        
    }

    void  HandleRotation() {
        if (isAiming) {
            transform.DOLookAt(2 * transform.position - cameraTransform.position, 0.2f, AxisConstraint.Y);
        }
        else {
            transform.DOLookAt(transform.position + _rigidbody.velocity, 0.2f, AxisConstraint.Y);
        }
        
    }

    public void TakeDamage(float damage) {
        health--;
        healthBar.localScale = new Vector2(health / 5, 1);

        audioSourceScream.PlayOneShot(audioSourceScream.clip);
        StartCoroutine(HitFeedback());
    }

    IEnumerator HitFeedback() {
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 2f;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 2f;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 2f;
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 2f;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 2f;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 2f;
        yield return new WaitForSeconds(0.2f);
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0f;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0f;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0f;
        cinemachineFreeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0f;
        cinemachineFreeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0f;
        cinemachineFreeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_FrequencyGain = 0f;
    }

    public void Die() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
