using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mode_Build : Mode {

    private GameObject _wall;

    public override void OnEnterMode() {
        base.OnEnterMode();
        Debug.Log("Entering Build Mode.");
        _wall = Instantiate(ConstructionManager.instance.GetConstructionGameobject(ConstructionType.Wall));
    }

    private void OnUpdateCoreBlock() {
        _wall.transform.position = GridManager.instance.AlignToGrid(_modeManager.cursorSensorPoint);
        
    }

    public override void OnExitMode() {
        base.OnEnterMode();
        Debug.Log("Exiting Build Mode.");
    }

    // Default functions
    public override ModeType GetModeType() {
        return ModeType.Build;
    }
    public override ModeType OnUpdate() {
        if (Input.GetButtonDown("Edit")) return ModeType.Edit;
        if (Input.GetButtonDown("Weapon")) return ModeType.Weapon;

        OnUpdateCoreBlock();

        return ModeType.Build;
    }
}
