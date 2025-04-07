//IMR_CharacterRespawn.cs
/*
 * This script is called whenever a character dies. Worked with Opsive.
 * It probably does not work without Opsive.
 * 
 * This script goes onto the character itself.
 * 
 * When a character dies, it calls a function from IMR_SpawnPoints.
 * That script then calls from IMR_Gameplay which picks a Spawn Post, then a Spawn Point,
 *	and then returns it here for Opsive to pick up when respawning.
 * 
 * I think I can rework this script to do the actual respawning, or to call HorseAnimSet's
 *  respawner (so that it resets all the things that need it.)
*/



using UnityEngine;
using System.Collections;
using UnityEngine.Events;



public class IMR_CharacterRespawn : MonoBehaviour
{
	IMR_RespawnManager RespawnManager_Script;

	private Transform MyPosition;
	public int MyID = 799;  //10/27/2024 - this should go to CharacterAttributes later.

	//bool FirstSpawn = true;  //Only used in some game modes - changes to false immediately after initial spawn.



	void Awake()
	{
		MyPosition = GetComponent<Transform>();

		RespawnManager_Script = GameObject.Find("Game Settings").GetComponent<IMR_RespawnManager>();
	}



	public void Spawn()
	{
		Transform spawnPoint;
		spawnPoint = RespawnManager_Script.GetSpawnLocation(gameObject.layer, MyID);

		//MyPosition.SetPosition(spawnPoint.position);  //probably only works with Opsive.
		//MyPosition.SetRotation(spawnPoint.rotation);

		MyPosition.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

		//EventHandler.ExecuteEvent(m_GameObject, "OnRespawn");  //probably only works with Opsive.

		//FirstSpawn = false;  //not the most efficient method, but it works and isn't overly taxing.
	}
}
