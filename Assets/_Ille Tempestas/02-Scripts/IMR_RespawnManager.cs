//IMR_RespawnManager.cs
/*
 * Formerly used to hold and assign generic spawn points for bots and players;
 * Now used only to locate spawn points by team, but no longer contains lists;
 * Lists are held in IMR_GameSetup.cs
 * 
 * This script goes onto a SpawnPost object.
*/



using UnityEngine;
using System.Collections;



public class IMR_RespawnManager : MonoBehaviour
{



	public Transform GetSpawnLocation(int teamLayer, int ID)
	{
		Transform ChosenSpawnPoint;
		IMR_Gameplay Gameplay_Script = GameObject.Find("Game").GetComponent<IMR_Gameplay>();

		if (teamLayer == LayerMask.NameToLayer("GreenTeam") || teamLayer == LayerMask.NameToLayer("Player"))
		{
			ChosenSpawnPoint = Gameplay_Script.PickSequentialSpawnPoint("GreenTeam", ID);

			//In the future, make changes to how spawnpoint picking works - Sequential works fine for now, but
			// it will need adjustments due to the existence of SpawnPosts.  However, these things only affect
			// game modes like CP Capture.  CZs and Warheads should be generally unaffected.
			// Problem lies in the global Index.  Should make global per SpawnPost, not for whole game.
		}

		else if (teamLayer == LayerMask.NameToLayer("RedTeam"))
		{
			ChosenSpawnPoint = Gameplay_Script.PickSequentialSpawnPoint("RedTeam", ID);
		}

		else
		{
			ChosenSpawnPoint = null;
		}

		return ChosenSpawnPoint;
	}
}
