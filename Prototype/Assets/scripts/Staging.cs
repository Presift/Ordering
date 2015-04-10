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
//			Debug.Log (model.selectedTile.name);

//			if( !model.selectedTile.preset )
//			{
				model.StartMove( null );
//			}
//			model.UpdateSubmitButton ();

		}

	}


//	void UpdateSubmitButton()
//	{
//		bool submissionReady = model.ReadyForSubmission();
//		model.controller.ActivateSubmissionButton( submissionReady );
//	}
}
