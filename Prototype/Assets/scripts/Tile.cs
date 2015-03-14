using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public TileHolder targetHolderScript;
	float cameraZPosition;
	public Vector3 lastHeldPosition;
	public bool justLeavingHolder;
	public Model model;
	public TileHolder previouslyOccupiedHolder;

	// Use this for initialization
	void Start () {
		cameraZPosition = Camera.main.transform.position.z;
		lastHeldPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	//MOVEMENT
	void OnMouseDown()
	{
		if (targetHolderScript != null) 
		{
			justLeavingHolder = true;	
		}
		else //leaving staging position
		{
			justLeavingHolder = false;
			model.SetStagingState( lastHeldPosition, false );
		}
	}

	void OnMouseDrag()
	{
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = cameraZPosition;
		mousePosition.z = -cameraZPosition;

		Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint (mousePosition);

		transform.position = mouseWorldPosition;
	}

	void OnMouseUp()
	{
		//if colliding with available tile holder
		if (targetHolderScript != null) 
		{
			if( targetHolderScript.occupingTile != null )
			{
				//swap this tile with occuping tile 
				targetHolderScript.SwapTiles( this );
			}
			else
			{
				//snap to available holder
				transform.position = targetHolderScript.transform.position;
				targetHolderScript.SetOccupied( this );
//				justLeavingHolder = true;
			}
			targetHolderScript.Highlight( false );
		}
		else
		{
			//go to unoccupied tile staging place
			StagingArea unoccupied = model.GetUnoccupiedStagingArea();
			transform.position = unoccupied.position;

		}

		lastHeldPosition = transform.position;
//		Debug.Log (gameObject.name + " " + lastHeldPosition );
	}

	//COLLISION

	void OnTriggerEnter2D( Collider2D other )
	{
		// if other is a tileHolder and is not preset
		if (other.name.Contains ("Holder")) 
		{
			//if not already highlighting another holder
			if( targetHolderScript == null )
			{
				TileHolder holdScript = ( TileHolder )other.gameObject.GetComponent(typeof( TileHolder ));
				if( !holdScript.preSet )
				{
					SetTargetAndHolderColor( true, holdScript );
				}
			}


		}
	}
	
	void OnTriggerExit2D( Collider2D other )
	{
		if (other.name.Contains ("Holder")) 
		{
			TileHolder holdScript = ( TileHolder )other.gameObject.GetComponent(typeof( TileHolder ));
			//if not exiting a preset holder
			if ( !holdScript.preSet ) 
			{
				//if exiting holder that is current target
				if( holdScript == targetHolderScript )
				{
					//if holder tile is occupied
					if( holdScript.occupingTile != null )
					{
						//if occuping tile is this tile
						if( holdScript.occupingTile == this )
						{
							//set occuping tile to null
							previouslyOccupiedHolder = targetHolderScript;
							holdScript.SetOccupied( null ); 
						}
					}
					else
					{

					}

					targetHolderScript = null;
					holdScript.Highlight( false );
				}
			}
			
		}

	}

	void OnTriggerStay2D( Collider2D other ) //prevent tile from highlighting 2 holders at once
	{
		//if target is null and object is tileHolder
		if (targetHolderScript == null && other.name.Contains ("Holder")) 
		{
			TileHolder holdScript = ( TileHolder )other.gameObject.GetComponent(typeof( TileHolder ));
			if( !holdScript.preSet )
			{
				SetTargetAndHolderColor( true, holdScript );
			}

		}
	}

	void SetTargetAndHolderColor( bool highlight, TileHolder holdScript )
	{
		if (holdScript != null) 
		{
			targetHolderScript = holdScript;
			targetHolderScript.Highlight( highlight );
		}
		else
		{
			targetHolderScript.Highlight( highlight );
			targetHolderScript = holdScript;
		}
	}
}
