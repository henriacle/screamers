using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootMotionConfigurator : AIStateMachineLink
{
    // Inspector Assigned Reference Incrementing Variables
    [SerializeField] private int _rootPosition = 0;
    [SerializeField] private int _rootRotation = 0;

    // --------------------------------------------------------
    // Name	:	OnStateEnter
    // Desc	:	Called prior to the first frame the
    //			animation assigned to this state.
    // --------------------------------------------------------
    override public void OnStateEnter(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        // Request the enabling/disabling of root motion for this animation state 
        if (_stateMachine)
            _stateMachine.AddRootMotionRequest(_rootPosition, _rootRotation);
    }

    // --------------------------------------------------------
    // Name	:	OnStateExit
    // Desc	:	Called on the last frame of the animation prior
    //			to leaving the state.
    // --------------------------------------------------------
    override public void OnStateExit(Animator animator, AnimatorStateInfo animStateInfo, int layerIndex)
    {
        // Inform the AI State Machine that we wish to relinquish our root motion request.
        if (_stateMachine)
            _stateMachine.AddRootMotionRequest(-_rootPosition, -_rootRotation);
    }
}
