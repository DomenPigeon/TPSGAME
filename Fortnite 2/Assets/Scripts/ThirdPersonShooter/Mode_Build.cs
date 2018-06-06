using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mode_Build : Mode {

    // Serialized properties
    [SerializeField] private ConstructionType   _constructionType = ConstructionType.Wall;
    [SerializeField] private Material           _toBeBuildMaterial;
    [SerializeField] private Material           _cantBuildMaterial;

    // Private properties
    private ConstructionType    _previousConstructionType = ConstructionType.None;
    private GameObject          _toBeBuildConstruction;

    public override void OnEnterMode() {
        base.OnEnterMode();
        Debug.Log("Entering Build Mode.");
        _toBeBuildConstruction = Instantiate(ConstructionManager.instance.GetConstructionGameobject(_constructionType));
        _toBeBuildConstruction.GetComponent<Renderer>().material = _toBeBuildMaterial;
        Destroy(_toBeBuildConstruction.GetComponent<Collider>());
        _toBeBuildConstruction.transform.SetParent(_modeManager.environment);
    }

    private void OnUpdateCoreBlock() {
        if (Input.GetButtonDown("BuildKey1"))
            _constructionType = ConstructionType.Wall;
        if (Input.GetButtonDown("BuildKey2"))
            _constructionType = ConstructionType.Stairs;
        if (Input.GetButtonDown("BuildKey3"))
            _constructionType = ConstructionType.Floor;

        if(_previousConstructionType != _constructionType) {
            Destroy(_toBeBuildConstruction);
            _toBeBuildConstruction = null;
            _toBeBuildConstruction = Instantiate(ConstructionManager.instance.GetConstructionGameobject(_constructionType));
            _toBeBuildConstruction.GetComponent<Renderer>().material = _toBeBuildMaterial;
            Destroy(_toBeBuildConstruction.GetComponent<Collider>());
            _toBeBuildConstruction.transform.SetParent(_modeManager.environment);
        }

        UpdateToBeBuildConstructionPosition();

        _previousConstructionType = _constructionType;
    }

    void UpdateToBeBuildConstructionPosition() {
        // If you look upwards you change the position of the construction to be build for one unit
        int cameraPositionMultiplayer = _thirdPersonCamera.transform.rotation.eulerAngles.x > 90 ? 2 : 1;
        Vector3 pushInFrontPosition = transform.forward * GridManager.instance.xSize * cameraPositionMultiplayer;

        Vector3 floorPlayerTransform = GridManager.instance.AlignToGrid(transform.position + pushInFrontPosition);

        Vector3 adjustedPosition = floorPlayerTransform;

        if(_constructionType == ConstructionType.Floor) {
            adjustedPosition.y -= GridManager.instance.ySize / 2;
        }
        else {
            Vector3 constructionRotation = _toBeBuildConstruction.transform.rotation.eulerAngles;
            constructionRotation.y = Mathf.Round(transform.rotation.eulerAngles.y / 90) * 90f;
            _toBeBuildConstruction.transform.rotation = Quaternion.Euler(constructionRotation);
        }

        _toBeBuildConstruction.transform.position = adjustedPosition;
    }

    public override void OnExitMode() {
        base.OnEnterMode();
        Destroy(_toBeBuildConstruction);
        _toBeBuildConstruction = null;
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

    private void OnDrawGizmos() {
        switch (_constructionType) {
            case ConstructionType.Wall:

            break;
        }
    }
}
