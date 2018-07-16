using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundingBoxTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        var b = new Bounds();
        /*Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            b.Encapsulate(renderer.bounds);
        }*/


        /*MeshFilter[] colliders = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter collider in colliders){
            b.Encapsulate(collider.sharedMesh.bounds);
        }*/

        MeshCollider[] colliders = GetComponentsInChildren<MeshCollider>();
        foreach (MeshCollider collider in colliders)
        {
            b.Encapsulate(collider.sharedMesh.bounds);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center, b.size);
    }
}
