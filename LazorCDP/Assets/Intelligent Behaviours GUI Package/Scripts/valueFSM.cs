using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class valueFSM : MonoBehaviour {

    #region variables

    private StateMachineEngine NewFSM_FSM;
    

    private ValuePerception NewTransitionPerception;
    private State NewState;
    private State NewState1;
    
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
        NewTransitionPerception = NewFSM_FSM.CreatePerception<ValuePerception>(() => false /*Replace this with a boolean function*/);
        
        // States
        NewState = NewFSM_FSM.CreateEntryState("New State", NewStateAction);
        NewState1 = NewFSM_FSM.CreateState("New State 1", NewState1Action);
        
        // Transitions
        NewFSM_FSM.CreateTransition("New Transition", NewState, NewTransitionPerception, NewState1);
        
        // ExitPerceptions
        
        // ExitTransitions
        
    }

    // Update is called once per frame
    private void Update()
    {
        NewFSM_FSM.Update();
    }

    // Create your desired actions
    
    private void NewStateAction()
    {
        
    }
    
    private void NewState1Action()
    {
        
    }
    
}