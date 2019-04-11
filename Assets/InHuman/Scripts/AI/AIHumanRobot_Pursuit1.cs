using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIHumanRobot_Pursuit1 : AIHumanRobotState
{
    [SerializeField] [Range(0, 10)]         private float _speed = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float         _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)]   float         _lookAtAngleThreshold = 15.0f;
    [SerializeField] private float _slerpSpeed = 5.0f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _repathVisualMaxDuration = 5.0f;
    [SerializeField] private float _repathAudioMinDuration = 0.25f;
    [SerializeField] private float _repathAudioMaxDuration = 5.0f;
    [SerializeField] private float _maxDuration = 40.0f;

    // Private Fields
    private float _timer = 0.0f;
    private float _repathTimer = 0.0f;
    private float _currentLookAtWeight = 0.0f;

    // Mandatory Overrides
    public override AIStateType GetStateType() { return AIStateType.Pursuit; }

    // Default Handlers
    public override void OnEnterState()
    {
        Debug.Log("Entering Pursuit state");
        base.OnEnterState();
        if (_humanRobotStateMachine == null)
            return;

        _humanRobotStateMachine.NavAgentControl(true, false);
        _humanRobotStateMachine.seeking     = 0;
        _humanRobotStateMachine.firing      = false;
        _humanRobotStateMachine.reloading   = false;
        _humanRobotStateMachine.attackType  = 0;
        _currentLookAtWeight                = 0.0f;

        _timer = 0.0f;
        _repathTimer = 0.0f;


        if(_humanRobotStateMachine.navAgent.enabled)
        {
            _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.targetPosition);
            _humanRobotStateMachine.navAgent.isStopped = false;
        }

    }

    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer > _maxDuration)
            return AIStateType.Patrol;

        if (_stateMachine.targetType == AITargetType.Visual_Player && _humanRobotStateMachine.inMeleeRange)
        {
            return AIStateType.Attack;
        }

        if (_stateMachine.targetType == AITargetType.Visual_Player && _humanRobotStateMachine.inFireRange && _humanRobotStateMachine.weaponType != EnemyWeaponType.UNARMED)
        {
            _humanRobotStateMachine.firing = true;
            return AIStateType.Fire;
        }

        if (_humanRobotStateMachine.isTargetReached)
        {
            switch (_stateMachine.targetType)
            {

                case AITargetType.Visual_Light:
                    _stateMachine.ClearTarget();    // Clear the threat
                    return AIStateType.Alerted;     // Become alert and scan for targets

                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }
        }


        if (_humanRobotStateMachine.navAgent.isPathStale ||
            !_humanRobotStateMachine.navAgent.hasPath ||
            _humanRobotStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;
        }


        if (_humanRobotStateMachine.navAgent.pathPending)
            _humanRobotStateMachine.speed = 0;
        else
        {
            _humanRobotStateMachine.speed = _speed;

            if (!_humanRobotStateMachine.useRootRotation && 
                _humanRobotStateMachine.targetType == AITargetType.Visual_Player 
                && _humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player 
                && _humanRobotStateMachine.isTargetReached)
            {
                Vector3 targetPos = _humanRobotStateMachine.targetPosition;
                targetPos.y = _humanRobotStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _humanRobotStateMachine.transform.position);
                _humanRobotStateMachine.transform.rotation = newRot;
            }
            else
            if (!_stateMachine.useRootRotation && !_humanRobotStateMachine.isTargetReached)
            {
                if (_humanRobotStateMachine.navAgent.desiredVelocity != Vector3.zero)
                {
                    Quaternion newRot = Quaternion.LookRotation(_humanRobotStateMachine.navAgent.desiredVelocity);

                    _humanRobotStateMachine.transform.rotation = Quaternion.Slerp(_humanRobotStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
                }
            }
            else
            if (_humanRobotStateMachine.isTargetReached)
            {
                Debug.Log("chegando aqui no alerted");
                return AIStateType.Alerted;
            }
        }

        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            if (_humanRobotStateMachine.targetPosition != _humanRobotStateMachine.VisualThreat.position)
            {
                if (Mathf.Clamp(_humanRobotStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                {
                    _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.VisualThreat.position);
                    _repathTimer = 0.0f;
                }
            }
            _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);

            return AIStateType.Pursuit;

        }

        if (_humanRobotStateMachine.targetType == AITargetType.Visual_Player)
            return AIStateType.Pursuit;

        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            if (_humanRobotStateMachine.targetType == AITargetType.Audio || _humanRobotStateMachine.targetType == AITargetType.Visual_Food)
            {
                _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
                return AIStateType.Alerted;
            }
            else
            if (_humanRobotStateMachine.targetType == AITargetType.Visual_Light)
            {
                int currentID = _humanRobotStateMachine.targetColliderID;

                if (currentID == _humanRobotStateMachine.VisualThreat.collider.GetInstanceID())
                {
                    if (_humanRobotStateMachine.targetPosition != _humanRobotStateMachine.VisualThreat.position)
                    {
                        if (Mathf.Clamp(_humanRobotStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                        {
                            _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.VisualThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.VisualThreat);
                    return AIStateType.Alerted;
                }
            }
        }
        else
        if (_humanRobotStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            Debug.Log("audio");
            if (_humanRobotStateMachine.targetType == AITargetType.Visual_Food)
            {
                _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.AudioThreat);
                return AIStateType.Alerted;
            }
            else
            if (_humanRobotStateMachine.targetType == AITargetType.Audio)
            {
                int currentID = _humanRobotStateMachine.targetColliderID;

                if (currentID == _humanRobotStateMachine.AudioThreat.collider.GetInstanceID())
                {
                    if (_humanRobotStateMachine.targetPosition != _humanRobotStateMachine.AudioThreat.position)
                    {
                        if (Mathf.Clamp(_humanRobotStateMachine.AudioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
                        {
                            _humanRobotStateMachine.navAgent.SetDestination(_humanRobotStateMachine.AudioThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.AudioThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _humanRobotStateMachine.SetTarget(_humanRobotStateMachine.AudioThreat);
                    return AIStateType.Alerted;
                }
            }
        }

        // Default
        return AIStateType.Pursuit;
    }

   /* public override void OnAnimatorIKUpdated()
    {
        if (_humanRobotStateMachine == null)
            return;

        if (Vector3.Angle(_humanRobotStateMachine.transform.forward, _humanRobotStateMachine.targetPosition - _humanRobotStateMachine.transform.position) < _lookAtAngleThreshold)
        {
            _humanRobotStateMachine.animator.SetLookAtPosition(_humanRobotStateMachine.targetPosition + Vector3.up);
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _lookAtWeight, Time.deltaTime);
            _humanRobotStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
        else
        {
            _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
            _humanRobotStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
        }
    }*/
}