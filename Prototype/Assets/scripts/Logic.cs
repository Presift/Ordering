using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logic : MonoBehaviour {

	public Model model;
	public MetaData metaData;
	public CSVReader levelReader;

	public RuleStack trialRules;

	Dictionary < int, int > levelToBucketDict = new Dictionary < int, int >();
	Dictionary < int, List< Level >> bucketNumToAvailableLevelsDict = new Dictionary < int, List< Level >>();

	public int chanceOfImpossible;

	int impossiblesUsed;
	
	public int consecutiveTollensErrors;
	public int errorsCountToNeedHelp = 3;
	
//	public string previousPossiblePresetKey;
	public List< string > previousSubmissions;
	List< string > bestPossiblePresets;

	bool attemptedCollectionOfBestPossible;
	public string currentPresetKey;
	List< string > previousImpossiblePresets;
//	string previousImpossiblePresetKey;
//	public List< string > previousSubmissionsInRound;
	List<Rule> previousRulesBroken;

	List<Level> allLevels = new List<Level> ();

	public Level currentLeveling;
	public int maxAttemptsToCreateRules = 10;

	float readingTimeConditional = 1.25f;
	float readingTimeOther = .5f;
	float constantTime = 1.5f;
	float timePerTile = .5f;
	float worstTimeMultiplier = 5;

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

	public void AddPreviousSubmission( string submission )
	{
		previousSubmissions.Add (submission);
	}

	public void SetMinMaxResponseTimesForLevelChange()
	{
		float minReadingTime = 0;

		for(int i = 0; i < trialRules.ruleStack.Count; i ++ )
		{
			if( trialRules.ruleStack[ i ] is Conditional )
			{
				minReadingTime += readingTimeConditional;
			}
			else
			{
				minReadingTime += readingTimeOther;
			}
		}

		float timeForTilePlacement = timePerTile * currentLeveling.tilesCount;

		model.responseTimeForMaxLevelChange = timeForTilePlacement + minReadingTime + constantTime;
		model.responseTimeForMinLevelChange = model.responseTimeForMaxLevelChange * worstTimeMultiplier;
//		Debug.Log ("TimeForMaxLevelChange : " + model.responseTimeForMaxLevelChange);
//		Debug.Log ("TimeForMinLevelChange : " + model.responseTimeForMinLevelChange);
	}

	void ResetForNewRound()
	{
		previousSubmissions = new List< string > ();
		previousImpossiblePresets = new List< string >();
		bestPossiblePresets= new List< string >();
		attemptedCollectionOfBestPossible = false;
		currentPresetKey = null;

		previousRulesBroken = null;
//		previousSubmissions = new List< string > ();
		model.SetImpossible (false);
		impossiblesUsed = 0;
		trialRules = new RuleStack ();

	}

	public string CreateRules( List<Tile> tiles, int remainingAttemptsToCreateRules, RuleStack bestAttemptedRules )
	{
		ResetForNewRound ();

		List< float > ruleIndentifiers = new List< float > ();

		int difficultyPointsSpend = 0;
		int rulesCreated = 0;


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
	
		if( trialRules.ruleStack.Count < currentLeveling.maxRules )
		{
			Debug.Log ("******************************************");
			Debug.Log ("RULE COUNT UNDER MAX RULES FOR LEVEL " + currentLeveling.level );
			Debug.Log (remainingAttemptsToCreateRules + "REMAINING ATTEMPTS TO CREATE RULES ");
			trialRules.ConstructRandomOrderedVerbal ();
			Debug.Log ( "failed rules : " );
			trialRules.PrintRules();
			Debug.Log ("******************************************");

			if( bestAttemptedRules.ruleStack.Count < trialRules.ruleStack.Count  )
			{
				bestAttemptedRules = trialRules;
			}
			if( remainingAttemptsToCreateRules > 0 )
			{
				CreateRules( tiles, remainingAttemptsToCreateRules - 1, bestAttemptedRules );
			}
			else
			{
				trialRules = bestAttemptedRules;
			}

		}

		if( bestAttemptedRules.ruleStack.Count > trialRules.ruleStack.Count  )
		{
			trialRules = bestAttemptedRules;
		}

		metaData.SetStatsForTrial (model.currentLevel, model.currentTrialInRound, model.currentRound, trialRules.ruleStack.Count, ruleIndentifiers, tiles.Count);
		metaData.SetTimeSinceProblemStart (Time.time);
		
		trialRules.ConstructRandomOrderedVerbal ();
		//		Debug.Log ("rule count : " + trialRules.ruleStack.Count);
		
		if( trialRules.incorrectSubmissions.Count == 0 )
		{
			Debug.Log (" NO IMPOSSIBLE SUBMISSIONS!!! " );
		}

		SetMinMaxResponseTimesForLevelChange ();

		model.currentChallenge.SetPresetCount ( 0 );

		return trialRules.verbal;
	}
	

	void ReAddHolderPositionsFromRescindedRule( List< int > absolutePositions, Rule rule )
	{
		if( rule is AbsolutePositionRule )
		{
			AbsolutePositionRule absRule = rule as AbsolutePositionRule;
//			absolutePositions.Add ( rule.absolutePositionIndex );
			AddIntegerIfNotInList( rule.absolutePositionIndex, absolutePositions );
		}
		else if( rule is Conditional )
		{
			Conditional condRule = rule as Conditional;

			if( condRule.rule1 is AbsolutePositionRule )
			{
				AbsolutePositionRule absRule = condRule.rule1 as AbsolutePositionRule;
				AddIntegerIfNotInList( condRule.rule1.absolutePositionIndex, absolutePositions );
//				absolutePositions.Add ( condRule.rule1.absolutePositionIndex );
			}
			if( condRule.rule2 is AbsolutePositionRule )
			{
				AbsolutePositionRule absRule = condRule.rule2 as AbsolutePositionRule;
				AddIntegerIfNotInList( condRule.rule2.absolutePositionIndex, absolutePositions );
//				absolutePositions.Add ( condRule.rule2.absolutePositionIndex );
			} 
		}
	}

	void AddIntegerIfNotInList( int position, List< int > absolutePositions )
	{
		if( !absolutePositions.Contains( position ))
		{
			absolutePositions.Add ( position );
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

//		if (absolutePosition >= tilesToOrder.Count) 
		if( tilesToOrder.Count < holderPositions.Count) 
		{
			Debug.Log ("******************************************");
			Debug.Log ( " broken index : " + absolutePosition );
			Debug.Log ( "length of hold positions : " + holderPositions.Count );

			for( int i = 0; i < holderPositions.Count; i ++ )
			{
				Debug.Log (holderPositions[ i ] );
			}
			Debug.Log ("******************************************");
		}

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
//			Debug.Log ( holderPositions.Count );
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
//			Debug.Log ( " FAIL : " + newConditional.verbal );
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
	
	public bool TrialGetsPresets()
	{
		if( model.currentTrialInRound == 0 )
		{
			return HappenedByChance (currentLeveling.chanceOfPresetsOnFirstTrial);
		}
		else
		{
			return true;
		}
	}

	public string NewTrialSetUp()
	{
//		model.
		//refresh tile positions
		bool createImpossible = false;
		string presetTiles = null;

		if( impossiblesUsed < currentLeveling.maxImpossiblePerTrial && model.impossibleEnabled )
		{
			createImpossible = HappenedByChance( currentLeveling.chanceOfImpossible );
			Debug.Log ( " create impossible : " + createImpossible );

		}
//		Debug.Log ("create impossible : " + createImpossible );
		if( createImpossible )
		{
			presetTiles = AttemptToCreateImpossibleBoard( model.tilesToOrder );
		}
//		else
		if( presetTiles == null )
		{
			presetTiles = CreatePossibleBoard();
		}

		if( presetTiles == null && !createImpossible && model.impossibleEnabled )
		{
			presetTiles = AttemptToCreateImpossibleBoard( model.tilesToOrder );
		}

		Debug.Log (presetTiles);

		return presetTiles;
	}
	
	void RemoveMatchingBestPresetsIfPreviousSubmissionIsMatch( )
	{
		List< string > bestPresetsToRemove = new List< string > ();

		for( int i = 0; i < bestPossiblePresets.Count; i ++ )
		{
			string bestPreset = bestPossiblePresets[ i ];
			if( PreviousSubmissionValidForNewPreset( bestPreset ))
			{
				bestPresetsToRemove.Add ( bestPreset );
			}
		}

		for( int removal = 0; removal < bestPresetsToRemove.Count; removal ++ )
		{
			bestPossiblePresets.Remove( bestPresetsToRemove[ removal ] );
		}
	}
	
	string GetBestKeyFromList()
	{
		if( currentPresetKey == null )
		{
			return bestPossiblePresets [Random.Range (0, bestPossiblePresets.Count)];
		}
		else
		{
			//get preset with fewest preset positions in common to previous preset key
			int fewestMatches = 1000;
			string bestKey = null;

			for( int i = 0; i < bestPossiblePresets.Count; i ++ )
			{
				int positionMatchesFound = 0;
				
				//	remove any presets that have presets in all same positions as current preset
				for( int charIndex = 0; charIndex < currentPresetKey.Length; charIndex ++ )
				{
					if( currentPresetKey[ charIndex ] != 'n' && bestPossiblePresets[ i ][ charIndex ] != 'n' )
					{
						positionMatchesFound ++;
					}
				}
				
				if( positionMatchesFound < fewestMatches )
				{
					fewestMatches = positionMatchesFound;
					bestKey = bestPossiblePresets[ i ];
				}
			}

			return bestKey;
		}
	}

	string CreatePossibleBoard()
	{
		Debug.Log ("CREATING POSSIBLE BOARD");
		//create  key string from previousSubmission

		RemoveMatchingBestPresetsIfPreviousSubmissionIsMatch ();
		string presetTiles = null;

		if( attemptedCollectionOfBestPossible && bestPossiblePresets.Count > 0 )
		{
			presetTiles = GetBestKeyFromList();
		}

		else if( !attemptedCollectionOfBestPossible )
		{
			//atempt to collect best possibles
			attemptedCollectionOfBestPossible = true;

			List< string > tileBank = GetTilesAsListOfLetters (model.tilesToOrder);
			
			bestPossiblePresets = GetBestPresetToCompleteBoard ( 3, tileBank);
			
			if( bestPossiblePresets.Count > 0 )
			{
				Debug.Log ("BEST PRESETS");

				for( int i = 0; i < bestPossiblePresets.Count; i ++ )
				{
					Debug.Log ( bestPossiblePresets[ i ] );
				}

//				presetTiles = bestPossiblePresets [Random.Range (0, bestPossiblePresets.Count)];
				presetTiles = GetBestKeyFromList();

				Debug.Log (" preset : " + presetTiles);
				//	create string with only 1 placed tile in newSubmission
				
//				previousSubmissions.Add ( presetTiles );
				//	previousPossiblePresetKey = presetTiles;
//				return presetTiles;
			}

		}

		if( presetTiles != null )
		{
			model.SetImpossible( false );
			currentPresetKey = presetTiles;
		}
		else
		{
			currentPresetKey = null;
		}

		UpdateBestPossiblePresets ( presetTiles );
		return presetTiles;

	}

	void UpdateBestPossiblePresets( string presetUsed )
	{

		if( presetUsed != null )
		{
			int maxPositionMatches = 0;
			
			for (int charIndex = 0; charIndex < presetUsed.Length; charIndex ++) 
			{
				if( presetUsed[ charIndex ] != 'n' )
				{
					maxPositionMatches ++;
				}
			}

			List< string> presetsToRemove = new List<string>();

			for( int i = 0; i < bestPossiblePresets.Count; i ++ )
			{
				int positionMatchesFound = 0;

				//	remove any presets that have presets in all same positions as current preset
				for( int charIndex = 0; charIndex < presetUsed.Length; charIndex ++ )
				{
					if( presetUsed[ charIndex ] != 'n' && bestPossiblePresets[ i ][ charIndex ] != 'n' )
					{
						positionMatchesFound ++;
					}
				}

				if( positionMatchesFound == maxPositionMatches )
				{
					presetsToRemove.Add ( bestPossiblePresets[ i ] );
					Debug.Log ("REMOVED " + bestPossiblePresets[ i ] + " FROM BEST PRESETS");
				}
			}

			for( int remove = 0; remove < presetsToRemove.Count; remove ++ )
			{
				bestPossiblePresets.Remove( presetsToRemove[ remove ] );
			}

//			bestPossiblePresets.Remove( presetUsed );
		}
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
		if( previousSubmissions.Contains( possiblePreset ))
			{
				return false;
			}
			return true;
	}

	int GetInstancesOfCorrectSubmissions( string possiblePreset )
	{
		int possiblePresetCount = trialRules.CountWildCardInDictionary (possiblePreset, trialRules.correctSubmissions);

		return possiblePresetCount;
	}

	int GetInstancesOfIncorrectSubmissions( string possiblePreset )
	{
		int possiblePresetCount = trialRules.CountWildCardInDictionary (possiblePreset, trialRules.incorrectSubmissions);
		
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

	bool CompletionCountIsWorthReplacementOfBest( int newKeyPresetCount, int bestPresetCount, int newKeyInstances, int fewestPossibleCompletions )
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

	bool PresetUsesConditionalLogicInLeveling( bool usesModusPonens, bool usesModusTollens, bool usesTollensAndImpliesA, bool usesClauseBAndNotA )
	{
		if( usesClauseBAndNotA && !currentLeveling.showClauseBNotA )
		{
			return false;
		}
		if( usesModusTollens && !currentLeveling.modusTollens )
		{
			return false;
		}
		if( currentLeveling.modusTollens && !currentLeveling.showClauseBNotA )
		{
			if( !usesModusTollens )
			{
				return false;
			}
			else if( currentLeveling.modusTollensImplyNotA && ! usesTollensAndImpliesA )
			{
				return false;
			}
		}
		if( usesTollensAndImpliesA && !currentLeveling.modusTollensImplyNotA )
		{
			return false;
		}
//		if (usesModusPonens && !currentLeveling.modusPonens) 
//		{
//			return false;	
//		}
		if ( !currentLeveling.showClauseBNotA  && !currentLeveling.modusTollens && currentLeveling.modusPonens ) 
		{
			if( !usesModusPonens )
			{
				return false;
			}
				
		}

		return true;
	}

//	bool ForcesModusPonens( bool rule1AlwaysTrue, 
//	bool PresetOnlyUsesConditionalLogicInLeveling( bool rule1AlwaysTrue, bool rule1AlwaysFalse, bool rule2AlwaysTrue, bool rule2AlwaysFalse )
//	{
//
//		if( !currentLeveling.showClauseBNotA )
//		{
//			//if preset forces bAndNotA
//			return false;
//		}
//		if(!currentLeveling.modusTollens )
//		{
//			//if preset forces tollens
//			return false;
//		}
//		if( !currentLeveling.modusTollensImplyNotA )
//		{
//			//if preset forces
//			return false;
//		}
//		if (!currentLeveling.showClauseBNotA  && !currentLeveling.modusTollens && currentLeveling.modusPonens) 
//		{
//			//if preset does not force ponens
//			return false;	
//		}
//		
//		return true;
//	}

	bool PresetAlwaysSatisfiesRule( string preset, Rule rule )
	{
		if( KeyNeverBreaksRule( preset, rule ))
		{
			return true;
		}

		return false;
	}

	bool KeySatisfiesAnyNonConditionalRules( string preset, List<Rule> nonConditionalRules )
	{
		for( int i = 0; i < nonConditionalRules.Count; i ++ )
		{
			if( PresetAlwaysSatisfiesRule( preset, nonConditionalRules[ i ] ))
			{
				return true;
			}
		}

		return false;
	}

	List<Rule> GetNonConditionalRules()
	{
		List<Rule> otherRules = new List<Rule> ();

		for( int i = 0; i < trialRules.ruleStack.Count; i ++ )
		{
			if( !(trialRules.ruleStack[ i ] is Conditional ))
			{
				otherRules.Add ( trialRules.ruleStack[ i ] );
			}
		}

		return otherRules;
	}

	bool IsGoodPreset( string preset, List<Rule> nonConditionalRules )
	{
		if( !PresetIsNew( preset ))
		{
			return false;
		}
		if( !trialRules.WildCardKeyInDictionary( preset, trialRules.correctSubmissions))
		{
			return false;
		}
		if( !trialRules.WildCardKeyInDictionary( preset, trialRules.incorrectSubmissions ))
		{
			return false;
		}
		if(PreviousSubmissionValidForNewPreset( preset))
		{
			return false;
		}
		if( KeySatisfiesAnyNonConditionalRules( preset, nonConditionalRules ))
		{
			return false;
		}
		return true;
	}

	List< string > GetBestPresetToCompleteBoard ( int maxPresets, List< string > tileBank )
	{
		int fewestPossibleCompletions = 1000;
		List< string > bestKeys = new List< string >();
		int bestPresetCount = 10;

		List< Conditional > condRules = GetConditionalsFromStack (trialRules);
		List<Rule> otherRules = GetNonConditionalRules ();

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

//					if( PresetIsNew( preset ) && trialRules.WildCardKeyInDictionary( preset, trialRules.correctSubmissions) && trialRules.WildCardKeyInDictionary( preset, trialRules.incorrectSubmissions ) && !PreviousSubmissionValidForNewPreset( preset) && KeySatisfiesAnyNonConditionalRules)
					if( IsGoodPreset( preset, otherRules ))
					{
						//test to see if preset has fewer possibilites than best
						int correctSubmissions = GetInstancesOfCorrectSubmissions ( preset );

//						int incorrectSubmissions = GetInstancesOfIncorrectSubmissions ( preset );

						bool presetReplacesBest = false;

						if( containsConditional )
						{

							bool testKeyUsesBAndNotA = false;
							bool testKeyUsesModusTollens = false;
							bool testKeyUsesTollensAndImpliesA = false;
							bool testKeyUsesModusPonens = false;

							bool presetOnlyUsesLogicSetInLeveling = true;

							for( int cond = 0; cond < condRules.Count; cond ++ )
							{
								Conditional condRule = condRules[ cond ];
								bool alwaysBreaksRule1 = KeyAlwaysBreaksRule( preset, condRule.rule1 );
								bool neverBreaksRule1 = KeyNeverBreaksRule( preset, condRule.rule1 );
								bool alwaysBreaksRule2 = KeyAlwaysBreaksRule( preset, condRule.rule2 );
								bool neverBreaksRule2 = KeyNeverBreaksRule( preset, condRule.rule2 );

								bool presetForcesPonens = KeyIsPositiveOfConditional( neverBreaksRule1 );

								bool presetForcesBAndNotA =  KeyIsBAndNeverA( neverBreaksRule2, alwaysBreaksRule2 );

								bool presetForcesTollens = KeyIsContraPositiveOfConditional( alwaysBreaksRule2 );

								bool onlyShowsNotB = false;

								if( presetForcesTollens && !alwaysBreaksRule1 )
								{
									testKeyUsesTollensAndImpliesA = true;
								}

								if( presetForcesPonens )
								{
									testKeyUsesModusPonens = true;
								}
								if( presetForcesTollens )
								{
									testKeyUsesModusTollens = true;

									if( onlyShowsNotB )
									{
										testKeyUsesTollensAndImpliesA = true;
									}
								}
								if( presetForcesBAndNotA )
								{
									testKeyUsesBAndNotA = true;
								}

							}

							if( !PresetUsesConditionalLogicInLeveling( testKeyUsesBAndNotA, testKeyUsesModusTollens, testKeyUsesTollensAndImpliesA, testKeyUsesModusPonens ))
							{
								presetOnlyUsesLogicSetInLeveling = false;
							}

							if( presetOnlyUsesLogicSetInLeveling && correctSubmissions == fewestPossibleCompletions && currentPresets == bestPresetCount )
							{
								bestKeys.Add ( preset );
//								Debug.Log ( "preset : " + preset + ", incorrect : " + incorrectSubmissions + ", correct : " + correctSubmissions );
//								Debug.Log("best key " + preset + " : " + ", uses ponens : " + testKeyUsesModusPonens + ", uses bAndNotA : " + testKeyUsesBAndNotA + ", uses tollens : " + testKeyUsesModusTollens + " , uses tollens, implies a : " + testKeyUsesTollensAndImpliesA );
							}
							else if( presetOnlyUsesLogicSetInLeveling && CompletionCountIsWorthReplacementOfBest( currentPresets, bestPresetCount, correctSubmissions, fewestPossibleCompletions ))
							{
								presetReplacesBest = true;
//								Debug.Log("best key " + preset + " : " + ", uses ponens : " + testKeyUsesModusPonens + ", uses bAndNotA : " + testKeyUsesBAndNotA + ", uses tollens : " + testKeyUsesModusTollens + " , uses tollens, implies a : " + testKeyUsesTollensAndImpliesA );
							}

						}
						// if no conditionals in stack
						else
						{
							if ( CompletionCountIsWorthReplacementOfBest(currentPresets, bestPresetCount, correctSubmissions, fewestPossibleCompletions) )
							{
//								fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
								presetReplacesBest = true;

							}
							else if ( correctSubmissions == fewestPossibleCompletions && currentPresets == bestPresetCount )
							{
								bestKeys.Add ( preset );
//								Debug.Log ( "preset : " + preset + ", incorrect : " + incorrectSubmissions + ", correct : " + correctSubmissions );
//								Debug.Log ( " added : " + preset );
							}
						}

						if( presetReplacesBest )
						{
							bestKeys = new List< string >();
							bestKeys.Add ( preset );
//							Debug.Log ( " added : " + preset );
							fewestPossibleCompletions = correctSubmissions;
							bestPresetCount = currentPresets;
//							Debug.Log ( "preset : " + preset + ", incorrect : " + incorrectSubmissions + ", correct : " + correctSubmissions );
//							Debug.Log ("***keys count : " + bestKeys.Count);
						}
					}

				}
			}

			currentPresets ++;
//			Debug.Log ( "increase presets to : " + currentPresets );
		}
		Debug.Log ("fewest possible completions : " + fewestPossibleCompletions);
//		Debug.Log ("best keys count : " + bestKeys.Count);
//		Debug.Log (" modus Ponens : " + bestKeyUsesModusPonens + ", modus Tollens : " + bestKeyUsesModusTollens + ", bAndNotA : " + bestKeyUsesBAndNotA);

//		model.currentChallenge.SetPresetCount (bestPresetCount);
//		model.currentChallenge.SetConditionalLogic (bestKeyUsesModusPonens, bestKeyUsesModusTollens, bestKeyUsesBAndNotA);
//		Debug.Log ("current challenge using tollens : " + model.currentChallenge.usesModusTollens);

		return bestKeys;

	}


//	List<string> GetBestPresetForConditionals ( int maxPresets, List< string > tileBank, string conditionalLogic )
//	{
//	
//		List< string > bestKeysMP = new List< string >();
//		int fewestCompletionsMP = 1000;
//		int bestPresetCountMP = 10;
//
//		List< string > bestKeysTP = new List< string >();
//		int fewestCompletionsTP = 1000;
//		int bestPresetCountTP = 10;
//
//		List< string > bestKeysBNotA = new List< string >();
//		int fewestCompletionsBNotA = 1000;
//		int bestPresetCountBNotA = 10;
//		
//		List< Conditional > condRules = GetConditionalsFromStack (trialRules);
//		List<Rule> otherRules = GetNonConditionalRules ();
//		
//		int currentPresets = 1;
//		
//		while( currentPresets <= maxPresets )
//		{
//			List< List<string> > tileCombos = new List< List<string> >();
//			List< string > newCombo = new List< string > ();
//			GetAllCombinationsForPresets( tileBank, newCombo, currentPresets, tileCombos);
//			
//
//			//for each combo in allCombos
//			for( int comboIndex = 0; comboIndex < tileCombos.Count; comboIndex ++ )
//			{
//				List<int> digitBank = CreateDigitBank( tileBank.Count );
//				
//				List< string > presetKeyCombos = new List< string > ();
//				
//				List<int> currPosOrder = new List<int> ();
//				
//				GetPresetOrdersFromCombos( currPosOrder, digitBank, digitBank.Count, tileCombos[ comboIndex ], presetKeyCombos );
//				
//				for( int presetOrderIndex = 0; presetOrderIndex < presetKeyCombos.Count; presetOrderIndex ++ )
//				{
//					string preset = presetKeyCombos[ presetOrderIndex ];
//					
//					//					if( PresetIsNew( preset ) && trialRules.WildCardKeyInDictionary( preset, trialRules.correctSubmissions) && trialRules.WildCardKeyInDictionary( preset, trialRules.incorrectSubmissions ) && !PreviousSubmissionValidForNewPreset( preset) && KeySatisfiesAnyNonConditionalRules)
//					if( IsGoodPreset( preset, otherRules ))
//					{
//						//test to see if preset has fewer possibilites than best
//						int correctSubmissions = GetInstancesOfCorrectSubmissions ( preset );
//						
//						//						int incorrectSubmissions = GetInstancesOfIncorrectSubmissions ( preset );
//						
//						bool presetReplacesBest = false;
//
//							
//						bool testKeyUsesBAndNotA = false;
//						bool testKeyUsesModusTollens = false;
//						bool testKeyUsesTollensAndImpliesA = false;
//						bool testKeyUsesModusPonens = false;
//						
////						bool presetOnlyUsesLogicSetInLeveling = true;
//						
//						for( int cond = 0; cond < condRules.Count; cond ++ )
//						{
//							Conditional condRule = condRules[ cond ];
//
//							bool alwaysBreaksRule1 = KeyAlwaysBreaksRule( preset, condRule.rule1 );
//							bool neverBreaksRule1 = KeyNeverBreaksRule( preset, condRule.rule1 );
//							bool alwaysBreaksRule2 = KeyAlwaysBreaksRule( preset, condRule.rule2 );
//							bool neverBreaksRule2 = KeyNeverBreaksRule( preset, condRule.rule2 );
//							
//							bool presetForcesPonens = KeyIsPositiveOfConditional( neverBreaksRule1 );
//							
//							bool presetForcesBAndNotA =  KeyIsBAndNeverA( neverBreaksRule2, alwaysBreaksRule2 );
//							
//							bool presetForcesTollens = KeyIsContraPositiveOfConditional( alwaysBreaksRule2 );
//							
//							bool onlyShowsNotB = false;
//							
//							if( presetForcesTollens && !alwaysBreaksRule1 )
//							{
//								testKeyUsesTollensAndImpliesA = true;
//							}
//							
//							if( presetForcesPonens )
//							{
//								testKeyUsesModusPonens = true;
//							}
//
////							if( presetForcesTollens )
////							{
////								testKeyUsesModusTollens = true;
////								
////								if( onlyShowsNotB )
////								{
////									testKeyUsesTollensAndImpliesA = true;
////								}
////							}
//							if( presetForcesBAndNotA )
//							{
//								testKeyUsesBAndNotA = true;
//							}
//
//							if( testKeyUsesModusPonens && correctSubmissions == fewestCompletionsMP && currentPresets == bestPresetCountMP )
//							{
//								bestKeysMP.Add ( preset );
//							}
//							else if( testKeyUsesModusPonens && CompletionCountIsWorthReplacementOfBest( currentPresets, bestPresetCountMP, correctSubmissions, fewestCompletionsMP ))
//							{
//								bestKeysMP = new List< string >();
//								bestKeysMP.Add ( preset );
//
//								fewestCompletionsMP.A
//							}
//							
//							if( !PresetUsesConditionalLogicInLeveling( testKeyUsesBAndNotA, testKeyUsesModusTollens, testKeyUsesTollensAndImpliesA, testKeyUsesModusPonens ))
//							{
//								presetOnlyUsesLogicSetInLeveling = false;
//							}
//							
//							if( presetOnlyUsesLogicSetInLeveling && correctSubmissions == fewestPossibleCompletions && currentPresets == bestPresetCount )
//							{
//								bestKeys.Add ( preset );
//								//								Debug.Log ( "preset : " + preset + ", incorrect : " + incorrectSubmissions + ", correct : " + correctSubmissions );
//								//								Debug.Log("best key " + preset + " : " + ", uses ponens : " + testKeyUsesModusPonens + ", uses bAndNotA : " + testKeyUsesBAndNotA + ", uses tollens : " + testKeyUsesModusTollens + " , uses tollens, implies a : " + testKeyUsesTollensAndImpliesA );
//							}
//							else if( presetOnlyUsesLogicSetInLeveling && CompletionCountIsWorthReplacementOfBest( currentPresets, bestPresetCount, correctSubmissions, fewestPossibleCompletions ))
//							{
//								presetReplacesBest = true;
//								//								Debug.Log("best key " + preset + " : " + ", uses ponens : " + testKeyUsesModusPonens + ", uses bAndNotA : " + testKeyUsesBAndNotA + ", uses tollens : " + testKeyUsesModusTollens + " , uses tollens, implies a : " + testKeyUsesTollensAndImpliesA );
//							}
//							
//						}
//						
//						if( presetReplacesBest )
//						{
////							bestKeys = new List< string >();
////							bestKeys.Add ( preset );
////
////							fewestPossibleCompletions = correctSubmissions;
////							bestPresetCount = currentPresets;
//
//						}
//					}
//					
//				}
//			}
//			
//			currentPresets ++;
//
//		}
//		
//		return bestKeys;
//		
//	}


	bool PreviousSubmissionValidForNewPreset( string preset)
	{
		if( previousSubmissions.Count == 0 )
		{
			return false;
		}
	
		for( int i = 0; i < previousSubmissions.Count; i ++ )
		{
			//if previous correct submission uses all presets
			if( trialRules.WildCardSubmissionKeyNameMatch( preset, previousSubmissions[ i ] ))
			{
				return true;
			}
		}

		return false;
	}

	int ReplaceBestKeysWithTestKey( List< string > bestKeys, string preset, int presetInstances )
	{
		bestKeys = new List< string >();
		bestKeys.Add ( preset );

		return presetInstances;
	}
	



//	RuleStack ReturnRuleStackFromComboList( List< List<Rule> > allRuleCombos, int comboListIndex )
//	{
//		RuleStack rulesToBreak = new RuleStack ();
//		List<Rule> rules = allRuleCombos [ comboListIndex ];
//
//		for( int i = 0; i < rules.Count; i ++ )
//		{
//			rulesToBreak.AddRule ( rules [ i ] );
//		}
//
//		return rulesToBreak;
//	}

	List<Rule> ReturnRulesFromComboList( List< List<Rule> > allRuleCombos, int comboListIndex )
	{
		List<Rule> rulesToBreak = new List<Rule> ();
		List<Rule> rules = allRuleCombos [ comboListIndex ];
		
		for( int i = 0; i < rules.Count; i ++ )
		{
			rulesToBreak.Add( rules [ i ] );
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

//	public void ExcludeRuleStackFromCombos()
//	{
//
//	}

	string AttemptToCreateImpossibleBoard( List<Tile> tilesToOrder )
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
//		int maxPresetTiles = Mathf.Min ( tilesToOrder.Count - 1, 2 );
		int minPresetTiles = 1;


		while( presetTiles.Count == 0 && minPresetTiles <= maxPresetTiles )
		{
			for( int i = 0; i < allRuleCombos.Count; i ++ )
			{
				List<Rule> rulesToBreak = ReturnRulesFromComboList ( allRuleCombos, i );

//				if( rulesToBreak != previousRulesBroken )
//				{
					List< Rule > otherRules = trialRules.GetRulesInStackNotInList (rulesToBreak);
					
					presetTiles = AttemptToGetImpossibleKey( minPresetTiles, tileBank, rulesToBreak, otherRules );
					
					if(  presetTiles.Count != 0 )
					{
						Debug.Log (presetTiles.Count + " impossible presets found ");
						previousRulesBroken = rulesToBreak;
						model.currentChallenge.SetPresetCount (minPresetTiles);
						model.currentChallenge.SetImpossibleStats( true, rulesToSetImpossible );
						break;
					}
//				}

			}

			if( presetTiles.Count != 0 )
			{
				Debug.Log ("breaking out of while loop ");
				break;
			}
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

		if(  presetTiles.Count != 0 )
		{
			Debug.Log ("CREATING IMPOSSIBLE BOARD");
			model.SetImpossible( true );
			impossiblesUsed ++;
			Debug.Log ("RULES BROKEN: " + previousRulesBroken.Count );

			for( int i = 0; i < previousRulesBroken.Count; i ++ )
			{
				Debug.Log ( previousRulesBroken[ i ].verbal );
			}


			presets = presetTiles[ Random.Range( 0, presetTiles.Count) ];
//			previousImpossiblePresetKey = presets;
			previousImpossiblePresets.Add ( presets );

			return presets;
		}
		else
		{
//			model.SetImpossible( false );
//			presets = CreatePossibleBoard( previousSubmission );
			Debug.Log ("impossible preset NOT found, creating possible board ");
			return null;
		}

	}


	List<string> AttemptToGetImpossibleKey( int presets, List<string> tileBank, List<Rule> rulesToBreak, List< Rule> nonBreakingRules )
	{
		List<string> impossibleKeys = new List<string> ();

		bool singleRuleBreak = rulesToBreak.Count == 1;

//		Debug.Log ("RULES TO BREAK : ");
//		for (int rule = 0; rule < rulesToBreak.ruleStack.Count; rule ++) {
//			Debug.Log (rulesToBreak.ruleStack [ rule ].verbal );
//		}

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
				if( !previousImpossiblePresets.Contains( presetKeyCombos[ presetOrderIndex ] ))
//				if( presetKeyCombos[ presetOrderIndex ] != previousImpossiblePresetKey )
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


	bool ImpossibleKey( string possibleKey, List<Rule> rulesToBreak, List<Rule> nonbreakingRules,  bool singleRuleBreak )
	{
		
		if(!PresetIsPossibleCorrectSubmission( possibleKey ))
		{
//			Debug.Log ("PRESET NOT IN CORRECT SUBMISSIONS : " + possibleKey);
			
			//test to see if preset is a possible correct answer each concerned breakable rule
			bool passBreakableRulesTest = false;

			
			if( singleRuleBreak )
			{
				List< string > correctSubmissions = GetCorrectSubmissionsForWildKey( possibleKey, rulesToBreak[ 0 ] );
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

				for ( int ruleToBreak = 0; ruleToBreak < rulesToBreak.Count; ruleToBreak ++ )
				{
					List< string > correctSubsForRule = GetCorrectSubmissionsForWildKey( possibleKey, rulesToBreak[ ruleToBreak ] );
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
					for ( int ruleToExclude = 0; ruleToExclude < rulesToBreak.Count; ruleToExclude ++ )
					{
						List< string > subsToExclude = new List<string>();
						List< List< string >> listOfSubsToFindShared = new List<List<string>> ();

						for ( int ruleToBreak = 0; ruleToBreak < rulesToBreak.Count; ruleToBreak ++ )
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

						else if( ruleToExclude == ( rulesToBreak.Count - 1 ))
						{
//							Debug.Log ( "EACH RULE NEEDED TO CREATE IMPOSSIBLE ");
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
					for( int i = 0; i < rulesToBreak.Count; i ++ )
					{
						Debug.Log (rulesToBreak[ i ].verbal);
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
//		Debug.Log ("rules in rulestack : " + combinedRules.ruleStack.Count);
		return combinedRules;
		
	}



	List<Tile> GetRandomValueFromDictionary( Dictionary<string, List<Tile>> submissionDict )  //move to Rule class
	{
		List<string> keys = new List < string > ( submissionDict.Keys );
		List<Tile> randomValue = submissionDict[ keys[ Random.Range (0, keys.Count) ] ];
		return randomValue;
	}

	

	public void UpdateLevelingStats( int currentLevel )
	{
		//if surpassed highest Level
		if( !levelToBucketDict.ContainsKey( currentLevel ) )
		{
//			currentLeveling = allLevels [ allLevels.Count - 1 ];
			int bucketNum = bucketNumToAvailableLevelsDict.Count;
			List< Level > availableLevels = bucketNumToAvailableLevelsDict[ bucketNum ];
			currentLeveling = availableLevels[ Random.Range( 0, availableLevels.Count ) ];
		}
		else
		{
//			currentLeveling = allLevels [currentLevel];
			//get bucket number for level
			int bucketNum = levelToBucketDict[ currentLevel ];
			List< Level > availableLevels = bucketNumToAvailableLevelsDict[ bucketNum ];
			currentLeveling = availableLevels[ Random.Range( 0, availableLevels.Count ) ];
		}

		Debug.Log (" level of current problem : " + currentLeveling.level);
//		if ( !model.impossibleEnabled )
//		{
//			chanceOfImpossible = 0;
//		}
//		else
//		{
//			chanceOfImpossible = currentLeveling.chanceOfImpossible;
//			Debug.Log ( "chance of impossible : " + chanceOfImpossible );
//		}
	}

	

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
//		Debug.Log (currentLeveling.chanceOfImpossible);
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
			newLevel.SetRuleCreationParameters ( levelInfo[ 1 ], levelInfo[ 2 ], levelInfo[ 3 ], levelInfo[ 4 ], levelInfo[ 5 ], levelInfo[ 6 ], levelInfo[ 7 ] );
			newLevel.SetConditionalParameters( levelInfo[ 8 ], levelInfo[ 9 ], levelInfo[ 10 ] );
			newLevel.SetImpossibleBoardParameters(levelInfo[ 11 ], levelInfo[ 12 ], levelInfo[ 13 ] );
			newLevel.SetPossibleBoardParameters( levelInfo[ 14 ], levelInfo[ 15 ], levelInfo[ 16 ], levelInfo[ 17 ] );
			newLevel.SetMaxTrials( levelInfo [ 18 ], levelInfo [ 19 ] );
			newLevel.SetBucketNumber( levelInfo [ 20 ] );
//			if( newLevel.chanceOfImpossible > 0 )
//			{
//				Debug.Log (" chance of impossible : " + newLevel.chanceOfImpossible + ", Level : " + i );
//			}

//			newLevel.chanceOfImpossible = 50;
//			newLevel.maxTrialsInRuleSet = 6;

			allLevels.Add( newLevel );
			levelToBucketDict.Add ( newLevel.level, newLevel.bucketNumber );
			SortLevelsIntoBuckets( newLevel );

			//set first level with impossible option
			if( model.firstLevelWithImpossibles == 0 && newLevel.maxRulesToSetImpossibleBoard > 0 )
			{
				Debug.Log ( "Level " + i + " is first level with Impossible ");
				model.firstLevelWithImpossibles = i;
			}

			//if using conditionals, ensure that rules to build conditionals is at least 2
			if( levelInfo[ 4 ] > 0 )
			{
				if( ( levelInfo[ 8 ] + levelInfo[ 9 ] + levelInfo[ 10 ] ) < 2 )
				{
					Debug.Log ( "Conditional " + i + " does not have sufficient building rules ");
				}
			}
		}
	}

	void SortLevelsIntoBuckets( Level newLevel )
	{
		// if new level's bucket already in dict
		if( bucketNumToAvailableLevelsDict.ContainsKey( newLevel.bucketNumber ) )
		{
			//add new level to list of levels in bucket's list of values
			bucketNumToAvailableLevelsDict[ newLevel.bucketNumber ].Add( newLevel );
		}
		else
		{
			//add new level's bucket and new list of available levels
			List< Level > availableLevels = new List< Level >();
			availableLevels.Add ( newLevel );
			bucketNumToAvailableLevelsDict.Add ( newLevel.bucketNumber, availableLevels );
		}
		   

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

	public int chanceOfPonens;
	public int chanceOfTollens;
	public int chanceOfBNotA;

	public bool showClauseBNotA;  // b and !a
	//	bool showClauseBImplyNotA;  // b and make a impossible
	
	public int maxModusTollens;
	public int maxBAndNotA;

	public int level;
	public int bucketNumber;

	public Level( int levelNum )
	{
		level = levelNum;
	}

	public void SetBucketNumber( int num )
	{
		bucketNumber = num;
	}

	public void SetRuleCreationParameters( int maxAbs, int maxAdj, int maxRel, int maxCond, int totalRules, int usingPosOfAbs, int eitherOr )
	{
		maxConditionals = maxCond;
		maxRelativePosRules = maxRel;
		maxAdjacencyRules = maxAdj;
		maxAbsPosRules = maxAbs;
		usingPositiveOfAbsolute = ConvertIntToBool( usingPosOfAbs );
		usingEitherOr = ConvertIntToBool( eitherOr );
//		maxRules = maxConditionals + maxRelativePosRules + maxAdjacencyRules + maxAbsPosRules;
		maxRules = totalRules;

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
		minRulesToSetImpossibleBoard = Mathf.Max (1, maxRulesToSetImpossibleBoard - 1 );
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


