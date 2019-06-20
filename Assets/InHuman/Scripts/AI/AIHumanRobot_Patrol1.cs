using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIHumanRobot_Patrol1 : AIHumanRobotState
{
    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;

    [SerializeField] [Range(0.0f, 3.0f)] float _speed = 1.0f;

    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering Patrol State");
        base.OnEnterState();
        if (_humanRobotStateMachine == null)
            return;

        _humanRobotStateMachine.NavAgentControl(true, false);
        _humanRobotStateMachine.speed = _speed;
        _humanRobotStateMachine.seeking = 0;
        _humanRobotStateMachine.firing = false;
        _humanRobotStateMachine.reloading = false;
        _humanRobotStateMachine.attackType = 0;

        if(_humanRobotStateMachine.navAgent.enabled)
        {
            _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.GetWaypointPosition(false));

            _humanRobotStateMachine.navAgent.isStopped = false;
        }
    }

    public override AIStateType OnUpdate()
    {
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

        if (_humanRobotStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }

        float angle = Vector3.Angle(_humanRobotStateMachine.transform.forward, (_humanRobotStateMachine.navAgent.steeringTarget - _humanRobotStateMachine.transform.position));

        if (angle > _turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        if (!_humanRobotStateMachine.useRootRotation)
        {
            if (_humanRobotStateMachine.navAgent.desiredVelocity != Vector3.zero)
            {
                Quaternion newRot = Quaternion.LookRotation(_humanRobotStateMachine.navAgent.desiredVelocity);
                _humanRobotStateMachine.transform.rotation = Quaternion.Slerp(_humanRobotStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            }
        }

        if(_humanRobotStateMachine.navAgent.enabled)
        {
            if (_humanRobotStateMachine.navAgent.isPathStale ||
                !_humanRobotStateMachine.navAgent.hasPath ||
                _humanRobotStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.GetWaypointPosition(true));
            }
        }


        return AIStateType.Patrol;
    }



    public override void OnDestinationReached(bool isReached)
    {
        if (_humanRobotStateMachine == null || !isReached)
            return;

        if (_humanRobotStateMachine.targetType == AITargetType.Waypoint)
            _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.GetWaypointPosition(true));
    }

}
