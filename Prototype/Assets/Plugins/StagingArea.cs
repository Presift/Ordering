using UnityEngine;
using System.Collections;

public class StagingArea {

	public Vector3 position;
	public bool occupied;

	public StagingArea( Vector3 newPosition, bool newOccupation )
	{
		position = newPosition;
		occupied = newOccupation;
	}
}
