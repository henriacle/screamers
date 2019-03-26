using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIHumanRobotState : AIState
{
    protected int _playerLayerMask  = -1;
    protected int _bodyPartLayer = -1;
    protected int _visualLayerMask = -1;
    protected AISoldierStateMachine _humanRobotStateMachine = null;

    void Awake()
    {
        _playerLayerMask = LayerMask.GetMask("Player", "AI Body Part") + 1;
        _visualLayerMask = LayerMask.GetMask("Player", "AI Body Part", "Visual Aggravator") + 1;
        _bodyPartLayer = LayerMask.NameToLayer("AI Body Part");
    }

    public override void SetStateMachine(AIStateMachine stateMachine)
    {
        if(stateMachine.GetType()==typeof(AISoldierStateMachine))
        {
            base.SetStateMachine(stateMachine);
            _humanRobotStateMachine = (AISoldierStateMachine)stateMachine;
        }
    }

    public override void OnTriggerEvent(AITriggerEventType eventType, Collider other) {
        if (_humanRobotStateMachine == null)
            return;

        if (eventType != AITriggerEventType.Exit)
        {
            AITargetType curType = _humanRobotStateMachine.VisualThreat.type;

            if (other.CompareTag("Player"))
            {
                float distance = Vector3.Distance(_humanRobotStateMachine.sensorPosition, other.transform.position);

                if (curType != AITargetType.Visual_Player || 
                   (curType==AITargetType.Visual_Player && distance < _humanRobotStateMachine.VisualThreat.distance))
                {
                    RaycastHit hitInfo;
                    if (ColliderIsVisible (other, out hitInfo, _playerLayerMask)) 
                    {
                        _humanRobotStateMachine.VisualThreat.Set(AITargetType.Visual_Player, other, other.transform.position, distance);
                    }
                }
            } else if(other.CompareTag("Flash Light") && curType != AITargetType.Visual_Player)
            {
                BoxCollider flashLightTrigger = (BoxCollider)other;
                float distanceToThreat = Vector3.Distance(_humanRobotStateMachine.sensorPosition, flashLightTrigger.transform.position);
                float zSize = flashLightTrigger.size.z * flashLightTrigger.transform.lossyScale.z;
                float aggrFactor = distanceToThreat / zSize;
                if(aggrFactor <= _humanRobotStateMachine.sight && aggrFactor <= _humanRobotStateMachine.intelligence)
                {
                    _humanRobotStateMachine.VisualThreat.Set(AITargetType.Visual_Light, other, other.transform.position, distanceToThreat);
                }
            } else if(other.CompareTag("AI Sound Emitter"))
            {
                SphereCollider soundTrigger = (SphereCollider)other;
                if (soundTrigger == null) return;

                // Get the position of the Agent Sensor 
                Vector3 agentSensorPosition = _humanRobotStateMachine.sensorPosition;

                Vector3 soundPos;
                float soundRadius;
                AIState.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);

                // How far inside the sound's radius are we
                float distanceToThreat = (soundPos - agentSensorPosition).magnitude;

                // Calculate a distance factor such that it is 1.0 when at sound radius 0 when at center
                float distanceFactor = (distanceToThreat / soundRadius);

                // Bias the factor based on hearing ability of Agent.
                distanceFactor += distanceFactor * (1.0f - _humanRobotStateMachine.hearing);

                // Too far away
                if (distanceFactor > 1.0f)
                    return;

                // if We can hear it and is it closer then what we previously have stored
                if (distanceToThreat < _humanRobotStateMachine.AudioThreat.distance)
                {
                    // Most dangerous Audio Threat so far
                    _humanRobotStateMachine.AudioThreat.Set(AITargetType.Audio, other, soundPos, distanceToThreat);
                }
            } else if(other.CompareTag("AI Food") 
                && curType!=AITargetType.Visual_Player 
                && curType != AITargetType.Visual_Light 
                && _humanRobotStateMachine.AudioThreat.type==AITargetType.None)
            {
                float distanceToThreat = Vector3.Distance(other.transform.position, _humanRobotStateMachine.sensorPosition);

                if(distanceToThreat<_humanRobotStateMachine.VisualThreat.distance)
                {
                    RaycastHit hitInfo;
                    if(ColliderIsVisible(other, out hitInfo, _visualLayerMask))
                    {
                        _humanRobotStateMachine.VisualThreat.Set(AITargetType.Visual_Food, other, other.transform.position, distanceToThreat);
                    }
                }
            }
        }
    }

    protected virtual bool ColliderIsVisible(Collider other, out RaycastHit hitInfo, int layerMask=-1)
    {
        hitInfo = new RaycastHit();

        if (_humanRobotStateMachine == null)
            return false;

        Vector3 head        = _humanRobotStateMachine.sensorPosition;
        Vector3 direction = other.transform.position - head;

        float angle = Vector3.Angle(direction, transform.forward);

        if (angle > _humanRobotStateMachine.fov * 0.5f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(
            head, 
            direction.normalized,
            _humanRobotStateMachine.sensorRadius * _humanRobotStateMachine.sight, 
            layerMask);

        float closestColliderDistance   = float.MaxValue;
        Collider closestCollider        = null;

        for(int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if(hit.distance < closestColliderDistance)
            {
                if (hit.transform.gameObject.layer == _bodyPartLayer)
                {
                    if(_stateMachine != GameSceneManager.instance.GetAIStateMachine(hit.rigidbody.GetInstanceID()))
                    {
                        closestColliderDistance = hit.distance;
                        closestCollider = hit.collider;
                        hitInfo = hit;
                    }
                } else
                {
                    closestColliderDistance = hit.distance;
                    closestCollider = hit.collider;
                    hitInfo = hit;
                }
            }
        }

        if (closestCollider && closestCollider.gameObject == other.gameObject) return true;

        return false;
    }
}
