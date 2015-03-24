using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logic : MonoBehaviour {

	public Model model;
//	public View view;

	public RuleStack trialRules;

	public int problemsPerTrial;

	public int maxRuleDifficulty;
	public int maxProblemDifficulty;
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

	//difficulty points
	int absPosition = 6;
	int negAbsPosition = 10;
	int beforeOrder = 10;
	int afterOrder = 12;
	int nextTo = 10;
	int notNextTo = 12;

	int maxRulesToSetNewProblem;
	bool usingEitherOr;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	
	}

	public string CreateRules( List<Tile> tiles, List<TileHolder> holders )
	{
		int difficultyPointsSpend = 0;
		int rulesCreated = 0;
		impossiblesUsed = 0;
		trialRules = new RuleStack ();

		List<Tile> shuffledTiles = ShuffleThis (tiles);

		//create tile dictionary where each tile has yet ot be used
		Dictionary<Tile, int > tileUsage = new Dictionary< Tile , int > ();
		for( int i = 0; i < shuffledTiles.Count; i ++ )
		{
			tileUsage.Add ( shuffledTiles[ i ], 0 );
		}

		int totalRules = maxAbsPosRules + maxRelativePosRules + maxAdjacencyRules + maxConditionals;
		Debug.Log ("max rules : " + totalRules);

		for( int conditional = 0; conditional < maxConditionals; conditional ++ )
		{
			Conditional newConditional = CreateConditionalRule( holders, tiles, tileUsage );
			Debug.Log ( newConditional.ConstructVerbal());
			Debug.Log (newConditional.verbal );
			if( !trialRules.RuleConflictsWithRuleStack( newConditional ))
			{
				trialRules.AddRule( newConditional );
				rulesCreated ++;
			}
			else
			{
				RemoveTilesUsedInRuleFromDict( tileUsage, newConditional );
			}
		}
		for( int abs = 0; abs < maxAbsPosRules; abs ++ )
		{
			AbsolutePositionRule newAbs = CreateAbsoluteRule( holders, tiles, tileUsage );
			if( !trialRules.RuleConflictsWithRuleStack( newAbs ))
			{
				trialRules.AddRule( newAbs );
				rulesCreated ++;
			}
			else
			{
				RemoveTilesUsedInRuleFromDict( tileUsage, newAbs );
			}
		}

		for( int relatives = 0; relatives < maxRelativePosRules; relatives ++ )
		{
			RelativePositionRule newRel = CreateRelativeRule( tiles, tileUsage );
			if( !trialRules.RuleConflictsWithRuleStack(newRel))
			{
				trialRules.AddRule( newRel );
				rulesCreated ++;
			}
			else
			{
				RemoveTilesUsedInRuleFromDict( tileUsage, newRel );
			}
		}

		for( int adjacency = 0; adjacency < maxAdjacencyRules; adjacency ++ )
		{
			AdjacencyRule newAdj = CreateAdjacencyRule( tiles, tileUsage );
			if( !trialRules.RuleConflictsWithRuleStack( newAdj ))
			{
				trialRules.AddRule( newAdj );
				rulesCreated ++;
			}
			else
			{
				RemoveTilesUsedInRuleFromDict( tileUsage, newAdj );
			}
		}

		trialRules.ConstructVerbal ();
		Debug.Log ("rule count : " + trialRules.ruleStack.Count);
		Debug.Log (" correct answers : " + trialRules.correctSubmissions.Count);
		return trialRules.verbal;
	}

	List<Tile> GetTilesToUse( Dictionary < Tile, int > tileUsage, int tilesNeeded, int rulesCreated )
	{
		int minReusedTiles = 0;
		if( rulesCreated >= 1 )
		{
			minReusedTiles = 1;
		}

		int maxReusedTiles = 1;
		int reusedTiles = 0;

		List<Tile> tilesToUse = new List<Tile> ();


		foreach( KeyValuePair< Tile , int > pair in tileUsage )
		{
			Tile tile = pair.Key;
			//if tile has been used only once and reused tiles is less than max resused tiels
			if( ( pair.Value == 1 ) && ( reusedTiles < maxReusedTiles ))
			{
				tilesToUse.Add ( tile );
				reusedTiles ++;
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
				}

				return tilesToUse;
			}
		}

		Debug.Log (" not enough tiles added ");
		return tilesToUse;
	}

	RelativePositionRule CreateRelativeRule( List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{

		//randomly determine before/after rule
		int order = Random.Range (0, 1);

		List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2, trialRules.ruleStack.Count );
		
		RelativePositionRule relPositionRule = new RelativePositionRule( order, tilesToUse[0], tilesToUse[1], tilesToOrder  ); 
		relPositionRule.ConstructVerbal();
		
		return relPositionRule;
	}

	AdjacencyRule CreateAdjacencyRule( List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{
		int nextTo = Random.Range (0, 1);  //determins next to/not next to

		List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2, trialRules.ruleStack.Count);
		
		AdjacencyRule adjRule = new AdjacencyRule( nextTo, tilesToUse[0], tilesToUse[1], tilesToOrder  ); 
		adjRule.ConstructVerbal();
		
		return adjRule;
	}

	AbsolutePositionRule CreateAbsoluteRule( List<TileHolder> holders, List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage)
	{

		//create absolute position rule
		int absolutePosition = Random.Range ( 0, holders.Count );

		int absPosTile = Random.Range (0, tilesToOrder.Count);

		//decide if using 1 or 2 tiles for rule
		if (usingEitherOr) 
		{
			int random = Random.Range (0, 1);
			if( random == 0 ) //create rule with 2 tiles
			{


				List<Tile> tilesToUse = GetTilesToUse (tileUsage, 2, trialRules.ruleStack.Count);
				
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToUse[0], tilesToUse[1], absolutePosition, tilesToOrder ); 
				absPositionRule.ConstructVerbal();
				
				return absPositionRule;
			}
			{
				List<Tile> tilesToUse = GetTilesToUse (tileUsage, 1, trialRules.ruleStack.Count);
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToUse[0], absolutePosition, tilesToOrder ); 
				absPositionRule.ConstructVerbal();
				
				return absPositionRule;
			}
				
		}
		else
		{
			List<Tile> tilesToUse = GetTilesToUse (tileUsage, 1, trialRules.ruleStack.Count);
			AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToUse[0], absolutePosition, tilesToOrder ); 
			absPositionRule.ConstructVerbal();
			
			return absPositionRule;
		}
	
	}

	Conditional CreateConditionalRule( List<TileHolder> holders, List<Tile> tilesToOrder, Dictionary < Tile, int > tileUsage )
	{
		List<Tile> tilesUsedInRules = new List<Tile> ();

		//choose 2 types of rules randomly ( not of same type )
		int random = Random.Range (0, 2);
		int random2 = Random.Range (0, 2);

		while( random == random2 )
		{
			random2 = Random.Range( 0, 2 );
		}

		Rule rule1;
		Rule rule2;

		if( random == 0 )
		{
			rule1 = CreateAbsoluteRule( holders, tilesToOrder, tileUsage );
		}
		else if( random == 1 )
		{
			rule1 = CreateAdjacencyRule( tilesToOrder, tileUsage );
		}
		else
		{
			rule1 = CreateRelativeRule( tilesToOrder, tileUsage );
		}
		
		if( random2 == 0 )
		{
			rule2 = CreateAbsoluteRule( holders, tilesToOrder, tileUsage );
		}
		else if( random2 == 1 )
		{
			rule2 = CreateAdjacencyRule( tilesToOrder, tileUsage );
		}
		else
		{
			rule2 = CreateRelativeRule( tilesToOrder, tileUsage );
		}

		Conditional newConditional = new Conditional (rule1, rule2, tilesToOrder );
		bool validRule = newConditional.IsValidRule ();

		while( !validRule )
		{
			//remove a point for each used tile in tileUsage dict
			RemoveTilesUsedInRuleFromDict( tileUsage, newConditional );
			newConditional = CreateConditionalRule( holders, tilesToOrder, tileUsage );
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
		Debug.Log ("create impossible : " + createImpossible );
		if( createImpossible )
		{
			presetTiles = AttemptToCreateImpossibleBoard( previousSubmission, model.tilesToOrder );
		}
		else
		{
			presetTiles = CreatePartiallyFilledBoard( previousSubmission );
		}
		Debug.Log (presetTiles);
		return presetTiles;
	}

	string CreatePartiallyFilledBoard( List<Tile> previousSubmission )
	{
		Debug.Log ("CREATING POSSIBLE BOARD");
		//create  key string from previousSubmission
		string oldSubmission = "";
		for( int tile = 0; tile < previousSubmission.Count; tile ++ )
		{
			oldSubmission += previousSubmission[ tile ].name[ 0 ];
		}

		Debug.Log (" previous : " + oldSubmission);

		//find a possible submission that does not match previous submission
		string newSubmission = trialRules.GetKeyWithNoMatchesToKey (oldSubmission, trialRules.correctSubmissions);

		Debug.Log (" new : " + newSubmission);
		//create string with only 1 placed tile in newSubmission
		int random = Random.Range (0, newSubmission.Length - 1);

		string presetTiles = "";
		for( int tile = 0; tile < previousSubmission.Count; tile ++ )
		{
			if( tile == random )
			{
				presetTiles += newSubmission[ tile ];
			}
			else
			{
				presetTiles += "n";
			}

		}

		model.SetImpossible( false );

		return presetTiles;
	}

	string AttemptToCreateImpossibleBoard( List<Tile> previousSubmission, List<Tile> tilesToOrder )
	{
		for( int i = 0; i < trialRules.ruleStack.Count; i ++ )
		{
			Debug.Log (trialRules.ruleStack[ i ].verbal );
			Debug.Log (trialRules.ruleStack[ i ].correctSubmissions.Count );
		}

		Debug.Log ("ATTEMPTING TO CREATE IMPOSSIBLE");
		RuleStack rulesToBreak = CreateRuleStackFromRandomRulesInCurrentTrial( maxRulesToSetNewProblem );
		List< Rule > otherRules = trialRules.GetRulesInStackNotInList (rulesToBreak.ruleStack);
		Debug.Log (" SHARED IMPOSSIBLE : " + rulesToBreak.sharedImpossibleSubmissions.Count);
		Debug.Log ("combined rules : " + rulesToBreak.ruleStack.Count);

		string presetTiles = null;
		List<Tile> impossibleOrder = null;

		Debug.Log ("AFTER BREAK STACK");
		for( int i = 0; i < trialRules.ruleStack.Count; i ++ )
		{
			Debug.Log (trialRules.ruleStack[ i ].verbal );
			Debug.Log (trialRules.ruleStack[ i ].correctSubmissions.Count );
		}
		while( presetTiles == null && rulesToBreak.ruleStack.Count > 0 )
		{
			presetTiles = GetBestImpossiblePresetTileOrder( tilesToOrder, rulesToBreak, otherRules );
			if( presetTiles == null )
			{
				otherRules.Add ( rulesToBreak.ruleStack[ rulesToBreak.ruleStack.Count - 1 ] );
				Debug.Log ("REMOVED RULE ");
				rulesToBreak.RemoveLastRuleAdded();
			}
		}

		
		if( presetTiles != null )
		{
			Debug.Log ("CREATING IMPOSSIBLE BOARD");
			model.SetImpossible( true );
			impossiblesUsed ++;
			Debug.Log ("found valid preset tile order ");
			return presetTiles;
		}
		else
		{
			model.SetImpossible( false );
			presetTiles = CreatePartiallyFilledBoard( previousSubmission );
			Debug.Log ("impossible preset NOT found, creating possible board ");
		}

		return presetTiles;
	}

	string GetBestImpossiblePresetTileOrder( List<Tile> tilesToOrder, RuleStack rulesToBreak, List<Rule> nonbreakingRules )
	{
		bool singleRuleBreak = rulesToBreak.ruleStack.Count == 1;
		string bestPresetOrder = null;
		//for each possible preset tile order of only 1 preset
		for( int position = 0; position < tilesToOrder.Count; position ++ )
		{
			for( int preset = 0; preset < tilesToOrder.Count; preset ++ )
			{
				string presetOrder = "";
				
				for( int presetPosition = 0; presetPosition < tilesToOrder.Count; presetPosition ++ )
				{
					if( presetPosition == position )
					{
						presetOrder += tilesToOrder[ preset ].name[ 0 ];
					}
					else
					{
						presetOrder += "n";
					}	
				}

				Debug.Log (presetOrder);
				// test to see if preset order is in trial rules correct submission
				if( !trialRules.WildCardKeyInDictionary( presetOrder, trialRules.correctSubmissions ))
				{
					Debug.Log ("PRESET NOT IN CORRECT SUBMISSIONS" );
					if( singleRuleBreak )
					{
						Debug.Log ( "SINGLE RULE BROKEN " );
						return presetOrder;
					}
					//test to see if preset is a possible correct answer each concerned breakable rule
					bool passBreakableRulesTest = true;
					for( int i = 0; i < rulesToBreak.ruleStack.Count; i ++ )
					{
						if( !rulesToBreak.ruleStack[ i ].WildCardKeyInDictionary( presetOrder, rulesToBreak.ruleStack[ i ].correctSubmissions ))
						{
							Debug.Log ( rulesToBreak.ruleStack[ i ].verbal );
							Debug.Log ( " PRESET BREAKS RULE TO BREAK " );
							passBreakableRulesTest = false;
							break;
						}
					}
					
					if( passBreakableRulesTest )
					{
						Debug.Log ( "PRESET DOES NOT BREAK INDIVIDUAL RULE ");
						bool passOtherRulesTest = true;
						//test to see if preset is impossible for non-concerned rules in trial rules
						for( int nonbreakingRule = 0; nonbreakingRule < nonbreakingRules.Count; nonbreakingRule ++ )
						{
							if( !nonbreakingRules[ nonbreakingRule ].WildCardKeyInDictionary( presetOrder, nonbreakingRules[ nonbreakingRule ].correctSubmissions ))
							{
								Debug.Log ( "PRESET NOT IN OTHER RULE'S POSSIBLE CORRECTS ");
								passOtherRulesTest = false;
								break;
							}
						}
						
						if( passOtherRulesTest )
						{
							Debug.Log ( "SUCCESSFUL MULTI-RULE PRESET BREAK" );
							return presetOrder;
						}
					}
				}

			}
		}
		
		return bestPresetOrder;
	}




	RuleStack CreateRuleStackFromRandomRulesInCurrentTrial( int numberOfRulesToCombine )  //nothing here ensures that rules will share common impossibilities
	{
		RuleStack combinedRules = new RuleStack ();
		Debug.Log ("rules to combine : " + numberOfRulesToCombine + ", trial rules : " + trialRules.ruleStack.Count);
		for(int i = 0; i < Mathf.Min ( numberOfRulesToCombine, trialRules.ruleStack.Count ); i ++ )
		{
			//get random rule and add to stack
			combinedRules.ConstructVerbal();
//			Debug.Log (combinedRules.verbal);
			int random = Random.Range( 0, trialRules.ruleStack.Count ); 
			Debug.Log ("random rule index : " + random );
			//whle this rule is already in stack
			while( combinedRules.RuleInStack( trialRules.ruleStack[ random ] ))
			{
				//get a new rule
				random = Random.Range( 0, trialRules.ruleStack.Count );
			}
			
			combinedRules.AddRule ( trialRules.ruleStack[ random ] );
		}
		Debug.Log ("rules in rulestack : " + combinedRules.ruleStack.Count);
		return combinedRules;

	}



	List<Tile> GetRandomValueFromDictionary( Dictionary<string, List<Tile>> submissionDict )  //move to Rule class
	{
		List<string> keys = new List < string > ( submissionDict.Keys );
		List<Tile> randomValue = submissionDict[ keys[ Random.Range (0, keys.Count - 1 ) ] ];
		return randomValue;
	}
	

	string GetImpossiblePresetTileOrder( List<Tile> impossibleOrder )
	{
		for( int tile = 0; tile < impossibleOrder.Count; tile ++ )
		{
			string testKey = "";

			//create testy string with only one preset tile
			for (int i = 0; i < impossibleOrder.Count; i ++ )
			{
				if( i == tile )
				{
					testKey += impossibleOrder[ tile ].name[ 0 ];
				}
				else
				{
					testKey += "n";
				}
			}

			//if test key match not found in possible trial rule submissions, but is in each indivi
			if(!trialRules.WildCardKeyInDictionary( testKey, trialRules.correctSubmissions ))
			{
				return testKey;
			}

		}

		return null;
	}



	public void UpdateLevelingStats( int currentLevel )
	{
		ResetStats ();

//		maxRelativePosRules = 1;
//		tilesCount = 3;
//		maxAbsPosRules = 1;
//		usingEitherOr = true;
//		maxAdjacencyRules = 1;

		if (currentLevel == 0) 
		{
			maxRelativePosRules = 1;
			tilesCount = 3;
			maxRulesToSetNewProblem = 1;

		}
		else if( currentLevel == 1 )
		{
//			maxRuleDifficulty = 10;
			maxRelativePosRules = 1;
			maxAbsPosRules = 1;
			tilesCount = 3;
			chanceOfImpossible = 75;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 1;
		}
		else if( currentLevel == 2 )
		{
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			tilesCount = 4;
			chanceOfImpossible = 50;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 2;
			usingEitherOr = true;
		}
		else if( currentLevel == 3 )
		{
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			tilesCount = 4;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 2;
			usingEitherOr = true;
		}
		else if( currentLevel == 4 )
		{
			maxRelativePosRules = 1;
			maxConditionals = 1;
			tilesCount = 4;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 2;
			usingEitherOr = true;
		}
		else if( currentLevel == 5 )
		{
			maxRelativePosRules = 1;
			maxConditionals = 1;
			maxAdjacencyRules = 1;
			tilesCount = 4;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 2;
			usingEitherOr = true;
		}
		else if( currentLevel == 6 )
		{
			maxRelativePosRules = 1;
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 2;
			usingEitherOr = true;
		}
		else if( currentLevel == 6 )
		{
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			maxConditionals = 1;
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 2;
			usingEitherOr = true;
		}
		else if( currentLevel == 7 )
		{
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			maxConditionals = 1;
			tilesCount = 5;
			chanceOfImpossible = 30;
			maxImpossiblePerTrial = 1;
			maxRulesToSetNewProblem = 3;
			usingEitherOr = true;
		}

//		maxRelativePosRules = 1;
//		maxAdjacencyRules = 1;
//		tilesCount = 4;
//		chanceOfImpossible = 100;
//		maxImpossiblePerTrial = 1;
//		maxRulesToSetNewProblem = 2;
//		usingEitherOr = true;

		maxRelativePosRules = 1;
		maxAdjacencyRules = 1;
		maxConditionals = 1;
		tilesCount = 5;
		chanceOfImpossible = 30;
		maxImpossiblePerTrial = 1;
		maxRulesToSetNewProblem = 3;
		usingEitherOr = true;
		
	}
	
	void ResetStats()
	{
		maxAbsPosRules = 0;
		maxRelativePosRules = 0;
		maxAdjacencyRules = 0;
		maxConditionals = 0;
		chanceOfImpossible = 0;
		maxImpossiblePerTrial = 0;
		maxRulesToSetNewProblem = 0;
		usingEitherOr = false;

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
		int random = Random.Range (0, 100);

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
