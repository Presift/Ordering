using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rule 
{
	
	public int ruleType;
	public Tile tile1;
	public Tile tile2;
	public int absolutePositionIndex;

	public string verbal;
	public string clause2OfConditional;
	public string clause1OfConditional;

	public int difficulty;
	public Dictionary<string, List<Tile>> correctSubmissions = new Dictionary<string, List<Tile>>();
	public Dictionary<string, List<Tile>> incorrectSubmissions = new Dictionary<string, List<Tile>>();
	public List<Tile> tilesUsedInRule = new List<Tile>();

	public float ruleIdentifier;
	
	protected Rule ()
	{
	}

	public virtual void SetRuleIndentifier()
	{

	}

	public void PrintEachDictionaryValue( Dictionary<string, List<Tile>> dict )
	{
		foreach( KeyValuePair<string, List<Tile>> pair in dict )
		{
			List<Tile> value = dict[ pair.Key ];
			PrintTileList( value );
		}
	}

	public void PrintTileList( List<Tile> tileList )
	{
		string listReadOut = "";
//		Debug.Log ("tile list count : " + tileList.Count);
		for (int i = 0; i < tileList.Count; i ++ )
		{
			listReadOut += tileList[ i ].name + " , ";
		}
//		Debug.Log ( listReadOut );
	}

	public virtual string GetNegationForInvertedConditional( int newPositionInConditional )
	{
		string inverted = "";
		return inverted;
	}

	public virtual string ConstructVerbal()
	{
		Debug.Log ("failed verbal");
		return "";
	}

	public virtual bool SubmissionFollowsRule( List<Tile> submission )
	{
		Debug.Log ("failure");
		return false;
	}

	public string GetKeyWithNoMatchesToKey( string testKey, Dictionary<string, List<Tile>> dict )
	{

		foreach( KeyValuePair<string, List<Tile>> pair in dict )
		{
			for( int charIndex = 0; charIndex < testKey.Length; charIndex ++ )
			{
				if( testKey[ charIndex ] == pair.Key[ charIndex ] )
				{
					break;
				}
			}
			Debug.Log ("found a key with no matches ");
			Debug.Log (pair.Key);
			return pair.Key;
		}

		Debug.Log ("no key found with 0 matches ");
		return testKey;
	}
	



	public bool WildCardKeyInDictionary( string testKey, Dictionary<string, List<Tile>> dict )
	{
		bool keyFoundInDict = false;
		
		//if test key match not found in possible trial rule submissions
		foreach( KeyValuePair<string, List<Tile>> pair in dict )
		{ 
			if( WildCardSubmissionKeyNameMatch( pair.Key, testKey ))
			{
				keyFoundInDict = true;
				break;
			}
		}

		return keyFoundInDict;
	}

	public int CountWildCardInDictionary( string testKey, Dictionary<string, List<Tile>> dict )
	{
		int appearanceInDict = 0;


		foreach( KeyValuePair<string, List<Tile>> pair in dict )
		{ 
			//if test key match found dict
			if( WildCardSubmissionKeyNameMatch( pair.Key, testKey ))
			{
				appearanceInDict ++;
			}
		}

		return appearanceInDict;
	}

	public bool WildCardSubmissionKeyNameMatch( string key1, string key2 ) //assumes keys are of equal length
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

	public void GetAllPossibleSubmissions( List<Tile> tilesInOrder, List<Tile> tilesInBank)  //called first with empty tilesToOrder and full List
	{
		if( tilesInBank.Count != 0 )
		{
			//continue permutating
			for( int i = 0; i < tilesInBank.Count; i ++ )
			{
				List<Tile> newPermutation = new List<Tile> ( tilesInOrder );
				newPermutation.Add ( tilesInBank[i] );

				List<Tile> newTileBank = new List<Tile>( tilesInBank );

				newTileBank.RemoveAt( i );

				GetAllPossibleSubmissions( newPermutation, newTileBank );  //combine this list of tiles with newTilePermuation list?

			}
		}
		else
		{
			PrintTileList ( tilesInOrder );
			TriagePossibleSubmission( tilesInOrder );
		}

	}

	public virtual void TriagePossibleSubmission( List<Tile> possibleSubmission )
	{
		string keyName = "";
		for( int i = 0; i < possibleSubmission.Count; i ++ )
		{
			keyName += possibleSubmission[ i ].name[ 0 ];
		}
		if( SubmissionFollowsRule( possibleSubmission ))
	   	{
//			Debug.Log ("CORRECT : ");
	

			correctSubmissions.Add ( keyName, possibleSubmission );
		}
		else 
		{
//			Debug.Log ("INCORRECT : ");
			incorrectSubmissions.Add ( keyName, possibleSubmission );
		}

//		PrintTileList (possibleSubmission);
	}
	
}


