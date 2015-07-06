using UnityEngine;
using System.Collections;

public class Rolling : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (new Vector3 (0f, -5f, 0f));
	}
}
