//IMR_Gameplay.cs
/*
 * Formerly BattleGame.cs.
 * Renamed to make better sense.
 * Contains the Game Mode and Spawning System setups.
 * 
 * Goes on global empty GameObject.
 */



using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;



public class IMR_Gameplay : MonoBehaviour
{
	public bool UseTimer = false;
	public string GameOverScene;

	public bool BotsEnabled;
	public bool SoldiersEnabled;
	public bool TanksEnabled;
	public bool WalkersEnabled;
	public bool FightersEnabled;

	public List<GameObject> PlayerPrefabs = new();
	public List<GameObject> GreenFactionPrefabs = new();
	public List<GameObject> RedFactionPrefabs = new();

	public int CurrentGreenFactionPopulation;
	public int CurrentRedFactionPopulation;
	public int MaxGreenPlayers;
	public int MaxGreenFactionPopulation;  //Only for Bots; Players not included in this population.
	public int MaxRedPlayers;
	public int MaxRedFactionPopulation;

	public List<GameObject> GreenFactionPlayerList = new();  //Analog (Human) Players only.
	public List<GameObject> GreenFactionSoldierList = new();

	public List<GameObject> RedFactionPlayerList = new();  //Bot (AI) Players only.
	public List<GameObject> RedFactionSoldierList = new();

	public List<GameObject> GreenFactionMemberList_Full = new();  //contains all Players, Bots, vehicles, anything "living".
	public List<GameObject> RedFactionMemberList_Full = new();

	public List<GameObject> Waypoints = new();
	public IMR_RespawnManager RespawnManager_Script;


	public enum Game_Mode { CP_Random, CP_Programmed, CP_SpawnPosts, CombatZone, LandingZone, Warheads, HackNSlash };
	[Tooltip("Use to specify Game Mode: Command Posts with random starting assignments, Command Posts with developer-programmed starting assignments, CombatZone for capturable CZs, LandingZone for destructible CZs, Warheads for sport version of gameplay with a Ball in the middle, and HacknSlash.")]
	public Game_Mode GameMode;

	public List<GameObject> CommandPostMasterList = new();
	public List<GameObject> RedTeamCommandPosts = new();
	public List<GameObject> GreenTeamCommandPosts = new();

	//used for CP_SpawnPost mode.
	public List<GameObject> RedTeamSpawnPosts = new();
	public List<GameObject> GreenTeamSpawnPosts = new();


	public List<GameObject> CombatZoneList = new();
	public int CurrentCZ = 0;

	int CurrentSpawnPointIndex = 0;



