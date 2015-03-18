using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Model : MonoBehaviour {

	public int currentLevel;
	public int pointsForCorrect = 100;
	public int score;
	public int currentProblemInTrial;
	public int trial;

	public int currentTotalTileCount;
	int occupiedTiles;
	public List<StagingArea> stagingAreas;
	public List<TileHolder> holders;
	public List<Tile> tilesToOrder;
	public Controller controller;

	public Tile selectedTile;



	// Use this for initialization
	void Awake () {
		currentLevel = 1; 
		currentTotalTileCount = 4;
	}
	
	// Update is called once per frame
	void Update () {
	
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

	public List<Tile> OrderedTiles()
	{
		List<Tile> ordered = new List<Tile> ();

		for( int i = 0; i < holders.Count; i ++ )
		{
			ordered.Add ( holders[ i ].occupyingTile );
		}
		return ordered;
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
	

	public void ManageCurrentSelection( Tile newSelection )
	{
		if (selectedTile == newSelection) 
		{
			//deselect tile
		}
		else if( selectedTile == null )
		{
			selectedTile = newSelection;
		}
		else
		{
			//deselect current
			selectedTile = newSelection;
		}
	}

	public void StartMove( TileHolder newTile )
	{
		selectedTile.StartMove (newTile);
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
			currentLevel --;
		}
			
	}


}
