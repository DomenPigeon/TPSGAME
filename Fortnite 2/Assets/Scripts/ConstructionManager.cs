using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConstructionType { None, Wall, Floor, Stairs }

public class ConstructionManager : MonoBehaviour {

    // Serialized properties
    [SerializeField] private GameObject _floor;
    [SerializeField] private GameObject _stairs;
    [SerializeField] private GameObject _wall;

    // Statics
    private static ConstructionManager _instance = null;
    public static ConstructionManager instance {
        get {
            if (_instance == null)
                _instance = (ConstructionManager)FindObjectOfType(typeof(ConstructionManager));
            return _instance;
        }
    }

    public GameObject GetConstructionGameobject(ConstructionType type) {
        switch (type) {
            case ConstructionType.Floor:
            return _floor;
            case ConstructionType.Stairs:
            return _stairs;
            case ConstructionType.Wall:
            return _wall;
            default:
            return new GameObject();
        }
    }
}
