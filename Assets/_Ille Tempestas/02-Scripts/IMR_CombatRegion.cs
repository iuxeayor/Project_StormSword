//IMR_CombatRegion.cs
/* 
 *  Used on regions designed for Combat Zones.
 *  This script allows the player to capture the region,
 *		but enemies within the region can stop the capture.
 *  Neither enemies nor allies can actually capture the
 *		region, though; only players.
*/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static IMR_CharacterAttributes;



public class IMR_CombatRegion : MonoBehaviour
{
	public List<GameObject> CharactersInRegionList;

	//public string CurrentOwner = "BlankPost";  //use Tags:  BlankPost [not currently owned], GreenPost, RedPost
	IMR_CombatZoneProperties CZmanager;
	IMR_CommandPostHealth CPhealth;
	public IMR_CombatZoneProperties.CZOwner RegionCZ_Owner;

	//This bool might not be necessary as there is a counter in the Combat Zone Properties instead. Not sure yet.
	[Tooltip("Is this region captured yet? Set automatically to True once Captured. Used by Combat Zones.")]
	public bool IsRegionCaptured = false;

	//These don't need to be public after done testing.
	public bool RedPresent = false;
	public bool GreenPresent = false;

	public float TimerRate = .5f;
	float NextRunTime = -1f;

	public int MaxDistance = 8;  //maximum distance to be counted as still within the region.  Used when OnExit fails.

	public Material BlankMaterial;
	public Material RedMaterial;
	public Material GreenMaterial;
	


	public void Start()
	{
		CZmanager = transform.parent.GetComponent<IMR_CombatZoneProperties>();
		CPhealth = transform.parent.Find("Command Post").GetComponent<IMR_CommandPostHealth>();
		RegionCZ_Owner = CZmanager.CurrentCZ_Owner;
	}



	public void Update()
	{
		if (Time.time >= NextRunTime)
		{
			NextRunTime = Time.time + TimerRate;

			CharacterActuallyWithinRange();

			if (CZmanager.IsCombatZoneActive)
			{
				//First, check to make sure that everyone in the list is actually within the Region.
				//Unity does not call OnExitTrigger() when objects are disabled/destroyed within the Region, so 
				// members may be in the list for a long time.
				//CharacterActuallyWithinRange();

				if (RedPresent && GreenPresent)
				{
					//DoNothing();
				}

				else if (!RedPresent && GreenPresent)  //Green is set to Player-only; don't care about bots.
				{
					if (RegionCZ_Owner == IMR_CombatZoneProperties.CZOwner.BlankPost)
					{
						if (CPhealth.RedHP > CPhealth.MinHP)
						{
							CPhealth.DepleteRedHP();
						}

						else if ((CPhealth.GreenHP >= CPhealth.MinHP && CPhealth.GreenHP < CPhealth.MaxHP) && CPhealth.RedHP <= CPhealth.MinHP)
						{
							CPhealth.ReplenishGreenHP();
						}

						//===================Combat Zone has been Conquered.=========================================
						else if (CPhealth.GreenHP >= CPhealth.MaxHP)
						{
							IsRegionCaptured = true;
							CZmanager.CombatGoalAccomplished();

						}
					}
				}

				else
				{
					//DoNothing();
				}
			}
		}
	}



	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponent<IMR_CharacterAttributes>() != null)
		{
			CharacterTags guestTags = other.gameObject.GetComponent<IMR_CharacterAttributes>().MyCharacterTags;

			if (guestTags == CharacterTags.RedTeam || guestTags == CharacterTags.GreenTeam || guestTags == CharacterTags.Player)
			{
				AddToList(other.gameObject);
			}

			CheckWhosInRegion();
		}
	}



	public void OnTriggerExit(Collider other)
	{
		RemoveFromList(other.gameObject);

		CheckWhosInRegion();
	}



	public void RemoveFromList(GameObject other)
	{
		if (CharactersInRegionList.Contains(other))
		{
			CharactersInRegionList.Remove(other);
		}
	}



	public void AddToList(GameObject other)
	{
		if (!CharactersInRegionList.Contains(other))
		{
			CharactersInRegionList.Add(other);
		}
	}



	public void CheckWhosInRegion()
	{
		if (CharactersInRegionList.Count > 0 && CharactersInRegionList != null)
		{
			for (int i = 0; i < CharactersInRegionList.Count; i++)
			{
				if (CharactersInRegionList[i] != null)
				{
					try
					{
						IMR_CharacterAttributes MyAttributes = CharactersInRegionList[i].GetComponent<IMR_CharacterAttributes>();

						if (MyAttributes.MyCharacterTags == CharacterTags.RedTeam && MyAttributes.Alive)
						{
							RedPresent = true;
							break;
						}

						else
						{
							RedPresent = false;
						}
					}

					catch
					{
						RemoveFromList(CharactersInRegionList[i]);
					}
				}
			}

			for (int i = 0; i < CharactersInRegionList.Count; i++)
			{
				if (CharactersInRegionList[i] != null)
				{
					try 
					{
						IMR_CharacterAttributes MyAttributes = CharactersInRegionList[i].GetComponent<IMR_CharacterAttributes>();

						//Note: only players should be counted in the combat capture region, not ally bots.
						if (MyAttributes.MyCharacterTags == CharacterTags.Player && MyAttributes.Alive)
						{
							GreenPresent = true;
							break;
						}

						else
						{
							GreenPresent = false;
						}
					}

					catch
					{
						RemoveFromList(CharactersInRegionList[i]);
					}
				}
			}
		}

		else
		{
			GreenPresent = false;
			RedPresent = false;
		}
	}



	//Removes characters who are not close enough to the CP to capture it.
	public void CharacterActuallyWithinRange()
	{
		float distance = MaxDistance * MaxDistance;  // use a square of the value you want or Mathf.Infinity.
		Vector3 position = transform.position;

		if (CharactersInRegionList.Count > 0 && CharactersInRegionList != null)
		{
			CheckWhosInRegion();

			for (int i = 0; i < CharactersInRegionList.Count; i++)
			{
				try
				{
					Vector3 diff = CharactersInRegionList[i].transform.position - position;
					float curDistance = diff.sqrMagnitude;

					if (curDistance > distance)
					{
						//not in range
						RemoveFromList(CharactersInRegionList[i]);
					}
				}

				catch 
				{
					RemoveFromList(CharactersInRegionList[i]);
				}
			}

			CheckWhosInRegion();
		}
	}
}