	void Awake ()
	{
		if (BotsEnabled)
		{
			//Use Random for classic BF-style gameplay.
			if (GameMode == Game_Mode.CP_Random)
			{
				int HalfOfAllSpawnPosts = CommandPostMasterList.Count / 2;

				//Before shuffling the list of SpawnPosts, assign an ID to each one, that way it can identify itself
				for (int i = 0; i < CommandPostMasterList.Count; i++)
				{
					CommandPostMasterList[i].GetComponent<IMR_CommandPostManager>().CPID = i;
				}

				Shuffle(CommandPostMasterList);  //shuffles the order of elements in the list.

				RedTeamCommandPosts = CommandPostMasterList.GetRange(0, HalfOfAllSpawnPosts);  //note, GetRange is not from startIndex to endIndex, it is startIndex and amountOfElementsDesired
				GreenTeamCommandPosts = CommandPostMasterList.GetRange(HalfOfAllSpawnPosts, HalfOfAllSpawnPosts);

				for (int i = 0; i < RedTeamCommandPosts.Count; i++)
				{
					RedTeamCommandPosts[i].GetComponentInChildren<IMR_CaptureRegion>().CurrentOwner = "RedPost";
					RedTeamCommandPosts[i].GetComponentInChildren<IMR_CommandPostHealth>().RedHP = 100;
					RedTeamCommandPosts[i].GetComponent<IMR_CommandPostManager>().TeamID = i;
				}

				for (int i = 0; i < GreenTeamCommandPosts.Count; i++)
				{
					GreenTeamCommandPosts[i].GetComponentInChildren<IMR_CaptureRegion>().CurrentOwner = "GreenPost";
					GreenTeamCommandPosts[i].GetComponentInChildren<IMR_CommandPostHealth>().GreenHP = 100;
					GreenTeamCommandPosts[i].GetComponent<IMR_CommandPostManager>().TeamID = i;
				}
			}



			//All CPs start off un-captured, so no need to shuffle.  Spawning does not occur at a CP, but instead at the nearest SpawnPost, except at first spawn - that is random.
			else if (GameMode == Game_Mode.CP_SpawnPosts)
			{
				for (int i = 0; i < CommandPostMasterList.Count; i++)
				{
					//Assign an ID to each CP, that way it can identify itself.
					CommandPostMasterList[i].GetComponent<IMR_CommandPostManager>().CPID = i;
				}

			}



			//CombatZone is a mode where you fight at one area, and once a point of interest is captured, you move onto another random area.
			else if (GameMode == Game_Mode.CombatZone)
			{
				//Pick a random starting Combat Zone from the CombatZoneList.

				//Before shuffling the list of CZs, assign an ID to each one, that way it can identify itself
				for (int i = 0; i < CombatZoneList.Count; i++)
				{
					CombatZoneList[i].GetComponent<IMR_CombatZoneProperties>().CombatZoneID = i;
				}

				Shuffle(CombatZoneList);

				CurrentCZ = 0;  //just reinforce that the starting CZ should be at index 0 -since the list is shuffled, just increment. Easy to start over when through the list.

				SetupNextCombatZone();
			}



			//LandingZone is Sequential Combat Zones (not random).
			else if (GameMode == Game_Mode.LandingZone)
			{
				//[IMR 1/20/2025] - This is not fully developed and may need tweaking.

				for (int i = 0; i < CombatZoneList.Count; i++)
				{
					CombatZoneList[i].GetComponent<IMR_CombatZoneProperties>().CombatZoneID = i;
				}

				CurrentCZ = 0;  //Just reinforce that the starting LZ should be at index 0.
								//	Use in case game needs to know to stop based on number of LZs conquered.

				SetupNextCombatZone();
			}



			//Warheads is the sports version of gameplay with a Ball in the middle.
			else if (GameMode == Game_Mode.Warheads)
			{
				//Simply add CP object to RedTeamCommandPosts and GreenTeamCommandPosts.
				//Also manually add Waypoints to the Waypoints list, as well.
			}



			//HackNSlash is essentially LandingZone, but with specialized spawning designed for ActionRPGs.
			else if (GameMode == Game_Mode.HackNSlash)
			{
				//[IMR 1/20/2025] - Since enemy diversity and timing is key to ActionRPGs, spawning is not as random as in shooter games.
				//	However, despite the different spawning system, much of the gameplay is similar to LandingZones
				//	(which is currently mainly built into Combat Zones which do not shuffle).

				for (int i = 0; i < CombatZoneList.Count; i++)
				{
					CombatZoneList[i].GetComponent<IMR_CombatZoneProperties>().CombatZoneID = i;
				}

				CurrentCZ = 0;  //just reinforce that the starting CZ should be at index 0 -since the list is shuffled, just increment. Easy to start over when through the list.

				CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().EnableLocalSpawnPosts();
				SetupNextCombatZone();
			}


			//[IMR 1/21/2025] - Doesn't appear to be in real use.
			RespawnManager_Script = GameObject.Find("Game Settings").GetComponent<IMR_RespawnManager>();



			if (SoldiersEnabled)
			{
				SetupPlayerLists();
			}
		}
	}



	public static List<GameObject> Shuffle(List<GameObject> aList)
	{
		System.Random _Random = new System.Random();
		GameObject tempObject;

		int count = aList.Count;

		for (int i = 0; i < count; i++)
		{
			int randomIndex = i + (int)(_Random.NextDouble() * (count - i));

			//swap
			tempObject = aList[randomIndex];
			aList[randomIndex] = aList[i];
			aList[i] = tempObject;
		}

		return aList;
	}



	public static GameObject[] ConvertListToArray(List<GameObject> myList)
	{
		GameObject[] a = new GameObject[myList.Count];

		for (int i = 0; i < myList.Count; i++)
		{
			a[i] = myList[i];
		}

		return a;
	}



	public static List<GameObject> ConvertArrayToList(GameObject[] a)
	{
		List<GameObject> myList = new List<GameObject>();

		for (int i = 0; i < a.Length; i++)
		{
			myList.Add(a[i]);
		}

		return myList;
	}