public class RelativePositionRule : Rule 
{

	public RelativePositionRule( int newRuleType, Tile newTile1, Tile newTile2, List<Tile> tilesInBank)
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
		List<Tile> tilesInOrder = new List<Tile> ();
		GetAllPossibleSubmissions ( tilesInOrder, tilesInBank);
		tilesUsedInRule.Add (tile1);
		tilesUsedInRule.Add (tile2);
		SetRuleIndentifier ();
	}

	public override void SetRuleIndentifier()
	{
		ruleIdentifier = 6;
	}

	public override string ConstructVerbal()  // rule type of 0 is before, rule type of 1 is after
	{

		if (ruleType == 0) 
		{
			verbal =  tile1.name + " is before " + tile2.name + ".";
			clause2OfConditional = "then " + tile1.name + " must be before " + tile2.name + ".";
			clause1OfConditional = "If " + tile1.name  + " is before " + tile2.name + ", ";

			return verbal;	
		}
			
		verbal =  tile1.name + " is after " + tile2.name + ".";
		clause2OfConditional = "then " + tile1.name + " must be after " + tile2.name + ".";
		clause1OfConditional = "If " + tile1.name +  " is after " + tile2.name + ", ";
		return verbal;
	}

	public override string GetNegationForInvertedConditional( int newPositionInConditional )
	{
		string invertedConditionalPart;

		if( newPositionInConditional == 1 )
		{
			if (ruleType == 0) 
			{
				invertedConditionalPart = "If " + tile1.name  + " is after " + tile2.name + ", ";
			}
			else
			{
				invertedConditionalPart = "If " + tile1.name + " is before " + tile2.name + ", ";
			}
		}
		else
		{
			if (ruleType == 0) 
			{
				invertedConditionalPart = "then " + tile1.name  + " is after " + tile2.name + ".";
			}
			else
			{
				invertedConditionalPart = "then " + tile1.name + " is before " + tile2.name + ".";
			}
		}

		return invertedConditionalPart;
	}
	

	public override bool SubmissionFollowsRule( List<Tile> submission )
	{
		//get positions of tiles
		int tile1Index = 0;
		int tile2Index = 0;
		
		for( int i = 0; i < submission.Count; i ++ )
		{
			if( submission[i] == tile1 )
			{
				tile1Index = i;
			}
			else if(submission[i] == tile2 )
			{
				tile2Index = i;
			}
		}

		switch (ruleType) 
		{
		case 0:
			return ( tile1Index < tile2Index );

		case 1:
			return ( tile1Index > tile2Index );

		default:
			Debug.Log ("not a valid rule type");
			return true;
		}
	}

}



public class AdjacencyRule : Rule 
{

