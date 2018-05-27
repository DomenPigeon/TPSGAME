using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// TODO
/// - sometimes not falling down from jump
/// - crowling

/// Setting up charachter
/// - Input Button Run
/// - Horizontal and Vertical Button should not snap, and have gravity and sensitivity set to 6


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController: MonoBehaviour {

    // Serialized properties
    [SerializeField] private float                  _runSpeed                   = 2.0f;
    [SerializeField] private float                  _walkSpeed                  = 1.0f;
    [SerializeField] private float                  _jumpSpeed                  = 5.0f;
    [SerializeField] private float                  _inAirMovementSpeed         = 1.0f;
    [SerializeField] private float                  _gravityMultiplyer          = 2.0f;
    [SerializeField] private float                  _stickToGroundForce         = 2.0f;
    [SerializeField] private float                  _forwardBackwardsTurnAngle  = 15.0f;

    // Private properties
    private bool        _isRunning              = false;
    private bool        _isJumping              = false;
    private bool        _jumpButtonPressed      = false;
    private bool        _previouslyGrounded     = false;
    private float       _verticalInput          = 0.0f;
    private float       _horizontalInput        = 0.0f;
    private float       _currentForwardSpeed    = 0.0f;
    private float       _currentSideSpeed       = 0.0f;
    private float       _distanceFromGround     = 0.0f;
    private float       _flyingTimer            = 0.0f;
    private float       _initialHeight          = 0.0f;
    private float       _defaultLerpMultiplyer  = 5.0f;

    // Private objects
    private Animator                _animator               = null;
    private CharacterController     _charachterController   = null;
    private Vector3                 _moveDirection          = Vector3.zero;
    private Vector3                 _initialCenterPos       = Vector3.zero;
    private Transform               _leftFootPosition       = null;
    private Transform               _topHeadPostion         = null;

    // Hashes
    private int _forwardSpeedHash       = Animator.StringToHash("forwardSpeed");
    private int _sideSpeedHash          = Animator.StringToHash("sideSpeed");
    private int _jumpHash               = Animator.StringToHash("Jump");
    private int _distanceFromGroundHash = Animator.StringToHash("distanceFromGround");

    private void Start() {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = true;
        _charachterController = GetComponent<CharacterController>();
        _initialHeight = _charachterController.height;
        _initialCenterPos = _charachterController.center;

        _leftFootPosition = GetComponentsInChildren<LocateChildObject>()[1].transform;
        _topHeadPostion = GetComponentsInChildren<LocateChildObject>()[0].transform;
    }

    private void Update() {
        _isRunning = false;
        if (Input.GetButton("Run")) {
            _isRunning = true;
        }

        if (Input.GetButtonDown("Jump")) {
            _jumpButtonPressed = true;
        }

        UpdateMovePosition();
    }


    /// <summary>
    /// Dealing with moving player around.
    /// </summary>
    void UpdateMovePosition() {
        // Get keyboard inputs
        _verticalInput = Input.GetAxis("Vertical");
        _horizontalInput = Input.GetAxis("Horizontal");


        /*  -----  MOVING FORWARD / BACKWARD -----  */
        // Set the correct speed accordingly 
        float speed = _isRunning ? _runSpeed : _walkSpeed;
        _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, (_verticalInput * speed), Time.deltaTime * _defaultLerpMultiplyer);
        if (Mathf.Abs(_currentForwardSpeed) < 0.001f) _currentForwardSpeed = 0;

        /*  -----  MOVING LEFT / RIGHT  -----  */
        _currentSideSpeed = _horizontalInput;


        /*  -----  MOVING FORWARD LEFT / RIGHT  -----  */
        if (_currentForwardSpeed > 0.9f) {
            float yRotation = _horizontalInput * _forwardBackwardsTurnAngle;
            // Set the forwardSideRot to current transform rotation
            Quaternion forwardSideRot = transform.localRotation;
            // Add the yRotation to it (Quaternions have to be multiplyed not added.)
            forwardSideRot *= Quaternion.Euler(0f, yRotation, 0f);
            transform.localRotation = forwardSideRot;
        }
        if (_currentForwardSpeed < -0.5f) {  // same as above but opposite direction -_horizontalInput
            float yRotation = -_horizontalInput * _forwardBackwardsTurnAngle;
            // Set the forwardSideRot to current transform rotation
            Quaternion forwardSideRot = transform.localRotation;
            // Add the yRotation to it (Quaternions have to be multiplyed not added.)
            forwardSideRot *= Quaternion.Euler(0f, yRotation, 0f);
            transform.localRotation = forwardSideRot;
        }

        /*  -----  JUMPING  -----  */
        if (_charachterController.isGrounded) {
            _flyingTimer = 0.0f;
            // Apply stick to ground force if grounded
            _moveDirection.y = -_stickToGroundForce;

            if (_jumpButtonPressed) {
                _isJumping = true;
                _moveDirection.y = _jumpSpeed;
            }

            _charachterController.height = Mathf.Lerp(_charachterController.height, _initialHeight, Time.deltaTime * _defaultLerpMultiplyer);
            _charachterController.center = Vector3.Lerp(_charachterController.center, _initialCenterPos, Time.deltaTime * _defaultLerpMultiplyer);
        }
        else {
            _flyingTimer += Time.deltaTime;
            if (_flyingTimer >= 0.1f) {
                _jumpButtonPressed = false;
                _isJumping = true;
            }
            // Apply modified gravity
            _moveDirection += Physics.gravity * _gravityMultiplyer * Time.deltaTime;

            // Correct charachter controller height when in the air jumping
            float calculatedHeight = _topHeadPostion.position.y - _leftFootPosition.position.y;
            _charachterController.height = calculatedHeight;
            Vector3 calculatedCenter = transform.InverseTransformPoint(_leftFootPosition.position * 0.5f + _topHeadPostion.position * 0.5f);
            _charachterController.center = calculatedCenter;
            // Calculate the distance from ground
            RaycastHit info;
            Physics.SphereCast(transform.position + transform.up * 0.5f, _charachterController.radius, Vector3.down, out info, 10f);
            _distanceFromGround = _leftFootPosition.position.y - info.point.y;
        }

        /*  -----  LANDING  -----  */
        if (!_previouslyGrounded && _charachterController.isGrounded) {
            _isJumping = false;
        }

        // Apply calculations to the animator
        _animator.SetFloat(_forwardSpeedHash, _currentForwardSpeed);
        _animator.SetFloat(_sideSpeedHash, _currentSideSpeed);
        _animator.SetBool(_jumpHash, _jumpButtonPressed);
        _animator.SetFloat(_distanceFromGroundHash, _distanceFromGround);

        _previouslyGrounded = _charachterController.isGrounded;
    }

    void OnAnimatorMove() {
        // Use root motion to move around if not jumping
        if (_charachterController.isGrounded) {
            _moveDirection.x = _animator.deltaPosition.x / Time.deltaTime;
            _moveDirection.z = _animator.deltaPosition.z / Time.deltaTime;
        }
        else {
            _moveDirection.x = transform.forward.x * _inAirMovementSpeed * _verticalInput;
            _moveDirection.z = transform.forward.z * _inAirMovementSpeed * _verticalInput;
        }
        // Move the carachter controlled for the calculated deltaMovePosition
        _charachterController.Move(_moveDirection * Time.deltaTime);
    }
}