	void SetupPlayerLists()
	{
		GameObject newSpawn;
		Transform spawnPoint;

		int masterID = 0;


		//If any mode but HackNSlash, then Instantiate players and bots and put them into lists.
		if (GameMode != Game_Mode.HackNSlash)
		{
			//First set up Players.
			for (int greenPlayer = 0; greenPlayer < MaxGreenPlayers; greenPlayer++)
			{
				spawnPoint = StartingSpawnPoint("GreenTeam");

				newSpawn = LoadModelAtPosition(PlayerPrefabs[0], spawnPoint);
				GreenFactionPlayerList.Add(newSpawn);

				GreenFactionMemberList_Full.Add(GreenFactionPlayerList[greenPlayer]);
				GreenFactionMemberList_Full[masterID].GetComponent<IMR_CharacterRespawn>().MyID = masterID;

				masterID++;
			}

			//Now set up Ally Bots.
			for (int greenBot = 0; greenBot < MaxGreenFactionPopulation; greenBot++)
			{
				spawnPoint = StartingSpawnPoint("GreenTeam");

				newSpawn = LoadModelAtPosition(GreenFactionPrefabs[0], spawnPoint);
				GreenFactionSoldierList.Add(newSpawn);

				GreenFactionMemberList_Full.Add(GreenFactionSoldierList[greenBot]);
				GreenFactionMemberList_Full[masterID].GetComponent<IMR_CharacterRespawn>().MyID = masterID;

				masterID++;
			}



			//can reset masterID for RedFaction setup.
			masterID = 0;


			//for now, not actually using Red Players - this is just setup for just in case.
			for (int redPlayer = 0; redPlayer < MaxRedPlayers; redPlayer++)
			{
				spawnPoint = StartingSpawnPoint("RedTeam");

				newSpawn = LoadModelAtPosition(PlayerPrefabs[0], spawnPoint);
				RedFactionPlayerList.Add(newSpawn);

				RedFactionMemberList_Full.Add(RedFactionPlayerList[redPlayer]);
				RedFactionMemberList_Full[masterID].GetComponent<IMR_CharacterRespawn>().MyID = masterID;

				masterID++;
			}


			//Set up Enemy Bots.
			for (int redBot = 0; redBot < MaxRedFactionPopulation; redBot++)
			{
				spawnPoint = StartingSpawnPoint("RedTeam");

				newSpawn = LoadModelAtPosition(RedFactionPrefabs[0], spawnPoint);
				RedFactionSoldierList.Add(newSpawn);

				RedFactionMemberList_Full.Add(RedFactionSoldierList[redBot]);
				RedFactionMemberList_Full[masterID].GetComponent<IMR_CharacterRespawn>().MyID = masterID;

				masterID++;
			}

			//SetupWaypoints();
		}



		//If HackNSlash mode, then let the Spawn Points do the spawning.
		else if(GameMode == Game_Mode.HackNSlash)
		{

		}
	}



	GameObject LoadModel(GameObject SpawnObject)
	{
		GameObject newSpawn;
		newSpawn = (GameObject)Instantiate(SpawnObject);

		return newSpawn;
	}


	
	GameObject LoadModelAtPosition(GameObject SpawnObject, Transform SpawnPoint)
	{
		GameObject newSpawn;
		newSpawn = (GameObject)Instantiate(SpawnObject, SpawnPoint.position, SpawnPoint.rotation);

		return newSpawn;
	}



	public GameObject StartingSpawnPost(string Team)
	{
		GameObject SpawnPost = null;
		int selectedPost = 0;

		if (GameMode == Game_Mode.CP_SpawnPosts)
		{
			if (Team == "RedTeam")
			{
				selectedPost = Random.Range(0, RedTeamSpawnPosts.Count);
				SpawnPost = RedTeamSpawnPosts[selectedPost];
			}

			else if (Team == "GreenTeam")
			{
				selectedPost = Random.Range(0, GreenTeamSpawnPosts.Count);
				SpawnPost = GreenTeamSpawnPosts[selectedPost];
			}

			else if (Team == "Player")
			{
				selectedPost = Random.Range(0, GreenTeamSpawnPosts.Count);
				SpawnPost = GreenTeamSpawnPosts[selectedPost];
			}
		}

		else
		{
			if (Team == "RedTeam")
			{
				selectedPost = Random.Range(0, RedTeamCommandPosts.Count);
				SpawnPost = RedTeamCommandPosts[selectedPost];
			}

			else if (Team == "GreenTeam")
			{
				selectedPost = Random.Range(0, GreenTeamCommandPosts.Count);
				SpawnPost = GreenTeamCommandPosts[selectedPost];
			}

			else if (Team == "Player")
			{
				selectedPost = Random.Range(0, GreenTeamCommandPosts.Count);
				SpawnPost = GreenTeamCommandPosts[selectedPost];
			}
		}

		return SpawnPost;
	}



