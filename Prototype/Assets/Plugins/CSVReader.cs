using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 


public class CSVReader : MonoBehaviour 
{
	public TextAsset csvFile; 
//	public Logic logic;
	public List< List<int> > levelingInfo;

	public void Awake()
	{
		string[,] grid = SplitCsvGrid(csvFile.text);
//		Debug.Log("size = " + (1+ grid.GetUpperBound(0)) + "," + (1 + grid.GetUpperBound(1))); 
//		
//		DebugOutputGrid(grid); 
		levelingInfo = OutputLevelInfo (grid); 
	}



	// outputs the content of a 2D array, useful for checking the importer
	static public void DebugOutputGrid(string[,] grid)
	{
		string textOutput = ""; 
		
		for (int y = 0; y < grid.GetUpperBound(1); y++) {	
			for (int x = 0; x < grid.GetUpperBound(0); x++) {
				//				Debug.Log ("y : " + y);
				//				Debug.Log ("x : " + x);
				textOutput += grid[x,y]; 
				textOutput += "|";
				//				Debug.Log(grid[x ,y]);
			}
			textOutput += "\n"; 
		}
		Debug.Log(textOutput);
	}

	static public List< List<int> > OutputLevelInfo(string[,] grid)
	{
		List< List<int> > levelingInfo = new List<List<int>> ();

		
		for (int y = 0; y < grid.GetUpperBound(1); y++) 
		{	
			List< int > singleLevelInfo = new List<int>();

			for (int x = 0; x < grid.GetUpperBound(0); x++) 
			{
				//				Debug.Log ("y : " + y);
				//				Debug.Log ("x : " + x);
				int value = int.Parse( grid[x,y] );
//				Debug.Log ( value );
				singleLevelInfo.Add ( value );
				//				Debug.Log(grid[x ,y]);
			}
			levelingInfo.Add ( singleLevelInfo );
		}

		return levelingInfo;
	}

	
	// splits a CSV file into a 2D string array
	static public string[,] SplitCsvGrid(string csvText)
	{
		string[] lines = csvText.Split("\n"[0]); 
		// finds the max width of row
		int width = 0; 
		for (int i = 0; i < lines.Length; i++)
		{
			string[] row = SplitCsvLine( lines[i] ); 
			width = Mathf.Max(width, row.Length);
			//			Debug.Log (width);
			//			Debug.Log (lines.Length);
		}
		
		// creates new 2D string grid to output to
		string[,] outputGrid = new string[width + 1, lines.Length + 1]; 
		for (int y = 0; y < lines.Length; y++)
		{
			string[] row = SplitCsvLine( lines[y] ); 
			for (int x = 0; x < row.Length; x++) 
			{
				outputGrid[x,y] = row[x]; 
				
				// This line was to replace "" with " in my output. 
				// Include or edit it as you wish.
				outputGrid[x,y] = outputGrid[x,y].Replace("\"\"", "\"");
			}
		}
		
		return outputGrid; 
	}
	
	// splits a CSV row 
	static public string[] SplitCsvLine(string line)
	{
		return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line,
		                                                                                                    @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)", 
		                                                                                                    System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
		        select m.Groups[1].Value).ToArray();
	}
}