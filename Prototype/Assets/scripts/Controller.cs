using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

	public View view;
	public Model model;
	public MetaData metaData;

	public Button submitButton;
	public Button impossibleButton;
	public Button continueButton;
	public Button playAgainButton;

	public Text rules;
	public Logic logic;
//	public Text scoreDisplay;
//	public Text levelDisplay;
//	public Text problemDisplay;
	public Text statsDisplay;
	public GameObject statPanel;
	public GameObject rulePanel;
	public GameObject buttonPanel;
	public GameObject trialsEndDisplay;

	public Button upLevel;
	public Button downLevel;

	// Use this for initialization
	void Start () {

		UpdateDisplay ();

		NewTrial ();

		if( GameData.dataControl.debugOn )
		{
			ShowDebug();
		}
		else
		{
			HideDebug();
		}
	}
	
	// Update is called once per frame
	void Update () {
//		if(Input.GetKeyDown(KeyCode.UpArrow ))
//		{
//			model.UpdateLevel( true );
//			Debug.Log ("Trial : " + model.currentLevel );
//		}
//		else if( Input.GetKeyDown(KeyCode.DownArrow))
//		{
//			model.UpdateLevel( false );
//			Debug.Log ("Trial : " + model.currentLevel );
//		}
	}

	public void DecreaseLevel()
	{
		model.UpdateLevel( false );
		Debug.Log ("Trial : " + model.currentLevel );
		UpdateDisplay ();
	}

	public void IncreaseLevel()
	{
		model.UpdateLevel( true );
		Debug.Log ("Trial : " + model.currentLevel );
		UpdateDisplay ();
	}

	void ShowDebug()
	{
		upLevel.gameObject.SetActive (true);
		downLevel.gameObject.SetActive (true);
		Debug.Log ("show debug");
	}

	void HideDebug()
	{
		upLevel.gameObject.SetActive (false);
		downLevel.gameObject.SetActive (false);
		Debug.Log("hide debug");
	}

	public void ActivateSubmissionButton( bool activate )
	{
	
		submitButton.interactable = activate;
	}

	public void CheckSubmission()
	{
		//debug
		string submissionKey = model.OrderedTileKey ();
		Debug.Log ("Submission : " + submissionKey);
		if(model.impossible)
		{
			Debug.Log ("INCORRECT");
			RespondToAnswer( false );

		}
		else
		{
//			string submissionKey = model.OrderedTileKey ();

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
//			Debug.Log ( " submission : " + submissionKey );
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
		int responseTime = metaData.SetStatsOnAnswer ( correctAnswer, Time.time);
		int timeBonus = CalculateTimeBonusEarned (correctAnswer, responseTime);
		Debug.Log ("BONUS POINTS : " + timeBonus);
		metaData.SaveStats ();
		view.DisplayFeedback ( true, correctAnswer );
	
	}

	int CalculateTimeBonusEarned( bool correctAnswer, int responseTime )
	{
		if( !correctAnswer )
		{
			return 0;
		}
		else
		{
			int secondsToSpare = model.maxSecondsForTimeBonus - responseTime;

			int timeBonus = secondsToSpare * model.pointsPerSecondUnderBonusTime;

			return timeBonus;
		}
	}
	
	int CalculatePointsForCorrectAnswer()
	{
		int scoreIncrease = ( model.currentLevel + 1 ) * model.pointsForCorrect;

		return scoreIncrease;
	}

	int CalculateTimeBonusPointsPerSecond()
	{
		int maxPointsFromTimeBonus = model.scoreIncreaseForCorrectAnswer / 2;

		return maxPointsFromTimeBonus / model.maxSecondsForTimeBonus;
	}

	public void NextStep( bool correctAnswer )
	{
		if (correctAnswer) {
			int scoreIncrease = ( model.currentLevel + 1 ) * model.pointsForCorrect;
			model.score += model.scoreIncreaseForCorrectAnswer;
			
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
//				EndOfTrial();
				ShowOnlyNextTrialButton();
//				NewTrial();
			}
		} 
		else 
		{
			model.UpdateLevel( correctAnswer );
//			EndOfTrial();
			ShowOnlyNextTrialButton();
		}
		
		UpdateDisplay ();
	}

	void EndOfTrial()
	{
		GameData.dataControl.previousFinalLevel = model.currentLevel;
		GameData.dataControl.Save ();

		if( ( model.currentTrial ) == model.trialsPerPlaySession )
		{
			Debug.Log ( "trial over ");
			//show end display
			trialsEndDisplay.SetActive( true );

			//stop showing other game info
			statPanel.SetActive( false );
			rulePanel.SetActive( false );
			buttonPanel.SetActive( false );
		}
		else
		{
			NewTrial();
		}
	}

	void UpdateDisplay()
	{
		statsDisplay.text = "Score : " + model.score + "\n" + "Trial : " + ( model.currentTrial ) + "\n" + "Problem : " + (model.currentProblemInTrial + 1) + "\n" + "Level : " + (model.currentLevel + 1);
	}

	public void ContinueGame()
	{
		//stop showing end display
		trialsEndDisplay.SetActive( false );
		
		//show other game info
//		gamesDisplay.SetActive ( true );
		statPanel.SetActive( true );
		rulePanel.SetActive( true );
		buttonPanel.SetActive( true );
		
		model.currentTrial = 0;

		Debug.Log (" current trial : " + model.currentTrial);

		NewTrial ();
	}


	void NewTrial()
	{
//		Debug.Log ("starting new trial ");
		model.scoreIncreaseForCorrectAnswer = CalculatePointsForCorrectAnswer ();
		model.pointsPerSecondUnderBonusTime = CalculateTimeBonusPointsPerSecond ();

		metaData.ResetStats ();
		model.currentTrial ++;
		UpdateDisplay ();

		logic.UpdateLevelingStats (model.currentLevel);
		model.currentProblemInTrial = 0; 
		//destroy children of view
		DestroyChildren (view.gameObject.transform);
		//create new board
		model.stagingAreas = view.CreateBoard (logic.tilesCount);

		bool submissionReady = model.ReadyForSubmission();
		ActivateSubmissionButton( submissionReady );

		string ruleList = logic.CreateRules ( model.tilesToOrder );
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

	public void Menu()
	{
		Application.LoadLevel ("Menu");
	}

	public void ShowOnlyNextTrialButton()
	{
		continueButton.gameObject.SetActive (true);

		impossibleButton.gameObject.SetActive (false);
		submitButton.gameObject.SetActive (false);
	}

	public void OnContinue()
	{
		continueButton.gameObject.SetActive (false);
		
		impossibleButton.gameObject.SetActive (true);
		submitButton.gameObject.SetActive (true);

		EndOfTrial ();
	}
	
	

	public void EmergencyReset()
	{
		NewTrial ();
	}
}
