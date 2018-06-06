using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mode : MonoBehaviour {

    // Protected properties
    protected ModeManager   _modeManager;
    protected Camera        _thirdPersonCamera;

    // Abtract Methods
    public abstract ModeType GetModeType();
    public abstract ModeType OnUpdate();

    // Default handlers
    public virtual void OnEnterMode() { }
    public virtual void OnExitMode() { }

    // Public Method
    // Called by the parent mode machine to assign its reference
    public void SetModeManager(ModeManager modeManager, Camera thirdPersonCamera) {
        _modeManager = modeManager;
        _thirdPersonCamera = thirdPersonCamera;
    }
}
