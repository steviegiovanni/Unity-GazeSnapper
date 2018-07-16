using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class InputExtension : MonoBehaviour, IHasRay {
    public Snapper snapper;

    public Ray GetRay()
    {
        return GetComponent<GazeStabilizer>().StableRay;
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (snapper != null)
            snapper.Ray = GetRay();
    }
}
