﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModeType { Weapon, Build, Edit}

[RequireComponent(typeof(ThirdPersonController))]
public class ModeManager : MonoBehaviour {

    // Serialized properties
    [SerializeField] private Camera     _camera;
    [SerializeField] private ModeType   _currentModeType            = ModeType.Build;
    [SerializeField] float              _cursorSensorLength         = 6.0f;
    [SerializeField] float              _cursorSensorMoveFromOrigin = 2.0f;

    // Private properties
    private Dictionary<ModeType, Mode>  _modes                  = new Dictionary<ModeType, Mode>();
    private Mode                        _currentMode            = null;
    private RaycastHit                  _cursorSensorHitInfo;
    private Ray                         _cursorRay              = new Ray();
    private Vector3                     _cursorSensorPoint      = Vector3.zero;
    private Transform                   _environment            = null;
    private CharacterController         _characterController    = null;

    // Public properties
    public CharacterController      characterController { get { return _characterController; } }
    public RaycastHit               cursorSensorHitInfo { get { return _cursorSensorHitInfo; } }
    public Vector3                  cursorSensorPoint   { get { return _cursorSensorPoint; } }
    /// <summary>
    /// Environment transform, useful to order everything with parent setting
    /// </summary>
    public Transform                environment         { get { return _environment; } }

    private void Start() {
        _characterController = GetComponent<CharacterController>();
        GameObject environmentGameObject = GameObject.FindGameObjectWithTag("Environment");
        _environment = environmentGameObject.transform;

        FetchAllModes();

        // Set the current mode
        if (_modes.ContainsKey(_currentModeType)) {
            _currentMode = _modes[_currentModeType];
            _currentMode.OnEnterMode();
        }
        else _currentMode = null;
    }
    private void FetchAllModes() {
        Mode[] modes = GetComponents<Mode>();
        foreach(Mode mode in modes) {
            if(mode != null && !_modes.ContainsKey(mode.GetModeType())) {
                _modes.Add(mode.GetModeType(), mode);
                mode.SetModeManager(this, _camera);
            }
        }
    }

    private void Update() {
        if (_currentMode == null) return;
        ModeType newModeType = _currentMode.OnUpdate();

        if(newModeType != _currentModeType) {
            Mode newMode = null;
            if (_modes.TryGetValue(newModeType, out newMode)) {
                _currentMode.OnExitMode();
                newMode.OnEnterMode();
                _currentMode = newMode;
                _currentModeType = newModeType;
            }
            else {
                Debug.LogWarning("This ModeType does not exist!");
            }
        }
    }

    private void FixedUpdate() {
        CursorSensor();
    }


    void CursorSensor() {
        Vector3 direction = _camera.transform.forward;
        Vector3 origin = _camera.ViewportToWorldPoint(Vector3.zero) + direction * _cursorSensorMoveFromOrigin;
        _cursorRay = new Ray(origin, direction);

        Physics.Raycast(_cursorRay, out _cursorSensorHitInfo, _cursorSensorLength);
        
        if (_cursorSensorHitInfo.collider == null || _cursorSensorHitInfo.collider.tag == "Construction")
            _cursorSensorPoint = origin + direction * _cursorSensorLength;
        else {
            _cursorSensorPoint = _cursorSensorHitInfo.point;
        }
    }

    private void OnDrawGizmos() {
        DrawCursorDirection(Color.black);
    }

    private void DrawCursorDirection(Color color) {
        Gizmos.color = color;
        Gizmos.DrawLine(_cursorRay.origin, _cursorRay.direction * _cursorSensorLength);
    }
}
