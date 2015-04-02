using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Model : MonoBehaviour {

	public int currentLevel;
	public int pointsForCorrect = 100;
	public int score;
	public int currentProblemInTrial;
	public int trial;
	public int scoreIncreaseForCorrectAnswer;
//	public int maxPointsFrom
//	public int currentTotalTileCount;
	int occupiedTiles;
	public List<StagingArea> stagingAreas;
	public List<TileHolder> holders;
	public List<Tile> tilesToOrder;
	public Controller controller;

	public Tile selectedTile;
	public bool impossible;

	public int currentTrial;
	public int trialsPerPlaySession = 3;

	public int maxSecondsForTimeBonus = 60;
	public int pointsPerSecondUnderBonusTime;

	// Use this for initialization
	void Awake () {
		currentTrial = 0;

		GameData.dataControl.Load ();
		currentLevel = GameData.dataControl.previousFinalLevel;

//		currentTotalTileCount = 4;
	}
	
	// Update is called once per frame
	void Update () {
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

//	public bool ReadyForSubmission()
//	{
//		bool readyForSubmission = true;
//		int tilesOccupied = 0;
//			for( int i = 0; i < holders.Count; i ++ )
//		{
//			if( holders[ i ].occupyingTile == null )
//			{
//				readyForSubmission = false;
//			}
//			else
//			{
//				tilesOccupied ++;
//			}
//		}
//		Debug.Log ("holders occupied : " + tilesOccupied);
//		return readyForSubmission;	
//	}

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

	public void UpdateLevel( bool correctAnswer )
	{
		//if correct
		if (correctAnswer)

		{
			//increase level
			currentLevel ++;
		}
		else
		{
			//decrease level
			currentLevel = Mathf.Max ( 0, currentLevel - 1);
		}
			
	}


}
