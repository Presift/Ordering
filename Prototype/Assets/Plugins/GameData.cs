using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;


public class GameData : MonoBehaviour {
	
	public static GameData dataControl;
	
	//	private string dataFile = "/playerInfo.dat";
	//	private string levelingStats = "/levelingStats.dat";
	private string dataFile = "/playerInfo.txt";
	
	private string levelingStats = "/levelingStats";
	public int currentStatsFile = 0;
	
	public int previousFinalLevel;
	
	
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
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
	}
	public void Save()
	{
		BinaryFormatter bf = new BinaryFormatter();
		
		FileStream file = File.Create(GetFullPath( dataFile ));
		
		PlayerData data = new PlayerData();
		data.previousFinalLevel = previousFinalLevel;
		data.currentStatsFile = currentStatsFile;
		
		//serlialize data and close file
		bf.Serialize(file, data);
		file.Close ();
		
	}
	
	public void SavePerformanceStats( string stats )
	{
		//		string filePath = GetFullPath (levelingStats + currentStatsFile.ToString() + ".csv");
		string filePath = GetFullPath (levelingStats + ".txt");
		System.IO.File.AppendAllText(filePath, "\n" + stats );
	}
	
	public void Load()
	{
		if(File.Exists (GetFullPath( dataFile )))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(GetFullPath( dataFile ), FileMode.Open);
			Debug.Log (Application.persistentDataPath);
			Debug.Log (file.Name);
			PlayerData data = (PlayerData)bf.Deserialize(file);
			
			previousFinalLevel = data.previousFinalLevel;
			currentStatsFile = data.currentStatsFile;
			
			bf.Serialize(file, data);
			
			file.Close ();
		}
		
		currentStatsFile ++;
	}
	
	public string GetFullPath( string saveFile ){
		return Application.persistentDataPath + saveFile;
		
	}
}

[Serializable]
class PlayerData
{
	public int previousFinalLevel;
	public int currentStatsFile;
}