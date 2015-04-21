using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class View : MonoBehaviour {

	public Controller control;
	public Model model;

	public List<Color> tileColors;
	public List<string> colorNames;
	public Color tileHighlight;

	int maxTileCount;
	float tileWidth;
	float tileHolderSpace;
	float tileHolderWidth;
	public float tileAsFractionOfHolder;
	public float spaceAsFractionOfHolder;

	public GameObject correctImage;
	public GameObject incorrectImage;
	GameObject activeImage;

	bool showFeedback = false;
	float feedbackShowDuration = 1f;
	float timeShowingFeedback = 0;


	Vector3 centerBoardPosition = new Vector3( 0, -1.0f, 0 );
	Vector3 verticalCenterPosition = new Vector3 (1.5f, 1, 0);
	
	GameObject tileHolder;
	GameObject coloredTile;


	// Use this for initialization
	void Awake () {


		tileHolder = (GameObject)Resources.Load("tileHolder");
		coloredTile = (GameObject)Resources.Load("coloredTile");

		SpriteRenderer holderRenderer = (SpriteRenderer)tileHolder.GetComponent (typeof(SpriteRenderer));
		float originalTileSizeHolder = holderRenderer.bounds.extents.x * 2;


		maxTileCount = tileColors.Count;

		Tile tileScript = (Tile)coloredTile.GetComponent (typeof(Tile));
		tileScript.model = model;

		CalculateSizes ( model.verticalLayout );

		RescaleObjects ();
	}
	
	// Update is called once per frame
	void Update () {
	
		if( showFeedback )
		{
			TimeFeedbackDisplay();
		}
	}

	public void DisplayFeedback( bool show, bool correctAnswer )
	{
		if ( show )
		{
			showFeedback = true;
			if( correctAnswer )
			{
				activeImage = correctImage;
			}
			else
			{
				activeImage = incorrectImage;
			}
		}
		else
		{
			timeShowingFeedback = 0;
			showFeedback = false;
			control.NextStep( activeImage == correctImage );
		}

		activeImage.SetActive ( showFeedback );

	}

	void TimeFeedbackDisplay()
	{

		if( timeShowingFeedback < feedbackShowDuration )
		{
			timeShowingFeedback += Time.deltaTime;
		}
		else
		{
			DisplayFeedback( false, true );
			activeImage = null;
		}
	}

	void CalculateSizes( bool verticalDisplay )
	{
		if( !verticalDisplay )
		{
			//get world width
			Vector3 leftScreen = Camera.main.ScreenToWorldPoint (new Vector3( 0, 0, 0 ));
			Vector3 rightScreen = Camera.main.ScreenToWorldPoint (new Vector3(Screen.width, 0, 0 ));
			float worldWidth = Mathf.Abs (rightScreen.x - leftScreen.x);
			
			//calculate max tile holder size
			tileHolderWidth = worldWidth / ( maxTileCount + (( maxTileCount + 1 ) * spaceAsFractionOfHolder));
			tileHolderSpace = tileHolderWidth * spaceAsFractionOfHolder;
			tileWidth = tileHolderWidth * tileAsFractionOfHolder;
		}
		else
		{
			//get world height
			Vector3 topScreen = Camera.main.ScreenToWorldPoint (new Vector3( 0, Screen.height, 0 ));
			Vector3 bottomScreen = Camera.main.ScreenToWorldPoint (new Vector3(0, 0, 0 ));
			float worldHeight = Mathf.Abs (topScreen.y - bottomScreen.y);
			Debug.Log ( " top : " + topScreen.y + " , bottom : " + bottomScreen.y );
			Debug.Log ( "height : " +  worldHeight );
			float verticalSpaceForTiles = worldHeight / 5 * 4;

			//calculate max tile holder size
			tileHolderWidth = verticalSpaceForTiles / ( maxTileCount + (( maxTileCount + 1 ) * spaceAsFractionOfHolder));
			Debug.Log ( " holder width : " + tileHolderWidth );
			tileHolderSpace = tileHolderWidth * spaceAsFractionOfHolder;
			Debug.Log ( "holder space : " + tileHolderSpace );
			tileWidth = tileHolderWidth * tileAsFractionOfHolder;
			Debug.Log ( "tile width : " + tileWidth );
		}

	}



	void RescaleObjects()  
	{
		//scale holder
		SpriteRenderer holderRenderer = (SpriteRenderer)tileHolder.GetComponent (typeof(SpriteRenderer));
		float originalTileSizeHolder = holderRenderer.bounds.extents.x * 2;
		float scaleChangeHolder = tileHolderWidth / originalTileSizeHolder;
		Debug.Log ("holder width : " + tileHolderWidth + ",  original size : " + originalTileSizeHolder);
		tileHolder.transform.localScale *= scaleChangeHolder;
		Debug.Log (scaleChangeHolder);

		//scale tile
		SpriteRenderer tileRenderer = (SpriteRenderer)coloredTile.GetComponent (typeof(SpriteRenderer));
		float originalTileSize = tileRenderer.bounds.extents.x * 2;
		float scaleChangeTile = tileWidth / originalTileSize;
		coloredTile.transform.localScale *= scaleChangeTile;

	}



	public void PresetTilesWithHolders( string presets, List<Tile> tilesToOrder, List<TileHolder> holders )
	{
		for( int i = 0; i < presets.Length; i ++ )
		{
			//if preset is a tile to be placed
			if(presets[ i ] != 'n' )
			{
				for(int tile = 0; tile < tilesToOrder.Count; tile ++ )
				{
					if( tilesToOrder[ tile ].name[ 0 ] == presets[ i ] )
					{
//						Tile tileScript = tilesToOrder[ tile ].GetComponent<Tile>();
						tilesToOrder[ tile ].preset = true;
						tilesToOrder[ tile ].StartMove( holders[i], true );

						tilesToOrder[ tile ].Highlight( false );
						holders[ i ].preset = true;
						holders[i].Highlight( true );
						break;
					}
				}
			}
		}
	}

	public List<StagingArea> CreateBoard( int tileCount )
	{
		List<StagingArea> stagingAreas = new List<StagingArea>();
		List<TileHolder> holders = new List<TileHolder> ();
		List<Tile> tiles = new List<Tile> ();

		//calculate positions for tile holders
		List <Vector3> tilePositions = GetTileHolderPositions (tileCount);


		for( int tileIndex = 0; tileIndex < tileCount; tileIndex ++ )
		{
			Vector3 tilePosition = tilePositions[ tileIndex ];

			//create tile holders and place centered on board, spaced evenly
			GameObject holder = ( GameObject ) Instantiate( tileHolder, tilePosition, Quaternion.identity );
			SetTextOnHolder( holder, (tileIndex + 1 ).ToString());
			holder.transform.parent = transform;
			TileHolder holderScript =  holder.GetComponent<TileHolder>();
			holderScript.model = model;
			holderScript.spotNumber = tileIndex + 1;
			holder.name = ( tileIndex + 1 ).ToString();
			holders.Add( holderScript );

			//create a tile with a unique color and color name
			//place tiles above holders
			Vector3 stagingPosition;

			if( !model.verticalLayout )
			{
				stagingPosition = tilePosition + new Vector3 ( 0, tileHolderWidth + tileHolderSpace, 0 );
			}
			else
			{
				stagingPosition = tilePosition + new Vector3 ( tileHolderWidth + tileHolderSpace, 0, 0 );
			}

			StagingArea stagingArea = new StagingArea( stagingPosition, true );
			GameObject tile = ( GameObject ) Instantiate( coloredTile, stagingPosition, Quaternion.identity );
			tile.name = colorNames[ tileIndex ];

			tile.transform.parent = transform;

			Tile tileScript = tile.GetComponent<Tile>();
			tileScript.SetStartValues( tileColors[ tileIndex ], tileHighlight, stagingArea );

			tiles.Add (tileScript);
			stagingAreas.Add ( stagingArea );
		}
		model.SetHolders (holders);
		model.SetTilesToOrder (tiles);
		return stagingAreas;
	}

	void SetTextOnHolder( GameObject textHolder, string text )
	{
		Transform textObj = textHolder.transform.GetChild( 0 );
		textObj.GetComponent<TextMesh> ().text = text;
	

	}


	public void WipePreviousProblem( List<Tile> tiles, List<TileHolder> tileHolders, List<StagingArea> stagingAreas )
	{
		//set all holders to open
		//set all staging areas to open
		for( int i = 0; i < tiles.Count; i ++ )
		{
			if( tileHolders[i].occupyingTile != null )
			{
				tileHolders[i].SetOccupied( null );
				tileHolders[i].preset = false;
			}

			tiles[i].currentHolder = null;
			tiles[i].preset = false;
			tileHolders[i].Highlight( false );
			tiles[i].targetHolder = null;
			tiles[i].StartMove( null, true );
		}
	}


	List<Vector3> GetTileHolderPositions( int tileCount)
	{
		List<Vector3> tilePositions = new List<Vector3>();

		float totalWidth = tileCount * tileHolderWidth + ((tileCount - 1) * tileHolderSpace);

		Vector3 startPosition;

		if (!model.verticalLayout) 
		{
			startPosition = centerBoardPosition - new Vector3 ((totalWidth - tileHolderWidth) / 2, 0, 0);	
		}
		else
		{
			startPosition = verticalCenterPosition - new Vector3 (0, (totalWidth - tileHolderWidth) / 2, 0);
		}



		if( !model.verticalLayout )
		{
			for( int tile = 0; tile < tileCount; tile ++ )
			{
				Vector3 tilePosition = startPosition + new Vector3( tile * ( tileHolderWidth + tileHolderSpace ), 0, 0 );
				tilePositions.Add ( tilePosition );		
			}
		}
		else
		{
			for( int tile = 0; tile < tileCount; tile ++ )
			{
				Vector3 tilePosition = startPosition + new Vector3( 0, tile * ( tileHolderWidth + tileHolderSpace ), 0 );
				tilePositions.Add ( tilePosition );		
			}
		}


		return tilePositions;
	}

}
