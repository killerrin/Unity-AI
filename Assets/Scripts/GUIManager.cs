using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, Screen.width, Screen.height), "Press Space to Reset Search");
    }
}
