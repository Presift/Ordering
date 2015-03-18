using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class View : MonoBehaviour {

	public List<Color> tileColors;
	public List<string> colorNames;

	int maxTileCount;
	float tileWidth;
	float tileHolderSpace;
	float tileHolderWidth;
	public float tileAsFractionOfHolder;
	public float spaceAsFractionOfHolder;

	Vector3 centerBoardPosition = new Vector3( 0, 0, 0 );
	
	GameObject tileHolder;
	GameObject coloredTile;
	public Model model;

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
		//set all holders to open
		//set all staging areas to open

		//place preset tiles in their preset holders
	}

	public void PresetTilesWithHolders( List<Tile> tilesToPositionInHolders, List<TileHolder> occupiedHolders)
	{

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

	List<Vector3> GetTileHolderPositions( int tileCount)
	{
		List<Vector3> tilePositions = new List<Vector3>();

		float totalWidth = tileCount * tileHolderWidth + ((tileCount - 1) * tileHolderSpace);
//		Debug.Log (totalWidth);

		Vector3 startPosition = centerBoardPosition - new Vector3 ((totalWidth - tileHolderWidth) / 2, 0, 0);

		for( int tile = 0; tile < tileCount; tile ++ )
		{
			Vector3 tilePosition = startPosition + new Vector3( tile * ( tileHolderWidth + tileHolderSpace ), 0, 0 );
			tilePositions.Add ( tilePosition );

		}

		return tilePositions;
	}


}
