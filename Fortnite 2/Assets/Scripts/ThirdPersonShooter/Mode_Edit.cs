using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mode_Edit : Mode {

    public override void OnEnterMode() {
        base.OnEnterMode();
        Debug.Log("Entering Edit Mode.");
    }
    public override void OnExitMode() {
        base.OnEnterMode();
        Debug.Log("Exiting Edit Mode.");
    }

    public override ModeType GetModeType() {
        return ModeType.Edit;
    }

    public override ModeType OnUpdate() {
        if (Input.GetButtonDown("Build")) return ModeType.Build;
        if (Input.GetButtonDown("Weapon")) return ModeType.Weapon;

        return ModeType.Edit;
    }
}
