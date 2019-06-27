using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiTatu_attack1 : AITatuState
{
    [SerializeField] [Range(0, 10)]         float _speed                = 0.0f;
    [SerializeField]                        float _stoppingDistance     = 1.0f;
    [SerializeField]                        float _releaseDistance      = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float _lookAtWeight         = 0.7f;
    [SerializeField] [Range(0.0f, 90.0f)]   float _lookAtAngleThreshold = 15.0f;
    [SerializeField]                        float _slerpSpeed           = 5.0f;
    [SerializeField]                        float _attackTime           = 5.0f;
    AudioSource                             _attackAudio                      = null;
    Rigidbody _body             = null;


    bool waitToAttackAgain      = false;
    float   _timer                = 0.0f;
    float   _directionChangeTimer = 0.0f;
    public AnimationCurve JumpCurve = new AnimationCurve();

    public override AIStateType GetStateType()
    {
        return AIStateType.Attack;
    }

    public override void OnEnterState()
    {
        Debug.Log("Entering tatu attack state");
        base.OnEnterState();
        if (_tatuStateMachine == null)
            return;

        _timer                  = _attackTime;
        _directionChangeTimer   = 0.0f;
        _body                   = gameObject.GetComponent<Rigidbody>();
        _attackAudio            = gameObject.GetComponent<AudioSource>();
        _tatuStateMachine.headbutt = 1;

        // Configure State Machine
        _tatuStateMachine.NavAgentControl(false, false);
        _tatuStateMachine.speed = 0;
        _tatuStateMachine.seeking = 0;
    }

    public override AIStateType OnUpdate()
    {
        Vector3 targetPos;
        Quaternion newRot;

        float distanceBetweeenPlayer = Vector3.Distance(_tatuStateMachine.transform.position, _tatuStateMachine.targetPosition);

        if (distanceBetweeenPlayer < _stoppingDistance)
        {
            _tatuStateMachine.speed = 0;
        }
        else
        {
            _tatuStateMachine.speed = _speed;
        }


        if (_tatuStateMachine.VisualThreat.type == AITargetType.Visual_Player)
        {
            _tatuStateMachine.SetTarget(_stateMachine.VisualThreat);

            targetPos = _tatuStateMachine.targetPosition;
            targetPos.y = _tatuStateMachine.transform.position.y;
            newRot = Quaternion.LookRotation(targetPos - _tatuStateMachine.transform.position);
            _tatuStateMachine.transform.rotation = Quaternion.Slerp(_tatuStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);
            
            if (!waitToAttackAgain)
            {
                StartCoroutine(Jump(0.3f));
                return AIStateType.Pursuit;
            }
            else
            {
                _timer -= Time.deltaTime;

                if (_timer <= 0.0f)
                {
                    _timer = _attackTime;
                    waitToAttackAgain = false;
                }
            }
        } else
        {
            Debug.Log("entrando aqui");
            return AIStateType.Idle;
        }

        return AIStateType.Attack;
    }

    IEnumerator Jump(float duration)
    {
        waitToAttackAgain = true;
        _attackAudio.Play();

        Vector3 startPos = _tatuStateMachine.transform.position;

        Vector3 endPos = _tatuStateMachine.targetPosition + (_tatuStateMachine.navAgent.baseOffset * Vector3.up);
        endPos.x = endPos.x - 1.0f;
        endPos.z = endPos.z - 1.0f;
        endPos.y = endPos.y - 1.0f;

        float time = 0.0f;


        while (time <= duration)
        {
            float t = time / duration;
            
            _tatuStateMachine.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);

            time += Time.deltaTime;
            yield return null;
        }

        _tatuStateMachine.navAgent.transform.position = endPos;
    }
}
