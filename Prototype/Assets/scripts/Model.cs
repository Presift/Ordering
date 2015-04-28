using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Model : MonoBehaviour {

	public bool verticalLayout;
	public int currentLevel;
	public float currentNuancedLevel;

//	public bool shortGame 
	public int pointsForCorrect = 100;
	public int score;
	public int currentTrialInRound;
//	public int trial;
	public int scoreIncreaseForCorrectAnswer;

	public bool impossibleEnabled;
	public int impossibleLevelsBuffer;

	int occupiedTiles;
	public List<StagingArea> stagingAreas;
	public List<TileHolder> holders;
	public List<Tile> tilesToOrder;
	public Controller controller;

	public Tile selectedTile;
	public bool impossible;

	public int currentRound;
	public int responseTotal = 0;
//	public int roundsPerPlaySession = 5;
	public int maxResponsesInPlaySession = 15;
	public int maxResponsesInShort = 5;

	public int maxSecondsForTimeBonus = 60;
	public int pointsPerSecondUnderBonusTime;

	float maxLevelChange = 4f;
	float minLevelChange = -.75f;

	float fitTestMaxLevelChange = 13f;
	float fitTestMinLevelChange = -5f;
//	float fitTestWorstTimeMultiplier = ;

	float currentMinLevelChange;
	float currentMaxLevelChange;
//	float currentWorstTimeMultiplier;

	public float responseTimeForMaxLevelChange;
	public float responseTimeForMinLevelChange;

	public CurrentSetUp currentChallenge;

	public int firstLevelWithImpossibles = 0;

	public bool paused = false;

	// Use this for initialization
	void Awake () {
//		currentRound = 0;

//		GameData.dataControl.Load ();
//		currentNuancedLevel = GameData.dataControl.previousFinalLevel;
//		currentLevel = (int)Mathf.Floor (currentNuancedLevel);
//		currentLevel = 100;
//		currentNuancedLevel = 100;
//
//		if( GameData.dataControl.fitTestTaken )
//		{
//			TakeFitTest( false );
//			Debug.Log ("fit test taken");
//		}
//		else
//		{
//			TakeFitTest( true );
//			Debug.Log ("fit test NOT taken");
//		}
//
////		TakeFitTest (false);
//
//		if( GameData.dataControl.shortGame )
//		{
//			maxResponsesInPlaySession = maxResponsesInShort;
//		}
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void TakeFitTest( bool takeTest )
	{
		if( !takeTest )
		{
			currentMaxLevelChange = maxLevelChange;
			currentMinLevelChange = minLevelChange;
		}
		else
		{
			currentMaxLevelChange = fitTestMaxLevelChange;
			currentMinLevelChange = fitTestMinLevelChange;
		}
	}

	public void CreateNewChallengeSet()
	{
		currentChallenge = new CurrentSetUp ();
	}

	public float CalculateLevelChange( bool correctAnswer, float responseTime )
	{
		if( !correctAnswer )
		{
			return Mathf.Min( -1, currentMinLevelChange );
		}
		else
		{
			if( responseTime < responseTimeForMaxLevelChange )
			{
//				Debug.Log (" max level Change ");
				return currentMaxLevelChange;
			}
			else if( responseTime > responseTimeForMinLevelChange )
			{
//				Debug.Log ("min level Change ");
				return currentMinLevelChange;
			}
			else
			{
				float timeDiffFromTimeForMaxLevelChange = responseTime - responseTimeForMaxLevelChange;

				float levelChangeSlope = ( currentMaxLevelChange - currentMinLevelChange ) / ( responseTimeForMinLevelChange - responseTimeForMaxLevelChange );

				float levelChange = currentMaxLevelChange - ( levelChangeSlope * timeDiffFromTimeForMaxLevelChange );

				return levelChange;
			}
		}
	}

	public void SetImpossible( bool notPossible )
	{
		impossible = notPossible;
	}

	public void SetStagingState( StagingArea area, bool occupied )
	{
		for( int i = 0; i < stagingAreas.Count; i ++ )
		{
			if( stagingAreas[ i ] == area )
			{
				stagingAreas[ i ].occupied = occupied;
			}
		}
	}

	public string OrderedTileKey()
	{
//		Debug.Log ("SUBMISSION");
//		List<Tile> ordered = new List<Tile> ();
		string submissionKey = "";;

		for( int i = 0; i < holders.Count; i ++ )
		{
//			Debug.Log (holders[ i ].occupyingTile.name);
			submissionKey += holders[ i ].occupyingTile.name[ 0 ];
		}
		return submissionKey;
	}
	

	public StagingArea GetUnoccupiedStagingArea()
	{

		for( int i = 0; i < stagingAreas.Count; i ++ )
		{
			if( stagingAreas[ i ].occupied == false )
			{
				stagingAreas[ i ].occupied = true;
				return stagingAreas[ i ];
			}
		}

		return null;
	}

	public void SetHolders( List<TileHolder> tileHolders )
	{
		holders = tileHolders;
	}

	public void SetTilesToOrder( List<Tile> tiles )
	{
		tilesToOrder = tiles;
	}

	public bool ReadyForSubmission()
	{
		for( int i = 0; i < holders.Count; i ++ )
		{
			if( holders[ i ].occupyingTile == null )
			{
				return false;
			}
		}
		return true;	
	}

	public void UpdateSubmitButton()
	{
		bool submissionReady = ReadyForSubmission();
		controller.ActivateSubmissionButton( submissionReady );
	}


	public void ManageCurrentSelection( Tile newSelection )
	{
//		Debug.Log (newSelection.name);
		if (selectedTile == newSelection) 
		{
			//deselect tile
			selectedTile.Highlight( false );
			selectedTile = null;
		}
		else if( selectedTile == null )
		{
			selectedTile = newSelection;
			newSelection.Highlight( true );
		}
		else
		{
			//deselect current
			selectedTile.Highlight( false );
			selectedTile = newSelection;
			newSelection.Highlight( true );
		}

//		Debug.Log (" selected tile : " + newSelection.name);
	}

	public void StartMove( TileHolder newHolder )
	{
//		Debug.Log ("start moving : " + selectedTile.name);
		selectedTile.StartMove (newHolder, false);
	}

	public void ShowAvailableMoves()
	{
		for( int i = 0; i < holders.Count; i ++ )
		{
			if( holders[ i ].occupyingTile == null )
			{
				holders[ i ].Highlight( true );
			}
		}
	}
	
	public void StopShowingMoves()
	{
		for( int i = 0; i < holders.Count; i ++ )
		{
			if( holders[ i ].occupyingTile == null )
			{
				holders[ i ].Highlight( false );
			}
		}
	}

	public void UpdateLevel( float levelChange )
	{
//		Debug.Log ("Level Change : " + levelChange);
		currentNuancedLevel = Mathf.Max( levelChange + currentNuancedLevel, 0 );
		currentLevel = ( int ) Mathf.Floor( currentNuancedLevel );
			
	}


}
