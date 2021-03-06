﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// TODO
/// - sometimes not falling down from jump

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
    [SerializeField] private float                  _crouchingHeight            = 5.0f;
    [SerializeField] private Vector3                _crouchingCenterPosition  = Vector3.zero;
    [SerializeField] private float                  _inAirMovementSpeed         = 1.0f;
    [SerializeField] private float                  _gravityMultiplyer          = 2.0f;
    [SerializeField] private float                  _stickToGroundForce         = 2.0f;

    // Private properties
    private bool        _isRunning              = false;
    private bool        _crouching              = false;
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
    private Transform               _leftFootTransform      = null;
    private Transform               _topHeadTransform       = null;

    // Hashes
    private int _forwardSpeedHash       = Animator.StringToHash("ForwardSpeed");
    private int _sideSpeedHash          = Animator.StringToHash("SideSpeed");
    private int _jumpHash               = Animator.StringToHash("Jump");
    private int _distanceFromGroundHash = Animator.StringToHash("DistanceFromGround");
    private int _crouchingHash          = Animator.StringToHash("Crouching");

    private void Start() {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = true;
        _charachterController = GetComponent<CharacterController>();
        _initialHeight = _charachterController.height;
        _initialCenterPos = _charachterController.center;

        _leftFootTransform = GetComponentsInChildren<LocateChildObject>()[1].transform;
        _topHeadTransform = GetComponentsInChildren<LocateChildObject>()[0].transform;
    }

    private void Update() {
        _isRunning = false;
        if (Input.GetButton("Run")) {
            _isRunning = true;
        }

        if (Input.GetButtonDown("Jump")) {
            if (_crouching)
                _crouching = !_crouching;
            else
                _jumpButtonPressed = true;
        }

        if (Input.GetButtonDown("Crouching")) {
            _crouching = !_crouching;
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

        // If there is a wall in front of you stop
        Ray wallRay = new Ray(transform.position + Vector3.up * _charachterController.height / 2f, transform.forward);
        if (Physics.Raycast(wallRay, _charachterController.radius * 1.7f)) {
            float considerOnlyBackMotion = _verticalInput < 0f ? _verticalInput * speed : 0f;
            _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, considerOnlyBackMotion, Time.deltaTime * _defaultLerpMultiplyer);
        }
        else _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, (_verticalInput * speed), Time.deltaTime * _defaultLerpMultiplyer);

        // Set the speed to zero if it is low
        if (Mathf.Abs(_currentForwardSpeed) < 0.001f) _currentForwardSpeed = 0;


        /*  -----  MOVING LEFT / RIGHT  -----  */
        _currentSideSpeed = _horizontalInput;


        /*  -----  JUMPING  -----  */
        if (_charachterController.isGrounded) {
            _flyingTimer = 0.0f;
            // Apply stick to ground force if grounded
            _moveDirection.y = -_stickToGroundForce;

            if (_jumpButtonPressed) {
                _moveDirection.y = _jumpSpeed;
            }

            // Crouching height correction
            if (_crouching) {
                _charachterController.height = Mathf.Lerp(_charachterController.height, _crouchingHeight, Time.deltaTime * _defaultLerpMultiplyer);
                _charachterController.center = Vector3.Lerp(_charachterController.center, _crouchingCenterPosition, Time.deltaTime * _defaultLerpMultiplyer);
            }
            else {
                _charachterController.height = Mathf.Lerp(_charachterController.height, _initialHeight, Time.deltaTime * _defaultLerpMultiplyer);
                _charachterController.center = Vector3.Lerp(_charachterController.center, _initialCenterPos, Time.deltaTime * _defaultLerpMultiplyer);
            }
        }
        else {
            _flyingTimer += Time.deltaTime;
            if (_flyingTimer >= 0.1f) {
                _jumpButtonPressed = false;
            }
            // Apply modified gravity
            _moveDirection += Physics.gravity * _gravityMultiplyer * Time.deltaTime;

            // Correct charachter controller height when in the air jumping
            float calculatedHeight = _topHeadTransform.position.y - _leftFootTransform.position.y;
            _charachterController.height = calculatedHeight;
            Vector3 calculatedCenter = transform.InverseTransformPoint(_leftFootTransform.position * 0.5f + _topHeadTransform.position * 0.5f);
            calculatedCenter.x = _initialCenterPos.x;
            calculatedCenter.z = _initialCenterPos.z;
            _charachterController.center = calculatedCenter;
        }


        /*  -----  LANDING  -----  */
        if (!_previouslyGrounded && _charachterController.isGrounded) {

        }


        /*  -----  HIT CEILING  -----  */
        if ((_charachterController.collisionFlags & CollisionFlags.Above) != 0)
            _moveDirection.y = 0.0f;


        _distanceFromGround = DistanceFromGruondCalcualtion();

        AnimatorInputs();

        _previouslyGrounded = _charachterController.isGrounded;
    }

    private float DistanceFromGruondCalcualtion() {
        float distanceFromGround = 0;
        float reachMultiplyer = _charachterController.radius;
        Vector3 halfHeight = (_charachterController.height / 2f) * Vector3.up;
        RaycastHit info;

        // Center position
        Physics.SphereCast(transform.position + halfHeight, _charachterController.radius, Vector3.down, out info);
        distanceFromGround = _leftFootTransform.position.y - info.point.y;
        // Forward
        Physics.Raycast(transform.position + halfHeight + transform.forward * reachMultiplyer, Vector3.down, out info, 10f);
        if (distanceFromGround > _leftFootTransform.position.y - info.point.y)
            distanceFromGround = _leftFootTransform.position.y - info.point.y;
        // Backward
        Physics.Raycast(transform.position + halfHeight + (-transform.forward) * reachMultiplyer, Vector3.down, out info, 10f);
        if (distanceFromGround > _leftFootTransform.position.y - info.point.y)
            distanceFromGround = _leftFootTransform.position.y - info.point.y;
        // Right
        Physics.Raycast(transform.position + halfHeight + transform.right * reachMultiplyer, Vector3.down, out info, 10f);
        if (distanceFromGround > _leftFootTransform.position.y - info.point.y)
            distanceFromGround = _leftFootTransform.position.y - info.point.y;
        // Left
        Physics.Raycast(transform.position + halfHeight + (-transform.right) * reachMultiplyer, Vector3.down, out info, 10f);
        if (distanceFromGround > _leftFootTransform.position.y - info.point.y)
            distanceFromGround = _leftFootTransform.position.y - info.point.y;

        return distanceFromGround;
    }

    /// <summary>
    /// Apply calculations to the animator
    /// </summary>
    private void AnimatorInputs() {
        _animator.SetFloat(_forwardSpeedHash, _currentForwardSpeed);
        _animator.SetFloat(_distanceFromGroundHash, _distanceFromGround);
        _animator.SetFloat(_sideSpeedHash, _currentSideSpeed);
        _animator.SetBool(_crouchingHash, _crouching);
        _animator.SetBool(_jumpHash, _jumpButtonPressed);
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