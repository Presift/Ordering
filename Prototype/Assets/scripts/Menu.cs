using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	public Toggle debug;
	public Toggle shortGame;

	// Use this for initialization
	void Start () {

		GameData.dataControl.Load ();

		if( GameData.dataControl.debugOn )
		{
			debug.isOn = true;
		}
		else
		{
			debug.isOn = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void NewGame()
	{
		GameData.dataControl.previousFinalLevel = 0;
		GameData.dataControl.fitTestTaken = false;
		GameData.dataControl.Save ();

		Application.LoadLevel ("Main Game");
	}

	public void OnDebugDown()
	{
		if ( debug.isOn )
		{
			GameData.dataControl.debugOn = true;

//			Debug.Log("checked");
		}
		else
		{
			GameData.dataControl.debugOn = false;
//			Debug.Log ("unchecked");
		}

		GameData.dataControl.Save();

	}

	public void OnShortGameDown()
	{
		if ( shortGame.isOn )
		{
			GameData.dataControl.shortGame = true;

		}
		else
		{
			GameData.dataControl.shortGame = false;

		}
		
		GameData.dataControl.Save();
	}
}
