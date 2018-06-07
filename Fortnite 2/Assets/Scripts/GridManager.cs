using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Construction {
    private GameObject          _gameObject;
    private Vector3             _startPoint;
    private Vector3             _endPoint;
    private ConstructionType   _type;
    private int                 _gameObjectID;  

    public GameObject           gameObject      { get { return _gameObject; } }
    public Vector3              startPoint      { get { return _startPoint; } }
    public Vector3              endPoint        { get { return _endPoint; } }
    public int                  gameObjectID    { get { return _gameObjectID; } }
    public ConstructionType    type            { get { return _type; } }

    public Construction(int id) {
        _gameObjectID = id;
        _gameObject = null;
        _startPoint = Vector3.zero;
        _endPoint = Vector3.zero;
        _type = ConstructionType.None;
    }
    public Construction(GameObject gameObject, ConstructionType type) {
        _gameObjectID = gameObject.GetInstanceID();
        _gameObject = gameObject;
        _startPoint = Vector3.zero;
        _endPoint = Vector3.zero;
        _type = type;

        CalculateStartEndPoint(gameObject.transform.position, gameObject.transform.rotation);
    }

    private void CalculateStartEndPoint(Vector3 startPosition, Quaternion rot) {
        _startPoint = startPosition;
        _endPoint = startPosition;
        /*
        if (_type == ContstructionType.Wall) {
            // The same wall can be rotated for 180 degrees if it is, we have to correct local right transfrom vector as done below when
            // calculating _endPoint
            int counterRight = 1;
            if (Mathf.RoundToInt(rot.eulerAngles.y) == 180) {
                startPosition.x -= GridManager.instance.xSize;
                counterRight = -1;
            }
            else if (Mathf.RoundToInt(rot.eulerAngles.y) == 90) {
                startPosition.z -= GridManager.instance.zSize;
                counterRight = -1;
            }
            _startPoint = startPosition;
            _endPoint = _startPoint + (_gameObject.transform.right * counterRight) * GridManager.instance.xSize + _gameObject.transform.up * GridManager.instance.ySize;
        }
        else if (_type == ContstructionType.Floor) {
            if (Mathf.RoundToInt(rot.eulerAngles.y) == 90) {
                startPosition.z -= GridManager.instance.zSize;
            }
            else if (Mathf.RoundToInt(rot.eulerAngles.y) == 180) {
                startPosition.x -= GridManager.instance.xSize;
                startPosition.z -= GridManager.instance.zSize;
            }
            else if (Mathf.RoundToInt(rot.eulerAngles.y) == -90) {
                startPosition.x -= GridManager.instance.xSize;
            }
            _startPoint = startPosition;
            // Because the floor stay always in the same spot, we can use global (Vector3.right...) transforms to calculate the end point
            _endPoint = _startPoint + Vector3.right * GridManager.instance.xSize + Vector3.forward * GridManager.instance.zSize;
        }
        else if (_type == ContstructionType.Stairs) {
            if (Mathf.RoundToInt(rot.eulerAngles.y) == 90) {
                startPosition.z -= GridManager.instance.zSize;
            }
            else if (Mathf.RoundToInt(rot.eulerAngles.y) == 180) {
                startPosition.x -= GridManager.instance.xSize;
                startPosition.z -= GridManager.instance.zSize;
            }
            else if (Mathf.RoundToInt(rot.eulerAngles.y) == -90) {
                startPosition.x -= GridManager.instance.xSize;
            }
            _startPoint = startPosition;
            // Because the floor stay always in the same spot, we can use global (Vector3.right...) transforms to calculate the end point
            _endPoint = _startPoint + Vector3.right * GridManager.instance.xSize + Vector3.forward * GridManager.instance.zSize + Vector3.up * GridManager.instance.ySize;
        }

    */
    }
}

public class GridManager : MonoBehaviour {

    // Private properties
    private float _xSize = 11.43f;
    private float _ySize = 8.91f;
    private float _zSize = 11.43f;

    // Public properties
    public float xSize { get { return _xSize; } }
    public float ySize { get { return _ySize; } }
    public float zSize { get { return _zSize; } }

