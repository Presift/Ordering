using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

	public View view;
	public Model model;
	public Button submitButton;
	public Text rules;
	public Logic logic;
	public Text scoreDisplay;
	public Text levelDisplay;


	// Use this for initialization
	void Start () {

		scoreDisplay.text = "Score : " + model.score;
		levelDisplay.text = "Level : " + ( model.currentLevel + 1 );
		NewTrial ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void ActivateSubmissionButton( bool activate )
	{
	
		submitButton.interactable = activate;
	}

	public void CheckSubmission()
	{
		if(model.impossible)
		{
			Debug.Log ("INCORRECT");
			RespondToAnswer( false );

		}
		else
		{
			string submissionKey = model.OrderedTileKey ();

			if( logic.trialRules.correctSubmissions.ContainsKey( submissionKey))
			{
				Debug.Log ("CORRECT");
				RespondToAnswer( true );

			}
			else
			{
				Debug.Log ("INCORRECT");
				RespondToAnswer (false);

			}
		}
	

	}

	public void CheckImpossible()
	{
		if(model.impossible )
		{
			Debug.Log ("CORRECT");
			RespondToAnswer( true );
		}
		else
		{
			Debug.Log ("INCORRECT");
			RespondToAnswer( false );
		}
	}

	public void RespondToAnswer( bool correctAnswer )
	{
//		Debug.Log (correctAnswer);
		if (correctAnswer) {
			int scoreIncrease = ( model.currentLevel + 1 ) * model.pointsForCorrect;
			model.score += scoreIncrease;
			scoreDisplay.text = "Score : " + model.score;

			//if trial is not yet completed
			if( model.currentProblemInTrial < ( logic.problemsPerTrial - 1 ))
			{
				//increase problem count
				model.currentProblemInTrial++;
				//create new problem
				NewProblem();
			}
			else
			{
				//update level
				model.UpdateLevel( correctAnswer );
				levelDisplay.text = "Level : " + ( model.currentLevel + 1 );
				//start new trial
				NewTrial();
			}
		} 
		else 
		{
			model.UpdateLevel( correctAnswer );
			levelDisplay.text = "Level : " + ( model.currentLevel + 1 );
		}
	}

	public void NewTrial()
	{
//		Debug.Log ("starting new trial ");
		logic.UpdateLevelingStats (model.currentLevel);
		model.currentProblemInTrial = 0; 
		//destroy children of view
		DestroyChildren (view.gameObject.transform);
		//create new board
		model.stagingAreas = view.CreateBoard (logic.tilesCount);

		bool submissionReady = model.ReadyForSubmission();
		ActivateSubmissionButton( submissionReady );

		string ruleList = logic.CreateRules ( model.tilesToOrder, model.holders );
		rules.text = ruleList;

	}
	
	public void NewProblem()
	{
		view.WipePreviousProblem (model.tilesToOrder, model.holders, model.stagingAreas);

		Debug.Log ("creating new problem");
		//determine logic for new problem and set board for new problem
		string presetBoard = logic.NewProblemSetUp (model.tilesToOrder);

		view.PresetTilesWithHolders (presetBoard, model.tilesToOrder, model.holders);

		bool submissionReady = model.ReadyForSubmission();
		ActivateSubmissionButton( submissionReady );
	}

	public void DestroyChildren( Transform parent )
	{
		
		foreach (Transform child in parent) 
		{
			Destroy(child.gameObject);
		}
	}
}
