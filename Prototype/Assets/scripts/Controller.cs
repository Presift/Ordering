using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	public View view;
	public Model model;

	// Use this for initialization
	void Start () {
		model.stagingAreas = view.CreateBoard ( model.currentTotalTileCount );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
