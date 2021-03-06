﻿using UnityEngine;
using System.Collections;

public class TileHolder : MonoBehaviour {

	public SpriteRenderer rend;
//	public bool occupied = false;  //already holding a tile
	public Tile occupyingTile;
	Color originalColor;
	public Color highlightedColor;
	public Model model;
	public int spotNumber;

	public bool preset;



	void Awake(){
		originalColor = rend.color;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Highlight( bool highlight )
	{
		if (highlight) 
		{
			rend.color = highlightedColor;

		}
		else
		{
			rend.color = originalColor;
		}
	
	}

	void OnMouseDown()
	{
//		Debug.Log ("holder : " + this.name);
		if (model.selectedTile != null && !preset && occupyingTile == null ) 
		{
			model.StartMove( this );
//			model.UpdateSubmitButton ();
		}


	}
	

	public void SetOccupied( Tile tile )
	{

		if( occupyingTile != null )
		{
			occupyingTile.SetCurrentHolder( null );
		}

		occupyingTile = tile;
		model.UpdateSubmitButton ();

	}

}
