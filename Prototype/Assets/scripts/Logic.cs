using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Logic : MonoBehaviour {

//	public List<Tile> tilesToOrder;
//	public List<TileHolder> holders;
	public List<Rule> trialRules;

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

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public string CreateRules( List<Tile> tiles, List<TileHolder> holders )
	{
		string rules = "";
		int difficultyPointsSpend = 0;
		impossiblesUsed = 0;
		trialRules = new List<Rule> ();

		List<Tile> shuffledTileOrder = ShuffleList ( tiles );

		int totalRules = maxAbsPosRules + maxRelativePosRules + maxAdjacencyRules + maxConditionals;
		Debug.Log ("total rules : " + totalRules);

		for( int abs = 0; abs < maxAbsPosRules; abs ++ )
		{
			//create absolute position rule
			int absolutePosition = Random.Range ( 0, holders.Count );
			TileHolder absPosHolder = holders[ absolutePosition ] ;
			Tile absPosTile = shuffledTileOrder[ 0 + abs ];
			Tile absPosTile2 = shuffledTileOrder[ 1 + abs ];
			
			Rule absPositionRule = new AbsolutePositionRule( 0, absPosTile, absPosTile2, absPosHolder ); 
			rules += "\n" + absPositionRule.ConstructVerbal();
			
			trialRules.Add (absPositionRule);
		}

		for( int relatives = 0; relatives < maxRelativePosRules; relatives ++ )
		{

		}

		return rules;
	}

	public void CreateNewProblem( List<Tile> previousSubmission, List<Tile> tiles )
	{
		bool createImpossible;

		if( impossiblesUsed < maxImpossiblePerTrial )
		{
			createImpossible = HappenedByChance( chanceOfImpossible );
		}

//		if( createImpossible )
//		{
//			impossiblesUsed ++;
			//get impossible order
//		}
	}

	bool IsRuleStackPossible( List<Rule> rules )
	{

		//for each possible submission in first rule
		foreach( KeyValuePair<string, List<Tile>> pair in rules[ 0 ].correctSubmissions )
		{ 
			//for each other rule
			for(int rule = 1; rule < rules.Count; rule ++ )
				//if a possible submission is not possible in another rule
			{
				if( !rules[ rule ].correctSubmissions.ContainsKey( pair.Key ))
				{
					break;
				}
			}
			return true;
		}
		return false;

	}

	bool SubmissionKeyNameMatch( string key1, string key2 ) //assumes keys are of equal length
	{
		List<char> usedChars = new List<char> ();
		for( int charIndex = 0; charIndex < key1.Length; charIndex ++ )
		{
			if( key1[ charIndex ] == key2[ charIndex ] )
			{
				continue;
			}
			//else if one char is n
			else if( key1[charIndex] == 'n' )
	        {
				if( usedChars.Contains( key2[charIndex] ))
			   	{
					return false;
				}
				usedChars.Add ( key2[charIndex] );
			}
			else if(key2[charIndex] == 'n' )
			{
				if( usedChars.Contains( key1[charIndex] ))
				{
					return false;
				}
				usedChars.Add ( key1[charIndex] );
			}
			else
			{
				return false;
			}
		}

		return true;
	}

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
		}
		else if( currentLevel == 2 )
		{
			maxRelativePosRules = 1;
			maxAdjacencyRules = 1;
			tilesCount = 4;
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
