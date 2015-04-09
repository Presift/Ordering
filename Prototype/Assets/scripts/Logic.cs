using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logic : MonoBehaviour {

	public Model model;
	public MetaData metaData;
	public CSVReader levelReader;

	public RuleStack trialRules;

//	public int problemsPerTrial;

//	public int maxRuleDifficulty;
//	public int maxProblemDifficulty;
//	public int maxRules;
//	public int tilesCount;
	public int chanceOfImpossible;
//	public int maxImpossiblePerTrial;
	int impossiblesUsed;

	//rules that can be used
//	int maxAbsPosRules;
//	int maxRelativePosRules;
//	int maxAdjacencyRules;
//	int maxConditionals;
//	bool usingEitherOr;
//	bool usingPositiveOfAbsolute;

	//integers for rule types
//	int absPosition = 1;
//	int eitherOrAbsPosition = 2;
//	int negAbsPosition = 3;
//
//	int adjacent = 4;
//	int notAdjacent = 5;
//
//	int relativeOrder = 6;

	//difficulty for impossible board
//	int maxRulesToSetImpossibleBoard;
//	int maxConditionalInImpossibleSet;
//
	public int consecutiveTollensErrors;
	public int errorsCountToNeedHelp = 3;

	//CONDITIONALS

	//construction
//	int maxAbsInConditional;
//	int maxAdjInConditional;
//	int maxRelInConditional;
//
//	// difficulty for possible board
//	bool modusPonens; //if a then b
//	bool modusTollens; //if !b, then !a
//	bool modusTollensImplyNotA;
//
//	bool showClauseBNotA;  // b and !a
//	bool showClauseBImplyNotA;  // b and make a impossible

//	int maxModusTollens;
//	int maxBAndNotA;
	
	string previousPossiblePresetKey;
	string previousImpossiblePresetKey;

	List<Level> allLevels = new List<Level> ();

	public Level currentLeveling;

	void Awake()
	{

		CompileLevels (levelReader.levelingInfo);

	}

	// Use this for initialization
	void Start () {


	}

	// Update is called once per frame
	void Update () {
	
	}

	public void SetMinMaxResponseTimesForLevelChange()
	{
		float minReadingTime = 0;

		for(int i = 0; i < trialRules.ruleStack.Count; i ++ )
		{
			if( trialRules.ruleStack[ i ] is Conditional )
			{
				minReadingTime += 1.5f;
			}
			else
			{
				minReadingTime += .75f;
			}
		}

		float timeForTilePlacement = .5f * currentLeveling.tilesCount;

		model.responseTimeForMaxLevelChange = timeForTilePlacement + minReadingTime + 2;
		model.responseTimeForMinLevelChange = model.responseTimeForMaxLevelChange * 6;
		Debug.Log ("TimeForMaxLevelChange : " + model.responseTimeForMaxLevelChange);
		Debug.Log ("TimeForMinLevelChange : " + model.responseTimeForMinLevelChange);
	}
	
	public string CreateRules( List<Tile> tiles )
//		public string CreateRules( List<Tile> tiles, List<TileHolder> holders )
	{
		List< float > ruleIndentifiers = new List< float > ();

		model.SetImpossible (false);
		int difficultyPointsSpend = 0;
		int rulesCreated = 0;
		impossiblesUsed = 0;
		trialRules = new RuleStack ();

		List< int > positionsToUse = new List< int > ();
		for( int i = 0; i < tiles.Count; i ++ )
		{
			positionsToUse.Add ( i );
		}

		List<Tile> shuffledTiles = ShuffleThis (tiles);

		//create tile dictionary where each tile has yet ot be used
		Dictionary<Tile, int > tileUsage = new Dictionary< Tile , int > ();
		for( int i = 0; i < shuffledTiles.Count; i ++ )
		{
			tileUsage.Add ( shuffledTiles[ i ], 0 );
		}

//		Debug.Log ("max rules : " + maxRules );

		for( int conditional = 0; conditional < currentLeveling.maxConditionals; conditional ++ )
		{
			if( rulesCreated < currentLeveling.maxRules )
			{
				Conditional newConditional = CreateConditionalRule( positionsToUse, tiles, tileUsage );
//				newConditional.ConstructVerbal();
				if( !trialRules.RuleConflictsWithRuleStack( newConditional, currentLeveling.maxTrialsInRuleSet ))
				{
//					Debug.Log ( "successful rule : " + newConditional.verbal );
					trialRules.AddRule( newConditional );
					ruleIndentifiers.Add ( newConditional.ruleIdentifier );
					rulesCreated ++;
				}
				else
				{
					Debug.Log ( "UNsuccessful rule : " + newConditional.verbal );
					RemoveTilesUsedInRuleFromDict( tileUsage, newConditional );
					ReAddHolderPositionsFromRescindedRule( positionsToUse, newConditional );
				}
			}
		}


		for( int relatives = 0; relatives < currentLeveling.maxRelativePosRules; relatives ++ )
		{
			if( rulesCreated < currentLeveling.maxRules )
			{
				RelativePositionRule newRel = CreateRelativeRule( tiles, tileUsage );
				if( !trialRules.RuleConflictsWithRuleStack(newRel, currentLeveling.maxTrialsInRuleSet ))
				{
//					Debug.Log ( "successful rule : " + newRel.verbal );
					trialRules.AddRule( newRel );
					ruleIndentifiers.Add ( newRel.ruleIdentifier );
					rulesCreated ++;
				}
				else
				{
					Debug.Log ( "UNsuccessful rule : " + newRel.verbal);
					RemoveTilesUsedInRuleFromDict( tileUsage, newRel );
				}
			}

		}
		
		for( int adjacency = 0; adjacency < currentLeveling.maxAdjacencyRules; adjacency ++ )
		{
			if( rulesCreated < currentLeveling.maxRules )
			{
				AdjacencyRule newAdj = CreateAdjacencyRule( tiles, tileUsage );
				if( !trialRules.RuleConflictsWithRuleStack( newAdj, currentLeveling.maxTrialsInRuleSet ))
				{
//					Debug.Log ( "successful rule : " + newAdj.verbal );
					trialRules.AddRule( newAdj );
					ruleIndentifiers.Add ( newAdj.ruleIdentifier );
					rulesCreated ++;
				}
				else
				{
					Debug.Log ( "Unsuccessful rule : " + newAdj.verbal );
					RemoveTilesUsedInRuleFromDict( tileUsage, newAdj );
				}
			}

		}
		for( int abs = 0; abs < currentLeveling.maxAbsPosRules; abs ++ )
		{
			if( rulesCreated < currentLeveling.maxRules )
			{
				AbsolutePositionRule newAbs = CreateAbsoluteRule( positionsToUse, tiles, tileUsage );
				if( !trialRules.RuleConflictsWithRuleStack( newAbs, currentLeveling.maxTrialsInRuleSet ))
				{
//					Debug.Log ( "successful rule : " + newAbs.verbal );
					trialRules.AddRule( newAbs );
					ruleIndentifiers.Add ( newAbs.ruleIdentifier );
					rulesCreated ++;
				}
				else
				{
					Debug.Log ( "Unsuccessful rule : " + newAbs.verbal );
					RemoveTilesUsedInRuleFromDict( tileUsage, newAbs );
					ReAddHolderPositionsFromRescindedRule( positionsToUse, newAbs );
				}
			}
		}

		metaData.SetStatsForTrial (model.currentLevel, model.currentTrialInRound, model.currentRound, trialRules.ruleStack.Count, ruleIndentifiers, tiles.Count);
		metaData.SetTimeSinceProblemStart (Time.time);

		trialRules.ConstructRandomOrderedVerbal ();
		Debug.Log ("rule count : " + trialRules.ruleStack.Count);
		for( int i = 0; i < trialRules.ruleStack.Count; i++ )
		{
			Debug.Log ( trialRules.ruleStack[ i ].verbal );
		}
//		Debug.Log (" ************CORRECT ANSWERS****************** : " );
//		trialRules.PrintEachDictionaryValue (trialRules.correctSubmissions);
		SetMinMaxResponseTimesForLevelChange ();

		model.currentChallenge.SetPresetCount ( 0 );

		return trialRules.verbal;
	}

	void ReAddHolderPositionsFromRescindedRule( List< int > absolutePositions, Rule rule )
	{
		if( rule is AbsolutePositionRule )
		{
			AbsolutePositionRule absRule = rule as AbsolutePositionRule;
			absolutePositions.Add ( rule.absolutePositionIndex );
		}
		else if( rule is Conditional )
		{
			Conditional condRule = rule as Conditional;

			if( condRule.rule1 is AbsolutePositionRule )
			{
				AbsolutePositionRule absRule = condRule.rule1 as AbsolutePositionRule;
				absolutePositions.Add ( condRule.rule1.absolutePositionIndex );
			}
			if( condRule.rule2 is AbsolutePositionRule )
			{
				AbsolutePositionRule absRule = condRule.rule2 as AbsolutePositionRule;
				absolutePositions.Add ( condRule.rule2.absolutePositionIndex );
			} 
		}
	}

	bool ReusedTilesAvailable( Dictionary < Tile, int > tileUsage, List< Tile > unavailableTiles )
	{
		foreach( KeyValuePair< Tile , int > pair in tileUsage )
		{
			Tile tile = pair.Key;

			if( pair.Value == 1 )
			{
				if( unavailableTiles != null )
				{
					if( !unavailableTiles.Contains( pair.Key ))
					{
						return true;
					}
				}
				else
				{
					return true;
				}

			}
		}

		return false;
	}

	bool TileInList( List< Tile > tilesList, Tile tile )
	{
		if( tilesList.Contains( tile ))
		{
			return true;
		}

		return false;
	}

	List<Tile> GetTilesToUse( Dictionary < Tile, int > tileUsage, int tilesNeeded, List< Tile > unavailableTiles = null )
	{
		bool availableReusedTiles = ReusedTilesAvailable ( tileUsage, unavailableTiles );
		int minReusedTiles = 0;
		int maxReusedTiles = 0;

//		Debug.Log ("available reused : " + availableReusedTiles);
		if( availableReusedTiles )
		{
			minReusedTiles = 1;
			maxReusedTiles = 1;
		}

//		Debug.Log (" min reused : " + minReusedTiles + ", maxReused : " + maxReusedTiles);

		bool findTilesWithFewestReuses = false;
		int fewestReuses = 100;
		Tile tileWithFewestReuses = null;

		int reusedTiles = 0;

		List<Tile> tilesToUse = new List<Tile> ();


		while( tilesToUse.Count < tilesNeeded )
		{
			foreach( KeyValuePair< Tile , int > pair in tileUsage )
			{
				Tile tile = pair.Key;

				//if tile has been used only once and reused tiles is less than max reused tiles
				if( ( pair.Value == 1 ) && ( reusedTiles < maxReusedTiles ))
				{
					if( unavailableTiles == null && !TileInList( tilesToUse, tile ))
					{
						tilesToUse.Add ( tile );
						reusedTiles ++;
					}
					else if( !unavailableTiles.Contains( pair.Key )  && !TileInList( tilesToUse, tile ))
					{
						tilesToUse.Add ( tile );
						reusedTiles ++;
					}
				}

				else if( pair.Value == 0 && ( reusedTiles >= minReusedTiles ) && !TileInList( tilesToUse, tile ))
				{
					tilesToUse.Add ( tile );
				}

				else if( findTilesWithFewestReuses && !TileInList( tilesToUse, tile ))
				{
					if( pair.Value < fewestReuses )
					{
						if( unavailableTiles == null )
						{
							fewestReuses = pair.Value;
							tileWithFewestReuses = pair.Key;
						}
						else if( !unavailableTiles.Contains( pair.Key ))
						{
							fewestReuses = pair.Value;
							tileWithFewestReuses = pair.Key;
						}
					}
				}

				if( tilesToUse.Count == tilesNeeded )
				{
					for( int i = 0; i < tilesToUse.Count; i ++ )
					{
						tileUsage[tilesToUse[i]] ++;
//						Debug.Log ( "USED : " + tilesToUse[i]);
					}
					
					return tilesToUse;
				}
			}

			findTilesWithFewestReuses = true;
			if( tileWithFewestReuses != null )
			{
				tilesToUse.Add ( tileWithFewestReuses );
//				Debug.Log ("ADDED TILE WITH FEWEST REUSES");

				if( tilesToUse.Count == tilesNeeded )
				{
					for( int i = 0; i < tilesToUse.Count; i ++ )
					{
						tileUsage[tilesToUse[i]] ++;
//						Debug.Log ( "USED : " + tilesToUse[i]);
					}
					
					return tilesToUse;
				}
			}

		}

//		Debug.Log (" not enough tiles added ");
		return null;
	}

	RelativePositionRule CreateRelativeRule( List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{

		//randomly determine before/after rule
		int order = Random.Range (0, 2);

		List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2 );

		if( tilesToUse == null )
		{
			return null;
		}

		RelativePositionRule relPositionRule = new RelativePositionRule( order, tilesToUse[0], tilesToUse[1], tilesToOrder  ); 
		relPositionRule.ConstructVerbal();
		
		return relPositionRule;
	}

	AdjacencyRule CreateAdjacencyRule( List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{
		int nextTo = Random.Range (0, 2);  //determins next to/not next to

		List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2 );

		AdjacencyRule adjRule = new AdjacencyRule( nextTo, tilesToUse[0], tilesToUse[1], tilesToOrder  ); 
		adjRule.ConstructVerbal();
		
		return adjRule;
	}

	AbsolutePositionRule CreateAbsoluteRule( List<int> holderPositions, List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage, List< Tile > unusableTiles = null, bool isPartOfConditional = false )
	{

		//create absolute position rule
		int absolutePosition = Random.Range ( 0, holderPositions.Count );	

		holderPositions.Remove (absolutePosition);

		int absPosTile = Random.Range (0, tilesToOrder.Count);

		//decide if using 1 or 2 tiles for rule
		if (currentLeveling.usingEitherOr) 
		{
			int random = Random.Range (0, 2);
			if( random == 0 ) //create rule with 2 tiles
			{

				List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2, unusableTiles );
				
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToUse[0], tilesToUse[1], absolutePosition, tilesToOrder ); 
				absPositionRule.ConstructVerbal();
				
				return absPositionRule;
			}
		
			else
			{
				int negatedAbsolute;  //0 is positive, 1 is negative ( i know this is counter intuitive )
				if( !isPartOfConditional && !currentLeveling.usingPositiveOfAbsolute )
				{
					negatedAbsolute = 1;
				}
				else
				{
					negatedAbsolute = 0;
				}

				List<Tile> tilesToUse = GetTilesToUse (tileUsage, 1, unusableTiles);
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( negatedAbsolute, tilesToUse[0], absolutePosition, tilesToOrder ); 
				absPositionRule.ConstructVerbal();
				
				return absPositionRule;
			}
				
		}
		else
		{
			List<Tile> tilesToUse = GetTilesToUse (tileUsage, 1, unusableTiles );
			AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToUse[0], absolutePosition, tilesToOrder ); 
			absPositionRule.ConstructVerbal();
			
			return absPositionRule;
		}
	
	}
