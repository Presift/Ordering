using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rule 
{
	
	public int ruleType;
	public Tile tile1;
	public Tile tile2;
	public TileHolder holder;
	public string verbal;
	public int difficulty;
	public Dictionary<string, List<Tile>> correctSubmissions = new Dictionary<string, List<Tile>>();
	public Dictionary<string, List<Tile>> incorrectSubmissions = new Dictionary<string, List<Tile>>();




	public Rule( int newRuleType , Tile newTile1, Tile newTile2, int difficultyPoints )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
		difficulty = difficultyPoints;
	}

	public Rule( int newRuleType, Tile newTile1, TileHolder holderPosition, int difficultyPoints )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		holder = holderPosition;
		difficulty = difficultyPoints;
	}

	public Rule( int newRuleType, Tile newTile1, Tile newTile2, TileHolder newHolder )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
		holder = newHolder;
	}


	protected Rule ()
	{
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
//		int fewestMatches = testKey.Length;
//		string differentKey = testKey;

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
			return pair.Key;
		}

		Debug.Log ("not key found with 0 matches ");
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

	bool WildCardSubmissionKeyNameMatch( string key1, string key2 ) //assumes keys are of equal length
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

	public void GetAllPossibleSubmissions( List<Tile> tilesInOrder, List<Tile> tilesInBank )  //called first with empty tilesToOrder and full List
	{

		if( tilesInBank.Count != 0 )
		{
			//continue permutating
			for( int i = 0; i < tilesInBank.Count; i ++ )
			{
				List<Tile> newPermutation = tilesInOrder;
				newPermutation.Add ( tilesInBank[i] );
//				Debug.Log (tilesInBank[i].name + " added to tiles list ");
				List<Tile> newTileBank = new List<Tile>();

				for( int bankItem = 0; bankItem < tilesInBank.Count; bankItem ++ )
				{
//					Debug.Log ("bank item : " + bankItem);
					newTileBank.Add (tilesInBank[ bankItem ]);
				}

				newTileBank.RemoveAt( i );
//				Debug.Log ("new bank : " + newTileBank.Count + ", original bank : " + tilesInBank.Count);

				GetAllPossibleSubmissions( newPermutation, newTileBank );  //combine this list of tiles with newTilePermuation list?

			}
		}
		else
		{
//			possibleSubmissions.Add ( tilesInOrder );
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
			correctSubmissions.Add ( keyName, possibleSubmission );
		}
		else 
		{
			incorrectSubmissions.Add ( keyName, possibleSubmission );
		}
	}
	
}


public class RelativePositionRule : Rule 
{
	public RelativePositionRule( int newRuleType, Tile newTile1, Tile newTile2)
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
	}

	public override string ConstructVerbal()  // rule type of 0 is before, rule type of 1 is after
	{

		if (ruleType == 0) 
		{
			verbal =  tile1.name + " is before " + tile2.name + ".";
			return verbal;	
		}
			
		verbal =  tile1.name + " is after " + tile2.name + ".";
		return verbal;
	}
	
	public override bool SubmissionFollowsRule( List<Tile> submission )
	{
		switch (ruleType) 
		{
		case 0:
			if( tile1.currentHolder.spotNumber < tile2.currentHolder.spotNumber )
			{
				return true;
			}
			return false;
		case 1:
			if( tile1.currentHolder.spotNumber > tile2.currentHolder.spotNumber )
			{
				return true;
			}
			return false;
		default:
			Debug.Log ("not a valid rule type");
			return true;
		}
	}

//	public override void TriagePossibleSubmissions( List<List<Tile>> possibleSubmissions )
//	{
//		
//	}

