//IMR_SpawnPostManager.cs
/*
 *	Use directly on a Spawn Post to give coords for 
 *		Enemies/Allies (Bots) to Spawn/Respawn.
 *	Does not actually hold the population, 
 *		only holds the spawn points for each Spawn Post.
 *	
*/



using UnityEngine;
using System.Collections;
using System.Linq;



public class IMR_SpawnPostManager : MonoBehaviour
{
	public GameObject CenterPoint;
    public Transform[] SpawnPoints;          // An array of the spawn points this enemy can spawn from.




	private void Awake()
	{
		//We want to disable all Spawn Points at start and then enable them as the Combat Zone comes online.
		DisableLocalSpawnPoints();
	}



	public void EnableLocalSpawnPoints()
	{
		foreach (Transform spawnpoint in SpawnPoints)
		{
			spawnpoint.gameObject.SetActive(true);
		}

	}



	public void DisableLocalSpawnPoints()
	{
		foreach(Transform spawnpoint in SpawnPoints)
		{
			spawnpoint.gameObject.SetActive(false);
		}

	}



}