	public AdjacencyRule( int newRuleType, Tile newTile1, Tile newTile2, List<Tile> tilesInBank  )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
		List<Tile> tilesInOrder = new List<Tile> ();
		GetAllPossibleSubmissions ( tilesInOrder, tilesInBank);
		tilesUsedInRule.Add (tile1);
		tilesUsedInRule.Add (tile2);
		SetRuleIndentifier ();
	}

	public override void SetRuleIndentifier()
	{
		if( ruleType == 0 )
		{
			ruleIdentifier = 4;
		}
		else
		{
			ruleIdentifier = 5;
		}
	}

	public override string ConstructVerbal() // 0 is adjacent, 1 is NOT adjacent
	{

		if( ruleType == 0 )
		{
			verbal =  tile1.name + " is next to " + tile2.name + ".";
			clause2OfConditional = "then " + tile1.name + " must be next to " + tile2.name + ".";
			clause1OfConditional = "If " + tile1.name +  " is next to " + tile2.name + ", ";
			return verbal;
		}
		else
		{
			verbal =  tile1.name + " is NOT next to " + tile2.name + ".";
			clause2OfConditional = "then " + tile1.name + " CANNOT be next to " + tile2.name + ".";
			clause1OfConditional = "If " + tile1.name +  " is NOT next to " + tile2.name + ", ";
			return verbal;
		}
	}

	public override string GetNegationForInvertedConditional( int newPositionInConditional )
	{
		string invertedConditionalPart;
		
		if( newPositionInConditional == 1 )
		{
			if (ruleType == 0) 
			{
				invertedConditionalPart = "If " + tile1.name  + " is NOT next to " + tile2.name + ", ";
			}
			else
			{
				invertedConditionalPart = "If " + tile1.name + " IS next to " + tile2.name + ", ";
			}
		}
		else
		{
			if (ruleType == 0) 
			{
				invertedConditionalPart = "then " + tile1.name  + " is NOT next to " + tile2.name + ".";
			}
			else
			{
				invertedConditionalPart = "then " + tile1.name + " IS next to " + tile2.name + ".";
			}
		}
		
		return invertedConditionalPart;
	}
	
	public override bool SubmissionFollowsRule( List<Tile> submission )
	{
		//get positions of tiles
		int tile1Index = 0;
		int tile2Index = 0;

		for( int i = 0; i < submission.Count; i ++ )
		{
			if( submission[i] == tile1 )
			{
				tile1Index = i;
			}
			else if(submission[i] == tile2 )
			{
				tile2Index = i;
			}
		}

		int distanceApart = Mathf.Abs (tile1Index - tile2Index); 

		switch (ruleType) 
		{
		case 0:

			return ( distanceApart == 1 );
		case 1:
			return ( distanceApart != 1 );
		default:
			Debug.Log ("not a valid rule type");
			return true;
		}
		
	}
	

}



public class AbsolutePositionRule : Rule 
{

	public AbsolutePositionRule( int newRuleType, Tile newTile1, int positionIndex, List<Tile> tilesInBank )
	{
		ruleType = newRuleType;

		tile1 = newTile1;
		absolutePositionIndex = positionIndex;
		List<Tile> tilesInOrder = new List<Tile> ();
//		Debug.Log ("tilesInBank 1 tile: " + tilesInBank.Count);
		GetAllPossibleSubmissions ( tilesInOrder, tilesInBank);
//		Debug.Log ("correctSubmissions : " + correctSubmissions.Count);
//		Debug.Log ("incorrect subs : " + incorrectSubmissions.Count);
		tilesUsedInRule.Add (tile1);
		SetRuleIndentifier();
	}

	public AbsolutePositionRule( int newRuleType, Tile newTile1, Tile newTile2, int positionIndex, List<Tile> tilesInBank )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
		absolutePositionIndex = positionIndex;
		List<Tile> tilesInOrder = new List<Tile> ();
//		Debug.Log ("tilesInBank 2 tiles : " + tilesInBank.Count);
		GetAllPossibleSubmissions ( tilesInOrder, tilesInBank );
//		Debug.Log ("correctSubmissions : " + correctSubmissions.Count);
//		Debug.Log ("incorrect subs : " + incorrectSubmissions.Count);
		tilesUsedInRule.Add (tile1);
		tilesUsedInRule.Add (tile2);
		SetRuleIndentifier();
	}

	public override void SetRuleIndentifier()
	{
		if( tile2 != null )
		{
			ruleIdentifier = 3;
		}
		
		if( ruleType == 0 )
		{
			ruleIdentifier = 1;
		}
		else
		{
			ruleIdentifier = 2;
		}
	}

	public override string ConstructVerbal() // 0 is in a spot, 1 is NOT in a spot
	{
		if( tile2 != null )
		{
			verbal = tile1.name + " or " + tile2.name + " is in position " + (absolutePositionIndex + 1 ) + ".";
			clause2OfConditional = "then " + tile1.name + " or " + tile2.name + " must be in position " + (absolutePositionIndex + 1 ) + ".";
			clause1OfConditional = "If " + tile1.name + " or " + tile2.name + " is in position " + (absolutePositionIndex + 1 ) + ", ";
			return verbal;
		}

		if( ruleType == 0 )
		{
			verbal =  tile1.name + " is in position " + (absolutePositionIndex + 1 ) + ".";
			clause2OfConditional = "then " + tile1.name +  " must be in position " + (absolutePositionIndex + 1 ) + ".";
			clause1OfConditional = "If " + tile1.name +  " is in position " + (absolutePositionIndex + 1 ) + ", ";
			return verbal;
		}
		else
		{
			verbal =  tile1.name + " is NOT in position " + (absolutePositionIndex + 1 ) + ".";
			clause2OfConditional = "then " + tile1.name +  " must NOT be in position " + (absolutePositionIndex + 1 ) + ".";
			clause1OfConditional = "If " + tile1.name +  " is NOT in position " + (absolutePositionIndex + 1 ) + ", ";
			return verbal;
		}
	}

	public override string GetNegationForInvertedConditional( int newPositionInConditional )
	{
		string invertedConditionalPart;
		
		if( newPositionInConditional == 1 )
		{
			if( tile2 != null )
			{
				invertedConditionalPart = "If " + tile1.name + " or " + tile2.name + " is NOT in position " + ( absolutePositionIndex + 1 ) + ",";
			}
			else if (ruleType == 0) 
			{
				invertedConditionalPart = "If " + tile1.name  + " is NOT in position " + ( absolutePositionIndex + 1 ) + ", ";
			}
			else
			{
				invertedConditionalPart = "If " + tile1.name + " IS in position " + ( absolutePositionIndex + 1 ) + ", ";
			}
		}
		else
		{
			if( tile2 != null )
			{
				invertedConditionalPart = "then " + tile1.name + " or " + tile2.name + " is NOT in position " + ( absolutePositionIndex + 1 ) + ".";
			}
			else if (ruleType == 0) 
			{
				invertedConditionalPart = "then " + tile1.name  + " is NOT in position " + ( absolutePositionIndex + 1 ) + ".";
			}
			else
			{
				invertedConditionalPart = "then " + tile1.name + " IS in position " + ( absolutePositionIndex + 1 ) + ".";
			}
		}
		
		return invertedConditionalPart;
	}
	
	public override bool SubmissionFollowsRule( List<Tile> submission )
	{
		if (tile2 != null) 
		{
			return ( submission[ absolutePositionIndex ] == tile1 ) || (submission[ absolutePositionIndex ] == tile2 );

		}

		switch (ruleType) 
		{
		case 0:
			return submission[ absolutePositionIndex ] == tile1; 
		case 1:
			return submission[ absolutePositionIndex ] != tile1;
		default:
			Debug.Log ("not a valid rule type");
			return true;
		}
		
	}

}



