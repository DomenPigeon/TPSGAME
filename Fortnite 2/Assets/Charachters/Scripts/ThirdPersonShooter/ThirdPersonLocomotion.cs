using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Setting up charachter
/// - Input Button Run
/// - Horizontal and Vertical Button should not snap, and have gravity and sensitivity set to 6


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class ThirdPersonLocomotion: MonoBehaviour {

    // Serialized properties
    [SerializeField] private Camera                 _camera                     = null;
    [SerializeField] private float                  _runSpeed                   = 2.0f;
    [SerializeField] private float                  _walkSpeed                  = 1.0f;
    [SerializeField] private float                  _jumpSpeed                  = 5.0f;
    [SerializeField] private float                  _inAirMovementSpeed         = 1.0f;
    [SerializeField] private float                  _gravityMultiplyer          = 2.0f;
    [SerializeField] private float                  _stickToGroundForce         = 2.0f;
    [SerializeField] private float                  _forwardBackwardsTurnAngle  = 15.0f;
    [SerializeField] private MouseLook _mouseLook = new MouseLook();

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
    private Vector3                 _cursorSensorHitPoint   = Vector3.zero;
    private RaycastHit              _cursorSensorHitInfo;
    private Transform               _leftFootPosition       = null;
    private Transform               _topHeadPostion         = null;

    // Hashes
    private int _forwardSpeedHash       = Animator.StringToHash("forwardSpeed");
    private int _sideSpeedHash          = Animator.StringToHash("sideSpeed");
    private int _jumpHash               = Animator.StringToHash("Jump");
    private int _distanceFromGroundHash = Animator.StringToHash("distanceFromGround");

    // Public properties
    public Vector3 cursorSensorHitPoint { get { return _cursorSensorHitPoint; } }
    public RaycastHit cursorSensorHitInfo { get { return _cursorSensorHitInfo; } }

    private void Start() {
        _animator = GetComponent<Animator>();
        _animator.applyRootMotion = true;
        _charachterController = GetComponent<CharacterController>();
        _initialHeight = _charachterController.height;
        _initialCenterPos = _charachterController.center;
        _mouseLook.Init(transform, _camera.transform, _charachterController.height);

        _leftFootPosition = GetComponentsInChildren<LocateChildObject>()[0].transform;
        _topHeadPostion = GetComponentsInChildren<LocateChildObject>()[1].transform;
    }

    private void Update() {
        _mouseLook.LookRotation(transform, _camera.transform);

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
        _currentForwardSpeed = Mathf.Lerp(_currentForwardSpeed, _verticalInput * speed, Time.deltaTime * _defaultLerpMultiplyer);


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
            _charachterController.height = Mathf.Lerp(_charachterController.height, calculatedHeight, Time.deltaTime * _defaultLerpMultiplyer);
            Vector3 calculatedCenter = _initialCenterPos;
            calculatedCenter.y += (_initialHeight - calculatedHeight);
            _charachterController.center = Vector3.Lerp(_charachterController.center, calculatedCenter, Time.deltaTime * _defaultLerpMultiplyer);

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


[Serializable]
public class MouseLook {
    [Header("Camera not child of Character")]
    [SerializeField] private bool isCameraChildOfCharacter      = false;
    [SerializeField] private float rDistanceFromCharacter       = 2f;
    [SerializeField] private float xAxisOfCameraPosition        = 0.2f;
    [SerializeField] private float cameraHeight                 = 1.6f;
    [SerializeField] private float cameraFootAngleChange        = 65.0f;
    [SerializeField] private float cameraFootAngleSmoothness    = 65.0f;

    [Header("Other MouseLook properties")]
    [SerializeField] private float XSensitivity = 2f;
    [SerializeField] private float YSensitivity = 2f;
    [SerializeField] private bool clampVerticalRotationX = true;
    [SerializeField] private float clampMin = -90F;
    [SerializeField] private float clampMax = 90F;
    [SerializeField] private bool smooth;
    [SerializeField] private float smoothTime = 5f;
    [SerializeField] private bool lockCursor = true;

    private Quaternion CharacterTargetRot;
    private Quaternion CameraTargetRot;
    private bool cursorIsLocked = true;
    private float cameraRelativeYZAngle = 0f;
    private float characterHeight = 1.65f;

    public void Init(Transform character, Transform camera, float height) {
        CharacterTargetRot = character.localRotation;
        CameraTargetRot = camera.localRotation;
        characterHeight = height;
    }

    public void LookRotation(Transform character, Transform camera) {
        float yRot = Input.GetAxis("Mouse X") * XSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

        if (clampVerticalRotationX)
            CameraTargetRot = ClampRotationAroundXAxis(CameraTargetRot);

        if (isCameraChildOfCharacter)
            CameraChildOfCharacter(character, camera, xRot, yRot);
        else
            CameraNotChildOfCharacter(character, camera, xRot, yRot);

        UpdateCursorLock();
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q) {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, clampMin, clampMax);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }
    public void UpdateCursorLock() {
        if (lockCursor == false) return;

        if (Input.GetKeyUp(KeyCode.Escape)) {
            SetCursorLock(false);
        }
        else if (Input.GetMouseButtonUp(0)) {
            SetCursorLock(true);
        }
    }
    public void SetCursorLock(bool value) {
        cursorIsLocked = value;
        if (cursorIsLocked) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void CameraChildOfCharacter(Transform character, Transform camera, float xRotInput, float yRotInput) {
        CharacterTargetRot *= Quaternion.Euler(0f, yRotInput, 0f);
        CameraTargetRot *= Quaternion.Euler(-xRotInput, 0f, 0f);
        if (smooth) {
            character.localRotation = Quaternion.Slerp(character.localRotation, CharacterTargetRot,
                smoothTime * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, CameraTargetRot,
                smoothTime * Time.deltaTime);
        }
        else {
            character.localRotation = CharacterTargetRot;
            camera.localRotation = CameraTargetRot;
        }
    }
    private void CameraNotChildOfCharacter(Transform character, Transform camera, float xRotInput, float yRotInput) {
        CharacterTargetRot *= Quaternion.Euler(0f, yRotInput, 0f);
        CameraTargetRot = CharacterTargetRot;
        if (smooth) {
            character.localRotation = Quaternion.Slerp(character.localRotation, CharacterTargetRot,
                smoothTime * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, CameraTargetRot,
                smoothTime * Time.deltaTime);
        }
        else {
            character.localRotation = CharacterTargetRot;
            camera.localRotation = CameraTargetRot;
        }

        // Rotate camera around the player on the XZ axis.
        Vector3 cameraTargetPos = character.localPosition + character.right * xAxisOfCameraPosition;
        cameraTargetPos.y += cameraHeight;
        cameraTargetPos.x -= Mathf.Sin(character.eulerAngles.y * Mathf.Deg2Rad) * rDistanceFromCharacter;
        cameraTargetPos.z -= Mathf.Cos(character.eulerAngles.y * Mathf.Deg2Rad) * rDistanceFromCharacter;

        if (smooth) {
            camera.localPosition = Vector3.Lerp(camera.localPosition, cameraTargetPos, smoothTime * Time.deltaTime);
        }
        else {
            camera.localPosition = cameraTargetPos;
        }

        // Rotate camera around the player on the YZ axis relative to the player
        cameraRelativeYZAngle -= xRotInput;
        cameraRelativeYZAngle = Mathf.Clamp(cameraRelativeYZAngle, clampMin, clampMax);
        Vector3 pivotPointToRotateAround = character.localPosition;
        pivotPointToRotateAround.y += characterHeight / 2f;
        if (cameraRelativeYZAngle < -cameraFootAngleChange) {
            float moveUp = (Mathf.Abs(cameraFootAngleChange + cameraRelativeYZAngle)) / cameraFootAngleSmoothness;
            pivotPointToRotateAround.y += moveUp;
        }
        camera.RotateAround(pivotPointToRotateAround, character.right, cameraRelativeYZAngle);
    }
}