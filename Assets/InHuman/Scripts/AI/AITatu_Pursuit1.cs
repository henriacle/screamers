using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AITatu_Pursuit1 : AITatuState
{
    [SerializeField] [Range(0, 10)] private float _speed = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _lookAtWeight = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)] float _lookAtAngleThreshold = 15.0f;
    [SerializeField] private float _slerpSpeed = 5.0f;
    [SerializeField] private float _repathDistanceMultiplier = 0.035f;
    [SerializeField] private float _repathVisualMinDuration = 0.05f;
    [SerializeField] private float _repathVisualMaxDuration = 5.0f;
    [SerializeField] private float _repathAudioMinDuration = 0.25f;
    [SerializeField] private float _repathAudioMaxDuration = 5.0f;
    [SerializeField] private float _maxDuration = 40.0f;
    [SerializeField] private float _stoppingDistance = 1.0f;

    // Private Fields
    private float _timer = 0.0f;
    private float _repathTimer = 0.0f;
    private float _currentLookAtWeight = 0.0f;

    // Mandatory Overrides
    public override AIStateType GetStateType() { return AIStateType.Pursuit; }

    // Default Handlers
    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_tatuStateMachine == null)
            return;

        _tatuStateMachine.NavAgentControl(true, false);
        _tatuStateMachine.seeking = 0;
        _currentLookAtWeight = 0.0f;
        _timer = 0.0f;
        _repathTimer = 0.0f;


        if (_tatuStateMachine.navAgent.enabled)
        {
            _tatuStateMachine.navAgent.SetDestination(_tatuStateMachine.targetPosition);
            _tatuStateMachine.navAgent.isStopped = false;
        }

    }

    public override AIStateType OnUpdate()
    {
        _timer += Time.deltaTime;
        _repathTimer += Time.deltaTime;

        if (_timer > _maxDuration)
            return AIStateType.Idle;

        if (_stateMachine.targetType == AITargetType.Visual_Player && _tatuStateMachine.inFireRange &&
        Vector3.Distance(_tatuStateMachine.transform.position, _tatuStateMachine.targetPosition) < _stoppingDistance)
        {
            return AIStateType.Attack;
        }


        if (_tatuStateMachine.navAgent.isPathStale ||
            !_tatuStateMachine.navAgent.hasPath ||
            _tatuStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return AIStateType.Alerted;
        }


        if (_tatuStateMachine.navAgent.pathPending)
        {
            _tatuStateMachine.speed = 0;
        }
        else
        {
            _tatuStateMachine.speed = _speed;
            _tatuStateMachine.animator.SetFloat("Speed", _speed);

            if (!_tatuStateMachine.useRootRotation &&
                _tatuStateMachine.targetType == AITargetType.Visual_Player
                && _tatuStateMachine.VisualThreat.type == AITargetType.Visual_Player
                && _tatuStateMachine.isTargetReached)
            {
                Vector3 targetPos = _tatuStateMachine.targetPosition;
                targetPos.y = _tatuStateMachine.transform.position.y;
                Quaternion newRot = Quaternion.LookRotation(targetPos - _tatuStateMachine.transform.position);
                _tatuStateMachine.transform.rotation = newRot;
            }
            else
            if (!_stateMachine.useRootRotation && !_tatuStateMachine.isTargetReached)
            {
                if (_tatuStateMachine.navAgent.desiredVelocity != Vector3.zero)
                {
                    Quaternion newRot = Quaternion.LookRotation(_tatuStateMachine.navAgent.desiredVelocity);

                    _tatuStateMachine.transform.rotation = Quaternion.Slerp(_tatuStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
                }
            }
            else
            if (_tatuStateMachine.isTargetReached)
            {
                Debug.Log("chegando aqui no alerted");
                return AIStateType.Alerted;
            }
        }

        if (_tatuStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            if (_tatuStateMachine.targetPosition != _tatuStateMachine.VisualThreat.position)
            {
                if (Mathf.Clamp(_tatuStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                {
                    _tatuStateMachine.navAgent.SetDestination(_tatuStateMachine.VisualThreat.position);
                    _repathTimer = 0.0f;
                }
            }
            _tatuStateMachine.SetTarget(_tatuStateMachine.VisualThreat);

            return AIStateType.Pursuit;

        }

        if (_tatuStateMachine.targetType == AITargetType.Visual_Player)
            return AIStateType.Pursuit;

        if (_tatuStateMachine.VisualThreat.type == AITargetType.Visual_Light)
        {
            if (_tatuStateMachine.targetType == AITargetType.Audio || _tatuStateMachine.targetType == AITargetType.Visual_Food)
            {
                _tatuStateMachine.SetTarget(_tatuStateMachine.VisualThreat);
                return AIStateType.Alerted;
            }
            else
            if (_tatuStateMachine.targetType == AITargetType.Visual_Light)
            {
                int currentID = _tatuStateMachine.targetColliderID;

                if (currentID == _tatuStateMachine.VisualThreat.collider.GetInstanceID())
                {
                    if (_tatuStateMachine.targetPosition != _tatuStateMachine.VisualThreat.position)
                    {
                        if (Mathf.Clamp(_tatuStateMachine.VisualThreat.distance * _repathDistanceMultiplier, _repathVisualMinDuration, _repathVisualMaxDuration) < _repathTimer)
                        {
                            _tatuStateMachine.navAgent.SetDestination(_tatuStateMachine.VisualThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _tatuStateMachine.SetTarget(_tatuStateMachine.VisualThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _tatuStateMachine.SetTarget(_tatuStateMachine.VisualThreat);
                    return AIStateType.Alerted;
                }
            }
        }
        else
        if (_tatuStateMachine.AudioThreat.type == AITargetType.Audio)
        {
            Debug.Log("audio");
            if (_tatuStateMachine.targetType == AITargetType.Visual_Food)
            {
                _tatuStateMachine.SetTarget(_tatuStateMachine.AudioThreat);
                return AIStateType.Alerted;
            }
            else
            if (_tatuStateMachine.targetType == AITargetType.Audio)
            {
                int currentID = _tatuStateMachine.targetColliderID;

                if (currentID == _tatuStateMachine.AudioThreat.collider.GetInstanceID())
                {
                    if (_tatuStateMachine.targetPosition != _tatuStateMachine.AudioThreat.position)
                    {
                        if (Mathf.Clamp(_tatuStateMachine.AudioThreat.distance * _repathDistanceMultiplier, _repathAudioMinDuration, _repathAudioMaxDuration) < _repathTimer)
                        {
                            _tatuStateMachine.navAgent.SetDestination(_tatuStateMachine.AudioThreat.position);
                            _repathTimer = 0.0f;
                        }
                    }

                    _tatuStateMachine.SetTarget(_tatuStateMachine.AudioThreat);
                    return AIStateType.Pursuit;
                }
                else
                {
                    _tatuStateMachine.SetTarget(_tatuStateMachine.AudioThreat);
                    return AIStateType.Alerted;
                }
            }
        }

        // Default
        return AIStateType.Pursuit;
    }

    /* public override void OnAnimatorIKUpdated()
     {
         if (_tatuStateMachine == null)
             return;

         if (Vector3.Angle(_tatuStateMachine.transform.forward, _tatuStateMachine.targetPosition - _tatuStateMachine.transform.position) < _lookAtAngleThreshold)
         {
             _tatuStateMachine.animator.SetLookAtPosition(_tatuStateMachine.targetPosition + Vector3.up);
             _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, _lookAtWeight, Time.deltaTime);
             _tatuStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
         }
         else
         {
             _currentLookAtWeight = Mathf.Lerp(_currentLookAtWeight, 0.0f, Time.deltaTime);
             _tatuStateMachine.animator.SetLookAtWeight(_currentLookAtWeight);
         }
     }*/
}
