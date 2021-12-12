using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class testFSM : MonoBehaviour {

    #region variables

    private StateMachineEngine NewFSM_FSM;
    

    private PushPerception saludoPerception;
    private State hola;
    private State adios;
    
    //Place your variables here

    #endregion variables

    // Start is called before the first frame update
    private void Start()
    {
        NewFSM_FSM = new StateMachineEngine(false);
        

        CreateStateMachine();
    }
    
    
    private void CreateStateMachine()
    {
        // Perceptions
        // Modify or add new Perceptions, see the guide for more
        saludoPerception = NewFSM_FSM.CreatePerception<PushPerception>();
        
        // States
        hola = NewFSM_FSM.CreateEntryState("hola", holaAction);
        adios = NewFSM_FSM.CreateState("adios", adiosAction);
        
        // Transitions
        NewFSM_FSM.CreateTransition("saludo", hola, saludoPerception, adios);
        
        // ExitPerceptions
        
        // ExitTransitions
        
    }

    // Update is called once per frame
    private void Update()
    {
        NewFSM_FSM.Update();
    }

    // Create your desired actions
    
    private void holaAction()
    {
        
    }
    
    private void adiosAction()
    {
        
    }
    
}