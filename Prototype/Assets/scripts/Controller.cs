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

	public Toggle debugToggle;

	//vertical display
	public Text verticalRules;

	//horizontal display
	public Text horizontalRules;

	public Text rules;
	public Text score;
	public Text trial;
	public Text level;

	public Logic logic;

//	public Text statsDisplay;
	public GameObject statPanel;
	public GameObject rulePanel;
	public GameObject buttonPanel;

	public Text trialsEndDisplay;

	public Button upLevel;
	public Button downLevel;

	void Awake (){
		if( model.verticalLayout )
		{
			rules = verticalRules;
			verticalRules.gameObject.SetActive( true );
			horizontalRules.gameObject.SetActive ( false );
		}
		else
		{
			rules = horizontalRules;
			horizontalRules.gameObject.SetActive( true );
			verticalRules.gameObject.SetActive ( false );
		}

//		if( GameData.dataControl.shortGame )
//		{
//			model.maxResponsesInPlaySession = model.maxResponsesInShort;
//		}
	}

	void Start () {

		UpdateDisplay ();

		NewRound ();

		if( GameData.dataControl.debugOn )
		{
			ShowDebug();
			debugToggle.gameObject.SetActive( true );
		}
		else
		{
			HideDebug();
			debugToggle.gameObject.SetActive( false );
		}

		logic.consecutiveTollensErrors = GameData.dataControl.consecutiveModusTollensIncorrect;

		if( model.currentLevel >= model.firstLevelWithImpossibles )
		{
			EnableImpossible( true );
		}
		else
		{
			EnableImpossible( false );
		}

//		Debug.Log ("FONT SIZE: " + rules.fontSize);

	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.UpArrow ))
		{
			string inversion = logic.trialRules.ConstructVerbalWithInvertedConditionals();
			Debug.Log (inversion);
		}

	}

	public void DecreaseLevel()
	{
		model.UpdateLevel( -1 );
//		Debug.Log ("Trial : " + model.currentLevel );
		UpdateDisplay ();
		NewRound ();
	}

	public void IncreaseLevel()
	{
		model.UpdateLevel( 1 );
//		Debug.Log ("Trial : " + model.currentLevel );
		UpdateDisplay ();
		NewRound ();
	}

	void ShowDebug()
	{
		upLevel.gameObject.SetActive (true);
		downLevel.gameObject.SetActive (true);
		level.gameObject.SetActive (true);
//		Debug.Log ("show debug");
	}

	void HideDebug()
	{
		upLevel.gameObject.SetActive (false);
		downLevel.gameObject.SetActive (false);
		level.gameObject.SetActive (false);
//		Debug.Log("hide debug");
	}

	public void ActivateSubmissionButton( bool activate )
	{
	
		submitButton.interactable = activate;
	}

	string GetSubmissionWithSansPresets( string submission )
	{
		if (logic.currentPresetKey == null) 
		{
			return submission;
		}
		else
		{
			string presets = logic.currentPresetKey;
			string trueSubmission = "";  //submission without presets
//			Debug.Log ("presets: " + presets );
			//for each character in in submission
			for( int charIndex = 0; charIndex < submission.Length; charIndex ++ )
			{
				if( submission[ charIndex ] == presets[ charIndex ] )
				{
					trueSubmission += "n";
				}
				else
				{
					trueSubmission += submission[ charIndex ];
				}
			}
			Debug.Log ( trueSubmission );
 			return trueSubmission;
		}
	}

	public void CheckSubmission()
	{
		//debug
		string submissionKey = model.OrderedTileKey ();
//		submissionKey = GetSubmissionWithSansPresets (submissionKey);
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
				logic.AddPreviousSubmission( GetSubmissionWithSansPresets(submissionKey) );

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
		model.responseTotal ++;

		if( correctAnswer && model.currentChallenge.usesModusTollens )
		{
			logic.consecutiveTollensErrors = 0;
		}
		else if( !correctAnswer && model.currentChallenge.usesModusTollens )
		{
			logic.consecutiveTollensErrors ++;
			Debug.Log ( "consecutive erros : " + logic.consecutiveTollensErrors );

			if( logic.consecutiveTollensErrors >= logic.errorsCountToNeedHelp )
			{
				Debug.Log (" SHOW INVERSION ");
				string inversion = logic.trialRules.ConstructVerbalWithInvertedConditionals();
				rules.text = inversion;
			}
		}

		int responseTime = metaData.SetStatsOnAnswer ( correctAnswer, Time.time);
		int timeBonus = CalculateTimeBonusEarned (correctAnswer, responseTime);
		model.score += timeBonus;
		UpdateScoreDisplay ();

		float levelChange = model.CalculateLevelChange (correctAnswer, responseTime);
		model.UpdateLevel (levelChange);
		Debug.Log ("RESPONSE TIME : " + responseTime);
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

			int timeBonus = Mathf.Min ( 0, secondsToSpare * model.pointsPerSecondUnderBonusTime);

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
			UpdateScoreDisplay();

			//if trial is not yet completed
			if( model.currentTrialInRound < ( logic.currentLeveling.maxTrialsInRuleSet - 1 ) && ( model.responseTotal ) != model.maxResponsesInPlaySession )
			{
				//increase problem count
				model.currentTrialInRound++;
				//create new problem
				NewTrial();
			}
			else 
			{
				EndOfTrial();
			}
		} 
		else 
		{
			ShowOnlyContinueButton();
		}

	}

	public void EnableImpossible( bool enable )
	{
//		Debug.Log (enable);
		if( enable )
		{
			model.impossibleEnabled = true;
			impossibleButton.gameObject.SetActive( true );
		}
		else
		{
			model.impossibleEnabled = false;
			impossibleButton.gameObject.SetActive( false );
		}
	}

	void EndOfPlaySession()
	{
		GameData.dataControl.previousFinalLevel = model.currentNuancedLevel;
		GameData.dataControl.consecutiveModusTollensIncorrect = logic.consecutiveTollensErrors;
		GameData.dataControl.fitTestTaken = true;
		//		GameData.dataControl.impossibleEnabled = model.impossibleIsEnabled;
		
		GameData.dataControl.Save ();
		
		Debug.Log ( "trial over ");
		trialsEndDisplay.text = "Good job!  You finished at level " + ( model.currentLevel + 1 );
		//show end display
		trialsEndDisplay.transform.parent.gameObject.SetActive( true );

		view.gameObject.SetActive (false);
		//stop showing other game info
		statPanel.SetActive( false );
		rulePanel.SetActive( false );
		buttonPanel.SetActive( false );
	}
	
	void EndOfTrial()
	{
		if( ( model.responseTotal ) == model.maxResponsesInPlaySession )
		{
			EndOfPlaySession();
		}
		else
		{
			NewRound();
		}

//		UpdateDisplay ();
	}

	void UpdateDisplay()
	{
//		statsDisplay.text = "Score : " + model.score + "\n" + "Round : " + ( model.currentRound ) + "\n" + "Trial : " + (model.currentTrialInRound + 1) + "\n" + "Level : " + (model.currentLevel + 1);
		UpdateScoreDisplay ();
		UpdateLevelDisplay ();
		UpdateRoundDisplay ();
		UpdateTrialDisplay ();
	}

	void UpdateScoreDisplay()
	{
		score.text = "Score : " + model.score;
	}

	void UpdateLevelDisplay()
	{
		level.text = "Level : " + ( model.currentLevel + 1 );
	}

	void UpdateRoundDisplay()
	{
//		round.text = "Round : " + (model.currentRound) + " of " + model.roundsPerPlaySession;
	}

	void UpdateTrialDisplay()
	{
		trial.text = "Trial : " + (model.responseTotal + 1)  + " of " + model.maxResponsesInPlaySession;
	}
	

	public void ContinueGame()
	{
		if( ( model.currentLevel + model.impossibleLevelsBuffer ) >= model.firstLevelWithImpossibles )
		{
			EnableImpossible( true );
		}
		else
		{
			EnableImpossible( false );
		}

		model.TakeFitTest (false);

		//stop showing end display
		trialsEndDisplay.transform.parent.gameObject.SetActive( false );
		
		//show other game info
		statPanel.SetActive( true );
		rulePanel.SetActive( true );
		buttonPanel.SetActive( true );
		view.gameObject.SetActive (true);

		model.responseTotal = 0;

		NewRound ();
	}


	void NewRound()
	{
//		Debug.Log ("starting new trial ");
//		UpdateDisplay ();

		model.scoreIncreaseForCorrectAnswer = CalculatePointsForCorrectAnswer ();
		model.pointsPerSecondUnderBonusTime = CalculateTimeBonusPointsPerSecond ();

		metaData.ResetStats ();
		model.currentRound ++;
//		UpdateDisplay ();

		logic.UpdateLevelingStats (model.currentLevel);
		model.currentTrialInRound = 0; 

		UpdateDisplay ();

		model.CreateNewChallengeSet ();

		//destroy children of view
		DestroyChildren (view.gameObject.transform);
		//create new board
		model.stagingAreas = view.CreateBoard ( logic.currentLeveling.tilesCount );

		bool submissionReady = model.ReadyForSubmission();
		ActivateSubmissionButton( submissionReady );

		string ruleList = logic.CreateRules ( model.tilesToOrder, logic.maxAttemptsToCreateRules, new RuleStack() );
		rules.text = ruleList;

		bool createPresets = logic.TrialGetsPresets ();

		if( createPresets )
		{
			Debug.Log ("PRESETS IN FIRST TRIAL OF ROUND");
			NewTrial();
		}
	}
	
	public void NewTrial()
	{
//		UpdateDisplay ();
		UpdateTrialDisplay ();

		model.CreateNewChallengeSet ();

		view.WipePreviousProblem (model.tilesToOrder, model.holders, model.stagingAreas);

		Debug.Log ("creating new problem");
		//determine logic for new problem and set board for new problem
		string presetBoard = logic.NewTrialSetUp ();

		//if no good preset board
		if( presetBoard == null )
		{
			//start new round with new rules
			Debug.Log ("******************************************");
			Debug.Log ("STARTING NEW ROUND BECAUSE TRIAL PRESETS NULL ");
			Debug.Log ("******************************************");
			NewRound();
		}
		else
		{
			view.PresetTilesWithHolders (presetBoard, model.tilesToOrder, model.holders);
			
			bool submissionReady = model.ReadyForSubmission();
			ActivateSubmissionButton( submissionReady );
		}

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

	public void ShowOnlyContinueButton()
	{
		continueButton.gameObject.SetActive (true);

		if( model.impossibleEnabled )
		{
			impossibleButton.gameObject.SetActive (false);
		}

		submitButton.gameObject.SetActive (false);
	}

	public void OnContinue()
	{
		continueButton.gameObject.SetActive (false);
		
		if( model.impossibleEnabled )
		{
			impossibleButton.gameObject.SetActive (true);
			Debug.Log ("impossible enable ");
		}

		submitButton.gameObject.SetActive (true);

		EndOfTrial();


	}
	
	public void OnDebugToggle()
	{
		if ( debugToggle.isOn )
		{
			ShowDebug();
		}
		else
		{
			HideDebug();
		}
	}

//	public void EmergencyReset()
//	{
//		NewRound ();
//	}
}
