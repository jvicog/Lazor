
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardBehaviour : MonoBehaviour, IDamageable<float>
{
    // States Updates
    delegate void FsmUpdate();
    private FsmUpdate fsmUpdate;
    
    // State Machine Parameters
    #region StateMachineSetUp
        public StateMachineEngine guardStateMachine;
    
        private State patrolling;
        public State combat;
        private State searching;
        private State dead;
        private State scaped;

        private Perception playerSeenPerception;
        private Perception guardKilledPerception;
        private Perception scapedPerception;
        private Perception searchPerception;
        private Perception notSeenInSearch;
    #endregion
    
    // Combat Tree Behaviour

    #region CombatTreeSetUp

        private BehaviourTreeEngine CombatTree_BT;
    

        private SelectorNode NewSelectorNode;
        private SequenceNode NewSequenceNode;
        private LeafNode LowHealth;
        private SelectorNode NewSelectorNode1;
        private SequenceNode NewSequenceNode2;
        private LeafNode Hascures;
        private SequenceNode NewSequenceNode3;
        private LeafNode Heal;
        private LeafNode Scape;
        private SequenceNode NewSequenceNode1;
        private LeafNode ChasePlayer;
        private LeafNode Reload;
        private LeafNode Isplayerseen;
        private SelectorNode NewSelectorNode2;
        private SequenceNode NewSequenceNode4;
        private LeafNode Shoot;
        private LeafNode LowAmmo;
        private LeafNode Reload1;
        private LoopDecoratorNode LoopN_NewSelectorNode;

    #endregion
    
    // NavMesh
    private NavMeshAgent _navMeshAgent;
    
    // Animator
    [SerializeField] private Animator _animator;
    
    // Patrol
    [SerializeField] private List<Transform> patrolCheckPoints;
    private int currentCheckpoint = 0;
    private bool checkPointReached;
    private IEnumerator waitingCoroutine;
    
    // Vision
    private List<Ray> visionRays = new List<Ray>();
    [SerializeField] private Transform head;
    [SerializeField] private CameraRayCaster raycaster;
    private GameObject player;
    
    // Status
    [SerializeField] private float health = 5;
    [SerializeField] private int cures = 1;
    [SerializeField] private int bulletsAvailable = 6;
    private bool needToScape;
    private bool injured;
    private bool canShoot = true;
    private float shootCoolDown = 5;
    private float timeNotSeen;
    private float searchingTime;
    
    
    // World Stuff
    private List<Transform> healingPlaces = new List<Transform>();
    private List<Transform> exitPlaces = new List<Transform>();
    private WorldManager worldManager;

    [SerializeField] private GameObject shootEffect;


    private void Awake() {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        
        var aux = FindObjectsOfType<HealingPlace>();

        foreach (var h in aux) {
            healingPlaces.Add(h.transform);
        }
        
        var aux2 = FindObjectsOfType<ExitPoint>();

        foreach (var e in aux2) {
            exitPlaces.Add(e.transform);
        }

        fsmUpdate = PatrolUpdate;
        
        player = FindObjectOfType<PlayerController>().gameObject;

        guardStateMachine = new StateMachineEngine();
        InitFSM(guardStateMachine);
        
        CombatTree_BT = new BehaviourTreeEngine(false);
        CreateBehaviourTree();

        worldManager = FindObjectOfType<WorldManager>();
    }

    private void OnEnable() {
        worldManager.onPlayerSeen.AddListener(()=>playerSeenPerception.Fire());
    }

    private void OnDisable() {
        worldManager.onPlayerSeen.RemoveListener(()=>playerSeenPerception.Fire());
    }

    private void Update() {

        fsmUpdate();
    }

    #region Patrolling

    void PatrolUpdate() {
        if (Vector3.Distance(transform.position, patrolCheckPoints[currentCheckpoint].position) < 1) {
            checkPointReached = true;
            _navMeshAgent.speed = 1;
            currentCheckpoint = (currentCheckpoint+1) % patrolCheckPoints.Count;
            waitingCoroutine = WaitToNextMovement();
            StartCoroutine(waitingCoroutine);
        }
        
        if (!checkPointReached) {
            _navMeshAgent.destination = patrolCheckPoints[currentCheckpoint].position;
        }
    }

    IEnumerator WaitToNextMovement() {
        var waitTime = Random.Range(3, 9);
        _animator.SetBool("isWalking", false);
        _navMeshAgent.enabled = false;
        yield return new WaitForSeconds(waitTime);
        _navMeshAgent.enabled = true;
        _animator.SetBool("isWalking", true);
        checkPointReached = false;
    }

    void SearchingUpdate() {
        PatrolUpdate();
        searchingTime += Time.deltaTime;

        if (searchingTime > 15) {
            notSeenInSearch.Fire();
            _animator.SetBool("isCombat", false);
        }
    }

    public void PlayerDetection(Transform p) {
        if (raycaster.LaunchRays(p, 30)) {
            playerSeenPerception.Fire();
            print("Te vi");
            worldManager.onPlayerSeen.Invoke();
        }
    }
    
    void InitFSM(StateMachineEngine fsm) {
        patrolling = fsm.CreateEntryState("Patrolling", (() => {
            fsmUpdate = PatrolUpdate;
            _navMeshAgent.enabled = true;
            _animator.SetBool("isCombat", false);
            _navMeshAgent.destination = patrolCheckPoints[0].position;
        }));

        combat = fsm.CreateState("Combat", () => {
            fsmUpdate = CombatUpdate;
            transform.DOLookAt(player.transform.position, 0.2f, AxisConstraint.Y);
            _animator.SetBool("isCombat", true);
            _navMeshAgent.speed = 3;
            _navMeshAgent.enabled = true;
            searchingTime = 0;
            timeNotSeen = 0;
            
            if(waitingCoroutine!= null)
                StopCoroutine(waitingCoroutine);
        });

        dead = fsm.CreateState("Dead", (() => {
            Die();
        }));
        
        searching = fsm.CreateState("Searching", (() => {
            fsmUpdate = SearchingUpdate;
            _animator.SetBool("isWalking", true);
            _navMeshAgent.enabled = true;
            _navMeshAgent.destination = patrolCheckPoints[0].position;
            currentCheckpoint = 0;
        }));

        scaped = fsm.CreateState("Escape", () => {
            Destroy(gameObject);
        });
        
        playerSeenPerception = fsm.CreatePerception<PushPerception>();
        guardKilledPerception = fsm.CreatePerception<PushPerception>();
        scapedPerception = fsm.CreatePerception<PushPerception>();
        searchPerception = fsm.CreatePerception<PushPerception>();
        notSeenInSearch = fsm.CreatePerception<PushPerception>();

        fsm.CreateTransition("patrolToCombat", patrolling,
            playerSeenPerception, combat);

        fsm.CreateTransition("patrolToDeath", patrolling,
            guardKilledPerception, dead);
        fsm.CreateTransition("combatToDeath", combat,
            guardKilledPerception, dead);
        fsm.CreateTransition("escape", combat, scapedPerception, scaped);
        fsm.CreateTransition("search", combat, searchPerception, searching);
        fsm.CreateTransition("searchToCombat", searching,
            playerSeenPerception, combat);
        fsm.CreateTransition("searchToDeath", searching,
            guardKilledPerception, dead);
        fsm.CreateTransition("searchToPatrol", searching, notSeenInSearch, patrolling);

    }

    #endregion

    #region Combat
    void CombatUpdate() {
        if (!injured) {
            
            if (Vector3.Distance(transform.position, player.transform.position) < 4) {
                _navMeshAgent.enabled = false;
                _animator.SetBool("isWalking", false);
            }

            if (!raycaster.LaunchRays(player.transform, 30)) {
                timeNotSeen += Time.deltaTime;
            }
            else {
                timeNotSeen = 0;
            }

            if (timeNotSeen > 10) {
                searchPerception.Fire();
            }
        }
            
        
            
        CombatTree_BT.Update();
    }
    
     private void CreateBehaviourTree()
    {
        // Nodes
        NewSelectorNode = CombatTree_BT.CreateSelectorNode("New Selector Node");
        NewSequenceNode = CombatTree_BT.CreateSequenceNode("New Sequence Node", false);
        LowHealth = CombatTree_BT.CreateLeafNode("Low Health?", LowHealthAction, LowHealthSuccessCheck);
        NewSelectorNode1 = CombatTree_BT.CreateSelectorNode("New Selector Node 1");
        NewSequenceNode2 = CombatTree_BT.CreateSequenceNode("New Sequence Node 2", false);
        Hascures = CombatTree_BT.CreateLeafNode("Has cures?", HascuresAction, HascuresSuccessCheck);
        NewSequenceNode3 = CombatTree_BT.CreateSequenceNode("New Sequence Node 3", false);
        Heal = CombatTree_BT.CreateLeafNode("Heal", HealAction, HealSuccessCheck);
        Scape = CombatTree_BT.CreateLeafNode("Scape", ScapeAction, ScapeSuccessCheck);
        NewSequenceNode1 = CombatTree_BT.CreateSequenceNode("New Sequence Node 1", false);
        ChasePlayer = CombatTree_BT.CreateLeafNode("Chase Player", ChasePlayerAction, ChasePlayerSuccessCheck);
        Reload = CombatTree_BT.CreateLeafNode("Reload", ReloadAction, ReloadSuccessCheck);
        Isplayerseen = CombatTree_BT.CreateLeafNode("Is player seen?", IsplayerseenAction, IsplayerseenSuccessCheck);
        NewSelectorNode2 = CombatTree_BT.CreateSelectorNode("New Selector Node 2");
        NewSequenceNode4 = CombatTree_BT.CreateSequenceNode("New Sequence Node 4", false);
        Shoot = CombatTree_BT.CreateLeafNode("Shoot", ShootAction, ShootSuccessCheck);
        LowAmmo = CombatTree_BT.CreateLeafNode("Low Ammo?", LowAmmoAction, LowAmmoSuccessCheck);
        Reload1 = CombatTree_BT.CreateLeafNode("Reload 1", Reload1Action, Reload1SuccessCheck);
        LoopN_NewSelectorNode = CombatTree_BT.CreateLoopNode("LoopN_NewSelectorNode", NewSelectorNode);
        
        // Child adding
        NewSelectorNode.AddChild(NewSequenceNode);
        NewSelectorNode.AddChild(NewSequenceNode1);
        NewSelectorNode.AddChild(ChasePlayer);
        
        NewSequenceNode.AddChild(LowHealth);
        NewSequenceNode.AddChild(NewSelectorNode1);
        
        NewSelectorNode1.AddChild(NewSequenceNode2);
        NewSelectorNode1.AddChild(Scape);
        
        NewSequenceNode2.AddChild(Hascures);
        NewSequenceNode2.AddChild(NewSequenceNode3);
        
        NewSequenceNode3.AddChild(Heal);
        NewSequenceNode3.AddChild(Reload);
        
        NewSequenceNode1.AddChild(Isplayerseen);
        NewSequenceNode1.AddChild(NewSelectorNode2);
        
        NewSelectorNode2.AddChild(NewSequenceNode4);
        NewSelectorNode2.AddChild(Shoot);
        
        NewSequenceNode4.AddChild(LowAmmo);
        NewSequenceNode4.AddChild(Reload1);
        
        // SetRoot
        CombatTree_BT.SetRootNode(LoopN_NewSelectorNode);
        
        // ExitPerceptions
        
        // ExitTransitions
        
    }
     public void TakeDamage(float damage) {
        health -= damage;
        
        
        if (health <= 0) {
            guardKilledPerception.Fire();
        }
        else {
            playerSeenPerception.Fire();
            worldManager.onPlayerSeen.Invoke();
        }
    }

    public void Die() {
        _animator.SetBool("isDead", true);
        _navMeshAgent.enabled = false;
        worldManager.guardsCount--;
        StartCoroutine(Disapear());
    }

    IEnumerator Disapear() {
        yield return new WaitForSeconds(4);
        Destroy(gameObject);
    }
    
     // Create your desired actions
    
    private void LowHealthAction()
    {
        
    }
    
    private ReturnValues LowHealthSuccessCheck()
    {
        if (health <= 2) {
            injured = true;
            _animator.SetBool("isInjured", true);
            return ReturnValues.Succeed;
        }
        return ReturnValues.Failed;
    }
    
    private void HascuresAction()
    {
        
    }
    
    private ReturnValues HascuresSuccessCheck()
    {
        if (cures > 0) {
            return ReturnValues.Succeed;
        }

        needToScape = true;
        return ReturnValues.Failed;
    }
    
    private void HealAction() {
        
    }
    
    private ReturnValues HealSuccessCheck() {
        var target = Vector3.Distance(healingPlaces[0].position, transform.position) < Vector3.Distance(healingPlaces[1].position, transform.position)?
            healingPlaces[0].position : healingPlaces[1].position;
        _navMeshAgent.enabled = true;
        _navMeshAgent.SetDestination(target);
        _animator.SetBool("isWalking", true);
        if (Vector3.Distance(transform.position, target) < 2) {
            health = 5;
            cures -= 1;
            injured = false;
            _animator.SetBool("isWalking", false);
            _animator.SetBool("isInjured", false);
            return ReturnValues.Succeed;
        }

        return ReturnValues.Running;
    }
    
    private void ScapeAction() {
        print("Mescapo");
        if (needToScape) {
            if (health <= 0) return;
            var target = Vector3.Distance(exitPlaces[0].position, transform.position) < Vector3.Distance(exitPlaces[1].position, transform.position)?
            exitPlaces[0].position : exitPlaces[1].position;
            _navMeshAgent.enabled = true;
            _navMeshAgent.SetDestination(target);
        }
        
        
            
    }
    
    private ReturnValues ScapeSuccessCheck()
    {
        if (Vector3.Distance(transform.position, exitPlaces[0].position) < 1) {
            scapedPerception.Fire();
        }
        
        return ReturnValues.Succeed;
    }
    
    private void ChasePlayerAction() {
        if (Vector3.Distance(transform.position, player.transform.position) > 1) {
            _navMeshAgent.enabled = true;
            
            
            _navMeshAgent.SetDestination(player.transform.position);
            _animator.SetBool("isWalking", true);
        }
        else {
            _navMeshAgent.enabled = false;
            _animator.SetBool("isWalking", false);
        }
        
    }
    
    private ReturnValues ChasePlayerSuccessCheck()
    {
        //Write here the code for the success check for ChasePlayer
        return ReturnValues.Succeed;
    }
    
    private void ReloadAction() {
    }
    
    private ReturnValues ReloadSuccessCheck()
    {
        print("Reload");
        bulletsAvailable = 6;
        return ReturnValues.Succeed;
    }
    
    private void IsplayerseenAction()
    {
        
    }
    
    private ReturnValues IsplayerseenSuccessCheck()
    {
        if (raycaster.LaunchRays(player.transform, 10)) {
            print("Vistaaa");
            if(Vector3.Distance(transform.position, player.transform.position) < 5)
            {
                _navMeshAgent.enabled = false;
                _animator.SetBool("isWalking", false);
            }else
            {
                _animator.SetBool("isWalking", true);
                _navMeshAgent.enabled = true;
                _navMeshAgent.SetDestination(player.transform.position);
            }
            
            return ReturnValues.Succeed;
        }
        
        print("Noteveo uwu");
        return ReturnValues.Failed;
    }
    
    private void ShootAction() {
        transform.DOLookAt(player.transform.position, 0.2f, AxisConstraint.Y);
    }
    
    private ReturnValues ShootSuccessCheck()
    {
        
        if (canShoot && Vector3.Distance(transform.position, player.transform.position) < 5) {
            _navMeshAgent.enabled = false;
            _animator.SetBool("isWalking", false);
            bulletsAvailable -= 1;
            StartCoroutine(CanShootCoroutine(shootCoolDown));
            shootEffect.SetActive(true);
            var random = Random.Range(0, 10);

           if (random > 3) {
                player.GetComponent<PlayerController>().TakeDamage(1);
           }
           
        }
        
        return ReturnValues.Succeed;
    }

    IEnumerator CanShootCoroutine(float t) {
        canShoot = false;
        yield return new WaitForSeconds(t);
        canShoot = true;
    }


    private void LowAmmoAction()
    {
        
    }
    
    private ReturnValues LowAmmoSuccessCheck()
    {
        if (bulletsAvailable <= 1) {
            bulletsAvailable = 6;
            return ReturnValues.Succeed;
        }

        return ReturnValues.Failed;
    }
    
    private void Reload1Action()
    {
        
    }
    
    private ReturnValues Reload1SuccessCheck()
    {
        //Write here the code for the success check for Reload1
        return ReturnValues.Failed;
    }
    #endregion

    public void CorpseSeen() {
        worldManager.onPlayerSeen.Invoke();
        
    }
}
