using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonModeMachine : MonoBehaviour {
    
    // Private properties
    private Animator _animator = null;

	// Use this for initialization
	void Start () {
        _animator = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

}