//

	Conditional CreateConditionalRule( List<int> holderPositions, List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{

		List<Tile> unusableTiles = new List<Tile> ();

		List< Rule > rulesPoolForConditional = new List< Rule > ();

		for( int i = 0; i < currentLeveling.maxRelInConditional; i ++ )
		{
			RelativePositionRule relRule = CreateRelativeRule( tilesToOrder, tileUsage );
			rulesPoolForConditional.Add ( relRule as Rule );
		}

		for( int i = 0; i < currentLeveling.maxAdjInConditional; i ++ )
		{
			AdjacencyRule adjRule = CreateAdjacencyRule( tilesToOrder, tileUsage );
			rulesPoolForConditional.Add ( adjRule as Rule );
		}

		for( int i = 0; i < currentLeveling.maxAbsInConditional; i ++ )
		{
			Debug.Log ( holderPositions.Count );
			AbsolutePositionRule absRule = CreateAbsoluteRule( holderPositions, tilesToOrder, tileUsage, unusableTiles, true );
			unusableTiles.Add ( absRule.tile1 );

			if ( absRule.tile2 != null )
			{
				unusableTiles.Add ( absRule.tile2 );
			}
			rulesPoolForConditional.Add ( absRule as Rule );
		}

		Rule rule1 = rulesPoolForConditional [Random.Range (0, rulesPoolForConditional.Count)];
		rulesPoolForConditional.Remove (rule1);
		Rule rule2 = rulesPoolForConditional [Random.Range (0, rulesPoolForConditional.Count)];

		Conditional newConditional = new Conditional (rule1, rule2, tilesToOrder );
		bool validRule = newConditional.IsValidRule ();

		while( !validRule )
		{
			//remove a point for each used tile in tileUsage dict
			Debug.Log ( " FAIL : " + newConditional.verbal );
			RemoveTilesUsedInRuleFromDict( tileUsage, newConditional );
			ReAddHolderPositionsFromRescindedRule( holderPositions, newConditional );
			newConditional = CreateConditionalRule( holderPositions, tilesToOrder, tileUsage );
			validRule = newConditional.IsValidRule();
		}

		return newConditional;
	}

	public void RemoveTilesUsedInRuleFromDict( Dictionary < Tile, int > tileUsage, Rule rule )
	{
		List<Tile> tilesUsed = rule.tilesUsedInRule;

		for( int i = 0; i < tilesUsed.Count; i ++ )
		{
			Tile tileToUnuse = tilesUsed[ i ];
			tileUsage[ tileToUnuse ] --;
		}
	}
	

	public string NewTrialSetUp( List<Tile> previousSubmission )
	{
//		model.
		//refresh tile positions
		bool createImpossible = false;
		string presetTiles;
		bool setUpWithPresets;

		//if first trial
		if( model.currentTrialInRound == 0 )
		{
			setUpWithPresets = HappenedByChance (currentLeveling.chanceOfPresetsOnFirstTrial);
		}
		else
		{
			setUpWithPresets = true;
		}


		if( impossiblesUsed < currentLeveling.maxImpossiblePerTrial )
		{
			createImpossible = HappenedByChance( chanceOfImpossible );

		}
//		Debug.Log ("create impossible : " + createImpossible );
		if( createImpossible )
		{
			presetTiles = AttemptToCreateImpossibleBoard( previousSubmission, model.tilesToOrder );
		}
		else
		{
			presetTiles = CreatePossibleBoard( previousSubmission );
		}
		Debug.Log (presetTiles);
		return presetTiles;
	}
	

	string CreatePossibleBoard( List<Tile> previousSubmission )
	{
		Debug.Log ("CREATING POSSIBLE BOARD");
		//create  key string from previousSubmission

		List< string > tileBank = GetTilesAsListOfLetters (model.tilesToOrder);

		List < string > bestPresetTiles = GetBestPresetToCompleteBoard (2, tileBank);

		string presetTiles = bestPresetTiles [Random.Range (0, bestPresetTiles.Count)];

		Debug.Log (presetTiles);
		//	create string with only 1 placed tile in newSubmission
		
		model.SetImpossible( false );
		
		previousPossiblePresetKey = presetTiles;
		return presetTiles;
	}

	void ConcatenateAndPrintListOfStrings( List< string > stringList )
	{
		string stringToPrint = "";

		for( int i = 0; i < stringList.Count; i ++ )
		{
			stringToPrint += stringList[ i ];
		}

		Debug.Log (stringToPrint);
	}

	List< string > GetTilesAsListOfLetters( List< Tile > tiles )
  	{
		List< string > letters = new List< string > ();
		for( int i = 0; i < tiles.Count; i ++ )
		{
			letters.Add ( tiles[ i ].name[ 0 ].ToString() );
		}
		return letters;
	}

	bool PresetIsNew( string possiblePreset )  //fewest number of preset tiles that completely fills in tiles
	{

		//if preset matches previouspossible submission
		if (previousPossiblePresetKey != null) 
		{
			if( previousPossiblePresetKey == possiblePreset )
			{
				return false;
			}
		}

		return true;

	}

	int GetPresetInstances( string possiblePreset )
	{
		int possiblePresetCount = trialRules.CountWildCardInDictionary (possiblePreset, trialRules.correctSubmissions);

		return possiblePresetCount;
	}

	void GetAllCombinationsForPresets( List< string > presetBank, List< string > currentCombo, int maxPresetTiles, List< List<string> > allCombos  )
	{
		if( currentCombo.Count < maxPresetTiles )
		{
			//continue permutating
			for( int i = 0; i < presetBank.Count; i ++ )
			{
				List< string > newCombo = new List< string > ( currentCombo );
				newCombo.Add ( presetBank[ i ] );

				List< string > newBank = new List<string> ( presetBank );
				newBank.Remove( presetBank[ i ] );

				GetAllCombinationsForPresets( newBank, newCombo, maxPresetTiles, allCombos );  //combine this list of tiles with newTilePermuation list?
				
			}
		}
		else
		{
//			ConcatenateAndPrintListOfStrings( currentCombo );

			allCombos.Add ( currentCombo );
		}
	}

	void GetPresetOrdersFromCombos( List< int > currPositionOrder, List< int > positionBank, int stringLength,  List< string > presetTileCombo, List< string > allPresetTileCombos, string filler = "n" ) 
	{
		if( currPositionOrder.Count < presetTileCombo.Count )
		{
			List < int > workingSet = new List < int > ( positionBank );
			//continue permutating
			for( int i = 0; i < workingSet.Count; i ++ )
			{
				List<int> newPositionOrder = new List<int> ( currPositionOrder );
				newPositionOrder.Add ( workingSet[i] );

				positionBank.Remove( workingSet[i] );
				List<int> newPositionBank = new List<int>( positionBank );

				GetPresetOrdersFromCombos( newPositionOrder, newPositionBank, stringLength, presetTileCombo, allPresetTileCombos  );  //combine this list of tiles with newTilePermuation list?
				
			}
		}
		else
		{
			List<string> stringBank = new List<string>( presetTileCombo );
			string newKeyCombo = "";
			for( int position = 0; position < stringLength; position ++ )
			{
				if( currPositionOrder.Count > 0 )
				{
					if( position == currPositionOrder[ 0 ] )
					{
						newKeyCombo += stringBank[ 0 ];
						currPositionOrder.RemoveAt( 0 );
						stringBank.RemoveAt( 0 );
					}
					else
					{
						newKeyCombo += filler;
					}
				}
				else
				{
					newKeyCombo += filler;
				}
			}
//			Debug.Log ( newKeyCombo );
			allPresetTileCombos.Add ( newKeyCombo );

		}
	}
	

	bool KeyAlwaysBreaksRule( string key, Rule rule )
	{
		if( rule.WildCardKeyInDictionary( key, rule.correctSubmissions ))
		{
			return false;
		}

		return true;
	}

	bool KeyNeverBreaksRule( string key, Rule rule )
	{
		if( rule.WildCardKeyInDictionary( key, rule.incorrectSubmissions ))
		{
			return false;
		}
		return true;
	}

	bool KeyIsContraPositiveOfConditional( bool clause2AlwaysBroken )
	{
		if( clause2AlwaysBroken )
		{
			return true;
		}

		return false;
	}

	bool KeyIsPositiveOfConditional( bool clause1AlwaysTrue )
	{
		if( clause1AlwaysTrue )
		{
			return true;
		}

		return false;
	}

	bool KeyIsBAndNeverA( bool clause2AlwaysTrue, bool clause1AlwaysFalse )
	{
		if( clause2AlwaysTrue && clause1AlwaysFalse )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	bool PresetIsPossibleCorrectSubmission( string key )
	{
		if( trialRules.WildCardKeyInDictionary( key, trialRules.correctSubmissions ))
		
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	bool CompletionCountIsWorthPresetCount( int newKeyPresetCount, int bestPresetCount, int newKeyInstances, int fewestPossibleCompletions )
	{
		if( newKeyPresetCount > bestPresetCount && fewestPossibleCompletions > 0 )
		{
			//ensure that instances for new preset are fewer than fewest instances - additional presets
			if( newKeyInstances < ( fewestPossibleCompletions - ( newKeyPresetCount - bestPresetCount )))
			{
				return true;
			}

			return false;
		}

		else if ( newKeyPresetCount == bestPresetCount && fewestPossibleCompletions > 0 )
		{
			if( newKeyInstances < fewestPossibleCompletions )
			{
				return true;
			}
		}
		// presets less than bestPreset Count ( would indicated that no best key has been added yet )
		else if ( newKeyInstances < fewestPossibleCompletions && fewestPossibleCompletions > 0 )
		{
			return true;
		}

		return false;
	}

	List< string > GetBestPresetToCompleteBoard ( int maxPresets, List< string > tileBank )
	{
		int fewestPossibleCompletions = 1000;
		List< string > bestKeys = new List< string >();
		int bestPresetCount = 10;

		List< Conditional > condRules = GetConditionalsFromStack (trialRules);

		bool containsConditional = condRules.Count > 0;
		//only important for conditionals
		bool bestKeyUsesModusPonens = false;
		bool bestKeyUsesModusTollens = false;
		bool bestKeyUsesBAndNotA = false;

		int currentPresets = 1;

		while( currentPresets <= maxPresets )
		{
			List< List<string> > tileCombos = new List< List<string> >();
			List< string > newCombo = new List< string > ();
			GetAllCombinationsForPresets( tileBank, newCombo, currentPresets, tileCombos);

//			Debug.Log ( " modus tollens : " + modusTollens );
//			Debug.Log (" modus ponens : " + modusPonens );
//			Debug.Log (" bAnd Not A : " + showClauseBNotA );
			//for each combo in allCombos
			for( int comboIndex = 0; comboIndex < tileCombos.Count; comboIndex ++ )
			{
				List<int> digitBank = CreateDigitBank( tileBank.Count );
				
				List< string > presetKeyCombos = new List< string > ();
				
				List<int> currPosOrder = new List<int> ();
				
				GetPresetOrdersFromCombos( currPosOrder, digitBank, digitBank.Count, tileCombos[ comboIndex ], presetKeyCombos );
				
				for( int presetOrderIndex = 0; presetOrderIndex < presetKeyCombos.Count; presetOrderIndex ++ )
				{
					string preset = presetKeyCombos[ presetOrderIndex ];

					if( PresetIsNew( preset ) && trialRules.WildCardKeyInDictionary( preset, trialRules.correctSubmissions))
					{
						//test to see if preset has fewer possibilites than best
						int instances = GetPresetInstances( preset );
						bool presetReplacesBest = false;

						if( containsConditional )
						{

							bool testKeyUsesBAndNotA = false;
							bool testKeyUsesModusTollens = false;
							bool testKeyUsesModusPonens = false;

							for( int cond = 0; cond < condRules.Count; cond ++ )
							{
								Conditional condRule = condRules[ cond ];
								bool alwaysBreaksRule1 = KeyAlwaysBreaksRule( preset, condRule.rule1 );
								bool neverBreaksRule1 = KeyNeverBreaksRule( preset, condRule.rule1 );
								bool alwaysBreaksRule2 = KeyAlwaysBreaksRule( preset, condRule.rule2 );
								bool neverBreaksRule2 = KeyNeverBreaksRule( preset, condRule.rule2 );

								if( currentLeveling.showClauseBNotA )
								{
									if( KeyIsBAndNeverA( neverBreaksRule2, alwaysBreaksRule2 ))
									{
										testKeyUsesBAndNotA = true;
//										Debug.Log ( "bAndNotA : " + preset );
									}
								}
								if( currentLeveling.modusTollens )
								{
									if( KeyIsContraPositiveOfConditional( alwaysBreaksRule2 ))
									{

//										Debug.Log ("tollens : " + preset );
										if( currentLeveling.modusTollensImplyNotA )  //this is easier for most people; it shows clause 2 as false and has user fill in NOT A ( rather than preset breaking always both b and a )
										{
											if( !alwaysBreaksRule1 )
											{
												testKeyUsesModusTollens = true;
											}
										}
										else
										{
											testKeyUsesModusTollens = true;
										}
									}
								}
								if( currentLeveling.modusPonens )
								{
									if( KeyIsPositiveOfConditional( neverBreaksRule1 ))
									{
										testKeyUsesModusPonens = true;
//										Debug.Log ( "ponens : " +preset );
									}
									
								}
							}

							if( !bestKeyUsesBAndNotA && testKeyUsesBAndNotA )
							{
//								fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
								presetReplacesBest = true;
								bestKeyUsesBAndNotA = true;
							}
							else if ( bestKeyUsesBAndNotA && testKeyUsesBAndNotA && instances == fewestPossibleCompletions && currentPresets == bestPresetCount  )
							{
								bestKeys.Add ( preset );
//								Debug.Log ( " added : " + preset );
							}
							else if( !bestKeyUsesModusTollens && testKeyUsesModusTollens && !bestKeyUsesBAndNotA )
							{
//								fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
								presetReplacesBest = true;
								bestKeyUsesModusTollens = true;
							}
							else if( bestKeyUsesModusTollens && testKeyUsesModusTollens && !bestKeyUsesBAndNotA && instances == fewestPossibleCompletions && currentPresets == bestPresetCount )
							{
								bestKeys.Add ( preset );
//								Debug.Log ( " added : " + preset );
							}
							else if( !bestKeyUsesModusPonens && testKeyUsesModusPonens && !bestKeyUsesModusTollens )
							{
//								fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
								presetReplacesBest = true;
								bestKeyUsesModusPonens = true;
							}
							else if( bestKeyUsesModusPonens && testKeyUsesModusPonens && !bestKeyUsesModusTollens && instances == fewestPossibleCompletions && currentPresets == bestPresetCount )
							{
								bestKeys.Add ( preset );
//								Debug.Log ( " added : " + preset );
							}
						}
						// if no conditionals in stack
						else
						{
							if ( CompletionCountIsWorthPresetCount(currentPresets, bestPresetCount, instances, fewestPossibleCompletions) )
							{
//								fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
								presetReplacesBest = true;
							}
							else if ( instances == fewestPossibleCompletions && currentPresets == bestPresetCount )
							{
								bestKeys.Add ( preset );
//								Debug.Log ( " added : " + preset );
							}
						}

						if( presetReplacesBest )
						{
							bestKeys = new List< string >();
							bestKeys.Add ( preset );
//							Debug.Log ( " added : " + preset );
							fewestPossibleCompletions = instances;
//							fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
							bestPresetCount = currentPresets;

//							Debug.Log ("***keys count : " + bestKeys.Count);
						}
					}

				}
			}

			currentPresets ++;
			Debug.Log ( "increase presets to : " + currentPresets );
		}
		Debug.Log ("fewest possible completions : " + fewestPossibleCompletions);
		Debug.Log ("best keys count : " + bestKeys.Count);
		Debug.Log (" modus Ponens : " + bestKeyUsesModusPonens + ", modus Tollens : " + bestKeyUsesModusTollens + ", bAndNotA : " + bestKeyUsesBAndNotA);

		model.currentChallenge.SetPresetCount (bestPresetCount);
		model.currentChallenge.SetConditionalLogic (bestKeyUsesModusPonens, bestKeyUsesModusTollens, bestKeyUsesBAndNotA);
		Debug.Log ("current challenge using tollens : " + model.currentChallenge.usesModusTollens);

		return bestKeys;

	}


	int ReplaceBestKeysWithTestKey( List< string > bestKeys, string preset, int presetInstances )
	{
		bestKeys = new List< string >();
		bestKeys.Add ( preset );

		return presetInstances;
	}



	RuleStack ReturnRuleStackFromComboList( List< List<Rule> > allRuleCombos, int comboListIndex )
	{
		RuleStack rulesToBreak = new RuleStack ();
		List<Rule> rules = allRuleCombos [ comboListIndex ];

		for( int i = 0; i < rules.Count; i ++ )
		{
			rulesToBreak.AddRule ( rules [ i ] );
		}

		return rulesToBreak;
	}

	bool RuleStackContainsConditional( RuleStack rules )
	{
		for( int i = 0; i < rules.ruleStack.Count; i ++ )
		{
			if( rules.ruleStack[ i ] is Conditional )
			{
				return true;
			}
		}

		return false;
	}

	List< Conditional > GetConditionalsFromStack( RuleStack rules )
	{
		List<Conditional> condRules = new List< Conditional > ();

		for( int i = 0; i < rules.ruleStack.Count; i ++ )
		{
			if( rules.ruleStack[ i ] is Conditional )
			{
				condRules.Add ( rules.ruleStack[ i ] as Conditional );
			}
		}
		
		return condRules;
	}



	string AttemptToCreateImpossibleBoard( List<Tile> previousSubmission, List<Tile> tilesToOrder )
	{

		Debug.Log ("ATTEMPTING TO CREATE IMPOSSIBLE");
		List< List<Rule> > allRuleCombos = new List< List<Rule> > ();
		List< Rule > newCombo = new List<Rule > ();
		List< Rule > ruleBank = new List<Rule>( trialRules.ruleStack );

		int rulesToSetImpossible = Mathf.Min (currentLeveling.maxRulesToSetImpossibleBoard, trialRules.ruleStack.Count);
		GetAllCombinationsOfRules (ruleBank, newCombo, rulesToSetImpossible, allRuleCombos);

		List<string> presetTiles = new List<string>();
		string presets;

		Debug.Log (" combo list count : " + allRuleCombos.Count); 
		List< string > tileBank = GetTilesAsListOfLetters (model.tilesToOrder);

		int maxPresetTiles = Mathf.Min ( tilesToOrder.Count - 2, 3 );
		int minPresetTiles = 1;
		
		while( presetTiles.Count == 0 && minPresetTiles <= maxPresetTiles )
		{
			for( int i = 0; i < allRuleCombos.Count; i ++ )
			{
				RuleStack rulesToBreak = ReturnRuleStackFromComboList ( allRuleCombos, i );
				
//				UpdateMinPresets (minPresetTiles, rulesToBreak);
				
				List< Rule > otherRules = trialRules.GetRulesInStackNotInList (rulesToBreak.ruleStack);

				presetTiles = AttemptToGetImpossibleKey( minPresetTiles, tileBank, rulesToBreak, otherRules );

				if(  presetTiles.Count != 0 )
				{
					Debug.Log (presetTiles.Count + " impossible presets found ");
					model.currentChallenge.SetPresetCount (minPresetTiles);
					model.currentChallenge.SetImpossibleStats( true, rulesToSetImpossible );
					break;
				}
			}

			//if this is read then preset is null
			//increase min preset tiles
			minPresetTiles ++;

			// if presets are maxed and rules can still be dropped from rule combinations
			if( minPresetTiles > maxPresetTiles && rulesToSetImpossible > currentLeveling.minRulesToSetImpossibleBoard )
			{
				//make more rule combos with fewer rules
				allRuleCombos = new List< List<Rule> > ();
				newCombo = new List<Rule > ();
				ruleBank = new List<Rule>( trialRules.ruleStack );

				rulesToSetImpossible --;

				GetAllCombinationsOfRules (ruleBank, newCombo, rulesToSetImpossible, allRuleCombos);

				tileBank = GetTilesAsListOfLetters (model.tilesToOrder);
				
				maxPresetTiles = Mathf.Min ( tilesToOrder.Count - 2, 3 );
				minPresetTiles = 1;
			}
		}

		if(  presetTiles.Count == 0 )
		{
			Debug.Log ("CREATING IMPOSSIBLE BOARD");
			model.SetImpossible( true );
			impossiblesUsed ++;
			Debug.Log ("found valid preset tile order ");

			presets = presetTiles[ Random.Range( 0, presetTiles.Count) ];
			previousImpossiblePresetKey = presets;
		}
		else
		{
			model.SetImpossible( false );
			presets = CreatePossibleBoard( previousSubmission );
			Debug.Log ("impossible preset NOT found, creating possible board ");
		}

		return presets;
	}


	List<string> AttemptToGetImpossibleKey( int presets, List<string> tileBank, RuleStack rulesToBreak, List< Rule> nonBreakingRules )
	{
		List<string> impossibleKeys = new List<string> ();

		bool singleRuleBreak = rulesToBreak.ruleStack.Count == 1;

		Debug.Log ("RULES TO BREAK : ");
		for (int rule = 0; rule < rulesToBreak.ruleStack.Count; rule ++) {
			Debug.Log (rulesToBreak.ruleStack [ rule ].verbal );
		}

		List< List<string> > tileCombos = new List< List<string> >();
		List< string > newCombo = new List< string > ();

		GetAllCombinationsForPresets( tileBank, newCombo, presets, tileCombos);

		//for each combo in allCombos
		for( int comboIndex = 0; comboIndex < tileCombos.Count; comboIndex ++ )
		{
			List<int> digitBank = CreateDigitBank( tileBank.Count );

			List< string > presetKeyCombos = new List< string > ();

			List<int> currPosOrder = new List<int> ();

			GetPresetOrdersFromCombos( currPosOrder, digitBank, digitBank.Count, tileCombos[ comboIndex ], presetKeyCombos );

			for( int presetOrderIndex = 0; presetOrderIndex < presetKeyCombos.Count; presetOrderIndex ++ )
			{
				if( presetKeyCombos[ presetOrderIndex ] != previousImpossiblePresetKey )
				{
					if( ImpossibleKey( presetKeyCombos[ presetOrderIndex ], rulesToBreak, nonBreakingRules, singleRuleBreak ))
					{
//						return ;
						impossibleKeys.Add (presetKeyCombos[ presetOrderIndex ]);
					}
				}
			}
		}
			
		return impossibleKeys;
	}

	List<int> CreateDigitBank( int length )
	{
		List<int> digitBank = new List<int> ();
		for( int i = 0; i < length; i ++ )
		{
			digitBank.Add ( i );
		}

		return digitBank;
	}

	List< string > GetCorrectSubmissionsForWildKey( string key, Rule rule )
	{
		List< string > correctSubmissions = new List< string > ();
		
		foreach( KeyValuePair<string, List<Tile>> pair in rule.correctSubmissions )
		{ 
			if( rule.WildCardSubmissionKeyNameMatch( key, pair.Key ))
			{
				correctSubmissions.Add ( pair.Key );
			}
		}
		
		return correctSubmissions;
		
	}

	bool FoundASubmissionNotInExcludedListButSharedByAllLists( List< string > excluded, List<List<string>> included )
	{
		// for each submission in first included rule submissions
		for( int sub = 0; sub < included[ 0 ].Count; sub ++ )
		{
			string submission = included[ 0 ][ sub ];

			// if submission is not in excluded rule
			if( !excluded.Contains( submission ) )
			{
				//check to see if submission is in other included rules
				if( included.Count == 1 )
				{
					return true;
				}
				for( int shared = 1; shared < included.Count; shared ++ )
				{
					if( !included[ shared ].Contains( submission ))
					{
						break;
					}
					else
					{
						//if submission is shared by all of the included rules' correct submissions
						if( shared == ( included.Count - 1 ))
						{
							return true;
						}
					}
				}
			}
		}

		return false;
	}


	bool ImpossibleKey( string possibleKey, RuleStack rulesToBreak, List<Rule> nonbreakingRules,  bool singleRuleBreak )
	{
		
		if(!PresetIsPossibleCorrectSubmission( possibleKey ))
		{
//			Debug.Log ("PRESET NOT IN CORRECT SUBMISSIONS : " + possibleKey);
			
			//test to see if preset is a possible correct answer each concerned breakable rule
			bool passBreakableRulesTest = false;

			
			if( singleRuleBreak )
			{
				List< string > correctSubmissions = GetCorrectSubmissionsForWildKey( possibleKey, rulesToBreak.ruleStack[ 0 ] );
//				Debug.Log ( "correct submissions : " + correctSubmissions.Count );
				if( correctSubmissions.Count == 0 )
				{
					passBreakableRulesTest = true;
				}

			}
			else
			{
				bool eachRuleHasAPossibleCorrect = true;

				List< List< string > > correctSubmissionsForKeyByRule = new List< List< string > >();

				for ( int ruleToBreak = 0; ruleToBreak < rulesToBreak.ruleStack.Count; ruleToBreak ++ )
				{
					List< string > correctSubsForRule = GetCorrectSubmissionsForWildKey( possibleKey, rulesToBreak.ruleStack[ ruleToBreak ] );
					if( correctSubsForRule.Count == 0 )
					{
						eachRuleHasAPossibleCorrect = false;
						break;
					}
					else
					{
						correctSubmissionsForKeyByRule.Add ( correctSubsForRule );
					}
				}


				if( eachRuleHasAPossibleCorrect )
				{
//					Debug.Log (" each rule has possible corrects : " + eachRuleHasAPossibleCorrect );

					//check that each rule does not share a correct submission that is shared by the other breakable rules
					for ( int ruleToExclude = 0; ruleToExclude < rulesToBreak.ruleStack.Count; ruleToExclude ++ )
					{
						List< string > subsToExclude = new List<string>();
						List< List< string >> listOfSubsToFindShared = new List<List<string>> ();

						for ( int ruleToBreak = 0; ruleToBreak < rulesToBreak.ruleStack.Count; ruleToBreak ++ )
						{
							if( ruleToBreak == ruleToExclude )
							{
								subsToExclude = correctSubmissionsForKeyByRule[ ruleToBreak ];
							}
							else
							{
								listOfSubsToFindShared.Add ( correctSubmissionsForKeyByRule[ ruleToBreak ] );
							}
						}

						bool ruleNeededToCreateImpossible = FoundASubmissionNotInExcludedListButSharedByAllLists( subsToExclude, listOfSubsToFindShared );

						if( !ruleNeededToCreateImpossible )
						{
							break;
						}

						else if( ruleToExclude == ( rulesToBreak.ruleStack.Count - 1 ))
						{
							Debug.Log ( "EACH RULE NEEDED TO CREATE IMPOSSIBLE ");
							passBreakableRulesTest = true; 
						}
					}

				}
				
			}

			
			if( passBreakableRulesTest )
			{
//				Debug.Log ( "PRESET DOES NOT BREAK INDIVIDUAL RULE ");
				bool passOtherRulesTest = true;
				//test to see if preset is impossible for non-concerned rules in trial rules
				for( int nonbreakingRule = 0; nonbreakingRule < nonbreakingRules.Count; nonbreakingRule ++ )
				{
					if( !nonbreakingRules[ nonbreakingRule ].WildCardKeyInDictionary( possibleKey, nonbreakingRules[ nonbreakingRule ].correctSubmissions ))
					{
//						Debug.Log ( "PRESET NOT IN OTHER RULE'S POSSIBLE CORRECTS ");
						passOtherRulesTest = false;
						break;
					}
				}
				
				if( passOtherRulesTest )
				{
//					Debug.Log ( "SUCCESSFUL PRESET BREAK" );
					for( int i = 0; i < rulesToBreak.ruleStack.Count; i ++ )
					{
						Debug.Log (rulesToBreak.ruleStack[ i ].verbal);
					}
					return true;
				}
			}
		}
		
		return false;
	}



	void GetAllCombinationsOfRules ( List<Rule> ruleBank, List<Rule> currRuleCombo, int maxRulesInStack, List< List<Rule> > allCombos )
	{
		if( currRuleCombo.Count < maxRulesInStack )
		{
			List < Rule > workingSet = new List < Rule > ( ruleBank );

			for( int i = 0; i < workingSet.Count; i ++ )
			{
				List< Rule > newCombo = new List< Rule > ( currRuleCombo );
				newCombo.Add ( workingSet[ i ] );

				ruleBank.Remove( workingSet[ i ] );
				List< Rule > newBank = new List<Rule> ( ruleBank );

				GetAllCombinationsOfRules( newBank, newCombo, maxRulesInStack, allCombos );  //combine this list of tiles with newTilePermuation list?
				
			}
		}
		else
		{
			allCombos.Add ( currRuleCombo );
		}
	}

	
	RuleStack CreateRuleStackFromRuleList( List< Rule > rulesList )  //nothing here ensures that rules will share common impossibilities
	{
		RuleStack combinedRules = new RuleStack ();
	
		for(int i = 0; i < rulesList.Count; i ++ )
		{
			combinedRules.AddRule ( rulesList[ i ] );
			
		}
		Debug.Log ("rules in rulestack : " + combinedRules.ruleStack.Count);
		return combinedRules;
		
	}



	List<Tile> GetRandomValueFromDictionary( Dictionary<string, List<Tile>> submissionDict )  //move to Rule class
	{
		List<string> keys = new List < string > ( submissionDict.Keys );
		List<Tile> randomValue = submissionDict[ keys[ Random.Range (0, keys.Count) ] ];
		return randomValue;
	}


//	void SetRuleCreationParameters( int maxCond, int maxRel, int maxAdj, int maxAbs )
//	{
//		maxConditionals = maxCond;
//		maxRelativePosRules = maxRel;
//		maxAdjacencyRules = maxAdj;
//		maxAbsPosRules = maxAbs;
//		maxRules = maxConditionals + maxRelativePosRules + maxAdjacencyRules + maxAbsPosRules;
//
//	}
//
//	void SetConditionalParameters( int maxRel, int maxAdj, int maxAbs )
//	{
//		maxAbsInConditional = maxAbs;
//		maxAdjInConditional = maxAdj;
//		maxRelInConditional = maxRel;
//	}
//
//	void SetTileCount( int tilesInTrial )
//	{
//		tilesCount = tilesInTrial;
//	}
//
//	void SetImpossibleBoardParameters( int rulesToSetBoard, int impChance, int maxImp )
//	{
//		maxRulesToSetImpossibleBoard = rulesToSetBoard;
//		chanceOfImpossible = impChance;
//		maxImpossiblePerTrial = maxImp;
//	}
//
//	void SetPossibleBoardParameters( bool AThenB, bool notBThenNotA, bool contraPositivePresetDoNotAlwaysBreakClause1, bool showBAndNotA )
//	{
//		modusPonens = AThenB;
//		modusTollens = notBThenNotA;
//		showBAndNotA  = showClauseBNotA;
//		modusTollensImplyNotA = contraPositivePresetDoNotAlwaysBreakClause1;
//	}

	public void UpdateLevelingStats( int currentLevel )
	{
		currentLeveling = allLevels [currentLevel];

		if ( !model.impossibleEnabled )
		{
			chanceOfImpossible = 0;
		}
		else
		{
			chanceOfImpossible = currentLeveling.chanceOfImpossible;
		}
	}

//	public void UpdateLevelingStats( int currentLevel )
//	{
//		ResetStats ();

//		Debug.Log (currentLevel);

//		if (currentLevel == 0) 
//		{
//			SetRuleCreationParameters( 0, 1, 0, 0 );
//			SetTileCount ( 3 );
//			SetImpossibleBoardParameters( 1, 30, 1 );
//			SetPossibleBoardParameters( false, false, false, false );
//			usingPositiveOfAbsolute = true;
//		}
//
//		else if( currentLevel == 1 )
//		{
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			maxRules = 2;
//			tilesCount = 3;
//			chanceOfImpossible = 75;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingPositiveOfAbsolute = true;
//		}
//		else if( currentLevel == 2 )
//		{
//			maxRelativePosRules = 1;
//			maxAdjacencyRules = 1;
//			maxRules = 2;
//			tilesCount = 4;
//			chanceOfImpossible = 40;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingEitherOr = true;
//			usingPositiveOfAbsolute = true;
//		}
//		else if( currentLevel == 3 )
//		{
//			maxRelativePosRules = 2;
//			maxRules = 2;
//			tilesCount = 4;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingEitherOr = true;
//			usingPositiveOfAbsolute = true;
//		}
//		else if( currentLevel == 4 )
//		{
//			maxConditionals = 0;
//			maxRelativePosRules = 1;
//			maxAdjacencyRules = 1;
//			maxRules = 2;
//			tilesCount = 4;
//			chanceOfImpossible = 50;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			usingPositiveOfAbsolute = true;
//		}
//		else if( currentLevel == 5 )
//		{
//			maxConditionals = 1;
//			maxRules = 1;
//			tilesCount = 4;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, false, false, false );
//			SetConditionalParameters( 0, 1, 1 );
//			usingPositiveOfAbsolute = true;
//		}
//		else if( currentLevel == 6 )
//		{
//			maxConditionals = 1;
//			maxRules = 1;
//			tilesCount = 3;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingEitherOr = false;
//			SetPossibleBoardParameters( true, true, true, false );
//			SetConditionalParameters( 0, 0, 2 );
//			usingPositiveOfAbsolute = true;
//		}
//		else if( currentLevel == 7 )
//		{
//			maxRules = 1;
//			maxConditionals = 1;
//			
//			tilesCount = 4;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, false );
//			SetConditionalParameters( 0, 1, 1 );
//			usingPositiveOfAbsolute = true;
//		}
//		else if( currentLevel == 8 )
//		{
//			maxRules = 2;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 1;
//			maxAdjacencyRules = 1;
//
//			tilesCount = 4;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, false );
//			SetConditionalParameters( 1, 0, 1 );
//			usingPositiveOfAbsolute = true;
//		}
//
//		else if( currentLevel == 9 )
//		{
//			maxRules = 2;
//			maxConditionals = 1;
//			maxRelativePosRules = 2;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 1;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, false );
//			SetConditionalParameters( 0, 2, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 10 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 0;
//			maxRelativePosRules = 1;
//			maxAdjacencyRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 50;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 11 )
//		{
//			maxRules = 3;
//
//			maxConditionals = 1;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 12 )
//		{
//			maxRules = 3;
//
//			maxConditionals = 1;
//			maxRelativePosRules = 1;
//			maxAdjacencyRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 13 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 14 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 2;
//			maxAbsPosRules = 0;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 30;
//			maxImpossiblePerTrial = 1;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 15 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 2;
////			maxAbsPosRules = 2;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 16 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 2;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 2;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 17 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 2;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 3;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 18 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 2;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 3;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 19 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 1;
//			maxRelativePosRules = 2;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 3;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 20 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 2;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 3;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 21 )
//		{
//			maxRules = 3;
//			
//			maxConditionals = 2;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 5;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 3;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 22 )
//		{
//			maxRules = 4;
//			
//			maxConditionals = 2;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 6;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 3;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else if( currentLevel == 23 )
//		{
//			maxRules = 4;
//			
//			maxConditionals = 2;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 6;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 4;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//		}
//		else
//		{
//			maxRules = 4;
//			
//			maxConditionals = 2;
//			maxRelativePosRules = 1;
//			maxAbsPosRules = 1;
//			
//			tilesCount = 6;
//			chanceOfImpossible = 25;
//			maxImpossiblePerTrial = 2;
//			maxRulesToSetImpossibleBoard = 4;
//			usingEitherOr = true;
//			SetPossibleBoardParameters( true, true, true, true );
//			SetConditionalParameters( 1, 1, 0 );
//			usingPositiveOfAbsolute = false;
//
//		}
//	
//		
//		
//	}
	

//	void ResetStats()
//	{
//		maxAbsPosRules = 0;
//		maxRelativePosRules = 0;
//		maxAdjacencyRules = 0;
//		maxConditionals = 0;
//		chanceOfImpossible = 0;
//		maxImpossiblePerTrial = 0;
//		maxRulesToSetImpossibleBoard = 0;
//		usingEitherOr = false;
//
//		previousPossiblePresetKey = null;
//		previousImpossiblePresetKey = null;
//
//		maxAbsInConditional = 0;
//		maxAdjInConditional = 0;
//		maxRelInConditional = 0;
//
////		SetPossibleBoardParameters( false, false, false, false );
//
//	}
	

	List<Tile> ShuffleThis(List<Tile> listToShuffle ){
		List<Tile> data = new List<Tile> (listToShuffle);
		int size = data.Count;
		
		for (int i = 0; i < size; i++){
			int indexToSwap = Random.Range(i, size);
			Tile oldValue = data[i];
			data[i] = data[indexToSwap];
			data[indexToSwap] = oldValue;
		}
		return data;
	}

	bool HappenedByChance( int chance ) //out of 100
	{
		int random = Random.Range (1, 101);
//		Debug.Log (" ROLLED : " + random + " , CHANCE : " + chance);
		if( random > chance )
		{
			return false;
		}
		return true;
	}

	public void CompileLevels( List< List<int> > levelingInfo )
	{
		for( int i = 0; i < levelingInfo.Count; i ++ )
		{
			List< int > levelInfo = levelingInfo[ i ];

			Level newLevel = new Level( i );
			
			newLevel.SetTileCount ( levelInfo[ 0 ] );
			newLevel.SetRuleCreationParameters ( levelInfo[ 1 ], levelInfo[ 2 ], levelInfo[ 3 ], levelInfo[ 4 ], levelInfo[ 5 ], levelInfo[ 6 ] );
			newLevel.SetConditionalParameters( levelInfo[ 7 ], levelInfo[ 8 ], levelInfo[ 9 ] );
			newLevel.SetImpossibleBoardParameters(levelInfo[ 10 ], levelInfo[ 11 ], levelInfo[ 12 ] );
			newLevel.SetPossibleBoardParameters( levelInfo[ 13 ], levelInfo[ 14 ], levelInfo[ 15 ], levelInfo[ 16 ] );
			newLevel.SetMaxTrials( levelInfo [ 17 ], levelInfo [ 18 ] );

			allLevels.Add( newLevel );
		}
	}

	int CalculateConditionalDifficulty( Rule rule1, Rule rule2 )
	{
		int difficulty = (rule1.difficulty + rule2.difficulty) * 2;
		return difficulty; 
	}

	int ProblemDifficulty( int ruleDifficulty, int additionalDifficulty )
	{
		return ruleDifficulty + additionalDifficulty;
	}
}

public class CurrentSetUp
{
	public int presetCount = 0;

	public bool usesModusPonens = false;
	public bool usesModusTollens = false;
	public bool usesbAndNotA = false;

	public bool impossible = false;
	public int rulesBreaking = 0;

	public CurrentSetUp()
	{
		
	}

	public void SetImpossibleStats( bool notPossible, int breakingRules )
	{
		impossible = notPossible;
		rulesBreaking = breakingRules;
	}

	public void SetConditionalLogic( bool modusPonens, bool modusTollens, bool bAndNotA )
	{
		usesModusPonens = modusPonens;
		usesModusTollens = modusTollens;
		usesbAndNotA = bAndNotA;
	}

	public void SetPresetCount( int count )
	{
		presetCount = count;
	}
}



public class Level
{
	public int maxRules;
	public int tilesCount;
	public int chanceOfImpossible;
	public int maxImpossiblePerTrial;
	public int maxTrialsInRuleSet;

	//rules that can be used
	public int maxAbsPosRules;
	public int maxRelativePosRules;
	public int maxAdjacencyRules;
	public int maxConditionals;
	public bool usingEitherOr;
	public bool usingPositiveOfAbsolute;

	public int chanceOfPresetsOnFirstTrial;

	//difficulty for impossible board
	public int maxRulesToSetImpossibleBoard;
	public int minRulesToSetImpossibleBoard;
//	public int maxConditionalInImpossibleSet;


	//CONDITIONALS

	//construction
	public int maxAbsInConditional;
	public int maxAdjInConditional;
	public int maxRelInConditional;
	
	// difficulty for possible board
	public bool modusPonens; //if a then b
	public bool modusTollens; //if !b, then !a
	public bool modusTollensImplyNotA;
	
	public bool showClauseBNotA;  // b and !a
	//	bool showClauseBImplyNotA;  // b and make a impossible
	
	public int maxModusTollens;
	public int maxBAndNotA;

	public int level;

	public Level( int levelNum )
	{
		level = levelNum;
	}


	public void SetRuleCreationParameters( int maxAbs, int maxAdj, int maxRel, int maxCond, int usingPosOfAbs, int eitherOr )
	{
		maxConditionals = maxCond;
		maxRelativePosRules = maxRel;
		maxAdjacencyRules = maxAdj;
		maxAbsPosRules = maxAbs;
		usingPositiveOfAbsolute = ConvertIntToBool( usingPosOfAbs );
		usingEitherOr = ConvertIntToBool( eitherOr );
		maxRules = maxConditionals + maxRelativePosRules + maxAdjacencyRules + maxAbsPosRules;

	}
	
	public void SetConditionalParameters( int maxRel, int maxAdj, int maxAbs )
	{
		maxAbsInConditional = maxAbs;
		maxAdjInConditional = maxAdj;
		maxRelInConditional = maxRel;
	}
	
	public void SetTileCount( int tilesInTrial )
	{
		tilesCount = tilesInTrial;
	}

	public void SetMaxTrials( int maxTrials, int chanceOfPresets )
	{
		maxTrialsInRuleSet = maxTrials;
		chanceOfPresetsOnFirstTrial = chanceOfPresets;
	}
	
	public void SetImpossibleBoardParameters( int rulesToSetBoard, int impChance, int maxImp )
	{
		maxRulesToSetImpossibleBoard = rulesToSetBoard;
		minRulesToSetImpossibleBoard = Mathf.Max (0, maxRulesToSetImpossibleBoard --);
		chanceOfImpossible = impChance;
		maxImpossiblePerTrial = maxImp;
	}
	
	public void SetPossibleBoardParameters( int AThenB, int notBThenNotA, int contraPositivePresetDoNotAlwaysBreakClause1, int showBNotA )
	{
		modusPonens = ConvertIntToBool( AThenB );
		modusTollens = ConvertIntToBool( notBThenNotA );
		showClauseBNotA  = ConvertIntToBool( showBNotA );
		modusTollensImplyNotA = ConvertIntToBool( contraPositivePresetDoNotAlwaysBreakClause1 );
	}

	bool ConvertIntToBool( int num )
	{
		if( num == 0 )
		{
			return false;
		}
		return true;
	}
}


