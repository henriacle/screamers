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
    [SerializeField] private GameObject leftEye  = null;
    [SerializeField] private GameObject rightEye = null;
    [SerializeField] private AudioSource _robotScream = null;
    [SerializeField] float _stoppingDistance = 1.0f;
    [SerializeField] float keepMinDistance = 5f;
    [SerializeField] float keepMaxDistance = 9f;
    private OriginalWeaponSystem _weaponSystem = null;
    private Weapon _npcWeapon = null;
    [SerializeField] Transform chest;
    private Transform rightHand;
    private Transform leftHand;

    Material leftEyeMaterial = null;
    Material rightEyeMaterial = null;
    private float _eyeIntensity = 5.0f;
    private bool _screamPlayer = false;
    // Private Fields
    private float _timer = 0.0f;
    private float _repathTimer = 0.0f;
    private float _currentLookAtWeight = 0.0f;
    public Vector3 Offset;


    // Mandatory Overrides
    public override AIStateType GetStateType() { return AIStateType.Pursuit; }

    public void setEyeColor(Color color, float intensity)
    {
        if(leftEyeMaterial != null && rightEyeMaterial != null)
        {
            leftEyeMaterial.SetColor("_EmissionColor", color * _eyeIntensity);
            rightEyeMaterial.SetColor("_EmissionColor", color * _eyeIntensity);
        }
    }

    // Default Handlers
    public override void OnEnterState()
    {
        chest = _humanRobotStateMachine.animator.GetBoneTransform(HumanBodyBones.Chest);
        rightHand = _humanRobotStateMachine.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        leftHand = _humanRobotStateMachine.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);

        if (!_robotScream.isPlaying 
            && !_humanRobotStateMachine.dead 
            && !_humanRobotStateMachine.scream)
        {
            _humanRobotStateMachine.scream = true;
            _robotScream.Play();
        }

        base.OnEnterState();
        if (_humanRobotStateMachine == null)
            return;

        if(leftEye != null)
            leftEyeMaterial     =   leftEye.GetComponent<Renderer>().material;
        if(rightEye != null)
            rightEyeMaterial    =   rightEye.GetComponent<Renderer>().material;

        setEyeColor(Color.red, _eyeIntensity);

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
        _weaponSystem = gameObject.GetComponentInChildren<OriginalWeaponSystem>();
        if(_weaponSystem.weapons.Length > 0)
        {
            _npcWeapon = _weaponSystem.weapons[_weaponSystem.weaponIndex].GetComponent<Weapon>();
            _npcWeapon.reloadAutomatically = true;
        }
    }

    public override void OnExitState()
    {
        _humanRobotStateMachine.attackType = 0;
    }

    public void OnEasyWeaponsLaunch()
    {
        Debug.Log("easy weapons launch");
    }

    IEnumerator OnEasyWeaponsFire()
    {
        yield return new WaitForSeconds(1.5f);
        _npcWeapon.canFire = true;
    }

    IEnumerator OnEasyWeaponsReload()
    {
        _humanRobotStateMachine.reloading = true;
        yield return new WaitForSeconds(1.5f);
        _npcWeapon.currentAmmo = _npcWeapon.ammoCapacity;
        _humanRobotStateMachine.reloading = false;
        _npcWeapon.canFire = true;
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

        if (_stateMachine.targetType == AITargetType.Visual_Player 
            && _humanRobotStateMachine.inFireRange 
            && _humanRobotStateMachine.weaponType != EnemyWeaponType.UNARMED)
        {
            Vector3 targetPos;
            Quaternion newRot;

            _humanRobotStateMachine.firing = true;
            if (Vector3.Distance(_humanRobotStateMachine.transform.position, _humanRobotStateMachine.targetPosition) < keepMinDistance)
            {
                _humanRobotStateMachine.speed = 4;
            }
            else if (Vector3.Distance(_humanRobotStateMachine.transform.position, _humanRobotStateMachine.targetPosition) > keepMaxDistance)
            {
                _humanRobotStateMachine.speed = _speed;
            }
            else
            {
                _humanRobotStateMachine.speed = 0;
            }

            _humanRobotStateMachine.SetTarget(_stateMachine.VisualThreat);

            if (_humanRobotStateMachine.inMeleeRange)
            {
                return AIStateType.Attack;
            };

            if (!_humanRobotStateMachine.reloading)
            {
                _humanRobotStateMachine.firing = true;
            }

            targetPos = _humanRobotStateMachine.targetPosition;
            targetPos.y = _humanRobotStateMachine.transform.position.y - 20.0f;
            newRot = Quaternion.LookRotation(targetPos - _humanRobotStateMachine.transform.position);
            _humanRobotStateMachine.transform.rotation = Quaternion.Slerp(_humanRobotStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

            _humanRobotStateMachine.attackType = 0;

            if (_npcWeapon.currentAmmo > 0 && _npcWeapon.canFire)
            {
                _npcWeapon.AIFiring();
            }

            return AIStateType.Pursuit;
        }

        if (_humanRobotStateMachine.isTargetReached)
        {
            switch (_stateMachine.targetType)
            {

                case AITargetType.Visual_Light:
                    _stateMachine.ClearTarget();    // Clear the threat
                    setEyeColor(Color.yellow, _eyeIntensity);
                    return AIStateType.Alerted;     // Become alert and scan for targets

                case AITargetType.Visual_Food:
                    return AIStateType.Feeding;
            }
        }


        if (_humanRobotStateMachine.navAgent.isPathStale ||
            !_humanRobotStateMachine.navAgent.hasPath ||
            _humanRobotStateMachine.navAgent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            setEyeColor(Color.yellow, _eyeIntensity);
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

    public override void OnAnimatorIKUpdated()
    {
        if (_humanRobotStateMachine == null)
            return;

        /*
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
        */
    }

    public override void LateUpdate()
    {
        if (!_humanRobotStateMachine) return;

        if (_humanRobotStateMachine.VisualThreat.type == AITargetType.Visual_Player
        && _humanRobotStateMachine.targetPosition != null
        && _humanRobotStateMachine.inFireRange
        && chest != null)
        {
            chest.LookAt(_humanRobotStateMachine.targetPosition);
            chest.rotation = chest.rotation * Quaternion.Euler(Offset);
        }
    }
}