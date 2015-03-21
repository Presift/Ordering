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
//		string rules = "";
		int difficultyPointsSpend = 0;
		impossiblesUsed = 0;
		trialRules = new RuleStack ();

		int totalRules = maxAbsPosRules + maxRelativePosRules + maxAdjacencyRules + maxConditionals;
		Debug.Log ("max rules : " + totalRules);

		for( int conditional = 0; conditional < maxConditionals; conditional ++ )
		{
			Conditional newConditional = CreateConditionalRule( holders, tiles );
			Debug.Log ( newConditional.ConstructVerbal());
			Debug.Log (newConditional.verbal );
			if( !trialRules.RuleConflictsWithRuleStack( newConditional ))
			{
				trialRules.AddRule( newConditional );
			}
		}
		for( int abs = 0; abs < maxAbsPosRules; abs ++ )
		{
			AbsolutePositionRule newAbs = CreateAbsoluteRule( holders, tiles );
			if( !trialRules.RuleConflictsWithRuleStack( newAbs ))
			{
				trialRules.AddRule( newAbs );
			}
		}

		for( int relatives = 0; relatives < maxRelativePosRules; relatives ++ )
		{
			RelativePositionRule newRel = CreateRelativeRule( tiles );
			if( !trialRules.RuleConflictsWithRuleStack(newRel))
			{
				trialRules.AddRule( newRel );
			}
		}

		for( int adjacency = 0; adjacency < maxAdjacencyRules; adjacency ++ )
		{
			AdjacencyRule newAdj = CreateAdjacencyRule( tiles );
			if( !trialRules.RuleConflictsWithRuleStack( newAdj ))
			{
				trialRules.AddRule( newAdj );
			}
		}

		trialRules.ConstructVerbal ();
		Debug.Log ("rule count : " + trialRules.ruleStack.Count);
		Debug.Log (" correct answers : " + trialRules.correctSubmissions.Count);
		return trialRules.verbal;
	}

	RelativePositionRule CreateRelativeRule( List<Tile> tilesToOrder )
	{

		//randomly determine before/after rule
		int order = Random.Range (0, 1);

		int relPosTile = Random.Range (0, tilesToOrder.Count);
		int relPosTile2 = Random.Range (0, tilesToOrder.Count);
		
		while (relPosTile == relPosTile2) 
		{
			relPosTile2 = Random.Range (0, tilesToOrder.Count);
		}
		
		RelativePositionRule relPositionRule = new RelativePositionRule( order, tilesToOrder[relPosTile], tilesToOrder[relPosTile2], tilesToOrder  ); 
		relPositionRule.ConstructVerbal();
		
		return relPositionRule;
	}

	AdjacencyRule CreateAdjacencyRule(  List<Tile> tilesToOrder )
	{
		int nextTo = Random.Range (0, 1);  //determins next to/not next to

		int relPosTile = Random.Range (0, tilesToOrder.Count);
		int relPosTile2 = Random.Range (0, tilesToOrder.Count);

		while (relPosTile == relPosTile2) 
		{
			relPosTile2 = Random.Range (0, tilesToOrder.Count);
		}
		
		AdjacencyRule adjRule = new AdjacencyRule( nextTo, tilesToOrder[relPosTile], tilesToOrder[relPosTile2], tilesToOrder  ); 
		adjRule.ConstructVerbal();
		
		return adjRule;
	}

	AbsolutePositionRule CreateAbsoluteRule( List<TileHolder> holders, List<Tile> tilesToOrder )
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
				int absPosTile2 = Random.Range (0, tilesToOrder.Count);
				
				while (absPosTile == absPosTile2) 
				{
					absPosTile2 = Random.Range (0, tilesToOrder.Count);
				}
				
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToOrder[absPosTile], tilesToOrder[absPosTile2], absolutePosition, tilesToOrder ); 
				absPositionRule.ConstructVerbal();
				
				return absPositionRule;
			}
			{
				AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToOrder[absPosTile], absolutePosition, tilesToOrder ); 
				absPositionRule.ConstructVerbal();
				
				return absPositionRule;
			}
				
		}
		else
		{
			AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToOrder[absPosTile], absolutePosition, tilesToOrder ); 
			absPositionRule.ConstructVerbal();
			
			return absPositionRule;
		}
	
	}

	Conditional CreateConditionalRule( List<TileHolder> holders, List<Tile> tilesToOrder )
	{
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
			rule1 = CreateAbsoluteRule( holders, tilesToOrder );
		}
		else if( random == 1 )
		{
			rule1 = CreateAdjacencyRule( tilesToOrder );
		}
		else
		{
			rule1 = CreateRelativeRule( tilesToOrder );
		}

		
		if( random2 == 0 )
		{
			rule2 = CreateAbsoluteRule( holders, tilesToOrder );
		}
		else if( random2 == 1 )
		{
			rule2 = CreateAdjacencyRule( tilesToOrder );
		}
		else
		{
			rule2 = CreateRelativeRule( tilesToOrder );
		}

		Conditional newConditional = new Conditional (rule1, rule2, tilesToOrder );
		bool validRule = newConditional.IsValidRule ();

		while( !validRule )
		{
			newConditional = CreateConditionalRule( holders, tilesToOrder );
			validRule = newConditional.IsValidRule();
		}

		return newConditional;
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
			presetTiles = AttemptToCreateImpossibleBoard( previousSubmission );
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

		//find a possible submission that does not match previous submission
		string newSubmission = trialRules.GetKeyWithNoMatchesToKey (oldSubmission, trialRules.correctSubmissions);

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

	string AttemptToCreateImpossibleBoard( List<Tile> previousSubmission )
	{
		Debug.Log ("ATTEMPTING TO CREATE IMPOSSIBLE");
		RuleStack rulesToBreak = CreateRuleStackFromRandomRulesInCurrentTrial( maxRulesToSetNewProblem );
		Debug.Log (" SHARED IMPOSSIBLE : " + rulesToBreak.sharedImpossibleSubmissions.Count);
		Debug.Log ("combined rules : " + rulesToBreak.ruleStack.Count);
//		Debug.Log ("impossibles : " + rulesToBreak.incorrectSubmissions.Count );
		string presetTiles = null;
		bool submissionsRemaining = true;
		List<Tile> impossibleOrder = null;
		
		while( submissionsRemaining && presetTiles == null )
		{
			//for each shared impossible
			foreach( KeyValuePair<string, List<Tile>> pair in rulesToBreak.sharedImpossibleSubmissions )
			{ 
				impossibleOrder = rulesToBreak.sharedImpossibleSubmissions[ pair.Key ];
				trialRules.PrintTileList( impossibleOrder );
//				Debug.Log ("impossible key : " + impossibleOrder );
				presetTiles = GetPresetTileOrder( impossibleOrder );
				Debug.Log (presetTiles);
				if(presetTiles != null )
				{
					break;
				}
			}
			
			submissionsRemaining = false;
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



	RuleStack CreateRuleStackFromRandomRulesInCurrentTrial( int numberOfRulesToCombine )  //nothing here ensures that rules will share common impossibilities
	{
		RuleStack combinedRules = new RuleStack ();
		Debug.Log ("rules to combine : " + numberOfRulesToCombine + ", trial rules : " + trialRules.ruleStack.Count);
		for(int i = 0; i < Mathf.Min ( numberOfRulesToCombine, trialRules.ruleStack.Count ); i ++ )
		{
			//get random rule and add to stack
			combinedRules.ConstructVerbal();
			Debug.Log (combinedRules.verbal);
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
	

	string GetPresetTileOrder( List<Tile> impossibleOrder )
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

			//if test key match not found in possible trial rule submissions
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
			maxRulesToSetNewProblem = 1;
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

		chanceOfImpossible = 100;
		maxImpossiblePerTrial = 1;
		maxRulesToSetNewProblem = 2;
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
