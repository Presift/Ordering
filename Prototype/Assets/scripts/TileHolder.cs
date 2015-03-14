using UnityEngine;
using System.Collections;

public class TileHolder : MonoBehaviour {

	public SpriteRenderer rend;
//	public bool occupied = false;  //already holding a tile
	public Tile occupingTile;
	Color originalColor;
	public Color highlightedColor;

	public bool preSet;



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
	

	public void SetOccupied( Tile tile )
	{
		occupingTile = tile;
	}

	public void SwapTiles( Tile newTile )
	{
		//if newTile is coming from an occupied holder
		if (newTile.previouslyOccupiedHolder != null) 
		{
			Debug.Log ("swapped with previously occupied ");
			occupingTile.targetHolderScript = newTile.previouslyOccupiedHolder;
			occupingTile.targetHolderScript.occupingTile = occupingTile;
			occupingTile.transform.position = occupingTile.targetHolderScript.transform.position;
			newTile.previouslyOccupiedHolder = null;
		}
		else
		{
			Debug.Log ("just swapped ");
			occupingTile.transform.position = newTile.lastHeldPosition;
		}


		//snap new tile this holder position
		newTile.transform.position = transform.position;

		//set new holder occupation
		occupingTile = newTile;

		Highlight (false);
	}



}
