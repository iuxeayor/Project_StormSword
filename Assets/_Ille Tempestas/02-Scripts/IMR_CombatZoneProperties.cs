//IMR_CombatZoneProperties.cs
/* 
 * Contains lists of CPs for spawning, lists of waypoints,
 *  and activation status.
*/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Drawing;



public class IMR_CombatZoneProperties : MonoBehaviour
{
	public int CombatZoneID = 777;

	[Tooltip("True for the Current Combat Zone.")]
	public bool IsCombatZoneActive = false;

	[Tooltip("True when used once. Set back to false if all CZs have already had a turn and game still continues.")]
	public bool WasAlreadyUsed = false;

	[Tooltip("Set True only for the final Combat Zone of this Scene. Set Manually.")]
	public bool IsFinalCombatZone = false;

	IMR_Gameplay GameManager;
	GameObject CommandPost;




	[Tooltip("Shows the number of Goals that need to be Accomplished in order to Conquer this Combat Zone. This is automatically determined by getting the length of the list containing all the Combat Goals.")]
	public int NumberOfCombatGoals = 0;

	[Tooltip("Shows the number of Goals that have been Accomplished for this Combat Zone. This is automatically incremented each time a Goal is Accomplished.")]
	public int NumberOfAccomplishedCombatGoals = 0;

	[Tooltip("The List of Combat Goals to Accomplish - manually place the GOs of each Region or Destruction Target.")]
	public List<GameObject> CombatGoals;


	[Tooltip("The current owner of the CZ.")]
	public enum CZOwner 
	{
		 BlankPost
		,RedPost
		,GreenPost
	};

	[Tooltip("Use to specify Combat Zone Owner: BlankPost, RedPost, GreenPost")]
	public CZOwner CurrentCZ_Owner;


	public Material BlankMaterial;
	public Material RedMaterial;
	public Material GreenMaterial;


	public List<GameObject> LocalRedSpawnPosts = new List<GameObject>();
	public List<GameObject> LocalGreenSpawnPosts = new List<GameObject>();
	public List<GameObject> LocalWaypoints = new List<GameObject>();




	private void Awake()
	{
		//We want to disable all Spawn Points at start and then enable them as the Combat Zone comes online.
		DisableLocalSpawnPosts();
		NumberOfCombatGoals = CombatGoals.Count;

		GameManager = GameObject.Find("Game Settings").GetComponent<IMR_Gameplay>();
		CommandPost = transform.Find("Command Post").gameObject;


		switch (CurrentCZ_Owner)
		{
			case CZOwner.BlankPost:
				CommandPost.GetComponent<MeshRenderer>().material = BlankMaterial;
				break;

			case CZOwner.RedPost:
				CommandPost.GetComponent<MeshRenderer>().material = RedMaterial;
				break;

			case CZOwner.GreenPost:
				CommandPost.GetComponent<MeshRenderer>().material = GreenMaterial;
				break;

			default:
				break;
		}
	}



	public void EnableLocalSpawnPosts()
	{
		foreach (GameObject spawnpost in LocalRedSpawnPosts)
		{
			spawnpost.SetActive(true);
			spawnpost.GetComponent<IMR_SpawnPostManager>().EnableLocalSpawnPoints();
		}

		foreach (GameObject spawnpost in LocalGreenSpawnPosts)
		{
			spawnpost.SetActive(true);
			spawnpost.GetComponent<IMR_SpawnPostManager>().EnableLocalSpawnPoints();
		}

		//Not useing Way Points right now, so I won't do them yet.
	}



	public void DisableLocalSpawnPosts()
	{

		foreach (GameObject spawnpost in LocalRedSpawnPosts)
		{
			spawnpost.GetComponent<IMR_SpawnPostManager>().DisableLocalSpawnPoints();
			spawnpost.SetActive(false);

		}

		foreach (GameObject spawnpost in LocalGreenSpawnPosts)
		{
			spawnpost.GetComponent<IMR_SpawnPostManager>().DisableLocalSpawnPoints();
			spawnpost.SetActive(false);
		}

		//Not useing Way Points right now, so I won't do them yet.
	}




	[Tooltip("Use this function when a region is captured or some other goal is achieved. Once all parts of a Combat Goals are Accomplished, this function will then set the next Combat Zone up.")]
	public void CombatGoalAccomplished()
	{
		/*
		 *	Called from each Combat Goal item.
		 *	This function checks if all Combat Goals are Accomplished.
		 *	If not, nothing happens.
		 *	If so, then set up the next CZ and turn off the current one.
		 */

		NumberOfAccomplishedCombatGoals++;  //Since this is only called whenever a Goal is Accomplished, always increment.

		if(NumberOfAccomplishedCombatGoals >= NumberOfCombatGoals) 
		{
			//The this region is conquered - set up next and disable current.

			CurrentCZ_Owner = CZOwner.GreenPost;
			CommandPost.GetComponent<MeshRenderer>().material = GreenMaterial;

			if(!IsFinalCombatZone)
			{
				IsCombatZoneActive = false;
				WasAlreadyUsed = true;

				GameManager.NextCombatZone();
			}

			else
			{
				//This is the Final Combat Zone. Since it's been Conquered, end the Scene.
				// Do whatever else needs to be done to wrap up the Scene and start the next.
			}
		}


	}




	public void ResetCZ()
	{
		CurrentCZ_Owner = CZOwner.BlankPost;

		CommandPost.GetComponent<MeshRenderer>().material = BlankMaterial;
		CommandPost.GetComponent<IMR_CommandPostHealth>().GreenHP = 0;
	}



	public void KeepGreen()
	{
		CurrentCZ_Owner = CZOwner.GreenPost;

		CommandPost.GetComponent<MeshRenderer>().material = GreenMaterial;
	}
}
