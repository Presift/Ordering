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

	public List<StagingArea> CreateBoard( int tileCount )
	{
		List<StagingArea> stagingAreas = new List<StagingArea>();

		//calculate positions for tile holders
		List <Vector3> tilePositions = GetTileHolderPositions (tileCount);


		for( int tileIndex = 0; tileIndex < tileCount; tileIndex ++ )
		{
			Vector3 tilePosition = tilePositions[ tileIndex ];

			//create tile holders and place centered on board, spaced evenly
			GameObject holder = ( GameObject ) Instantiate( tileHolder, tilePosition, Quaternion.identity );
			holder.transform.parent = transform;

			//create a tile with a unique color and color name
			//place tiles above holders
			tilePosition += new Vector3 ( 0, tileHolderWidth + tileHolderSpace, 0 );
			GameObject tile = ( GameObject ) Instantiate( coloredTile, tilePosition, Quaternion.identity );
			tile.name = colorNames[ tileIndex ];
			tile.renderer.material.color = tileColors[ tileIndex ];
			tile.transform.parent = transform;
			StagingArea stagingArea = new StagingArea( tilePosition, true );
			stagingAreas.Add ( stagingArea );
		}
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
//			Debug.Log ( tilePosition);
		}

		return tilePositions;
	}
}