	public GameObject PickSpawnPost(string Team, int ID)
	{
		GameObject SpawnPost = null;
		int selectedPost = 0;

		if (GameMode == Game_Mode.CP_SpawnPosts)
		{
			if (Team == "RedTeam")
			{
				if (RedFactionMemberList_Full[ID].layer == LayerMask.NameToLayer("Player"))
				{
					selectedPost = ClosestSpawnPostByIndex(Team, ID, RedTeamSpawnPosts);
				}

				else  //AI Bots work better if they spawn randomly instead of close to where they KO'd.
				{
					selectedPost = Random.Range(0, RedTeamSpawnPosts.Count);
				}

				SpawnPost = RedTeamSpawnPosts[selectedPost];
			}

			else if (Team == "GreenTeam" || Team == "Player")
			{
				if (GreenFactionMemberList_Full[ID].layer == LayerMask.NameToLayer("Player"))
				{
					selectedPost = ClosestSpawnPostByIndex(Team, ID, GreenTeamSpawnPosts);
				}

				else
				{
					selectedPost = Random.Range(0, GreenTeamSpawnPosts.Count);
				}

				SpawnPost = GreenTeamSpawnPosts[selectedPost];
			}
		}

		else
		{
			if (Team == "RedTeam")
			{
				selectedPost = Random.Range(0, RedTeamCommandPosts.Count);  //removed -1 from count b/c Random uses the count as Exclusive, meaning it doesn't include count in its consideration.
				SpawnPost = RedTeamCommandPosts[selectedPost];
			}

			else if (Team == "GreenTeam")
			{
				selectedPost = Random.Range(0, GreenTeamCommandPosts.Count);
				SpawnPost = GreenTeamCommandPosts[selectedPost];
			}

			else if (Team == "Player")
			{
				selectedPost = Random.Range(0, GreenTeamCommandPosts.Count);
				SpawnPost = GreenTeamCommandPosts[selectedPost];
			}
		}

		return SpawnPost;
	}


	public int ClosestSpawnPostByIndex(string Team, int ID, List<GameObject> ListOfSpawnPosts)
	{
		int bestTargetIndex = 0;
		float closestDistanceSqr = Mathf.Infinity;

		Vector3 currentPosition;

		//List<GameObject> TeamList = new List<GameObject>();

		if (Team == "GreenTeam" || Team == "Player")
		{
			//TeamList = GreenFactionMemberList_Full;
			currentPosition = GreenFactionMemberList_Full[ID].transform.position;
		}

		else
		{
			//TeamList = RedFactionMemberList_Full;
			currentPosition = RedFactionMemberList_Full[ID].transform.position;
		}




		for (int i = 0; i < ListOfSpawnPosts.Count; i++)
		{
			Transform potentialTarget = ListOfSpawnPosts[i].GetComponent<IMR_SpawnPostManager>().CenterPoint.transform;
			Vector3 directionToTarget = potentialTarget.position - currentPosition;
			float dSqrToTarget = directionToTarget.sqrMagnitude;

			if (dSqrToTarget < closestDistanceSqr)
			{
				closestDistanceSqr = dSqrToTarget;
				bestTargetIndex = i;
			}
		}

		return bestTargetIndex;
	}



	public Transform StartingSpawnPoint(string Team)
	{
		Transform SpawnPoint;
		GameObject SpawnPost = StartingSpawnPost(Team);

		if (CurrentSpawnPointIndex >= SpawnPost.GetComponent<IMR_SpawnPostManager>().SpawnPoints.Length)
		{
			CurrentSpawnPointIndex = 0;  //later, this variable should be global per SpawnPost, not for whole game.
		}

		SpawnPoint = SpawnPost.GetComponent<IMR_SpawnPostManager>().SpawnPoints[CurrentSpawnPointIndex];

		CurrentSpawnPointIndex++;

		return SpawnPoint;
	}



	public Transform PickSequentialSpawnPoint(string Team, int ID)
	{
		Transform SpawnPoint;
		GameObject SpawnPost = PickSpawnPost(Team, ID);

		if (CurrentSpawnPointIndex >= SpawnPost.GetComponent<IMR_SpawnPostManager>().SpawnPoints.Length)
		{
			CurrentSpawnPointIndex = 0;  //later, this variable should be global per SpawnPost, not for whole game.
		}

		SpawnPoint = SpawnPost.GetComponent<IMR_SpawnPostManager>().SpawnPoints[CurrentSpawnPointIndex];

		CurrentSpawnPointIndex++;

		return SpawnPoint;
	}



