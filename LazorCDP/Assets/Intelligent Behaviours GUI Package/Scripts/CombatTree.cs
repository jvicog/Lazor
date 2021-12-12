using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CombatTree : MonoBehaviour {

    #region variables

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
    
    //Place your variables here

    #endregion variables

    // Start is called before the first frame update
    private void Start()
    {
        CombatTree_BT = new BehaviourTreeEngine(false);
        

        CreateBehaviourTree();
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

    // Update is called once per frame
    private void Update()
    {
        CombatTree_BT.Update();
    }

    // Create your desired actions
    
    private void LowHealthAction()
    {
        
    }
    
    private ReturnValues LowHealthSuccessCheck()
    {
        //Write here the code for the success check for LowHealth
        return ReturnValues.Failed;
    }
    
    private void HascuresAction()
    {
        
    }
    
    private ReturnValues HascuresSuccessCheck()
    {
        //Write here the code for the success check for Hascures
        return ReturnValues.Failed;
    }
    
    private void HealAction()
    {
        
    }
    
    private ReturnValues HealSuccessCheck()
    {
        //Write here the code for the success check for Heal
        return ReturnValues.Failed;
    }
    
    private void ScapeAction()
    {
        
    }
    
    private ReturnValues ScapeSuccessCheck()
    {
        //Write here the code for the success check for Scape
        return ReturnValues.Failed;
    }
    
    private void ChasePlayerAction()
    {
        
    }
    
    private ReturnValues ChasePlayerSuccessCheck()
    {
        //Write here the code for the success check for ChasePlayer
        return ReturnValues.Failed;
    }
    
    private void ReloadAction()
    {
        
    }
    
    private ReturnValues ReloadSuccessCheck()
    {
        //Write here the code for the success check for Reload
        return ReturnValues.Failed;
    }
    
    private void IsplayerseenAction()
    {
        
    }
    
    private ReturnValues IsplayerseenSuccessCheck()
    {
        //Write here the code for the success check for Isplayerseen
        return ReturnValues.Failed;
    }
    
    private void ShootAction()
    {
        
    }
    
    private ReturnValues ShootSuccessCheck()
    {
        //Write here the code for the success check for Shoot
        return ReturnValues.Failed;
    }
    
    private void LowAmmoAction()
    {
        
    }
    
    private ReturnValues LowAmmoSuccessCheck()
    {
        //Write here the code for the success check for LowAmmo
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
    
}