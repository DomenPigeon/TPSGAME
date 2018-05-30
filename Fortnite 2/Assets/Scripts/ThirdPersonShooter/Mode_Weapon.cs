using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mode_Weapon : Mode {

    public override void OnEnterMode() {
        base.OnEnterMode();
        Debug.Log("Entering Weapon Mode.");
    }
    public override void OnExitMode() {
        base.OnEnterMode();
        Debug.Log("Exiting Weapon Mode.");
    }

    public override ModeType GetModeType() {
        return ModeType.Weapon;
    }

    public override ModeType OnUpdate() {
        if (Input.GetButtonDown("Build")) return ModeType.Build;
        if (Input.GetButtonDown("Edit")) return ModeType.Edit;

        return ModeType.Weapon;
    }
}
