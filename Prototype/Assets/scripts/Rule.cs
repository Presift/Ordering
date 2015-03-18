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
	public List<Tile> impossibleOrders;
	public Dictionary<string, List<Tile>> correctSubmissions;




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


	public virtual void SetCorrectSubmissions( List<Tile> tilesToOrder )
	{

	}

	public virtual void SetImpossibleOrders( List<Tile> tilesToOrder )
	{

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


	public override void SetCorrectSubmissions( List<Tile> tilesToOrder )
	{
		correctSubmissions = new Dictionary<string,List<Tile> > ();

		if( ruleType == 0 ) //before rule
		{
			for(int tile1Pos = 0; tile1Pos < ( tilesToOrder.Count - 1 ); tile1Pos ++ )
			{
				for( int tile2Pos = ( tile1Pos + 1 ); tile2Pos < tilesToOrder.Count; tile2Pos ++ )
				{
					string keyName = "";
					List<Tile> correctSubmission = new List<Tile>();
					for( int i = 0; i < tilesToOrder.Count; i ++ )
					{

						if( i == tile1Pos )
						{
							keyName += tile1.name[ 0 ];
							correctSubmission.Add (tile1);
						}
						else if( i == tile2Pos )
						{
							keyName += tile2.name[ 0 ];
							correctSubmission.Add (tile2);
						}
						else
						{
							keyName += "n";
							correctSubmission.Add ( null );
						}

					}
					correctSubmissions.Add (keyName, correctSubmission );
				}
			}
		}
		else
		{
			for(int tile1Pos = 1; tile1Pos < ( tilesToOrder.Count ); tile1Pos ++ )
			{
				for( int tile2Pos = ( tile1Pos - 1 ); tile2Pos >= 0; tile2Pos -- )
				{
					string keyName = "";
					List<Tile> correctSubmission = new List<Tile>();
					for( int i = 0; i < tilesToOrder.Count; i ++ )
					{
						
						if( i == tile1Pos )
						{
							keyName += tile1.name[ 0 ];
							correctSubmission.Add (tile1);
						}
						else if( i == tile2Pos )
						{
							keyName += tile2.name[ 0 ];
							correctSubmission.Add (tile2);
						}
						else
						{
							keyName += "n";
							correctSubmission.Add ( null );
						}
						
					}
					correctSubmissions.Add (keyName, correctSubmission );
				}
			}
		}
	}
	
	public override void SetImpossibleOrders( List<Tile> tilesToOrder )
	{
		
	}
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


	public override void SetCorrectSubmissions( List<Tile> tilesToOrder )
	{
		
	}
	
	public override void SetImpossibleOrders( List<Tile> tilesToOrder )
	{
		
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

//	override void CalculateDifficulty()
//	{
//		difficulty = 6 + ( ruleType * 2 );
//	}
	
	public override void SetCorrectSubmissions( List<Tile> tilesToOrder )
	{
		
	}
	
	public override void SetImpossibleOrders( List<Tile> tilesToOrder )
	{
		
	}
}

public class MultiRule : Rule 
{
	public Rule rule1;
	public Rule rule2;

	public MultiRule( Rule newRule1, Rule newRule2, int difficultyPoints )
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
	
//	void CalculateDifficulty()
//	{
//		difficulty = ( rule1.difficulty + rule2.difficulty ) * 2;
//	}
	
	public override void SetCorrectSubmissions( List<Tile> tilesToOrder )
	{
		
	}
	
	public override void SetImpossibleOrders( List<Tile> tilesToOrder )
	{
		
	}
}
