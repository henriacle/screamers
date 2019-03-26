using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIHumanRobot_Patrol1 : AIHumanRobotState
{
    // Inpsector Assigned 
    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    [SerializeField] [Range(0.0f, 3.0f)] float _speed = 1.0f;

    // ------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Called by parent State Machine to get this state's
    //			type.
    // ------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It initializes the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State");
        base.OnEnterState();
        if (_humanRobotStateMachine == null)
            return;

        // Configure State Machine
        _humanRobotStateMachine.NavAgentControl(true, false);
        _humanRobotStateMachine.speed = _speed;
        _humanRobotStateMachine.seeking = 0;
        _humanRobotStateMachine.firing = false;
        _humanRobotStateMachine.reloading = false;
        _humanRobotStateMachine.attackType = 0;

        // Set Destination
        _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.GetWaypointPosition(false));

        // Make sure NavAgent is switched on
        _humanRobotStateMachine.navAgent.isStopped = false;
    }


    // ------------------------------------------------------------
    // Name	:	OnUpdate
    // Desc	:	Called by the state machine each frame to give this
    //			state a time-slice to update itself. It processes 
    //			threats and handles transitions as well as keeping
    //			the zombie aligned with its proper direction in the
    //			case where root rotation isn't being used.
    // ------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // Do we have a visual threat that is the player
        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }

        // Sound is the third highest priority
        if (_humanRobotStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        /* We have seen a dead body so lets pursue that if we are hungry enough
        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Food)
        {
            // If the distance to hunger ratio means we are hungry enough to stray off the path that far
            if ((1.0f - _humanRobotStateMachine.satisfaction) > (_humanRobotStateMachine.VisualThreat.distance / _humanRobotStateMachine.sensorRadius))
            {
                _stateMachine.SetTarget(_stateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }*/

        // Calculate angle we need to turn through to be facing our target
        float angle = Vector3.Angle(_humanRobotStateMachine.transform.forward, (_humanRobotStateMachine.navAgent.steeringTarget - _humanRobotStateMachine.transform.position));

        // If its too big then drop out of Patrol and into Altered
        if (angle > _turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        // If root rotation is not being used then we are responsible for keeping zombie rotated
        // and facing in the right direction. 
        if (!_humanRobotStateMachine.useRootRotation)
        {
            // Generate a new Quaternion representing the rotation we should have
            if (_humanRobotStateMachine.navAgent.velocity != Vector3.zero)
            {
                Quaternion newRot = Quaternion.LookRotation(_humanRobotStateMachine.navAgent.desiredVelocity);
                // Smoothly rotate to that new rotation over time
                _humanRobotStateMachine.transform.rotation = Quaternion.Slerp(_humanRobotStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            }
        }

        // If for any reason the nav agent has lost its path then call the NextWaypoint function
        // so a new waypoint is selected and a new path assigned to the nav agent.
        if (_humanRobotStateMachine.navAgent.isPathStale ||
            !_humanRobotStateMachine.navAgent.hasPath ||
            _humanRobotStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.GetWaypointPosition(true));
        }


        // Stay in Patrol State
        return AIStateType.Patrol;
    }



    // ----------------------------------------------------------------------
    // Name	:	OnDestinationReached
    // Desc	:	Called by the parent StateMachine when the zombie has reached
    //			its target (entered its target trigger
    // ----------------------------------------------------------------------
    public override void OnDestinationReached(bool isReached)
    {
        // Only interesting in processing arricals not departures
        if (_humanRobotStateMachine == null || !isReached)
            return;

        // Select the next waypoint in the waypoint network
        if (_humanRobotStateMachine.targetType == AITargetType.Waypoint)
            _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.GetWaypointPosition(true));
    }

    // -----------------------------------------------------------------------
    // Name	:	OnAnimatorIKUpdated
    // Desc	:	Override IK Goals
    // -----------------------------------------------------------------------
    /*public override void 		OnAnimatorIKUpdated()	
	{
		if (_humanRobotStateMachine == null)
			return;

		_humanRobotStateMachine.animator.SetLookAtPosition ( _humanRobotStateMachine.targetPosition + Vector3.up );
		_humanRobotStateMachine.animator.SetLookAtWeight (0.55f );
	}*/
}
