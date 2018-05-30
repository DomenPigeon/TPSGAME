using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModeType { Weapon, Build, Edit}

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
    private Vector3                     _cursorSensorPoint   = Vector3.zero;

    // Public properties
    public RaycastHit   cursorSensorHitInfo { get { return _cursorSensorHitInfo; } }
    public Vector3      cursorSensorPoint { get { return _cursorSensorPoint; } }

    private void Start() {
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
                mode.SetModeManager(this);
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
        Ray ray = new Ray(origin, direction);

        Physics.Raycast(ray, out _cursorSensorHitInfo, _cursorSensorLength);
        
        if (_cursorSensorHitInfo.collider == null || _cursorSensorHitInfo.collider.tag == "Construction")
            _cursorSensorPoint = origin + direction * _cursorSensorLength;
        else {
            _cursorSensorPoint = _cursorSensorHitInfo.point;
        }

        Debug.DrawRay(origin, direction * _cursorSensorLength, Color.green);
        Debug.DrawLine(Vector3.zero, _cursorSensorHitInfo.point, Color.red);
        Debug.DrawLine(Vector3.zero, GridManager.instance.AlignToGrid(_cursorSensorHitInfo.point), Color.black);
        Debug.Log(_cursorSensorHitInfo.point);
    }

}
