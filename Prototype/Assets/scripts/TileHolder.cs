using UnityEngine;
using System.Collections;

public class TileHolder : MonoBehaviour {

	public SpriteRenderer rend;
	public bool occupied = false;  //already holding a tile
	Color originalColor;
	public Color highlightedColor;

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
	

	public void SetOccupied( bool filled )
	{
		occupied = filled;
	}



}
