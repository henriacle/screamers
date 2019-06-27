using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITatuStateMachine : AIStateMachine
{
    //Inspector custom parameters
    [SerializeField] [Range(10.0f, 360.0f)] float _fov = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _sight = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _hearing = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] float _agression = 0.5f;
    [SerializeField] [Range(0, 100)] float _health = 100.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float _intelligence = 1.0f;

    private int     _seeking = 0;
    private float   _speed = 0;
    private GameSceneManager _gameSceneManager = null;
    //hash
    private int _speedHash = Animator.StringToHash("Speed");
    private int _seekingHash = Animator.StringToHash("Seeking");
    private int _firingHash = Animator.StringToHash("Firing");
    private int _reloadingHash = Animator.StringToHash("Reloading");
    private int _attackHash = Animator.StringToHash("Attack");
    private int _crouchingHash = Animator.StringToHash("Crouching");
    private int _weaponTypeHash = Animator.StringToHash("Weapon Type");
    private OriginalWeaponSystem _weaponSystem = null;
    private Weapon _npcWeapon = null;
    [SerializeField] bool _is_robot = true;

    public bool replaceWhenDead = false;        // Whether or not a dead replacement should be instantiated.  (Useful for breaking/shattering/exploding effects)
    public GameObject deadReplacement;          // The prefab to instantiate when this GameObject dies
    public bool makeExplosion = false;          // Whether or not an explosion prefab should be instantiated
    public GameObject explosion;                // The explosion prefab to be instantiated


    //private
    private bool _firing = false;
    private bool _reloading = false;
    private bool _dying = false;
    private bool _headShot = false;
    private bool _crouching = false;
    private bool _isDead = false;
    private bool _scream = false;
    private int _hitType = 0;
    private int _attackType = 0;


    //public properties
    public float fov { get { return _fov; } }
    public float sight { get { return _sight; } }
    public float hearing { get { return _hearing; } }
    public float intelligence { get { return _intelligence; } }
    public int seeking { get { return _seeking; } set { _seeking = value; } }
    public float health { get { return _health; } set { _health = value; } }
    public float speed
    {
        get { return _speed; }
        set { _speed = value; }
    }

    public void gameObjectHit(HitInfo hit)
    {
        AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.hit.rigidbody.GetInstanceID());
        if (hit.aiBodyPart > 0)
        {
            takeDamage(hit.hit.point, 5.0f, hit.damage, hit.hit.rigidbody);
        }
    }

    public override void takeDamage(Vector3 position, float force, float damage, Rigidbody bodyPart, int hitDirection = 0)
    {
        base.takeDamage(position, force, damage, bodyPart, hitDirection);

        health = _health + damage;
        bool shouldRagdoll = (health <= 0) ? true : false;

        //if (_navAgent)
        //_navAgent.speed = 0;


        if (shouldRagdoll)
        {
            if (_is_robot)
            {
                Die();
            }
            else
            {
                _navAgent.enabled = false;
                _animator.enabled = false;
                _collider.enabled = false;
                _isDead = true;
                inMeleeRange = false;
                inFireRange = false;

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

    public void Die()
    {
        // Make death effects
        if (replaceWhenDead)
            Instantiate(deadReplacement, transform.position, transform.rotation);
        if (makeExplosion)
            Instantiate(explosion, transform.position, transform.rotation);

        // Remove this GameObject from the scene
        Destroy(gameObject);
    }

    protected override void Start()
    {
        base.Start();
        _gameSceneManager = GameSceneManager.instance;
    }

    protected override void Update()
    {
        base.Update();

        if (_animator != null)
        {
            _animator.SetFloat(_speedHash, _speed);
            _animator.SetBool(_firingHash, _firing);
            _animator.SetBool(_reloadingHash, _reloading);
            _animator.SetBool(_crouchingHash, _crouching);
            _animator.SetInteger(_seekingHash, _seeking);
            _animator.SetInteger(_attackHash, _attackType);
        }
    }
}
