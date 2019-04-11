using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiTatu_alerted1 : AITatuState
{
    // Inspector Assigned
    [SerializeField] [Range(1, 60)] float _maxDuration = 10.0f;
    [SerializeField] float _waypointAngleThreshold = 90.0f;
    [SerializeField] float _threatAngleThreshold = 10.0f;
    [SerializeField] float _directionChangeTime = 1.5f;

    // Private Fields
    float _timer = 0.0f;
    float _directionChangeTimer = 0.0f;

    // ------------------------------------------------------------------
    // Name	:	GetStateType
    // Desc	:	Returns the type of the state
    // ------------------------------------------------------------------
    public override AIStateType GetStateType()
    {
        return AIStateType.Alerted;
    }

    // ------------------------------------------------------------------
    // Name	:	OnEnterState
    // Desc	:	Called by the State Machine when first transitioned into
    //			this state. It initializes a timer and configures the
    //			the state machine
    // ------------------------------------------------------------------
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_tatuStateMachine == null)
            return;

        // Configure State Machine
        _tatuStateMachine.NavAgentControl(true, false);
        _tatuStateMachine.speed = 0;
        _tatuStateMachine.seeking = 0;

        _timer = _maxDuration;
        _directionChangeTimer = 0.0f;
    }

    // ---------------------------------------------------------------------
    // Name	:	OnUpdate
    // Desc	:	The engine of this state
    // ---------------------------------------------------------------------
    public override AIStateType OnUpdate()
    {
        // Reduce Timer
        _timer -= Time.deltaTime;
        _directionChangeTimer += Time.deltaTime;

        // Transition into a patrol state if available
        if (_timer <= 0.0f)
        {
            if (_tatuStateMachine.navAgent.enabled)
            {
                _tatuStateMachine.navAgent.SetDestination(_tatuStateMachine.GetWaypointPosition(false));
                _tatuStateMachine.navAgent.isStopped = false;
            }
            _timer = _maxDuration;
        }

        // Do we have a visual threat that is the player. These take priority over audio threats
        if (_tatuStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _tatuStateMachine.SetTarget(_tatuStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        if (_tatuStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _tatuStateMachine.SetTarget(_tatuStateMachine.AudioThreat);
            _timer = _maxDuration;
        }

        if (_tatuStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            Debug.Log("setting visual threat");
            _tatuStateMachine.SetTarget(_tatuStateMachine.VisualThreat);
            _timer = _maxDuration;
        }

        if (_tatuStateMachine.AudioThreat.type == AITargetType.None &&
            _tatuStateMachine.VisualThreat.type == AITargetType.Visual_Food &&
            _tatuStateMachine.targetType == AITargetType.None)
        {
            _tatuStateMachine.SetTarget(_stateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }

        float angle;

        if ((_tatuStateMachine.targetType == AITargetType.Audio || _tatuStateMachine.targetType == AITargetType.Visual_Light) && !_tatuStateMachine.isTargetReached)
        {
            angle = AIState.FindSignedAngle(_tatuStateMachine.transform.forward,
                                            _tatuStateMachine.targetPosition - _tatuStateMachine.transform.position);

            if (_tatuStateMachine.targetType == AITargetType.Audio && Mathf.Abs(angle) < _threatAngleThreshold)
            {
                return AIStateType.Pursuit;
            }

            if (_directionChangeTimer > _directionChangeTime)
            {
                if (Random.value < _tatuStateMachine.intelligence)
                {
                    _tatuStateMachine.seeking = (int)Mathf.Sign(angle);
                }
                else
                {
                    _tatuStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                }

                _directionChangeTimer = 0.0f;
            }
        }

        else

        if (_tatuStateMachine.targetType == AITargetType.Waypoint && !_tatuStateMachine.navAgent.pathPending)
        {
            angle = AIState.FindSignedAngle(_tatuStateMachine.transform.forward,
                                            _tatuStateMachine.navAgent.steeringTarget - _tatuStateMachine.transform.position);

            if (Mathf.Abs(angle) < _waypointAngleThreshold) return AIStateType.Patrol;


            if (_directionChangeTimer > _directionChangeTime)
            {
                _tatuStateMachine.seeking = (int)Mathf.Sign(angle);
                _directionChangeTimer = 0.0f;
            }
        }
        else
        {
            if (_directionChangeTimer > _directionChangeTime)
            {
                _tatuStateMachine.seeking = (int)Mathf.Sign(Random.Range(-1.0f, 1.0f));
                _directionChangeTimer = 0.0f;
            }
        }

        if (_directionChangeTimer > _directionChangeTime - 0.50f)
        {
            _tatuStateMachine.seeking = 0;
        }

        return AIStateType.Alerted;
    }
}
