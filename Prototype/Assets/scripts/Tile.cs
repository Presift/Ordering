using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {
	
	public Model model;
	public TileHolder targetHolder;
	public TileHolder currentHolder;
	public StagingArea currentStaging;
	public bool preset;
	Color originalColor;
	Color highlightColor;
	Renderer render;


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetStartValues( Color firstColor, Color tileHighlight, StagingArea staging )
	{
		originalColor = firstColor;
		render = GetComponent<Renderer> ();
		render.material.color = originalColor;
		highlightColor = new Color ((tileHighlight.r + originalColor.r ) / 2, (tileHighlight.g + originalColor.g ) / 2, (tileHighlight.b + originalColor.b ) / 2, (tileHighlight.a + originalColor.a ) / 2);
		SetCurrentStaging( staging );
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
		if (!preset)
		{
			model.ManageCurrentSelection (this);
		}
	}

	public void Highlight( bool highlight )
	{
		if( highlight )
		{
			render.material.color = highlightColor;
		}
		else
		{
			render.material.color = originalColor;
		}
	}


	public void StartMove( TileHolder newHolder )
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
		if (newHolder != null) 
		{
			transform.position = newHolder.transform.position;
			newHolder.SetOccupied( this );
			currentHolder = newHolder;
		}
		else
		{
			//go to staging area
			StagingArea area = model.GetUnoccupiedStagingArea();
			SetCurrentStaging( area ); 
		}

		model.ManageCurrentSelection (this);

	}




}