public class RuleStack: Rule
{
	public List<Rule> ruleStack;
//	public Dictionary<string, List<Tile>> sharedImpossibleSubmissions = new Dictionary<string, List<Tile>> ();
	
	
	public RuleStack()
	{
		ruleStack = new List<Rule> ();
	}

	public RuleStack( Rule newRule )
	{
		ruleStack = new List<Rule> ();

		this.AddRule (newRule);
		Debug.Log ("rule added ");
	}

	public void AddRule( Rule newRule )
	{
		if (ruleStack.Count == 0) 
		{
			correctSubmissions = new Dictionary<string, List<Tile>> ( newRule.correctSubmissions );
//			sharedImpossibleSubmissions = new Dictionary<string, List<Tile>> ( newRule.incorrectSubmissions );
			incorrectSubmissions = new Dictionary<string, List<Tile>> ( newRule.incorrectSubmissions );
		}
		else
		{
			UpdateCorrectAndIncorrectSubmissions( GetCorrectSubmissionsNotShared( newRule ));
//			RemoveCorrectSubmissionsNotShared( newRule );
//			RemoveIncorrectSubmissionsNotShared( newRule );
		}
		ruleStack.Add (newRule);
	}

	public List<Rule> GetRulesInStackNotInList( List <Rule > rules )
	{
		List<Rule> rulesNotInList = new List<Rule> ();

		for( int i = 0; i < ruleStack.Count; i ++ )
		{
			if( !rules.Contains( ruleStack[ i ] ))
			{
				rulesNotInList.Add( ruleStack[ i ]);
			}
		}

		return rulesNotInList;
	}

//	public void RemoveLastRuleAdded()
//	{
//		Rule ruleToRemove = ruleStack [ruleStack.Count - 1];
//		ruleStack.Remove (ruleToRemove);
//
//		if( ruleStack.Count == 2 )
//		{
//			correctSubmissions = ruleStack[ 0 ].correctSubmissions;
//			sharedImpossibleSubmissions = ruleStack[ 0 ].incorrectSubmissions;
//		}
//		else
//		{
//			List< Rule > rulesToReAdd = new List<Rule>( ruleStack );
//			ruleStack = new List<Rule>();
//
//			for( int i = 0; i < rulesToReAdd.Count; i ++ )
//			{
//				AddRule( rulesToReAdd[ i ] );
//			}
//		}
//
//	}