    // Statics
    public static GridManager instance {
        get {
            if (_instance == null)
                _instance = (GridManager)FindObjectOfType(typeof(GridManager));
            return _instance;
        }
    }
    private static GridManager          _instance               = null;
    private static List<Construction>   _buildedConstructions   = new List<Construction>();

    /// <summary>
    /// Rounds the position to fit in the grid.
    /// </summary>
    /// <param name="position">Position to be aligned with the grid.</param>
    /// <returns></returns>
    public Vector3 AlignToGrid(Vector3 position) {
        Vector3 alignedPosition = position;
        alignedPosition.x /= _xSize;
        alignedPosition.x = Mathf.Round(alignedPosition.x);
        alignedPosition.x *= _xSize;

        alignedPosition.y /= _ySize;
        alignedPosition.y = Mathf.Round(alignedPosition.y);
        alignedPosition.y *= _ySize;

        alignedPosition.z /= _zSize;
        alignedPosition.z = Mathf.Round(alignedPosition.z);
        alignedPosition.z *= _zSize;

        return alignedPosition;
    }

    /// <summary>
    /// Remove the builded construction from the list of _buildedConstructions
    /// </summary>
    /// <param name="colliderID">The ID of the gameObject to be deleted.</param>
    public void DeleteConstruction(int colliderID) {
        for(int i=0; i<_buildedConstructions.Count; i++) {
            if (_buildedConstructions[i].gameObjectID == colliderID)
                _buildedConstructions.Remove(_buildedConstructions[i]);
        }
    }

    /// <summary>
    /// Build a new construction if it is on an open slot
    /// </summary>
    /// <param name="construction">The new construction that wants to be builded.</param>
    /// <returns></returns>
    public void BuildConstruction(Construction construction) {
        if(CheckFreeSlot(construction))
            _buildedConstructions.Add(construction);
    }

    /// <summary>
    /// Check if there is a free slot
    /// </summary>
    /// <param name="construction">The new construction that wants to be builded.</param>
    /// <returns></returns>
    public bool CheckFreeSlot(Construction construction) {
        bool isPossibleToBuild = true;
        for (int i = 0; i < _buildedConstructions.Count; i++) {
            if (_buildedConstructions[i].startPoint == construction.startPoint)
                if (_buildedConstructions[i].endPoint == construction.endPoint)
                    isPossibleToBuild = false;
        }
        return isPossibleToBuild;
    }

    /// <summary>
    /// Gets the Construction object from the array of _buildedConstructions, if it exists
    /// </summary>
    /// <param name="colliderID">The gameObject id which corresponds to the object in the list.</param>
    /// <param name="construction">The Construction returne throug the out parameter</param>
    /// <returns></returns>
    public bool GetBuildedConstruction(int colliderID, out Construction construction) {
        construction = new Construction(0);
        for (int i = 0; i < _buildedConstructions.Count; i++) {
            if (_buildedConstructions[i].gameObjectID == colliderID) {
                construction = _buildedConstructions[i];
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the Construction object from the array of _buildedConstructions, if it exists
    /// </summary>
    /// <param name="colliderID">The gameObject id which corresponds to the object in the list.</param>
    /// <param name="construction">The Construction returne throug the out parameter</param>
    /// <param name="buttonPosition">Calculated position of the edit button</param>
    /// <returns></returns>
    public bool GetBuildedConstruction(int colliderID, out Construction construction, out Vector3 buttonPosition) {
        construction = new Construction(0);
        buttonPosition = Vector3.zero;
        for (int i = 0; i < _buildedConstructions.Count; i++) {
            if (_buildedConstructions[i].gameObjectID == colliderID) {
                Construction b = _buildedConstructions[i];
                construction = b;
                // Calculating the center position of the Build; add half of the distance from start to end in the direction from start to end
                Vector3 direction = (b.endPoint - b.startPoint).normalized;
                buttonPosition = b.startPoint + direction * (Vector3.Distance(b.startPoint, b.endPoint) / 2f);
                return true;
            }
        }
        return false;
    }
}
