using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MetaData : MonoBehaviour {

	int problemNum;			
	int trialNum;		
	int level;
	int ruleCount;
	List< float > ruleTypes;
	int tileCount;


	float responseTime;			
	bool correctAnswer;				
	
	int presetCount;

	bool modusPonens;
	bool modusTollens;
	bool bAndNotA;
	int solutionsToBoardSetUp;

//	bool impossibleBoard;
	List<float> rulesCreatingImpossibleBoard;

	float timeAtStartOfProblem;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetTimeSinceProblemStart( float time )
	{
		timeAtStartOfProblem = time;
	}

	void CalculateResonseTime( float timeResponded )
	{
		responseTime = timeResponded - timeAtStartOfProblem;

	}

	public void SetStatsForTrial( int newLevel, int problem, int trial, int ruleCnt, List<float> newRuleTypes, int newTileCount )
	{
		level = newLevel;

		problemNum = problem;

		trialNum = trial;


		ruleCount = ruleCnt;

		ruleTypes = newRuleTypes;

		tileCount = newTileCount;
	}

	public void SetStatsOnBoardSetUp( int presets, bool ponens, bool tollens, bool bNotA, int solutionsToBoard, List<float> rulesToBreak, float timeAtStart )
	{
		presetCount = presets;

		modusPonens = ponens;

		modusTollens = tollens;

		bAndNotA = bNotA;

		solutionsToBoardSetUp = solutionsToBoard;

		rulesCreatingImpossibleBoard = rulesToBreak;

		timeAtStartOfProblem = timeAtStart;
	}

	public int SetStatsOnAnswer( bool correct, float timeResponded )
	{
		correctAnswer = correct;
		CalculateResonseTime (timeResponded);

		return ( int )responseTime;
	}

	public void ResetStats()
	{
		problemNum = 0;
		trialNum = 0;
		level = 0;
		ruleCount = 0;
		responseTime = 0;
		correctAnswer = false;
		ruleTypes = new List<float> ();
		tileCount = 0;
		presetCount = 0;
		modusPonens = false;
		modusTollens = false;
		bAndNotA = false;
		solutionsToBoardSetUp = 0;
		rulesCreatingImpossibleBoard = new List<float> ();
		timeAtStartOfProblem = 0;
	}

	string GetListOfFloatsAsString( List< float > floats )
	{
		string floatsAsString = "(";

		for ( int i = 0; i < floats.Count; i ++ )
		{
//			Debug.Log ( floats[i]);
			floatsAsString += floats[ i ].ToString();

			if( i < (floats.Count - 1 ))
			{
				floatsAsString +=  " + ";
			}
			else
			{
				floatsAsString += ")";
			}
		}

		return floatsAsString;
	}

	public void SaveStats()
	{
		string data = level.ToString ();
		data += "," + problemNum.ToString ();
		data += "," + trialNum.ToString ();
		data += "," + ruleCount.ToString ();
		data += "," + GetListOfFloatsAsString( ruleTypes );
		data += "," + tileCount.ToString ();
//		data += "," + responseTime.ToString();
//		data += "," + correctAnswer.ToString();
//		data += "," + ruleTypes.ToString();

//		data += "," + presetCount.ToString ();
//		data += "," +modusPonens.ToString();
//		data += "," + modusTollens.ToString();
//		data += "," + bAndNotA.ToString();
//		data += "," + solutionsToBoardSetUp.ToString();
//		data += "," + rulesCreatingImpossibleBoard.ToString();
//		data += "," + timeAtStartOfProblem.ToString();
		GameData.dataControl.SavePerformanceStats (data);
		
	}
}
