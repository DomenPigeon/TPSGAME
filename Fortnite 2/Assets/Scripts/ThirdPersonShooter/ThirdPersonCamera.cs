using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

    [Header("Charachter properties")]
    [SerializeField] private Transform              _charachterTransform;
    [SerializeField] private float                  _charachterHeight           = 2.0f;
    [SerializeField] private float                  _rDistanceFromCharacter     = 2f;
    [SerializeField] private float                  _xAxisOfCameraPosition      = 0.2f;
    [SerializeField] private float                  _cameraHeight               = 1.6f;
    [SerializeField] private float                  _cameraFootAngleChange      = 65.0f;
    [SerializeField] private float                  _cameraFootAngleSmoothness  = 65.0f;

    [Header("Other Camera properties")]
    [SerializeField] private float  _XSensitivity           = 2f;
    [SerializeField] private float  _YSensitivity           = 2f;
    [SerializeField] private bool   _clampVerticalRotationX = true;
    [SerializeField] private float  _clampMin               = -90F;
    [SerializeField] private float  _clampMax               = 90F;
    [SerializeField] private bool   _smooth                 = false;
    [SerializeField] private float  _smoothTime             = 5f;
    [SerializeField] private bool   _lockCursor             = true;

    private Quaternion  _characterTargetRot;
    private Quaternion  _cameraTargetRot;
    private bool        _cursorIsLocked         = true;
    private float       _cameraRelativeYZAngle  = 0f;

    private void Start() {
        _characterTargetRot = _charachterTransform.localRotation;
        _cameraTargetRot = transform.localRotation;

        SetCursorLock(true);
    }

    private void Update() {
        LookRotation();
    }

    private void LookRotation() {
        float yRot = Input.GetAxis("Mouse X") * _XSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * _YSensitivity;

        if (_clampVerticalRotationX)
            _cameraTargetRot = ClampRotationAroundXAxis(_cameraTargetRot);

        UpdateCameraPosition(_charachterTransform, transform, xRot, yRot);
        UpdateCursorLock();
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q) {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, _clampMin, _clampMax);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }
    public void UpdateCursorLock() {
        if (_lockCursor == false) return;

        if (Input.GetKeyUp(KeyCode.Escape)) {
            SetCursorLock(false);
        }
        else if (Input.GetMouseButtonUp(0)) {
            SetCursorLock(true);
        }
    }
    public void SetCursorLock(bool value) {
        _cursorIsLocked = value;
        if (_cursorIsLocked) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void UpdateCameraPosition(Transform character, Transform camera, float xRotInput, float yRotInput) {
        _characterTargetRot *= Quaternion.Euler(0f, yRotInput, 0f);
        _cameraTargetRot = _characterTargetRot;
        if (_smooth) {
            character.localRotation = Quaternion.Slerp(character.localRotation, _characterTargetRot,
                _smoothTime * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, _cameraTargetRot,
                _smoothTime * Time.deltaTime);
        }
        else {
            character.localRotation = _characterTargetRot;
            camera.localRotation = _cameraTargetRot;
        }

        // Rotate camera around the player on the XZ axis.
        Vector3 cameraTargetPos = character.localPosition + character.right * _xAxisOfCameraPosition;
        cameraTargetPos.y += _cameraHeight;
        cameraTargetPos.x -= Mathf.Sin(character.eulerAngles.y * Mathf.Deg2Rad) * _rDistanceFromCharacter;
        cameraTargetPos.z -= Mathf.Cos(character.eulerAngles.y * Mathf.Deg2Rad) * _rDistanceFromCharacter;

        if (_smooth) {
            camera.localPosition = Vector3.Lerp(camera.localPosition, cameraTargetPos, _smoothTime * Time.deltaTime);
        }
        else {
            camera.localPosition = cameraTargetPos;
        }

        // Rotate camera around the player on the YZ axis relative to the player
        _cameraRelativeYZAngle -= xRotInput;
        _cameraRelativeYZAngle = Mathf.Clamp(_cameraRelativeYZAngle, _clampMin, _clampMax);
        Vector3 pivotPointToRotateAround = character.localPosition;
        pivotPointToRotateAround.y += _charachterHeight / 2f;
        if (_cameraRelativeYZAngle < -_cameraFootAngleChange) {
            float moveUp = (Mathf.Abs(_cameraFootAngleChange + _cameraRelativeYZAngle)) / _cameraFootAngleSmoothness;
            pivotPointToRotateAround.y += moveUp;
        }
        camera.RotateAround(pivotPointToRotateAround, character.right, _cameraRelativeYZAngle);
    }
}
