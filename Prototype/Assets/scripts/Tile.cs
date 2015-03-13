using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	TileHolder holderScript;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}



	//COLLISION

	void OnTriggerEnter2D( Collider2D other )
	{
		// if other is a tileHolder and is not already occupied
		if (other.name.Contains ("Holder")) 
		{
			TileHolder holdScript = ( TileHolder )other.gameObject.GetComponent(typeof( TileHolder ));
			if( !holdScript.occupied )
			{
				SetTargetAndHolderColor( true, holdScript );
			}

		}
	}
	
	void OnTriggerExit2D( Collider2D other )
	{
		//if other is target && other is this object's target
		if (other.gameObject == holderScript.gameObject) 
		{
			SetTargetAndHolderColor( false, null );
		}
	}

	void OnTriggerStay2D( Collider2D other )
	{
		//if target is null and object is tileHolder
		if (holderScript == null && other.name.Contains ("Holder")) 
		{
			TileHolder holdScript = ( TileHolder )other.gameObject.GetComponent(typeof( TileHolder ));
			if( !holdScript.occupied )
			{
				SetTargetAndHolderColor( true, holdScript );
			}

		}
	}

	void SetTargetAndHolderColor( bool highlight, TileHolder holdScript )
	{
		if (holdScript != null) 
		{
			holderScript = holdScript;
			holderScript.Highlight( highlight );
		}
		else
		{
			holderScript.Highlight( highlight );
			holderScript = holdScript;
		}




	}
}
