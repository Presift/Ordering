using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logic : MonoBehaviour {

	public Model model;
	public View view;

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

//		List<Tile> shuffledTileOrder = ShuffleList ( tiles );

		int totalRules = maxAbsPosRules + maxRelativePosRules + maxAdjacencyRules + maxConditionals;
		Debug.Log ("total rules : " + totalRules);

		Debug.Log (maxAbsPosRules);
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
			RelativePositionRule newRel = CreateRelativeRule( holders, tiles );
			if( !trialRules.RuleConflictsWithRuleStack(newRel))
			{
				trialRules.AddRule( newRel );
			}
		}

		trialRules.ConstructVerbal ();
		return trialRules.verbal;
	}

	RelativePositionRule CreateRelativeRule( List<TileHolder> holders, List<Tile> tilesToOrder )
	{

		//randomly determine before/after rule
		int order = Random.Range (0, 1);

		int relPosTile = Random.Range (0, tilesToOrder.Count);
		int relPosTile2 = Random.Range (0, tilesToOrder.Count);
		
		while (relPosTile == relPosTile2) 
		{
			relPosTile2 = Random.Range (0, tilesToOrder.Count);
		}
		
		RelativePositionRule relPositionRule = new RelativePositionRule( order, tilesToOrder[relPosTile], tilesToOrder[relPosTile2] ); 
		relPositionRule.ConstructVerbal();
		
		return relPositionRule;
	}

	AbsolutePositionRule CreateAbsoluteRule( List<TileHolder> holders, List<Tile> tilesToOrder )
	{

		//create absolute position rule
		int absolutePosition = Random.Range ( 0, holders.Count );
		TileHolder absPosHolder = holders[ absolutePosition ] ;

		int absPosTile = Random.Range (0, tilesToOrder.Count);
		int absPosTile2 = Random.Range (0, tilesToOrder.Count);

		while (absPosTile == absPosTile2) 
		{
			absPosTile2 = Random.Range (0, tilesToOrder.Count);
		}
		
		AbsolutePositionRule absPositionRule = new AbsolutePositionRule( 0, tilesToOrder[absPosTile], tilesToOrder[absPosTile2], absPosHolder ); 
		absPositionRule.ConstructVerbal();
		
		return absPositionRule;
	
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

		if( createImpossible )
		{
			presetTiles = AttemptToCreateImpossibleBoard( previousSubmission );
		}
		else
		{
			presetTiles = CreatePartiallyFilledBoard( previousSubmission );
		}

		return presetTiles;
	}

	string CreatePartiallyFilledBoard( List<Tile> previousSubmission )
	{
		//create  key string from previousSubmission
		string oldSubmission = "";
		for( int tile = 0; tile < previousSubmission.Count; tile ++ )
		{
			oldSubmission += previousSubmission[ tile ].name[ 0 ];
		}

		//find a possible submission that does not match previous submission
		string newSubmission = trialRules.GetKeyWithNoMatchesToKey (oldSubmission, trialRules.consolidatedCorrectSubmissions);

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

		return presetTiles;
	}

	string AttemptToCreateImpossibleBoard( List<Tile> previousSubmission )
	{
		RuleStack rulesToBreak = CreateRuleStackFromRandomRulesInCurrentTrial( maxRulesToSetNewProblem );
		string presetTiles = null;
		bool submissionsRemaining = true;
		List<Tile> impossibleOrder = null;
		
		while( submissionsRemaining && presetTiles == null )
		{
			//for each shared impossible
			foreach( KeyValuePair<string, List<Tile>> pair in rulesToBreak.sharedImpossibleSubmissions )
			{ 
				impossibleOrder = rulesToBreak.sharedImpossibleSubmissions[ pair.Key ];
				presetTiles = GetPresetTileOrder( impossibleOrder );
				
				if(presetTiles != null )
				{
					break;
				}
			}
			
			submissionsRemaining = false;
		}
		
		if( presetTiles != null )
		{
			model.SetImpossible( true );
			impossiblesUsed ++;
			Debug.Log ("found valid preset tile order ");
			return presetTiles;
		}
		else
		{
			presetTiles = CreatePartiallyFilledBoard( previousSubmission );
			Debug.Log ("impossible preset NOT found, creating possible board ");
		}

		return presetTiles;
	}



	RuleStack CreateRuleStackFromRandomRulesInCurrentTrial( int numberOfRulesToCombine )  //nothing here ensures that rules will share common impossibilities
	{
		RuleStack combinedRules = new RuleStack ();
		
		for(int i = 0; i < Mathf.Min ( numberOfRulesToCombine, trialRules.ruleStack.Count ); i ++ )
		{
			//get random rule and add to stack
			int random = Random.Range( 0, trialRules.consolidatedCorrectSubmissions.Count - 1 ); 

			//whle this rule is already in stack
			while( combinedRules.RuleInStack( trialRules.ruleStack[ random ] ))
			{
				//get a new rule
				random = Random.Range( 0, trialRules.consolidatedCorrectSubmissions.Count - 1 );
			}
			
			combinedRules.AddRule ( trialRules.ruleStack[ random ] );
		}
		
		return combinedRules;

	}

