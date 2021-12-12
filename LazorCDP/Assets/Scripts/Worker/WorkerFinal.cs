
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class WorkerFinal : MonoBehaviour, IDamageable<float> {

    #region variables

    delegate void FsmUpdate();

    private FsmUpdate fsmUpdate;
    private StateMachineEngine NewFSM_FSM;
    private UtilitySystemEngine Trabajando_SubUS;


    private PushPerception AlarmaTrabajandoPerception;
    private PushPerception AlarmaDandoLaAlarmaPerception;
    private PushPerception EscaparPerception;
    private PushPerception NoAlarmaPerception;
    private PushPerception Morir;
    private PushPerception MorirHuyendoPerception;
    private PushPerception MorirDandoLaAlarmaPerception;
    private PushPerception JugadorOCadaverVistoPerception;
    private State Huyendo;
    private State DandoLaAlarma;
    private State Muerto;
    private State Escapando;
    private State Trabajando;
    private State Inicio;
    [SerializeField] private float ganasDeOrinar;
    [SerializeField] private float sed;
    [SerializeField] private float cansancio;
    [SerializeField] private float ganasTarea1;
    [SerializeField] private float ganasTarea2;
    bool alarming;
    private float tiempoGanasDeOrinar;
    private float tiempoSed;
    private float tiempoCansacio;
    private float tiempoTarea1;
    private float tiempoTarea2;
    [SerializeField] private Boolean bucle = true;

    [SerializeField] private Boolean checkpoint = true;

    [SerializeField] private Transform baño;
    [SerializeField] private Transform cocina;
    [SerializeField] private Transform descanso;
    [SerializeField] private Transform trabajo1;
    [SerializeField] private Transform trabajo2;
    [SerializeField] private Transform alarma1;
    [SerializeField] private Transform alarma2;
    [SerializeField] private Transform exit1;
    [SerializeField] private Transform exit2;

    [SerializeField] private Animator _animator;

    private List<Ray> visionRays = new List<Ray>();
    [SerializeField] private Transform head;
    private NavMeshAgent _navMeshAgent;

    private EmojiChange emojiChange;

    [SerializeField] private float health = 3;

    public bool isDead;

    private WorldManager worldManager;

    //Place your variables here

    #endregion variables

    private void Awake() {
        emojiChange = GetComponentInChildren<EmojiChange>();
        tiempoGanasDeOrinar = Random.Range(0.2f, 0.5f);
        tiempoSed = Random.Range(0.2f, 0.5f);
        tiempoCansacio = Random.Range(0.5f, 1.0f);
        tiempoTarea1 = Random.Range(1.0f, 2.0f);
        tiempoTarea2 = Random.Range(1.0f, 2.0f);

        worldManager = FindObjectOfType<WorldManager>();
    }

    private void OnEnable() {
        worldManager.onPlayerSeen.AddListener((() => AlarmaTrabajandoPerception.Fire()));
    }

    private void OnDisable() {
        worldManager.onPlayerSeen.RemoveListener((() => AlarmaTrabajandoPerception.Fire()));
    }

    // Start is called before the first frame update
    private void Start() {
        fsmUpdate = TrabajandoAction;

        NewFSM_FSM = new StateMachineEngine(false);
        Trabajando_SubUS = new UtilitySystemEngine(true);
        _navMeshAgent = GetComponent<NavMeshAgent>();

        InitFSM(NewFSM_FSM);
    }


    private void InitFSM(StateMachineEngine NewFSM_FSM) {
        // Perceptions
        // Modify or add new Perceptions, see the guide for more
        AlarmaTrabajandoPerception = NewFSM_FSM.CreatePerception<PushPerception>();
        AlarmaDandoLaAlarmaPerception = NewFSM_FSM.CreatePerception<PushPerception>();
        EscaparPerception = NewFSM_FSM.CreatePerception<PushPerception>();
        NoAlarmaPerception = NewFSM_FSM.CreatePerception<PushPerception>();
        Morir = NewFSM_FSM.CreatePerception<PushPerception>();
        JugadorOCadaverVistoPerception = NewFSM_FSM.CreatePerception<PushPerception>();
        // States
        Huyendo = NewFSM_FSM.CreateState("Huyendo", (() => {
            fsmUpdate = HuyendoAction;

            _animator.SetBool("isWalking", false);
            _animator.SetBool("isRunning", true);
            alarming = false;
        }));

        DandoLaAlarma = NewFSM_FSM.CreateState("DandoLaAlarma", (() => { fsmUpdate = DandoLaAlarmaAction; }));

        Muerto = NewFSM_FSM.CreateState("Muerto", (() => { fsmUpdate = MuertoAction; }));

        Escapando = NewFSM_FSM.CreateState("Escapando", (() => {
            fsmUpdate = EscapandoAction;

            _animator.SetBool("isWalking", false);
            _animator.SetBool("isRunning", true);
            alarming = false;
        }));

        Trabajando = NewFSM_FSM.CreateEntryState("Trabajando", (() => {
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isRunning", false);
            bucle = true;
            fsmUpdate = TrabajandoAction;
        }));


        // Transitions
        NewFSM_FSM.CreateTransition("AlarmaDandoLaAlarma", DandoLaAlarma, AlarmaDandoLaAlarmaPerception, Huyendo);
        NewFSM_FSM.CreateTransition("Escapar", Huyendo, EscaparPerception, Escapando);
        NewFSM_FSM.CreateTransition("NoAlarma", Escapando, NoAlarmaPerception, Trabajando);
        NewFSM_FSM.CreateTransition("MorirHuyendo", Huyendo, Morir, Muerto);
        NewFSM_FSM.CreateTransition("MorirDandoLaAlarma", DandoLaAlarma, Morir, Muerto);
        NewFSM_FSM.CreateTransition("AlarmaTrabajando", Trabajando, AlarmaTrabajandoPerception, Huyendo);
        NewFSM_FSM.CreateTransition("JugadorOCadaverVisto", Trabajando, JugadorOCadaverVistoPerception, DandoLaAlarma);
        NewFSM_FSM.CreateTransition("MorirTrabajando", Trabajando, Morir, Muerto);
    }

    // Update is called once per frame
    private void Update() {

        if (isDead)return;
        fsmUpdate();

        if (health <= 0) {
            Morir.Fire();
            print("Ay");
        }
        
        if (_animator.GetBool("isRunning")) {
            _navMeshAgent.speed = 5;
        }
        else {
            _navMeshAgent.speed = 3;
        }
        
    }

    // Create your desired actions

    private void HuyendoAction() {
        emojiChange.changeEmoji(0);
        if (alarming) return;
        var exitTarget =
            Vector3.Distance(transform.position, exit1.position) < Vector3.Distance(transform.position, exit2.position)
                ? exit1.position
                : exit2.position;
        Debug.Log("Huyendo");
        bucle = false;
        checkpoint = true;
        _animator.SetBool("isWalking", false);
        _animator.SetBool("isRunning", true);
        _navMeshAgent.destination = exitTarget;
        if (Vector3.Distance(transform.position, exitTarget) < 1) {
            EscaparPerception.Fire();
        }
    }

    private void DandoLaAlarmaAction() {
        emojiChange.changeEmoji(0);
        if (alarming) return;
        var alarmTarget =
            Vector3.Distance(transform.position, alarma1.position) <
            Vector3.Distance(transform.position, alarma2.position)
                ? alarma1.position
                : alarma2.position;
        Debug.Log("Dando la alarma");
        bucle = false;
        checkpoint = true;
        _animator.SetBool("isRunning", true);
        _animator.SetBool("isWalking", false);
        _navMeshAgent.destination = alarmTarget;
        if (Vector3.Distance(transform.position, alarmTarget) < 1) {
            print("ALARMANDO");
            StartCoroutine(WaitToAlarm());
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isRunning", false);
        }
    }

    IEnumerator WaitToAlarm() {

        alarming = true;
        _animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(3);
        
            
        worldManager.onPlayerSeen.Invoke();
        print("Me voy");
        AlarmaDandoLaAlarmaPerception.Fire();
    }

    private void MuertoAction() {
        emojiChange.changeEmoji(0);
        Die();
    }

    private void EscapandoAction() {
        emojiChange.changeEmoji(0);
        Debug.Log("Escapando");
        _animator.SetBool("isWalking", false);

        if (worldManager.GuardsChasing || worldManager.guardsCount == 0) return;
        
        NoAlarmaPerception.Fire();

        _animator.SetBool("isRunning", false);
    }

    private void TrabajandoAction() {
        Debug.Log("Trabajando");

        CastRays();
        if (bucle) {
            sed += tiempoSed * Time.deltaTime;
            ganasDeOrinar += tiempoGanasDeOrinar * Time.deltaTime;
            ganasTarea1 += tiempoTarea1 * Time.deltaTime;
            ganasTarea2 += tiempoTarea2 * Time.deltaTime;
            cansancio += tiempoCansacio * Time.deltaTime;
        }

        if (sed > 10 && checkpoint)
            IrALaCafeteriaAction();
        else if (ganasDeOrinar > 10 && checkpoint)
            IrAlBañoAction();
        else if (ganasTarea1 > 10 && checkpoint)
            RealizarTarea1Action();
        else if (ganasTarea2 > 10 && checkpoint)
            RealizarTarea2Action();
        else if (cansancio > 10 && checkpoint)
            IrADescansarAction();
    }

    private void IrAlBañoAction() {
        _animator.SetBool("isWalking", true);
        emojiChange.changeEmoji(4);
        Debug.Log("Ir al baño");
        bucle = false;
        _navMeshAgent.destination = baño.position;
        if (Vector3.Distance(transform.position, baño.position) < 1 && checkpoint) {
            _animator.SetBool("isWalking", false);
            emojiChange.changeEmoji(0);
            checkpoint = false;
            StartCoroutine(WaitToNextMovement());
            ganasDeOrinar = 0;
        }
    }

    private void IrALaCafeteriaAction() {
        _animator.SetBool("isWalking", true);
        emojiChange.changeEmoji(1);
        Debug.Log("Ir a la cafeteria");
        bucle = false;
        _navMeshAgent.destination = cocina.position;
        if (Vector3.Distance(transform.position, cocina.position) < 1 && checkpoint) {
            _animator.SetBool("isWalking", false);
            emojiChange.changeEmoji(0);
            checkpoint = false;
            StartCoroutine(WaitToNextMovement());
            sed = 0;
        }
    }

    private void IrADescansarAction() {
        Debug.Log("Ir a descansar");
        emojiChange.changeEmoji(2);
        _animator.SetBool("isWalking", true);
        bucle = false;
        _navMeshAgent.destination = descanso.position;
        if (Vector3.Distance(transform.position, descanso.position) < 1 && checkpoint) {
            _animator.SetBool("isWalking", false);
            emojiChange.changeEmoji(0);
            checkpoint = false;
            StartCoroutine(WaitToNextMovement());
            cansancio = 0;
        }
    }

    private void RealizarTarea1Action() {
        _animator.SetBool("isWalking", true);
        emojiChange.changeEmoji(3);
        bucle = false;
        Debug.Log("Realizar Tarea 1");
        _navMeshAgent.destination = trabajo1.position;
        if (Vector3.Distance(transform.position, trabajo1.position) < 1 && checkpoint) {
            _animator.SetBool("isWalking", false);
            emojiChange.changeEmoji(0);
            checkpoint = false;
            StartCoroutine(WaitToNextMovement());
            ganasTarea1 = 0;
        }
    }

    private void RealizarTarea2Action() {
        _animator.SetBool("isWalking", true);
        emojiChange.changeEmoji(3);
        bucle = false;
        Debug.Log("Realizar tarea 2");
        _navMeshAgent.destination = trabajo2.position;
        if (Vector3.Distance(transform.position, trabajo2.position) < 1 && checkpoint) {
            _animator.SetBool("isWalking", false);
            emojiChange.changeEmoji(0);
            checkpoint = false;
            StartCoroutine(WaitToNextMovement());
            ganasTarea2 = 0;
        }
    }

    IEnumerator WaitToNextMovement() {
        var waitTime = Random.Range(3, 9);
        yield return new WaitForSeconds(waitTime);
        bucle = true;
        checkpoint = true;
    }

    void CastRays() {
        visionRays.Clear();
        visionRays.Add(new Ray(head.position, transform.TransformDirection(Vector3.forward)));
        visionRays.Add(new Ray(head.position, transform.TransformDirection(Vector3.forward + Vector3.left * 0.5f)));
        visionRays.Add(new Ray(head.position, transform.TransformDirection(Vector3.forward + Vector3.right * 0.5f)));

        RaycastHit hit;

        foreach (var ray in visionRays) {
            if (Physics.Raycast(ray, out hit, 5)) {
                if (hit.collider.CompareTag("Player")) {
                    JugadorOCadaverVistoPerception.Fire();
                    _animator.SetBool("isRunning", true);
                }
            }
        }
    }

    void OnDrawGizmosSelected() {
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.red;
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 5;
        Gizmos.DrawRay(head.position, direction);
        direction = transform.TransformDirection(Vector3.forward + Vector3.left * 0.5f) * 5;
        Gizmos.DrawRay(head.position, direction);
        direction = transform.TransformDirection(Vector3.forward + Vector3.right * 0.5f) * 5;
        Gizmos.DrawRay(head.position, direction);
    }

    public void TakeDamage(float damage) {
        health -= damage;
        _animator.SetBool("isWalking", false);
        _animator.SetBool("isRunning", true);
        if (NewFSM_FSM.GetCurrentState() == Huyendo || NewFSM_FSM.GetCurrentState() == DandoLaAlarma) return;
        JugadorOCadaverVistoPerception.Fire();
    }

    public void Die() {
        isDead = true;
        _navMeshAgent.enabled = false;
        _animator.SetBool("isDead", true);

    }

    public void Oido() {
        JugadorOCadaverVistoPerception.Fire();
    }

}