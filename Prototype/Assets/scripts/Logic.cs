using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logic : MonoBehaviour {

	public Model model;
	public MetaData metaData;
//	public View view;

	public RuleStack trialRules;

	public int problemsPerTrial;

//	public int maxRuleDifficulty;
//	public int maxProblemDifficulty;
	public int maxRules;
	public int tilesCount;
	public int chanceOfImpossible;
	public int maxImpossiblePerTrial;
	int impossiblesUsed;

	//rules that can be used
	int maxAbsPosRules;
	int maxRelativePosRules;
	int maxAdjacencyRules;
	int maxConditionals;
	bool usingEitherOr;

	//integers for rule types
	int absPosition = 1;
	int eitherOrAbsPosition = 2;
	int negAbsPosition = 3;

	int adjacent = 4;
	int notAdjacent = 5;

	int relativeOrder = 6;

	//difficulty for impossible board
	int maxRulesToSetImpossibleBoard;
	int maxConditionalInImpossibleSet;



	//CONDITIONALS

	//construction
	int maxAbsInConditional;
	int maxAdjInConditional;
	int maxRelInConditional;

	// difficulty for possible board
	bool modusPonens; //if a then b
	bool modusTollens; //if !b, then !a
	bool showClauseBNotA;  // b and !a
	bool showClauseBImplyNotA;  // b and make a impossible

	int maxModusTollens;
	int maxBAndNotA;
	
	string previousPossiblePresetKey;
	string previousImpossiblePresetKey;

	// Use this for initialization
	void Start () {
//
//		int nullToAdd = 0;
//		List< List< string>> allCombos = new List< List< string>> ();
//		List< string > newCombo = new List< string > ();
//		List<string> bank = new List<string>();
//		bank.Add ("a");
//		bank.Add ("b");
//		bank.Add ("c");
//		GetAllCombinationsForPresets (bank, newCombo, 2, allCombos );
//	
//		List<string> allKeyCombos = new List<string> ();
//		List<int> currPosOrder = new List<int> ();
//		List<int> digitBank = new List<int> ();
//		digitBank.Add (0);
//		digitBank.Add (1);
//		digitBank.Add (2);
//		digitBank.Add (3);
//		GetPresetOrdersFromCombos (currPosOrder, digitBank, digitBank.Count, allCombos [0], allKeyCombos );

	}

	// Update is called once per frame
	void Update () {
	
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

		for( int conditional = 0; conditional < maxConditionals; conditional ++ )
		{
			if( rulesCreated < maxRules )
			{
				Conditional newConditional = CreateConditionalRule( positionsToUse, tiles, tileUsage );
//				newConditional.ConstructVerbal();
				if( !trialRules.RuleConflictsWithRuleStack( newConditional, problemsPerTrial ))
				{
					Debug.Log ( "successful rule : " + newConditional.verbal );
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


		for( int relatives = 0; relatives < maxRelativePosRules; relatives ++ )
		{
			if( rulesCreated < maxRules )
			{
				RelativePositionRule newRel = CreateRelativeRule( tiles, tileUsage );
				if( !trialRules.RuleConflictsWithRuleStack(newRel, problemsPerTrial ))
				{
					Debug.Log ( "successful rule : " + newRel.verbal );
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
		
		for( int adjacency = 0; adjacency < maxAdjacencyRules; adjacency ++ )
		{
			if( rulesCreated < maxRules )
			{
				AdjacencyRule newAdj = CreateAdjacencyRule( tiles, tileUsage );
				if( !trialRules.RuleConflictsWithRuleStack( newAdj, problemsPerTrial ))
				{
					Debug.Log ( "successful rule : " + newAdj.verbal );
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
		for( int abs = 0; abs < maxAbsPosRules; abs ++ )
		{
			if( rulesCreated < maxRules )
			{
				AbsolutePositionRule newAbs = CreateAbsoluteRule( positionsToUse, tiles, tileUsage );
				if( !trialRules.RuleConflictsWithRuleStack( newAbs, problemsPerTrial ))
				{
					Debug.Log ( "successful rule : " + newAbs.verbal );
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

		metaData.SetStatsForTrial (model.currentLevel, model.currentProblemInTrial, model.currentTrial, trialRules.ruleStack.Count, ruleIndentifiers, tiles.Count);
		metaData.SetTimeSinceProblemStart (Time.time);

		trialRules.ConstructRandomOrderedVerbal ();
		Debug.Log ("rule count : " + trialRules.ruleStack.Count);
		for( int i = 0; i < trialRules.ruleStack.Count; i++ )
		{
			Debug.Log ( trialRules.ruleStack[ i ].verbal );
		}
//		Debug.Log (" ************CORRECT ANSWERS****************** : " );
//		trialRules.PrintEachDictionaryValue (trialRules.correctSubmissions);
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

	bool ReusedTilesAvailable( Dictionary < Tile, int > tileUsage )
	{
		foreach( KeyValuePair< Tile , int > pair in tileUsage )
		{
			Tile tile = pair.Key;

			if( pair.Value == 1 )
			{
				return true;
			}
		}

		return false;
	}

	List<Tile> GetTilesToUse( Dictionary < Tile, int > tileUsage, int tilesNeeded, List< Tile > unavailableTiles = null )
	{
		bool availableReusedTiles = ReusedTilesAvailable (tileUsage);
		int minReusedTiles = 0;
		int maxReusedTiles = 0;
		
		if( availableReusedTiles && unavailableTiles == null )
		{
			minReusedTiles = 1;
			maxReusedTiles = 1;
		}

		Debug.Log (" min reused : " + minReusedTiles + ", maxReused : " + maxReusedTiles);

		int reusedTiles = 0;

		List<Tile> tilesToUse = new List<Tile> ();


		foreach( KeyValuePair< Tile , int > pair in tileUsage )
		{
			Tile tile = pair.Key;
			//if tile has been used only once and reused tiles is less than max reused tiles
			if( ( pair.Value == 1 ) && ( reusedTiles < maxReusedTiles ))
			{
				if( unavailableTiles == null )
				{
					tilesToUse.Add ( tile );
					reusedTiles ++;
				}
				else if( !unavailableTiles.Contains( pair.Key ))
				{
					tilesToUse.Add ( tile );
					reusedTiles ++;
				}
			}
			else if( pair.Value == 0 && ( reusedTiles >= minReusedTiles ))
			{
				tilesToUse.Add ( tile );
			}
			if( tilesToUse.Count == tilesNeeded )
			{
				for( int i = 0; i < tilesToUse.Count; i ++ )
				{
					tileUsage[tilesToUse[i]] ++;
					Debug.Log ( "USED : " + tilesToUse[i]);
				}

				return tilesToUse;
			}
		}

//		Debug.Log (" adding more tiles ");

//		foreach( KeyValuePair< Tile , int > pair in tileUsage )
//		{
//			Tile tile = pair.Key;
//			//if tile has been used only once and reused tiles is less than max resused tiels
//			if( ( pair.Value == 1 ) && ( reusedTiles < maxReusedTiles ))
//			{
//				tilesToUse.Add ( tile );
//				reusedTiles ++;
//			}
//			else if( pair.Value == 0 && ( reusedTiles >= minReusedTiles ))
//			{
//				tilesToUse.Add ( tile );
//			}
//			if( tilesToUse.Count == tilesNeeded )
//			{
//				for( int i = 0; i < tilesToUse.Count; i ++ )
//				{
//					tileUsage[tilesToUse[i]] ++;
//				}
//				
//				return tilesToUse;
//			}
//		}
		Debug.Log (" not enough tiles added ");
		return tilesToUse;
	}

	RelativePositionRule CreateRelativeRule( List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{

		//randomly determine before/after rule
		int order = Random.Range (0, 2);

		List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2 );
		
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

	AbsolutePositionRule CreateAbsoluteRule( List<int> holderPositions, List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage, List< Tile > unusableTiles = null )
	{

		//create absolute position rule
		int absolutePosition = Random.Range ( 0, holderPositions.Count );
		Debug.Log (" absolutePosition : " + absolutePosition); 
		holderPositions.Remove (absolutePosition);
//		Debug.Log ("ABSOLUTE POSITION : " + absolutePosition);

		int absPosTile = Random.Range (0, tilesToOrder.Count);

		//decide if using 1 or 2 tiles for rule
		if (usingEitherOr) 
		{
			int random = Random.Range (0, 2);
			if( random == 0 ) //create rule with 2 tiles
			{

				List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2, unusableTiles );
				
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToUse[0], tilesToUse[1], absolutePosition, tilesToOrder ); 
				absPositionRule.ConstructVerbal();
				
				return absPositionRule;
			}
			{
				List<Tile> tilesToUse = GetTilesToUse (tileUsage, 1, unusableTiles);
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToUse[0], absolutePosition, tilesToOrder ); 
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
//	Conditional CreateConditionalRule( List<TileHolder> holders, List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
//	{
//		List<Tile> tilesUsedInRules = new List<Tile> ();
//		
//		//choose 2 types of rules randomly ( not of same type )
//		int random = Random.Range (0, 2);
//		int random2 = Random.Range (0, 2);
//		
//		while( random == random2 )
//		{
//			random2 = Random.Range( 0, 2 );
//		}
//		
//		Rule rule1;
//		Rule rule2;
//		
//		if( random == 0 )
//		{
//			rule1 = CreateAbsoluteRule( holders, tilesToOrder, tileUsage );
//		}
//		else if( random == 1 )
//		{
//			rule1 = CreateAdjacencyRule( tilesToOrder, tileUsage );
//		}
//		else
//		{
//			rule1 = CreateRelativeRule( tilesToOrder, tileUsage );
//		}
//		
//		if( random2 == 0 )
//		{
//			rule2 = CreateAbsoluteRule( holders, tilesToOrder, tileUsage );
//		}
//		else if( random2 == 1 )
//		{
//			rule2 = CreateAdjacencyRule( tilesToOrder, tileUsage );
//		}
//		else
//		{
//			rule2 = CreateRelativeRule( tilesToOrder, tileUsage );
//		}
//		
//		Conditional newConditional = new Conditional (rule1, rule2, tilesToOrder );
//		bool validRule = newConditional.IsValidRule ();
//		
//		while( !validRule )
//		{
//			//remove a point for each used tile in tileUsage dict
//			RemoveTilesUsedInRuleFromDict( tileUsage, newConditional );
//			newConditional = CreateConditionalRule( holders, tilesToOrder, tileUsage );
//			validRule = newConditional.IsValidRule();
//		}
//		
//		return newConditional;
//	}

	Conditional CreateConditionalRule( List<int> holderPositions, List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{

//		if( ( maxRelInConditional + maxAdjInConditional + maxAbsInConditional ) < 2 )
//		{
//			Debug.Log (" not enough rules to make conditional");
//			Debug.Log (" tile count : " + tilesToOrder.Count );
//			Debug.Log (" level : " + model.currentLevel );
//		}

		List<Tile> unusableTiles = new List<Tile> ();

		List< Rule > rulesPoolForConditional = new List< Rule > ();

		for( int i = 0; i < maxRelInConditional; i ++ )
		{
			RelativePositionRule relRule = CreateRelativeRule( tilesToOrder, tileUsage );
			rulesPoolForConditional.Add ( relRule as Rule );
		}

		for( int i = 0; i < maxAdjInConditional; i ++ )
		{
			AdjacencyRule adjRule = CreateAdjacencyRule( tilesToOrder, tileUsage );
			rulesPoolForConditional.Add ( adjRule as Rule );
		}

		for( int i = 0; i < maxAbsInConditional; i ++ )
		{
			Debug.Log ( holderPositions.Count );
			AbsolutePositionRule absRule = CreateAbsoluteRule( holderPositions, tilesToOrder, tileUsage, unusableTiles );
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
	

	public string NewProblemSetUp( List<Tile> previousSubmission )
	{
		//refresh tile positions
		bool createImpossible = false;
		string presetTiles;

		if( impossiblesUsed < maxImpossiblePerTrial )
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
//		List< string > bestPresetTiles = GetBestPresetToCompleteBoard( 
		Debug.Log (presetTiles);
		//		create string with only 1 placed tile in newSubmission
		
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

			Debug.Log ( " modus tollens : " + modusTollens );
			Debug.Log (" modus ponens : " + modusPonens );
			Debug.Log (" bAnd Not A : " + showClauseBNotA );
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

								if( showClauseBNotA )
								{
									if( KeyIsBAndNeverA( neverBreaksRule2, alwaysBreaksRule2 ))
									{
										testKeyUsesBAndNotA = true;
										Debug.Log ( "bAndNotA : " + preset );
									}
								}
								if( modusTollens )
								{
									if( KeyIsContraPositiveOfConditional( alwaysBreaksRule2 ))
									{
										testKeyUsesModusTollens = true;
										Debug.Log ("tollens : " + preset );
									}
								}
								if( modusPonens )
								{
									if( KeyIsPositiveOfConditional( neverBreaksRule1 ))
									{
										testKeyUsesModusPonens = true;
										Debug.Log ( "ponens : " +preset );
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
								Debug.Log ( " added : " + preset );
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
								Debug.Log ( " added : " + preset );
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
								Debug.Log ( " added : " + preset );
							}
						}
						// if no conditionals in stack
						else
						{
							if ( instances < fewestPossibleCompletions && fewestPossibleCompletions > 0)
							{
//								fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
								presetReplacesBest = true;
							}
							else if ( instances == fewestPossibleCompletions && currentPresets == bestPresetCount )
							{
								bestKeys.Add ( preset );
								Debug.Log ( " added : " + preset );
							}
						}

						if( presetReplacesBest )
						{
							bestKeys = new List< string >();
							bestKeys.Add ( preset );
							Debug.Log ( " added : " + preset );
							fewestPossibleCompletions = instances;
//							fewestPossibleCompletions = ReplaceBestKeysWithTestKey( bestKeys, preset, instances );
							bestPresetCount = currentPresets;

							Debug.Log ("***keys count : " + bestKeys.Count);
						}
					}

				}
			}

			currentPresets ++;
			Debug.Log ( "increase presets to : " + currentPresets );
		}
		Debug.Log ("best keys count : " + bestKeys.Count);
		Debug.Log (" modus Ponens : " + bestKeyUsesModusPonens + ", modus Tollens : " + bestKeyUsesModusTollens + ", bAndNotA : " + bestKeyUsesBAndNotA);
		return bestKeys;

	}


	int ReplaceBestKeysWithTestKey( List< string > bestKeys, string preset, int presetInstances )
	{
		bestKeys = new List< string >();
		bestKeys.Add ( preset );

		return presetInstances;
	}

//	int UpdateBestPresetCount( int 

//	string GetBestPresetToCompleteBoard ( int presets, List< string > tileBank )
//	{
//		int fewestPossibleCompletions = 1000;
//		string bestKey = null;
//
//		List< List<string> > tileCombos = new List< List<string> >();
//		List< string > newCombo = new List< string > ();
//		GetAllCombinationsForPresets( tileBank, newCombo, presets, tileCombos);
//
//		//for each combo in allCombos
//		for( int comboIndex = 0; comboIndex < tileCombos.Count; comboIndex ++ )
//		{
//			List<int> digitBank = CreateDigitBank( tileBank.Count );
//			
//			List< string > presetKeyCombos = new List< string > ();
//			
//			List<int> currPosOrder = new List<int> ();
//			
//			GetPresetOrdersFromCombos( currPosOrder, digitBank, digitBank.Count, tileCombos[ comboIndex ], presetKeyCombos );
//			
//			for( int presetOrderIndex = 0; presetOrderIndex < presetKeyCombos.Count; presetOrderIndex ++ )
//			{
//				string preset = presetKeyCombos[ presetOrderIndex ];
//
//				if( PresetIsNew( preset ) && trialRules.WildCardKeyInDictionary( preset, trialRules.correctSubmissions))
//				{
//					//test to see if preset has fewer possibilites than best
//					int instances = GetPresetInstances( preset );
//					if ( instances < fewestPossibleCompletions && fewestPossibleCompletions > 0)
//					{
//						bestKey = preset;
//						fewestPossibleCompletions = instances;
//					}
//				}
//
//				if( fewestPossibleCompletions == 1 )
//				{
//					return bestKey;
//				}
//			}
//		}
//
//		return bestKey;
//	}


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

		GetAllCombinationsOfRules (ruleBank, newCombo, Mathf.Min ( maxRulesToSetImpossibleBoard, trialRules.ruleStack.Count ), allRuleCombos);

		string presetTiles = null;

		Debug.Log (" combo list count : " + allRuleCombos.Count); 
		List< string > tileBank = GetTilesAsListOfLetters (model.tilesToOrder);

		int maxPresetTiles = 3;
		int minPresetTiles = 1;
		
		while( presetTiles == null || minPresetTiles > maxPresetTiles )
		{
			for( int i = 0; i < allRuleCombos.Count; i ++ )
			{
				RuleStack rulesToBreak = ReturnRuleStackFromComboList ( allRuleCombos, i );
				
//				UpdateMinPresets (minPresetTiles, rulesToBreak);
				
				List< Rule > otherRules = trialRules.GetRulesInStackNotInList (rulesToBreak.ruleStack);

				presetTiles = AttemptToGetImpossibleKey( minPresetTiles, tileBank, rulesToBreak, otherRules );

				if( presetTiles != null )
				{
					previousImpossiblePresetKey = presetTiles;
					break;
				}
			}

			//if this is read then preset is null
			//increase min preset tiles
			minPresetTiles ++;

			//FOR LATER
			//if minPresets > maxPresets and rules in trial > 1
				//try with fewer rules to combine
				//reset min/max preset counts
		}

		
		if( presetTiles != null )
		{
			Debug.Log ("CREATING IMPOSSIBLE BOARD");
			model.SetImpossible( true );
			impossiblesUsed ++;
			Debug.Log ("found valid preset tile order ");
			previousImpossiblePresetKey = presetTiles;
			return presetTiles;
		}
		else
		{
			model.SetImpossible( false );
			presetTiles = CreatePossibleBoard( previousSubmission );
			Debug.Log ("impossible preset NOT found, creating possible board ");
		}

		return presetTiles;
	}


	string AttemptToGetImpossibleKey( int presets, List<string> tileBank, RuleStack rulesToBreak, List< Rule> nonBreakingRules )
	{
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
						return presetKeyCombos[ presetOrderIndex ];
					}
				}
			}
		}
			
		return null;
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

	bool ImpossibleKey( string possibleKey, RuleStack rulesToBreak, List<Rule> nonbreakingRules,  bool singleRuleBreak )
	{
//		if( !trialRules.WildCardKeyInDictionary( possibleKey, trialRules.correctSubmissions ))
		if(!PresetIsPossibleCorrectSubmission( possibleKey ))
		{
			Debug.Log ("PRESET NOT IN CORRECT SUBMISSIONS" );

			//test to see if preset is a possible correct answer each concerned breakable rule
			bool passBreakableRulesTest = true;


			for( int i = 0; i < rulesToBreak.ruleStack.Count; i ++ )
			{
				//if key is not in rule to break's correct submissions
				if( !rulesToBreak.ruleStack[ i ].WildCardKeyInDictionary( possibleKey, rulesToBreak.ruleStack[ i ].correctSubmissions ))
				{
					if ( !singleRuleBreak )
					{
						Debug.Log ( " PRESET BREAKS RULE TO BREAK " );
						passBreakableRulesTest = false;
						break;
					}

				}
			}
			
			if( passBreakableRulesTest )
			{
				Debug.Log ( "PRESET DOES NOT BREAK INDIVIDUAL RULE ");
				bool passOtherRulesTest = true;
				//test to see if preset is impossible for non-concerned rules in trial rules
				for( int nonbreakingRule = 0; nonbreakingRule < nonbreakingRules.Count; nonbreakingRule ++ )
				{
					if( !nonbreakingRules[ nonbreakingRule ].WildCardKeyInDictionary( possibleKey, nonbreakingRules[ nonbreakingRule ].correctSubmissions ))
					{
						Debug.Log ( "PRESET NOT IN OTHER RULE'S POSSIBLE CORRECTS ");
						passOtherRulesTest = false;
						break;
					}
				}
				
				if( passOtherRulesTest )
				{
					Debug.Log ( "SUCCESSFUL PRESET BREAK" );
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


	void SetRuleCreationParameters( int maxCond, int maxRel, int maxAdj, int maxAbs )
	{
		maxConditionals = maxCond;
		maxRelativePosRules = maxRel;
		maxAdjacencyRules = maxAdj;
		maxAbsPosRules = maxAbs;
		maxRules = maxConditionals + maxRelativePosRules + maxAdjacencyRules + maxAbsPosRules;

	}

	void SetConditionalParameters( int maxRel, int maxAdj, int maxAbs )
	{
		maxAbsInConditional = maxAbs;
		maxAdjInConditional = maxAdj;
		maxRelInConditional = maxRel;
	}

	void SetTileCount( int tilesInTrial )
	{
		tilesCount = tilesInTrial;
	}

	void SetImpossibleBoardParameters( int rulesToSetBoard, int impChance, int maxImp )
	{
		maxRulesToSetImpossibleBoard = rulesToSetBoard;
		chanceOfImpossible = impChance;
		maxImpossiblePerTrial = maxImp;
	}

	void SetPossibleBoardParameters( bool AThenB, bool notBThenNotA, bool showBAndNotA, bool showBAndAImpossible )
	{
		modusPonens = AThenB;
		modusTollens = notBThenNotA;
		showBAndNotA  = showClauseBNotA;
		showBAndAImpossible = showClauseBImplyNotA;
	}

	public void UpdateLevelingStats( int currentLevel )
	{
		ResetStats ();

//		Debug.Log (currentLevel);

		if (currentLevel == 0) 
		{
			SetRuleCreationParameters( 0, 1, 0, 0 );
			SetTileCount ( 3 );
			SetImpossibleBoardParameters( 1, 30, 1 );
			SetPossibleBoardParameters( false, false, false, false );
		}

		else if( currentLevel == 1 )
		{
			maxRelativePosRules = 1;
			maxAbsPosRules = 1;
			maxRules = 2;
			tilesCount = 3;
			chanceOfImpossible = 75;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
		}
		else if( currentLevel == 2 )
		{
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			maxRules = 2;
			tilesCount = 4;
			chanceOfImpossible = 40;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
			usingEitherOr = true;
		}
		else if( currentLevel == 3 )
		{
			maxRelativePosRules = 2;
			maxRules = 2;
			tilesCount = 4;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
			usingEitherOr = true;
		}
		else if( currentLevel == 4 )
		{
			maxConditionals = 0;
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			maxRules = 2;
			tilesCount = 4;
			chanceOfImpossible = 50;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
		}
		else if( currentLevel == 5 )
		{
			maxConditionals = 1;
			maxRules = 1;
			tilesCount = 4;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, false, false, false );
			SetConditionalParameters( 0, 1, 1 );
		}
		else if( currentLevel == 6 )
		{
			maxConditionals = 1;
			maxRules = 1;
			tilesCount = 3;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
			usingEitherOr = false;
			SetPossibleBoardParameters( true, true, false, false );
			SetConditionalParameters( 0, 0, 2 );
		}
		else if( currentLevel == 7 )
		{
			maxRules = 1;
			maxConditionals = 1;
			
			tilesCount = 4;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, false, false );
			SetConditionalParameters( 0, 1, 1 );
		}
		else if( currentLevel == 8 )
		{
			maxRules = 2;
			
			maxConditionals = 1;
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			tilesCount = 4;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, false, false );
			SetConditionalParameters( 1, 0, 1 );
		}

		else if( currentLevel == 9 )
		{
			maxRules = 2;
			maxConditionals = 1;
			maxRelativePosRules = 2;
			
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 1;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, false );
			SetConditionalParameters( 0, 2, 0 );
		}
		else if( currentLevel == 10 )
		{
			maxRules = 3;
			
			maxConditionals = 0;
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			maxAbsPosRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 50;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, false );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 11 )
		{
			maxRules = 3;

			maxConditionals = 1;
			maxRelativePosRules = 1;
			maxAbsPosRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, false );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 12 )
		{
			maxRules = 3;

			maxConditionals = 1;
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, false );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 13 )
		{
			maxRules = 3;
			
			maxConditionals = 1;
			maxRelativePosRules = 1;
			maxAbsPosRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 14 )
		{
			maxRules = 3;
			
			maxConditionals = 1;
			maxRelativePosRules = 2;
			maxAbsPosRules = 0;
			
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 15 )
		{
			maxRules = 3;
			
			maxConditionals = 1;
			maxRelativePosRules = 2;
//			maxAbsPosRules = 2;
			
			tilesCount = 5;
			chanceOfImpossible = 25;
			maxImpossiblePerTrial = 2;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 16 )
		{
			maxRules = 3;
			
			maxConditionals = 1;
			maxRelativePosRules = 2;
			maxAbsPosRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 25;
			maxImpossiblePerTrial = 2;
			maxRulesToSetImpossibleBoard = 2;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 17 )
		{
			maxRules = 3;
			
			maxConditionals = 1;
			maxRelativePosRules = 1;
			maxAbsPosRules = 2;
			
			tilesCount = 5;
			chanceOfImpossible = 25;
			maxImpossiblePerTrial = 2;
			maxRulesToSetImpossibleBoard = 3;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 18 )
		{
			maxRules = 3;
			
			maxConditionals = 1;
			maxRelativePosRules = 1;
			maxAbsPosRules = 2;
			
			tilesCount = 5;
			chanceOfImpossible = 25;
			maxImpossiblePerTrial = 2;
			maxRulesToSetImpossibleBoard = 3;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 19 )
		{
			maxRules = 3;
			
			maxConditionals = 1;
			maxRelativePosRules = 2;
			maxAbsPosRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 25;
			maxImpossiblePerTrial = 2;
			maxRulesToSetImpossibleBoard = 3;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else if( currentLevel == 20 )
		{
			maxRules = 3;
			
			maxConditionals = 2;
			maxRelativePosRules = 1;
			maxAbsPosRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 25;
			maxImpossiblePerTrial = 2;
			maxRulesToSetImpossibleBoard = 3;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );
		}
		else
		{
			maxRules = 3;
			
			maxConditionals = 2;
			maxRelativePosRules = 1;
			maxAbsPosRules = 1;
			
			tilesCount = 5;
			chanceOfImpossible = 25;
			maxImpossiblePerTrial = 2;
			maxRulesToSetImpossibleBoard = 3;
			usingEitherOr = true;
			SetPossibleBoardParameters( true, true, true, true );
			SetConditionalParameters( 1, 1, 0 );

		}

//		maxRules = 4;
//		
//		maxConditionals = 1;
//		maxRelativePosRules = 2;
//		maxAbsPosRules = 1;
//		
//		tilesCount = 5;
//		chanceOfImpossible = 25;
//		maxImpossiblePerTrial = 2;
//		maxRulesToSetImpossibleBoard = 3;
//		usingEitherOr = true;
//		SetPossibleBoardParameters( true, true, true, true );
	
		
	}
	
	void ResetStats()
	{
		maxAbsPosRules = 0;
		maxRelativePosRules = 0;
		maxAdjacencyRules = 0;
		maxConditionals = 0;
		chanceOfImpossible = 0;
		maxImpossiblePerTrial = 0;
		maxRulesToSetImpossibleBoard = 0;
		usingEitherOr = false;

		previousPossiblePresetKey = null;
		previousImpossiblePresetKey = null;

		maxAbsInConditional = 0;
		maxAdjInConditional = 0;
		maxRelInConditional = 0;

		SetPossibleBoardParameters( false, false, false, false );

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
		int random = Random.Range (0, 101);
//		Debug.Log (" ROLLED : " + random + " , CHANCE : " + chance);
		if( random > chance )
		{
			return false;
		}
		return true;
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
