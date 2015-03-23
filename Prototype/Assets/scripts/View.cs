using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class View : MonoBehaviour {

	public Controller control;
	public Model model;

	public List<Color> tileColors;
	public List<string> colorNames;

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


	Vector3 centerBoardPosition = new Vector3( 0, 0, 0 );
	
	GameObject tileHolder;
	GameObject coloredTile;


	// Use this for initialization
	void Awake () {
		maxTileCount = tileColors.Count;

		tileHolder = (GameObject)Resources.Load("tileHolder");
		coloredTile = (GameObject)Resources.Load("coloredTile");
		Tile tileScript = (Tile)coloredTile.GetComponent (typeof(Tile));
		tileScript.model = model;

		CalculateSizes ();
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

	void CalculateSizes()
	{
		//get world width
		Vector3 leftScreen = Camera.main.ScreenToWorldPoint (new Vector3( 0, 0, 0 ));
		Vector3 rightScreen = Camera.main.ScreenToWorldPoint (new Vector3(Screen.width, 0, 0 ));
		float worldWidth = Mathf.Abs (rightScreen.x - leftScreen.x);

		//calculate max tile holder size
		tileHolderWidth = worldWidth / ( maxTileCount + ( maxTileCount + 1 ) * spaceAsFractionOfHolder);
		tileHolderSpace = tileHolderWidth * spaceAsFractionOfHolder;
		tileWidth = tileHolderWidth * tileAsFractionOfHolder;
	}

	void RescaleObjects()  
	{
		//scale holder
		SpriteRenderer holderRenderer = (SpriteRenderer)tileHolder.GetComponent (typeof(SpriteRenderer));
		float originalTileSizeHolder = holderRenderer.bounds.extents.x * 2;
		float scaleChangeHolder = tileHolderWidth / originalTileSizeHolder;
		tileHolder.transform.localScale *= scaleChangeHolder;

		//scale tile
		SpriteRenderer tileRenderer = (SpriteRenderer)coloredTile.GetComponent (typeof(SpriteRenderer));
		float originalTileSize = tileRenderer.bounds.extents.x * 2;
		float scaleChangeTile = tileWidth / originalTileSize;
		coloredTile.transform.localScale *= scaleChangeTile;

	}

	public void StageNewProblem(List<Tile> tilesToPositionInHolders, List<TileHolder> occupiedHolders )
	{

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
						tilesToOrder[ tile ].StartMove( holders[i]);
						tilesToOrder[ tile ].preset = true;
						holders[ i ].preSet = true;
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
			holders.Add( holderScript );

			//create a tile with a unique color and color name
			//place tiles above holders
			Vector3 stagingPosition = tilePosition + new Vector3 ( 0, tileHolderWidth + tileHolderSpace, 0 );
			StagingArea stagingArea = new StagingArea( stagingPosition, true );
			GameObject tile = ( GameObject ) Instantiate( coloredTile, stagingPosition, Quaternion.identity );
			tile.name = colorNames[ tileIndex ];
			tile.GetComponent<Renderer>().material.color = tileColors[ tileIndex ];
			Tile tileScript = tile.GetComponent<Tile>();
			tileScript.SetCurrentStaging( stagingArea );
			tile.transform.parent = transform;

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
				tileHolders[i].preSet = false;
			}

			tiles[i].currentHolder = null;
			tiles[i].preset = false;
			tileHolders[i].Highlight( false );
			tiles[i].targetHolder = null;
			tiles[i].StartMove( null );
		}
	}

	List<Vector3> GetTileHolderPositions( int tileCount)
	{
		List<Vector3> tilePositions = new List<Vector3>();

		float totalWidth = tileCount * tileHolderWidth + ((tileCount - 1) * tileHolderSpace);

		Vector3 startPosition = centerBoardPosition - new Vector3 ((totalWidth - tileHolderWidth) / 2, 0, 0);

		for( int tile = 0; tile < tileCount; tile ++ )
		{
			Vector3 tilePosition = startPosition + new Vector3( tile * ( tileHolderWidth + tileHolderSpace ), 0, 0 );
			tilePositions.Add ( tilePosition );

		}

		return tilePositions;
	}


}