	public bool RuleInStack( Rule newRule )
	{
		if( ruleStack.Count == 0 )
		{
			return false;
		}
		else
		{
			for( int i = 0; i < ruleStack.Count; i ++ )
			{
				if( ruleStack[i] == newRule )
				{
					return true; 
				}
			}
			return false;
		}
	}

	public List< string > GetCorrectSubmissionsNotShared( Rule newRule )
	{
		List<string> keysToRemove = new List<string> ();

		foreach( KeyValuePair<string, List<Tile>> pair in correctSubmissions )
		{ 
			string submission1 = pair.Key;
			if( !newRule.correctSubmissions.ContainsKey( submission1))
			{
				keysToRemove.Add ( submission1 );
			}
		}

//		for( int i = 0; i < keysToRemove.Count; i ++ )
//		{
//			correctSubmissions.Remove( keysToRemove[ i ] );
//		}

		return keysToRemove;
	}

	public void UpdateCorrectAndIncorrectSubmissions( List<string> keysFromNewRuleNotInCurrentCorrect )
	{
		for( int i = 0; i < keysFromNewRuleNotInCurrentCorrect.Count; i ++ )
		{
			incorrectSubmissions.Add ( keysFromNewRuleNotInCurrentCorrect[ i ], correctSubmissions[ keysFromNewRuleNotInCurrentCorrect[ i ] ] );
			correctSubmissions.Remove( keysFromNewRuleNotInCurrentCorrect[ i ] );
		}
	}

//	public void RemoveIncorrectSubmissionsNotShared( Rule newRule )
//	{
//		List<string> keysToRemove = new List<string> ();
//
//		foreach( KeyValuePair<string, List<Tile>> pair in sharedImpossibleSubmissions )
//		{ 
//			string submission1 = pair.Key;
//			if( !newRule.incorrectSubmissions.ContainsKey( submission1))
//			{
//				keysToRemove.Add ( submission1 );
//			}
//		}
//		for( int i = 0; i < keysToRemove.Count; i ++ )
//		{
//			sharedImpossibleSubmissions.Remove( keysToRemove[ i ] );
//		}
//	}

	public bool SharesImpossibleRules( Rule newRule )
	{
		if(ruleStack.Count == 0 )
		{
			return true;
		}
		else
		{
			foreach( KeyValuePair<string, List<Tile>> pair in newRule.incorrectSubmissions )
			{ 
				string submission1 = pair.Key;
				
				if( incorrectSubmissions.ContainsKey( submission1))
				{
					return true;
				}
			}
			return false;
		}
	}
	

