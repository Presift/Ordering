using UnityEngine;
using System.Collections;

public class TileHolder : MonoBehaviour {

	public SpriteRenderer rend;
//	public bool occupied = false;  //already holding a tile
	public Tile occupyingTile;
	Color originalColor;
	public Color highlightedColor;
	public Model model;
	public int spotNumber;

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

	void OnMouseDown()
	{
		if (model.selectedTile != null && !preSet && occupyingTile == null ) 
		{
			model.StartMove( this );
		}

		UpdateSubmitButton ();
	}
	

	public void SetOccupied( Tile tile )
	{
		occupyingTile = tile;
	}

	void UpdateSubmitButton()
	{
		bool submissionReady = model.ReadyForSubmission();
		model.controller.ActivateSubmissionButton( submissionReady );
	}





}
