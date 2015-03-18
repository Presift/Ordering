using UnityEngine;
using System.Collections;

public class Staging : MonoBehaviour {

	public Model model;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown()
	{

		if (model.selectedTile != null ) 
		{
			model.StartMove( null );
		}
		
		UpdateSubmitButton ();

	}


	void UpdateSubmitButton()
	{
		bool submissionReady = model.ReadyForSubmission();
		model.controller.ActivateSubmissionButton( submissionReady );
	}
}
