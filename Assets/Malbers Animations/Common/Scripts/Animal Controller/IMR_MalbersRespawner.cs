//IMR_MalbersRespawner.cs
/*
 * 12/6/2024
 * This was originally copied from MRespawnerNPC.cs (from Malbers's scripts).
 * I believe the fastest way to get the functionality of the script
 *		while also modifying it was to copy it.
 *	
 *	1/20/2025
 *	This script goes on each Spawn Point.
 *	Because in this game it makes sense to specify which enemy types and how many are at each Combat Zone,
 *		the enemies will not be able to spawn at random points.
 *		Instead, each bot will spawn at its dedicated point and will not respawn when the Combat Zone is conquered.
 *		
 *	The Gameplay script will register each Spawn Point into a list and will be able to control them from there.
 *		When a Combat Zone is conquered, its Spawn Points are deactivated,
 *		and a new Combat Zone and its Spawn Points are enabled.
 *	
 *	Modifications will need to be made in the OnCharacterDead() function in 2 places.
 *		Once the character is dead, it will get to a line where it calls a function in the Gameplay script.
 *		That script will register that the bot is dead.
 *		Then, after a Wave Timer, it will call back to a function here that will do the Respawn.
 *		(OR, the Gameplay script will simply send back a timer variable specifying how long to wait until the next wave.)
 *		
 *	For now, the Wave part of the script is yet unwritten.
 *	
 *	Also, in the near future, Spawn Points for each enemy type can use an empty Model of their type to represent them.
 *		This will help determine which enemies are present for an area, and how many of each.
 *		
 *	Spawn Posts will also be used later for more stuff:
 *		Especially for Group-Spawning,
 *		And for destruction, which allows players to destroy Spawn Posts in order to end enemy respawns in the area.
 *		The latter, however, will be rule-based or area-specific.
 *	
 *	
*/



using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using System.Collections.Generic;




namespace MalbersAnimations.Controller.AI
{
	/// <summary>Use this Script's Transform as the Respawn Point</summary>
	//[AddComponentMenu("Malbers/Animal Controller/Respawner NPC")]
	public class IMR_MalbersRespawner : MonoBehaviour
	{
		#region Respawn

		[Tooltip("Animal Prefab to Spawn - not the actual object itself, usually.")]
		public MAnimal NPC;

		public StateID RespawnState;
		public FloatReference RespawnTime = new FloatReference(10f);

		[Tooltip("If True: it will destroy the MainPlayer GameObject and Respawn a new One")]
		public BoolReference DestroyAfterRespawn = new BoolReference(true);


		/// <summary>Active Animal - the actual instantiated object.</summary>
		private MAnimal ActiveAnimal;


		#endregion

		[FormerlySerializedAs("OnRestartGame")]
		public GameObjectEvent OnRespawned = new GameObjectEvent();

		private bool Respawned;
		private MAnimalBrain NPCBrain;


		void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
		{
			FindNPCAnimal();
		}



		public virtual void DontDestroyOnLoad_GameObject(GameObject gameObject) => DontDestroyOnLoad(gameObject);

		void OnEnable()
		{
			if (!isActiveAndEnabled) return;
			transform.parent = null;
			DontDestroyOnLoad(transform);
			gameObject.name = gameObject.name + " Instance";
			SceneManager.sceneLoaded += OnLevelFinishedLoading;
			FindNPCAnimal();
			Debug.Log("Spawn Point Enabled." + gameObject.name);
		}


		private void OnDisable()
		{
			SceneManager.sceneLoaded -= OnLevelFinishedLoading;

			if (ActiveAnimal != null)
				ActiveAnimal.OnStateChange.RemoveListener(OnCharacterDead);  //Listen to the Animal changes of states

			Debug.Log("Spawn Point Disabled." + gameObject.name);

		}


