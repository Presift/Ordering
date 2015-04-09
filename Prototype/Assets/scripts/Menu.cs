using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	public Toggle debug;

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
		GameData.dataControl.previousFinalLevel = 2;
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
}