//	public override void SetCorrectSubmissions( List<Tile> tilesToOrder )
//	{
//		correctSubmissions = new Dictionary<string,List<Tile> > ();
//
//		if( ruleType == 0 ) //before rule
//		{
//			for(int tile1Pos = 0; tile1Pos < ( tilesToOrder.Count - 1 ); tile1Pos ++ )
//			{
//				for( int tile2Pos = ( tile1Pos + 1 ); tile2Pos < tilesToOrder.Count; tile2Pos ++ )
//				{
//					string keyName = "";
//					List<Tile> correctSubmission = new List<Tile>();
//					for( int i = 0; i < tilesToOrder.Count; i ++ )
//					{
//
//						if( i == tile1Pos )
//						{
//							keyName += tile1.name[ 0 ];
//							correctSubmission.Add (tile1);
//						}
//						else if( i == tile2Pos )
//						{
//							keyName += tile2.name[ 0 ];
//							correctSubmission.Add (tile2);
//						}
//						else
//						{
//							keyName += "n";
//							correctSubmission.Add ( null );
//						}
//
//					}
//					correctSubmissions.Add (keyName, correctSubmission );
//				}
//			}
//		}
//		else
//		{
//			for(int tile1Pos = 1; tile1Pos < ( tilesToOrder.Count ); tile1Pos ++ )
//			{
//				for( int tile2Pos = ( tile1Pos - 1 ); tile2Pos >= 0; tile2Pos -- )
//				{
//					string keyName = "";
//					List<Tile> correctSubmission = new List<Tile>();
//					for( int i = 0; i < tilesToOrder.Count; i ++ )
//					{
//						
//						if( i == tile1Pos )
//						{
//							keyName += tile1.name[ 0 ];
//							correctSubmission.Add (tile1);
//						}
//						else if( i == tile2Pos )
//						{
//							keyName += tile2.name[ 0 ];
//							correctSubmission.Add (tile2);
//						}
//						else
//						{
//							keyName += "n";
//							correctSubmission.Add ( null );
//						}
//						
//					}
//					correctSubmissions.Add (keyName, correctSubmission );
//				}
//			}
//		}
//	}
	

}



public class AdjacencyRule : Rule 
{
	public AdjacencyRule( int newRuleType, Tile newTile1, Tile newTile2 )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
	}
	public override string ConstructVerbal() // 0 is adjacent, 1 is NOT adjacent
	{

		if( ruleType == 0 )
		{
			verbal =  tile1.name + " is adjacent to " + tile2.name + ".";
			return verbal;
		}
		else
		{
			verbal =  tile1.name + " is NOT adjacent to " + tile2.name + ".";
			return verbal;
		}
	}
	
	public override bool SubmissionFollowsRule( List<Tile> submission )
	{
		switch (ruleType) 
		{
		case 0:
			if( tile1.currentHolder.spotNumber == holder.spotNumber )
			{
				return true;
			}
			return false;
		case 1:
			if( tile1.currentHolder.spotNumber != holder.spotNumber )
			{
				return true;
			}
			return false;
		default:
			Debug.Log ("not a valid rule type");
			return true;
		}
		
	}
	

}



public class AbsolutePositionRule : Rule 
{

	public AbsolutePositionRule( int newRuleType, Tile newTile1, TileHolder newHolder )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		holder = newHolder;
	}

	public AbsolutePositionRule( int newRuleType, Tile newTile1, Tile newTile2, TileHolder newHolder )
	{
		ruleType = newRuleType;
		tile1 = newTile1;
		tile2 = newTile2;
		holder = newHolder;
	}

	public override string ConstructVerbal() // 0 is in a spot, 1 is NOT in a spot
	{
		if( tile2 != null )
		{
			verbal = tile1.name + " or " + tile2.name + " is in position " + holder.spotNumber + ".";
		}

		if( ruleType == 0 )
		{
			verbal =  tile1.name + " is in position " + holder.spotNumber + ".";
			return verbal;
		}
		else
		{
			verbal =  tile1.name + " is not in position " + holder.spotNumber + ".";
			return verbal;
		}
	}
	
	public override bool SubmissionFollowsRule( List<Tile> submission )
	{
		if (tile2 != null) 
		{
			return ( tile1.currentHolder.spotNumber == holder.spotNumber	|| tile2.currentHolder.spotNumber == holder.spotNumber );
		}

		switch (ruleType) 
		{
		case 0:
			if( tile1.currentHolder.spotNumber == holder.spotNumber )
			{
				return true;
			}
			return false;
		case 1:
			if( tile1.currentHolder.spotNumber != holder.spotNumber )
			{
				return true;
			}
			return false;
		default:
			Debug.Log ("not a valid rule type");
			return true;
		}
		
	}

}

