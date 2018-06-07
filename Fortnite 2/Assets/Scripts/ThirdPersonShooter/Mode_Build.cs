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

        InitializeToBeBuildConstruction(_constructionType, _toBeBuildMaterial);
    }

    private void InitializeToBeBuildConstruction(ConstructionType type, Material material) {
        Destroy(_toBeBuildConstruction);
        _toBeBuildConstruction = null;

        _toBeBuildConstruction = Instantiate(ConstructionManager.instance.GetConstructionGameobject(type));
        _toBeBuildConstruction.GetComponent<Renderer>().material = material;

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
            InitializeToBeBuildConstruction(_constructionType, _toBeBuildMaterial);
        }

        UpdateToBeBuildConstructionPosition();

        _previousConstructionType = _constructionType;
    }

    void UpdateToBeBuildConstructionPosition() {
        Vector3 adjustedPosition = Vector3.zero;
        Vector3 playerPosition = transform.position + Vector3.up * _modeManager.characterController.height / 2f;
        // If you look upwards you move forward the position of the construction to be build for one unit
        float cameraXrotation = _thirdPersonCamera.transform.rotation.eulerAngles.x;
        // Warning: HARD CODED VALUES especially 91, also 60 and 269
        int cameraPositionMultiplayer = cameraXrotation > 60 && cameraXrotation < 91 ? 0 : (cameraXrotation > 269 ? 2 : 1);

        Vector3 constructionRotation = _toBeBuildConstruction.transform.rotation.eulerAngles;
        constructionRotation.y = Mathf.Round(transform.eulerAngles.y / 90) * 90f;

        if(_constructionType == ConstructionType.Floor) {
            if (cameraPositionMultiplayer > 0)
                playerPosition += transform.forward * GridManager.instance.xSize;
            adjustedPosition = GridManager.instance.AlignToGrid(playerPosition, 8);
        }
        else if(_constructionType == ConstructionType.Stairs) {
            playerPosition += transform.forward * GridManager.instance.xSize * cameraPositionMultiplayer;
            adjustedPosition = GridManager.instance.AlignToGrid(playerPosition, 14);
            _toBeBuildConstruction.transform.rotation = Quaternion.Euler(constructionRotation);
        }
        else {
            if(cameraPositionMultiplayer > 1)
                playerPosition += transform.forward * GridManager.instance.xSize;
            switch ((int)constructionRotation.y) {
                case 0: adjustedPosition = GridManager.instance.AlignToGrid(playerPosition, 11); break;
                case 90: adjustedPosition = GridManager.instance.AlignToGrid(playerPosition, 10); break;
                case 180: adjustedPosition = GridManager.instance.AlignToGrid(playerPosition, 9); break;
                case 270: adjustedPosition = GridManager.instance.AlignToGrid(playerPosition, 12); break;
                default: adjustedPosition = GridManager.instance.AlignToGrid(playerPosition, 11); break;
            }

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
        if (_modeManager == null) return;

        Vector3 playerPosition = transform.position + Vector3.up * _modeManager.characterController.height / 2f;
        DrawGridSquareVertices(playerPosition);
        DrawGridSquareFaceCenters(playerPosition);
    }
    private void DrawGridSquareVertices(Vector3 playerPosition) {
        float sphereSize = 0.1f;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 0), sphereSize);
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 1), sphereSize);
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 2), sphereSize);
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 3), sphereSize);
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 4), sphereSize);
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 5), sphereSize);
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 6), sphereSize);
        Gizmos.DrawSphere(GridManager.instance.AlignToGrid(playerPosition, 7), sphereSize);
    }
    private void DrawGridSquareFaceCenters(Vector3 playerPosition) {
        float sphereSize = 0.05f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GridManager.instance.AlignToGrid(playerPosition, 8), sphereSize);
        Gizmos.DrawWireSphere(GridManager.instance.AlignToGrid(playerPosition, 9), sphereSize);
        Gizmos.DrawWireSphere(GridManager.instance.AlignToGrid(playerPosition, 10), sphereSize);
        Gizmos.DrawWireSphere(GridManager.instance.AlignToGrid(playerPosition, 11), sphereSize);
        Gizmos.DrawWireSphere(GridManager.instance.AlignToGrid(playerPosition, 12), sphereSize);
        Gizmos.DrawWireSphere(GridManager.instance.AlignToGrid(playerPosition, 13), sphereSize);
    }
}