		void FindNPCAnimal()
		{
			//[IMR - 1/19/2025] - I believe this should be called by Gameplay script when it's time to load the character lists.

			if (Respawned) return; //meaning the animal was already respawned. 

			if (NPC != null)
			{
				Debug.Log("Wolf has spawned." + NPC.name);

				if (NPC.gameObject.IsPrefab())
				{
					ActiveAnimal = Instantiate(NPC);
				}
				else
				{
					ActiveAnimal = NPC;
				}

				SceneAnimal();
			}
			else
			{
				Debug.LogWarning("[Respawner Removed]. There's no Character assigned", this);
				Destroy(gameObject); //Destroy This GO since is already a Spawner in the scene
			}
		}

		private void SceneAnimal()
		{
			Debug.Log("Scene Animal function started." + ActiveAnimal.name);


			ActiveAnimal.OverrideStartState = RespawnState;
			ActiveAnimal.ResetController();
			ActiveAnimal.enabled = true;
			ActiveAnimal.OnStateChange.AddListener(OnCharacterDead);        //Listen to the Animal changes of states
			ActiveAnimal.Teleport_Internal(transform.position);             //Move the Animal to is Start Position
			ActiveAnimal.transform.rotation = (transform.rotation);         //Move the Animal to is Start Position
			ActiveAnimal.isPlayer.Value = false;
			Respawned = true;

			NPCBrain = ActiveAnimal.GetComponentInChildren<MAnimalBrain>();
			if (NPCBrain != null)
				NPCBrain.enabled = true;
			// Debug.Log("Placed");
		}


		/// <summary>Listen to the Animal States</summary>
		public void OnCharacterDead(int StateID)
		{
			if (!Respawned) return;
			Debug.Log("Wolf has died." + ActiveAnimal.name);

			if (StateID == StateEnum.Death)              //Means Death
			{
				// Debug.Log("OnCharacterDead" + StateID);
				ActiveAnimal.OnStateChange.RemoveListener(OnCharacterDead);        //Remove listener from the Animal

				Respawned = false;

				if (NPC != null)         //If the Player is a Prefab then then instantiate it on the created scene
				{
					if (NPC.gameObject.IsPrefab())
					{
						//[IMR 1/20/2025] - I think this is where I would reference back to the Gameplay script.
						//  Right here in this spot.
						//  Maybe create a function so that it can be called in both places within this function.

						this.Delay_Action(RespawnTime, () =>
						{
							DestroyCurrentDeathAnimal();
							this.Delay_Action(() => FindNPCAnimal());
						}
						);
					}
					else
					{
						var DeathS = ActiveAnimal.activeState as Death; //make sure the Death does not disable all things... since where reusing the same animal
						DeathS.disableAnimal = false;
						DeathS.DisableAllComponents = false;
						DeathS.DisableInternalColliders = false;
						//DeathS.RemoveAllTriggers = false;

						//[IMR 1/20/2025] - I think this is where I would reference back to the Gameplay script.
						//  Right here in this spot.
						//  Maybe create a function so that it can be called in both places within this function.

						this.Delay_Action(RespawnTime, () => SceneAnimal());

					}
				}
			}
		}

		void DestroyCurrentDeathAnimal()
		{
			if (ActiveAnimal != null)
			{
				if (DestroyAfterRespawn)
					Destroy(ActiveAnimal.gameObject);
				else
					DestroyAllComponents(ActiveAnimal);
			}
		}




		/// <summary>Destroy all the components on  Animal and leaves the mesh and bones</summary>
		private void DestroyAllComponents(MAnimal target)
		{
			if (!target) return;

			var components = target.GetComponentsInChildren<MonoBehaviour>();
			foreach (var comp in components) Destroy(comp);
			var colliders = target.GetComponentsInChildren<Collider>();
			if (colliders != null)
			{
				foreach (var col in colliders) Destroy(col);
			}
			var rb = target.GetComponentInChildren<Rigidbody>();
			if (rb != null) Destroy(rb);
			var anim = target.GetComponentInChildren<Animator>();
			if (anim != null) Destroy(anim);
		}
	}
}