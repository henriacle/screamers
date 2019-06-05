using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyWeaponType { UNARMED, RIFLE, PISTOL };

public class AISoldierStateMachine : AIStateMachine
{
    // Inspector Assigned
    [SerializeField] [Range(10.0f, 360.0f)] float       _fov            = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _sight          = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _hearing        = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _agression      = 0.5f;
    [SerializeField] [Range(0, 100)]        float       _health         = 100.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    float       _intelligence   = 1.0f;
    [SerializeField]                        EnemyWeaponType   _weaponType     = EnemyWeaponType.UNARMED;
    GameObject   _weapon                                                = null;
    AttachWeapon attachWeapon                                           = null;

    //private
    private int     _seeking    = 0;
    private bool    _firing     = false;
    private bool    _reloading  = false;
    private bool    _dying      = false;
    private bool    _headShot   = false;
    private bool    _crouching  = false;
    private int     _hitType    = 0;
    private int     _attackType = 0;
    private float   _speed      = 0;

    //hash
    private int _speedHash      = Animator.StringToHash("Speed");
    private int _seekingHash    = Animator.StringToHash("Seeking");
    private int _firingHash     = Animator.StringToHash("Firing");
    private int _reloadingHash  = Animator.StringToHash("Reloading");
    private int _attackHash     = Animator.StringToHash("Attack");
    private int _crouchingHash  = Animator.StringToHash("Crouching");
    private int _weaponTypeHash  = Animator.StringToHash("Weapon Type");
    private GameSceneManager _gameSceneManager = null;
    private OriginalWeaponSystem _weaponSystem = null;
    private Weapon _npcWeapon = null;

    //public properties
    public float        fov             { get { return _fov; } }
    public float        sight           { get { return _sight; } }
    public float        hearing         { get { return _hearing; } }
    public float        intelligence    { get { return _intelligence; } }
    public bool         headShot        { get { return _headShot; }}
    public bool         dying           { get { return _dying; }}
    public EnemyWeaponType weaponType      {
        get { return _weaponType; }
        set { _weaponType = value; }
    }
    public int          seeking         { get { return _seeking; }          set { _seeking = value; } }
    public bool         firing          { get { return _firing; }           set { _firing = value; } }
    public bool         reloading       { get { return _reloading; }        set { _reloading = value; } }
    public bool         crouching       { get { return _crouching; }        set { _crouching = value; } }
    public float        health          { get { return _health; }           set { _health = value; } }
    public int          hitType         { get { return _hitType; }          set { _hitType = value; } }
    public int          attackType      { get { return _attackType; }       set { _attackType = value; } }
    public float        speed
    {
        get { return _speed; }
        set { _speed = value; }
    }


    protected override void Start()
    {
        base.Start();
        _gameSceneManager = GameSceneManager.instance;
        if(_weaponType != EnemyWeaponType.UNARMED)
        {
            _weaponSystem = gameObject.transform.root.GetComponentInChildren<OriginalWeaponSystem>();
            _npcWeapon = _weaponSystem.weapons[_weaponSystem.weaponIndex].GetComponent<Weapon>();
            attachWeapon = GetComponent<AttachWeapon>();
            attachWeapon.setWeapon(_npcWeapon.gameObject);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (_weaponType == EnemyWeaponType.PISTOL)
        {
            attachWeapon.weaponStatus(true);
        }

        if (_animator!=null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_firingHash, _firing);
            _animator.SetBool(_reloadingHash, _reloading);
            _animator.SetBool(_crouchingHash, _crouching);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
            _animator.SetInteger(_weaponTypeHash, (int)_weaponType);
        }
    }

    public void gameObjectHit(HitInfo hit)
    {
        AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.hit.rigidbody.GetInstanceID());
        if(hit.aiBodyPart > 0)
        {
            takeDamage(hit.hit.point, 5.0f, hit.damage, hit.hit.rigidbody);
        }
    }

    public override void takeDamage(Vector3 position, float force, float damage, Rigidbody bodyPart, int hitDirection = 0)
    {
        base.takeDamage(position, force, damage, bodyPart, hitDirection);

        health = _health + damage;
        bool shouldRagdoll = (health <= 0) ? true : false;

        if (_navAgent)
            _navAgent.speed = 0;

        if (shouldRagdoll)
        {
            _navAgent.enabled = false;
            _animator.enabled = false;
            _collider.enabled = false;

            inMeleeRange    = false;
            inFireRange     = false;

            foreach (Rigidbody body in _bodyParts)
            {
                if (body)
                {
                    body.isKinematic = false;
                }
            }
        }
    }
}
