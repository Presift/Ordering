using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
//using System.IO.StreamWriter;
using System.Collections.Generic;


public class GameData : MonoBehaviour {
	
	public static GameData dataControl;
	
	//	private string dataFile = "/playerInfo.dat";
	private string levelingStats = "/levelingStats.csv";
	private string dataFile = "/playerInfo.csv";
	
	public float previousFinalLevel;
	public bool debugOn;
	public int consecutiveModusTollensIncorrect;
	public bool fitTestTaken;

//	public bool impossibleEnabled;
	
	void Awake(){
		if(dataControl == null)
		{
			DontDestroyOnLoad(gameObject);
			dataControl = this;
		}
		else if(dataControl != this)
		{
			Destroy(gameObject);
		}
		
	}
	
	void Start()
	{
//		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
	}



	public void Save()
	{

		StreamWriter file = new StreamWriter (GetFullPath (dataFile));
		
		PlayerData data = new PlayerData();
		data.previousFinalLevel = previousFinalLevel;
		data.debugOn = debugOn;
		data.consecutiveModusTollensIncorrect = consecutiveModusTollensIncorrect;

		file.WriteLine ( data.previousFinalLevel );
		file.WriteLine (data.debugOn);
		file.WriteLine (data.consecutiveModusTollensIncorrect);
		file.WriteLine (data.fitTestTaken);

		file.Close ();
		
	}
	
	public void SavePerformanceStats( string stats )
	{
//		string filePath = GetFullPath (levelingStats + currentStatsFile.ToString() + ".csv");
		string filePath = GetFullPath (levelingStats);
		System.IO.File.AppendAllText(filePath, "\n" + stats );
	}
	


	public void Load()
	{
		if(File.Exists (GetFullPath( dataFile )))
		{
			string filePath = GetFullPath( dataFile );
			Debug.Log (Application.persistentDataPath);

			StreamReader data = new StreamReader( filePath );

//			previousFinalLevel = Convert.ToInt32( data.ReadLine() );
			previousFinalLevel = float.Parse( data.ReadLine ());

			debugOn = Convert.ToBoolean( data.ReadLine ());

			consecutiveModusTollensIncorrect = Convert.ToInt32( data.ReadLine() );

			fitTestTaken = Convert.ToBoolean( data.ReadLine ());

			data.Close ();
		}

	}
	
	public string GetFullPath( string saveFile ){
		return Application.persistentDataPath + saveFile;
		
	}
}

//[Serializable]
class PlayerData
{
	public float previousFinalLevel;
	public bool debugOn;
	public int consecutiveModusTollensIncorrect;
	public bool fitTestTaken;
}