	public void RemoveSpawnPostFromOwnerList(int CPID, string Team)
	{
		if (Team == "RedPost")
		{
			for (int i = 0; i < RedTeamCommandPosts.Count; i++)
			{
				if (RedTeamCommandPosts[i].GetComponent<IMR_CommandPostManager>().CPID == CPID)
				{
					RedTeamCommandPosts.Remove(RedTeamCommandPosts[i]);
					break;
				}
			}
		}

		else if (Team == "GreenPost")
		{
			for (int i = 0; i < GreenTeamCommandPosts.Count; i++)
			{
				if (GreenTeamCommandPosts[i].GetComponent<IMR_CommandPostManager>().CPID == CPID)
				{
					GreenTeamCommandPosts.Remove(GreenTeamCommandPosts[i]);
					break;
				}
			}
		}

		else
		{
			//DoNothing();
		}


		if (GreenTeamCommandPosts.Count == 0)
		{
			//GetComponentInChildren<GameScore>().CPWinner("Red");
		}

		else if (RedTeamCommandPosts.Count == 0)
		{
			//GetComponentInChildren<GameScore>().CPWinner("Green");
		}
	}



	public void AddSpawnPostToOwnerList(int CPID, string Team)
	{
		if (Team == "RedPost")
		{
			for (int i = 0; i < CommandPostMasterList.Count; i++)
			{
				if (CommandPostMasterList[i].GetComponent<IMR_CommandPostManager>().CPID == CPID)
				{
					RedTeamCommandPosts.Add(CommandPostMasterList[i]);
					break;
				}
			}
		}

		else if (Team == "GreenPost")
		{
			for (int i = 0; i < CommandPostMasterList.Count; i++)
			{
				if (CommandPostMasterList[i].GetComponent<IMR_CommandPostManager>().CPID == CPID)
				{
					GreenTeamCommandPosts.Add(CommandPostMasterList[i]);
					break;
				}
			}
		}

		else
		{
			//DoNothing();
		}


		/*if(GreenTeamSpawnPosts.Count == SpawnPostMasterList.Count)
		{
			GetComponentInChildren<GameScore>().CPWinner("Green");
		}

		else if(RedTeamSpawnPosts.Count == SpawnPostMasterList.Count)
		{
			GetComponentInChildren<GameScore>().CPWinner("Red");
		}*/
	}



	public void NextCombatZone()
	{
		//The ID of the previous Combat Zone before the new is about to begin.
		int lastCZindex = CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().CombatZoneID;

		CurrentCZ++;

		//Do the following, only if this is random Combat Zone mode and we've reached the last one in the list.
		if (GameMode == Game_Mode.CombatZone)
		{
			if (CurrentCZ >= CombatZoneList.Count)
			{
				Shuffle(CombatZoneList);

				for (int i = 0; i < CombatZoneList.Count; i++)
				{
					CombatZoneList[i].GetComponent<IMR_CombatZoneProperties>().IsCombatZoneActive = false;
					CombatZoneList[i].GetComponent<IMR_CombatZoneProperties>().WasAlreadyUsed = false;
					CombatZoneList[i].GetComponent<IMR_CombatZoneProperties>().ResetCZ();
				}

				CurrentCZ = 0;

				if (CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().CombatZoneID == lastCZindex)
				{
					CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().KeepGreen();
					CurrentCZ++;
				}
			}
		}

		RemoveCombatZoneSpawnPosts();
		CombatZoneList[lastCZindex].GetComponent<IMR_CombatZoneProperties>().DisableLocalSpawnPosts();
		//RemoveWaypoints();



		CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().EnableLocalSpawnPosts();
		SetupNextCombatZone();
		//SetupWaypoints();
	}



	void SetupNextCombatZone()
	{
		CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().IsCombatZoneActive = true;

		//Add the CZ's Command Posts to this object's CP lists.
		int LocalRedSpawnPostCount = CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().LocalRedSpawnPosts.Count;


		for (int i = 0; i < LocalRedSpawnPostCount; i++)
		{
			RedTeamCommandPosts.Add(CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().LocalRedSpawnPosts[i]);
		}


		int LocalGreenSpawnPostCount = CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().LocalGreenSpawnPosts.Count;

		for (int i = 0; i < LocalGreenSpawnPostCount; i++)
		{
			GreenTeamCommandPosts.Add(CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().LocalGreenSpawnPosts[i]);
		}



		//RedTeamCommandPosts.Add(CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().LocalRedSpawnPosts[0]);
		//GreenTeamCommandPosts.Add(CombatZoneList[CurrentCZ].GetComponent<IMR_CombatZoneProperties>().LocalGreenSpawnPosts[0]);
	}



