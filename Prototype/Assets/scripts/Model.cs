using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Model : MonoBehaviour {

	public int currentLevel;
	public int currentTotalTileCount;
	int occupiedTiles;
	public List<StagingArea> stagingAreas;


	// Use this for initialization
	void Awake () {
		currentLevel = 1; 
		currentTotalTileCount = 4;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetStagingState( Vector3 stagingPosition, bool occupied )
	{
		for( int i = 0; i < stagingAreas.Count; i ++ )
		{
			if( stagingAreas[ i ].position == stagingPosition )
			{
				stagingAreas[ i ].occupied = occupied;
			}
		}
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

	//is ready for submission
}