//	List<Tile> GetSharedImpossibleOrder()
//	{
//		List<Tile> impossibleOrder = null;
//		RuleStack rulesToCreateImpossibleBoard = new RuleStack ();
//
//		for(int i = 0; i < Mathf.Min ( maxRulesToSetNewProblem, trialRules.ruleStack.Count ); i ++ )
//		{
//			//get random rule and add to stack
//			int random = Random.Range( 0, trialRules.consolidatedCorrectSubmissions.Count - 1 ); 
//		
//			while( rulesToCreateImpossibleBoard.RuleInStack( trialRules.ruleStack[ random ] ))
//			{
//				random = Random.Range( 0, trialRules.consolidatedCorrectSubmissions.Count - 1 );
//			}
//
//			rulesToCreateImpossibleBoard.AddRule ( trialRules.ruleStack[ random ] );
//		}
//
//		//if at least 1 shared impossible order
//		if( rulesToCreateImpossibleBoard.sharedImpossibleSubmissions.Count > 0 )
//		{
//
//			impossibleOrder = GetRandomValueFromDictionary( rulesToCreateImpossibleBoard.sharedImpossibleSubmissions );
//		}
//
//		return impossibleOrder;
//
//	}
	

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
			if(!trialRules.WildCardKeyInDictionary( testKey, trialRules.consolidatedCorrectSubmissions ))
			{
				return testKey;
			}

		}

		return null;
	}


//	bool WildCardSubmissionKeyNameMatch( string key1, string key2 ) //assumes keys are of equal length
//	{
//		List<char> usedChars = new List<char> ();
//		for( int charIndex = 0; charIndex < key1.Length; charIndex ++ )
//		{
//			if( key1[ charIndex ] == key2[ charIndex ] )
//			{
//				continue;
//			}
//			//else if one char is n
//			else if( key1[charIndex] == 'n' )
//	        {
//				if( usedChars.Contains( key2[charIndex] ))
//			   	{
//					return false;
//				}
//				usedChars.Add ( key2[charIndex] );
//			}
//			else if(key2[charIndex] == 'n' )
//			{
//				if( usedChars.Contains( key1[charIndex] ))
//				{
//					return false;
//				}
//				usedChars.Add ( key1[charIndex] );
//			}
//			else
//			{
//				return false;
//			}
//		}
//
//		return true;
//	}

	public List<Tile> ShuffleList(List<Tile> data )
	{
		int size = data.Count;
		
		for (int i = 0; i < size; i++){
			int indexToSwap = Random.Range(i, size);
			Tile oldValue = data[i];
			data[i] = data[indexToSwap];
			data[indexToSwap] = oldValue;
		}
		return data;
	}


	public void UpdateLevelingStats( int currentLevel )
	{
		Debug.Log (currentLevel);
		ResetStats ();
		if (currentLevel == 0) 
		{
//			maxRuleDifficulty = 8;
			maxAbsPosRules = 1;
			tilesCount = 3;

		}
		else if( currentLevel == 1 )
		{
//			maxRuleDifficulty = 10;
			maxRelativePosRules = 1;
			tilesCount = 3;
			chanceOfImpossible = 100;
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
		}
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

	}

	bool HappenedByChance( int chance ) //out of 100
	{
		int random = Random.Range (0, 100);

		if( random > chance )
		{
			return true;
		}
		return false;
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
