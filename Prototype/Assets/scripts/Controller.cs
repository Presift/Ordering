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


	// Use this for initialization
	void Start () {

//		model.stagingAreas = view.CreateBoard ( model.currentTotalTileCount );
//		bool submissionReady = model.ReadyForSubmission();
//		ActivateSubmissionButton( submissionReady );
//
//		logic.tilesToOrder = model.tilesToOrder;

		NewTrial ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void ActivateSubmissionButton( bool activate )
	{
	
		submitButton.interactable = activate;
	}

	public void CheckAnswer()
	{
		bool answer = true;
		List<Tile> submission = model.OrderedTiles ();
		for (int i = 0; i < logic.trialRules.Count; i ++) 
		{
			if( !logic.trialRules[ i ].SubmissionFollowsRule( submission ))
			{
				answer =  false;
			}
		}
		Debug.Log ("called ");
		RespondToAnswer (answer);
	}

	public void RespondToAnswer( bool correctAnswer )
	{
		Debug.Log (correctAnswer);
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
				//start new trial
				NewTrial();
			}
		} 
		else 
		{
			model.UpdateLevel( correctAnswer );
		}
	}

	public void NewTrial()
	{
		Debug.Log ("starting new trial ");
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
		Debug.Log ("creating new problem");
		//determine logic for new problem

		//set board for new problem
	}

	public void DestroyChildren( Transform parent )
	{
		
		foreach (Transform child in parent) 
		{
			Destroy(child.gameObject);
		}
	}
}