	public bool RuleConflictsWithRuleStack( Rule newRule, int requiredSharedPossibleSubmissions )
	{
		if( ruleStack.Count == 0 )
		{
			if ( newRule.correctSubmissions.Count > requiredSharedPossibleSubmissions && newRule.incorrectSubmissions.Count > 0 )
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		for( int i = 0; i < ruleStack.Count; i ++ )
		{
			//if rule is conditional
			if( ruleStack[ i ] is Conditional )
			{
				Conditional conditionalRule = ( Conditional )ruleStack[ i ];
				bool newRuleSatisfiesConditional1 = true;  //always satisfies clause 1 of conditional
				bool newRuleNegatesConditional1 = true;		//always breaks clause 1 of conditional

				//if newRule always satisfies 1st clause in conditional  or newRule always contradicts 1st clause of conditional
				foreach( KeyValuePair<string, List<Tile>> pair in newRule.correctSubmissions )
				{
					if( !conditionalRule.rule1.SubmissionFollowsRule( pair.Value ))
					{
						newRuleSatisfiesConditional1 = false;
					}
					if( conditionalRule.rule1.SubmissionFollowsRule( pair.Value ))
					{
						newRuleNegatesConditional1 = false;
					}
				}

				if( newRuleNegatesConditional1 || newRuleSatisfiesConditional1 )
				{
					return true;
				}
			}
		}

		int sharedSubmissions = 0;

		foreach( KeyValuePair<string, List<Tile>> pair in correctSubmissions )
		{ 
			string submission1 = pair.Key;

			if( newRule.correctSubmissions.ContainsKey( submission1))
			{
				sharedSubmissions ++;
			}

			if( sharedSubmissions == requiredSharedPossibleSubmissions )
			{
				return false;
			}
		}
		Debug.Log ("rules conflict, rule not added");
		return true;
	}

	public override string ConstructVerbal() // 0 is in a spot, 1 is NOT in a spot
	{
		verbal = "";
		for(int i = 0; i < ruleStack.Count; i ++ )
		{
			verbal += ruleStack[ i ].verbal + "\n";
		}
		return verbal;
	}

	public string ConstructRandomOrderedVerbal() // 0 is in a spot, 1 is NOT in a spot
	{
		ShuffleRules ();

		verbal = "";

		for(int i = 0; i < ruleStack.Count; i ++ )
		{
			verbal += ruleStack[ i ].verbal + "\n\n";
		}

		return verbal;
	}

	public string ConstructVerbalWithInvertedConditionals()
	{
		string rulesWithInvertedConditionals = "";

		for(int i = 0; i < ruleStack.Count; i ++ )
		{
			rulesWithInvertedConditionals += ruleStack[ i ].verbal + "\n";

			if( ruleStack[ i ] is Conditional )
			{
				Conditional rule = ruleStack[ i ] as Conditional;

				string invertedConditional = rule.ConstructInvertedConditional();

				rulesWithInvertedConditionals += "THIS ALSO MEANS : " + "\n";
				rulesWithInvertedConditionals += invertedConditional +  "\n\n";
			}

		}
		
		return rulesWithInvertedConditionals;
	}

	public void ShuffleRules()
	{
		for (int i = 0; i < ruleStack.Count; i++){
			int indexToSwap = Random.Range( i, ruleStack.Count );
			Rule oldValue = ruleStack[i];
			ruleStack[i] = ruleStack[ indexToSwap ];
			ruleStack[indexToSwap] = oldValue;
		}
	}
	
}

public class Conditional: Rule
{
	public Rule rule1;
	public Rule rule2;
	
	public Conditional( Rule newRule1, Rule newRule2, List<Tile> tilesInBank )
	{
		rule1 = newRule1;
		rule2 = newRule2;
		List<Tile> tilesInOrder = new List<Tile> ();
		GetAllPossibleSubmissions ( tilesInOrder, tilesInBank);

		tilesUsedInRule = rule1.tilesUsedInRule;
		SetRuleIndentifier ();

		for( int i = 0; i < newRule2.tilesUsedInRule.Count; i++ )
		{
			tilesUsedInRule.Add ( newRule2.tilesUsedInRule[ i ] );
		}

		ConstructVerbal ();
	}

	public override void SetRuleIndentifier()
	{
//		Debug.Log ("rule 1 identifier : " + rule1.ruleIdentifier + ", rule 2 identifer : " + rule2.ruleIdentifier);
		ruleIdentifier = rule1.ruleIdentifier + (rule2.ruleIdentifier / 10);
	}

	public bool IsValidRule()
	{

		if( rule1 is AbsolutePositionRule && rule2 is AbsolutePositionRule )
		{
			if( rule1.tile1 == rule2.tile1 )
			{
				return false;
			}
		}

		bool possibleToSatisfyBothClausesOfConditional = false;

		//if it is never possibel to satisfy both parts of conditional
		foreach( KeyValuePair<string, List<Tile>> pair in rule1.correctSubmissions )
		{
			string correctSubmission = pair.Key;
			if( rule2.correctSubmissions.ContainsKey( correctSubmission ))
			{
				possibleToSatisfyBothClausesOfConditional = true;
				break;
			}
		} 

		if( !possibleToSatisfyBothClausesOfConditional )
		{
			return false;
		}

		if( correctSubmissions.Count > 1 && rule1 != rule2 )
		{

			return true;
		}

		return false;
	}
	
	public override string ConstructVerbal()
	{
		verbal = rule1.clause1OfConditional + rule2.clause2OfConditional;
		return verbal;
	}

	public string ConstructInvertedConditional()
	{
		string inversion = rule2.GetNegationForInvertedConditional( 1 ) + rule1.GetNegationForInvertedConditional (2);
		Debug.Log (" INVERSION : " + inversion);
		return inversion;
	}
	
	public override bool SubmissionFollowsRule( List<Tile> submission )
	{
		if (rule1.SubmissionFollowsRule ( submission )) 
		{
			if( rule2.SubmissionFollowsRule( submission ))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		if( !rule2.SubmissionFollowsRule( submission ))
		{
			if(!rule1.SubmissionFollowsRule( submission ))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		return true;
		
	}
}


