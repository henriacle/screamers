using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerMoveStatus { NotMoving, Walking, Running, NotGrounded, Landing, Crouching }
public enum CurveControlledBobCallbackType { Horizontal, Vertical }

public delegate void CurveControlledBobCallback();

[System.Serializable]
public class CurveControlledBobEvent
{
    public float Time = 0.0f;
    public CurveControlledBobCallback Function = null;
    public CurveControlledBobCallbackType Type = CurveControlledBobCallbackType.Vertical;
}

[System.Serializable]
public class CurveControllerBob
{

    [SerializeField] AnimationCurve _bobcurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f),
                                                  new Keyframe(1f, 0f), new Keyframe(1.5f, -1f),
                                                  new Keyframe(2f, 0f));
    [SerializeField] float _horizontalMultiplier            = 0.01f;
    [SerializeField] float _verticalMultiplier              = 0.02f;
    [SerializeField] float _verticalHorizontalSpeedRatio    = 2.0f;
    [SerializeField] float _baseInterval                    = 1.0f;

    private float _prevXPlayHead;
    private float _prevYPlayHead;
    private float _xPlayHead;
    private float _yPlayHead;
    private float _curveEndTime;
    private List<CurveControlledBobEvent> _events = new List<CurveControlledBobEvent>();

    public void Initialize()
    {
        _curveEndTime   = _bobcurve[_bobcurve.length - 1].time;
        _xPlayHead      = 0.0f;
        _yPlayHead      = 0.0f;
        _prevYPlayHead  = 0.0f;
        _prevXPlayHead  = 0.0f;
    }

    public void RegisterEventCallback(float time, CurveControlledBobCallback function, CurveControlledBobCallbackType type)
    {
        CurveControlledBobEvent curvedEvent = new CurveControlledBobEvent();
        curvedEvent.Time        = time;
        curvedEvent.Function    = function;
        curvedEvent.Type        = type;
        _events.Add(curvedEvent);
        _events.Sort(
            delegate (CurveControlledBobEvent t1, CurveControlledBobEvent t2) {
                return (t1.Time.CompareTo(t2.Time));
        });
    }

    public Vector3 GetVectorOffset(float speed)
    {
        _xPlayHead += (speed * Time.deltaTime) / _baseInterval;
        _yPlayHead += ((speed * Time.deltaTime) / _baseInterval)*_verticalHorizontalSpeedRatio;

        if (_xPlayHead > _curveEndTime)
            _xPlayHead -= _curveEndTime;

        if (_yPlayHead > _curveEndTime)
            _yPlayHead -= _curveEndTime;

        for(int i = 0; i < _events.Count; i++)
        {
            CurveControlledBobEvent ev = _events[i];
            if (ev != null)
            {
                if(ev.Type == CurveControlledBobCallbackType.Vertical)
                {
                    if((_prevYPlayHead < ev.Time && _yPlayHead >= ev.Time) ||
                       (_prevYPlayHead > _yPlayHead && (ev.Time > _prevYPlayHead || ev.Time <= _yPlayHead)))
                    {
                        ev.Function();
                    }
                }
                else
                {
                    if ((_prevXPlayHead < ev.Time && _xPlayHead >= ev.Time) ||
                    (_prevXPlayHead > _xPlayHead && (ev.Time > _prevXPlayHead || ev.Time <= _xPlayHead)))
                    {
                        ev.Function();
                    }
                }
            }
        }

        float xPos = _bobcurve.Evaluate(_xPlayHead) * _horizontalMultiplier;
        float yPos = _bobcurve.Evaluate(_yPlayHead) * _verticalMultiplier;

        _prevXPlayHead = _xPlayHead;
        _prevYPlayHead = _yPlayHead;

        return new Vector3(xPos, yPos, 0f);
    }
}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    public List<AudioSource> AudioSources   = new List<AudioSource>();
    private int _audioTouse                 = 0;


    //Inspector Assigned Locomotion Settings
    [SerializeField] private float _walkSpeed           = 1.0f;
    [SerializeField] private float _runSpeed            = 4.5f;
    [SerializeField] private float _jumpSpeed           = 7.5f;
    [SerializeField] private float _crouchSpeed         = 1.5f;
    [SerializeField] private float _stickToGroundForce  = 5.0f;
    [SerializeField] private float _gravityMultiplier   = 2.5f;
    [SerializeField] private float _runStepLengthen     = 0.75f;
    [SerializeField] private GameObject _flashLight     = null; 
    [SerializeField] private CurveControllerBob _headBob = new CurveControllerBob();

    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook _mouseLook;

    private Camera  _camera                             = null;
    private bool    _jumpButtonPressed                  = false;
    private Vector2 _inputVector                        = Vector2.zero;
    private Vector3 _moveDirection                      = Vector3.zero;
    private bool    _previouslyGrounded                 = false;
    private bool    _isWalking                          = true;
    private bool    _isJumping                          = false;
    private bool    _isCrouching                        = false;
    private Vector3 _localSpaceCameraPos                = Vector3.zero;
    private float   _charHeight                         = 0.0f;

    //timers
    private float               _fallingTimer           = 0.0f;

    private CharacterController _characterController     = null;
    private PlayerMoveStatus    _movementStatus         = PlayerMoveStatus.NotMoving;

    //public properties

    // Public Properties
    public PlayerMoveStatus movementStatus { get { return _movementStatus; } }
    public float walkSpeed { get { return _walkSpeed; } }
    public float runSpeed { get { return _runSpeed; } }

    protected void Start()
    {
        // Pega a referência do character controller
        _characterController = GetComponent<CharacterController>();
        _charHeight = _characterController.height;

        // coloca a posição da camera no rig
        _camera = Camera.main;
        _localSpaceCameraPos = _camera.transform.localPosition;

        // coloca a posição inicial para não se movendo
        _movementStatus = PlayerMoveStatus.NotMoving;

        //Reseta os timers
        _fallingTimer = 0.0f;

        _mouseLook.Init(transform, _camera.transform);

        _headBob.Initialize();
        _headBob.RegisterEventCallback(1.5f, PlayFootstepSound, CurveControlledBobCallbackType.Vertical);

        if (_flashLight)
            _flashLight.SetActive(false);
    }

    protected void Update()
    {
        //Se estiver caindo incrementa o timer
        if (_characterController.isGrounded) _fallingTimer = 0.0f;
        else _fallingTimer += Time.deltaTime;

        //Permite que o mouse pocesse a rotação da camera
        if (Time.timeScale > Mathf.Epsilon)
            _mouseLook.LookRotation(transform, _camera.transform);

        if(!_jumpButtonPressed && !_isCrouching)
            _jumpButtonPressed = Input.GetButtonDown("Jump");

        if (Input.GetButtonDown("FlashLight"))
        {
            if (_flashLight)
                _flashLight.SetActive(!_flashLight.activeSelf);
        }

        if(Input.GetButtonDown("Crouch"))
        {
            _isCrouching = !_isCrouching;
            _characterController.height = _isCrouching == true ? _charHeight / 2.0f : _charHeight;
        }

        if (!_previouslyGrounded && _characterController.isGrounded)
        {
            if (_fallingTimer > 0.5f)
            {
                // TODO: Play Landing Sound
            }

            _moveDirection.y = 0f;
            _isJumping = false;
            _movementStatus = PlayerMoveStatus.Landing;
        }
        else
            if (!_characterController.isGrounded)
            _movementStatus = PlayerMoveStatus.NotGrounded;
        else
            if (_characterController.velocity.sqrMagnitude < 0.01f)
            _movementStatus = PlayerMoveStatus.NotMoving;
        else
            if (_isCrouching)
            _movementStatus = PlayerMoveStatus.Crouching;
        else
            if (_isWalking)
            _movementStatus = PlayerMoveStatus.Walking;
        else
            _movementStatus = PlayerMoveStatus.Running;

        _previouslyGrounded = _characterController.isGrounded;
    }


    protected void FixedUpdate()
    {
        // Le a posição do axis do mouse
        float horizontal    = Input.GetAxis("Horizontal");
        float vertical      = Input.GetAxis("Vertical");
        bool wasWalking     = _isWalking;
        _isWalking = !Input.GetKey(KeyCode.LeftShift);
        

        float speed = _isCrouching ? _crouchSpeed : _isWalking ? _walkSpeed : _runSpeed;
        _inputVector = new Vector2(horizontal, vertical);

        if (_inputVector.sqrMagnitude > 1) _inputVector.Normalize();

        Vector3 desiredMove = transform.forward * _inputVector.y + transform.right * _inputVector.x;

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo, _characterController.height / 2f, 1))
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        // incrementa o valor do movimento
        _moveDirection.x = desiredMove.x * speed;
        _moveDirection.z = desiredMove.z * speed;

        //Verifica se ele está no chão
        if(_characterController.isGrounded)
        {
            //Manter o personagem no chão
            _moveDirection.y = -_stickToGroundForce;

            //Se o botão de pular for apertado
            if(_jumpButtonPressed)
            {
                _moveDirection.y    = _jumpSpeed;
                _jumpButtonPressed  = false;
                _isJumping          = true;

                //TODO: Tocar o som de pulo
            }
        }
        else
        {
            // Caso não esteja no chão, aplicar a gravidade ao controller
            _moveDirection += Physics.gravity * _gravityMultiplier * Time.fixedDeltaTime;
        }

        // Move the Character Controller
        _characterController.Move(_moveDirection * Time.fixedDeltaTime);
        Vector3 speedXZ = new Vector3(_characterController.velocity.x, 0.0f, _characterController.velocity.z);

        if (speedXZ.magnitude > 0.01f)
        {
            _camera.transform.localPosition = _localSpaceCameraPos + _headBob.GetVectorOffset(
                speedXZ.magnitude * (_isWalking || _isWalking ? 1.0f : _runStepLengthen));
        }

    }

    void PlayFootstepSound()
    {
        if(_isCrouching)
        {
            AudioSources[_audioTouse].volume = 0.2f;
        } else
        {
            AudioSources[_audioTouse].volume = 1f;
        }
        AudioSources[_audioTouse].Play();
        _audioTouse = (_audioTouse == 0) ? 1 : 0;
    }
}