//public class MultiRule : Rule 
//{
//	public Rule rule1;
//	public Rule rule2;
//
//	public MultiRule( Rule newRule1, Rule newRule2, int difficultyPoints )
//	{
//		rule1 = newRule1;
//		rule2 = newRule2;
//		difficulty = difficultyPoints;
//	}
//
//	public override string ConstructVerbal()
//	{
//		verbal = "If " + rule1.verbal + ", " + rule2.verbal;
//		return verbal;
//	}
//	
//	public override bool SubmissionFollowsRule( List<Tile> submission )
//	{
//		if (rule1.SubmissionFollowsRule ( submission )) 
//		{
//			if( rule2.SubmissionFollowsRule( submission ))
//			{
//				return true;
//			}
//			else
//			{
//				return false;
//			}
//		}
//
//		if( !rule2.SubmissionFollowsRule( submission ))
//		{
//			if(!rule1.SubmissionFollowsRule( submission ))
//			{
//				return true;
//			}
//			else
//			{
//				return false;
//			}
//		}
//
//		return true;
//		
//	}
//}

public class RuleStack: Rule
{
	public List<Rule> ruleStack;
	public Dictionary<string, List<Tile>> consolidatedCorrectSubmissions;
	public Dictionary<string, List<Tile>> sharedImpossibleSubmissions;
	
	
	public RuleStack()
	{
		ruleStack = new List<Rule> ();
		consolidatedCorrectSubmissions = new Dictionary<string, List<Tile>> ();
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
			consolidatedCorrectSubmissions = newRule.correctSubmissions;
			sharedImpossibleSubmissions = newRule.incorrectSubmissions;
		}
		else
		{
			RemoveCorrectSubmissionsNotShared( newRule );
			RemoveIncorrectSubmissionsNotShared( newRule );
		}
		ruleStack.Add (newRule);
	}

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

	public void RemoveCorrectSubmissionsNotShared( Rule newRule )
	{
		foreach( KeyValuePair<string, List<Tile>> pair in newRule.correctSubmissions )
		{ 
			string submission1 = pair.Key;
			if( !consolidatedCorrectSubmissions.ContainsKey( submission1))
			{
				consolidatedCorrectSubmissions.Remove( submission1 );
			}
		}
	}

	public void RemoveIncorrectSubmissionsNotShared( Rule newRule )
	{
		foreach( KeyValuePair<string, List<Tile>> pair in newRule.incorrectSubmissions )
		{ 
			string submission1 = pair.Key;
			if( !sharedImpossibleSubmissions.ContainsKey( submission1))
			{
				sharedImpossibleSubmissions.Remove( submission1 );
			}
		}
	}

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
				
				if( sharedImpossibleSubmissions.ContainsKey( submission1))
				{
					return true;
				}
			}
			return false;
		}
	}
	public bool RuleConflictsWithRuleStack( Rule newRule )
	{
		if( ruleStack.Count == 0 )
		{
			return false;
		}


		foreach( KeyValuePair<string, List<Tile>> pair in newRule.correctSubmissions )
		{ 
			string submission1 = pair.Key;

			if( consolidatedCorrectSubmissions.ContainsKey( submission1))
			{
				return false;
			}
		}
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
}

public class Conditional: Rule
{
	public Rule rule1;
	public Rule rule2;
	
	public Conditional( Rule newRule1, Rule newRule2, int difficultyPoints )
	{
		rule1 = newRule1;
		rule2 = newRule2;
		difficulty = difficultyPoints;
	}
	
	public override string ConstructVerbal()
	{
		verbal = "If " + rule1.verbal + ", " + rule2.verbal;
		return verbal;
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
