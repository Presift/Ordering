using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
	
	public Model model;
	public TileHolder targetHolder;
	public TileHolder currentHolder;
	public StagingArea currentStaging;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetCurrentStaging( StagingArea staging )
	{
		currentStaging = staging;
		transform.position = staging.position;
	}

	public void SetCurrentHolder( TileHolder newHolder )
	{
		currentHolder = newHolder;
		if( currentHolder != null )
		{
			transform.position = newHolder.transform.position;
			currentHolder.occupyingTile = this;
		}
		else
		{
			if( targetHolder != null )
			{
				targetHolder.Highlight( false );
			}
			targetHolder = null;

		}

	}

	//MOVEMENT
	void OnMouseDown()
	{

		model.ManageCurrentSelection (this);

	}


	public void StartMove( TileHolder newTile )
	{
		//if currently in holder tile
		if( currentHolder != null )
		{
			currentHolder.SetOccupied( null );
		}

		//if currently in staging area
		if (currentStaging != null) 
		{
			model.SetStagingState( currentStaging, false );
			currentStaging = null;

		}

		//if new holder is not staging area
		if (newTile != null) 
		{
			transform.position = newTile.transform.position;
			newTile.SetOccupied( this );
			currentHolder = newTile;
		}
		else
		{
			//go to staging area
			StagingArea area = model.GetUnoccupiedStagingArea();
			Debug.Log (area);
			SetCurrentStaging( area ); 

		}

	}




}