	void RemoveCombatZoneSpawnPosts()
	{
		for (int i = 0; i < RedTeamCommandPosts.Count; i++)
		{
			RedTeamCommandPosts.Remove(RedTeamCommandPosts[i]);
		}


		for (int i = 0; i < GreenTeamCommandPosts.Count; i++)
		{
			GreenTeamCommandPosts.Remove(GreenTeamCommandPosts[i]);
		}


		//RedTeamCommandPosts.Remove(RedTeamCommandPosts[0]);
		//GreenTeamCommandPosts.Remove(GreenTeamCommandPosts[0]);
	}


	//Don't need these - they only work with Third Person Controller 1.0.
	/*void SetupWaypoints()
	{
		//BehaviorTree behaviorTree;
		//SharedGameObjectList waypoints;

		for (int greenBot = 0; greenBot < MaxGreenFactionPopulation; greenBot++)
		{
			//behaviorTree = GreenFactionSoldierList[greenBot].GetComponent<BehaviorTree>();
			//waypoints = behaviorTree.GetVariable("Waypoints") as SharedGameObjectList;

			if (CPStartup == CP_Startup.CombatZone)
			{
				for (int i = 0; i < CombatZoneList[CurrentCZ].GetComponent<CombatZone>().LocalWaypoints.Count; i++)
				{
					waypoints.Value.Add(CombatZoneList[CurrentCZ].GetComponent<CombatZone>().LocalWaypoints[i]);
				}
			}

			else if (CPStartup == CP_Startup.Warheads)
			{
				for (int i = 0; i < Waypoints.Count; i++)
				{
					waypoints.Value.Add(Waypoints[i]);
				}
			}

			else if (CPStartup == CP_Startup.CP_SpawnPosts)
			{
				for (int i = 0; i < Waypoints.Count; i++)
				{
					waypoints.Value.Add(Waypoints[i]);
				}
			}
		}


		for (int redBot = 0; redBot < MaxRedFactionPopulation; redBot++)
		{
			behaviorTree = RedFactionSoldierList[redBot].GetComponent<BehaviorTree>();
			waypoints = behaviorTree.GetVariable("Waypoints") as SharedGameObjectList;

			if (CPStartup == CP_Startup.CombatZone)
			{
				for (int i = 0; i < CombatZoneList[CurrentCZ].GetComponent<CombatZone>().LocalWaypoints.Count; i++)
				{
					waypoints.Value.Add(CombatZoneList[CurrentCZ].GetComponent<CombatZone>().LocalWaypoints[i]);
				}
			}
			
			else if (CPStartup == CP_Startup.Warheads)
			{
				for (int i = 0; i < Waypoints.Count; i++)
				{
					waypoints.Value.Add(Waypoints[i]);
				}
			}

			else if (CPStartup == CP_Startup.CP_SpawnPosts)
			{
				for (int i = 0; i < Waypoints.Count; i++)
				{
					waypoints.Value.Add(Waypoints[i]);
				}
			}
		}
	}*/



	/*void RemoveWaypoints()
	{
		BehaviorTree behaviorTree;
		SharedGameObjectList waypoints;

		for (int greenBot = 0; greenBot < MaxGreenFactionPopulation; greenBot++)
		{
			behaviorTree = GreenFactionSoldierList[greenBot].GetComponent<BehaviorTree>();
			waypoints = behaviorTree.GetVariable("Waypoints") as SharedGameObjectList;

			waypoints.Value.Clear();
		}


		for (int redBot = 0; redBot < MaxRedFactionPopulation; redBot++)
		{
			behaviorTree = RedFactionSoldierList[redBot].GetComponent<BehaviorTree>();
			waypoints = behaviorTree.GetVariable("Waypoints") as SharedGameObjectList;

			waypoints.Value.Clear();
		}
	}*/



	void Start()
	{
		if (UseTimer)
		{
			StartCoroutine(GameTimer());
		}
	}



	IEnumerator GameTimer()
	{
		yield return new WaitForSeconds(600f);  //10 mins

		SceneManager.LoadScene(GameOverScene, LoadSceneMode.Single);
	}



}
