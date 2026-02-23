//IMR_CaptureRegion.cs
/*
 * This is for Command Posts, primarily. Not used for CZs.
 * Use Combat Regions for CZs.
*/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class IMR_CaptureRegion : MonoBehaviour
{
	public List<GameObject> CharactersInRegionList;

	public GameObject CenterPoint;  //the object that denotes who owns this CP (used to be the sphere called "Command Post").

	public string CurrentOwner = "BlankPost";  //use Tags:  BlankPost [not currently owned], GreenPost, RedPost
	IMR_CommandPostManager CPmanager;
	IMR_CommandPostHealth CPhealth;
	MeshRenderer CPColor;
	IMR_Gameplay GameManager;


	//These don't need to be public after done testing.
	public bool RedPresent = false;
	public bool GreenPresent = false;

	public float TimerRate = .5f;
	float NextRunTime = -1f;

	public Material BlankMaterial;
	public Material RedMaterial;
	public Material GreenMaterial;
	


	public void Start()
	{
		tag = CurrentOwner;
		transform.parent.tag = CurrentOwner;
		CenterPoint.tag = CurrentOwner;

		GameManager = GameObject.Find("Game").GetComponent<IMR_Gameplay>();
		CPmanager = transform.parent.GetComponent<IMR_CommandPostManager>();
		CPhealth = CenterPoint.GetComponent<IMR_CommandPostHealth>();
		CPColor = CenterPoint.GetComponent<MeshRenderer>();

		switch (CurrentOwner)
		{
			case "BlankPost":
				CPColor.material = BlankMaterial;
				
				break;

			case "RedPost":
				CPColor.material = RedMaterial;
				
				break;

			case "GreenPost":
				CPColor.material = GreenMaterial;
				
				break;

			default:
				break;
		}
	}



	public void Update()
	{
		if (Time.time >= NextRunTime)
		{
			NextRunTime = Time.time + TimerRate;

			//First, check to make sure that everyone in the list is actually within the Region.
			//Unity does not call OnExitTrigger() when objects are disabled/destroyed within the Region, so 
			// members may be in the list for a long time.
			CharacterActuallyWithinRange();

			if (RedPresent && GreenPresent)
			{
				//DoNothing();
			}

			else if (RedPresent && !GreenPresent)
			{
				if (CurrentOwner == "BlankPost")
				{
					if (CPhealth.GreenHP > CPhealth.MinHP)
					{
						CPhealth.DepleteGreenHP();
					}

					else if ((CPhealth.RedHP >= CPhealth.MinHP && CPhealth.RedHP < CPhealth.MaxHP) && CPhealth.GreenHP <= CPhealth.MinHP)
					{
						CPhealth.ReplenishRedHP();
					}

					else if (CPhealth.RedHP >= CPhealth.MaxHP)
					{
						CurrentOwner = "RedPost";
						tag = CurrentOwner;
						transform.parent.tag = CurrentOwner;
						CenterPoint.tag = CurrentOwner;

						//Add to RedPost list.
						GameManager.AddSpawnPostToOwnerList(CPmanager.CPID, "RedPost");

						CPColor.material = RedMaterial;  //change CP color
					}
				}

				else if (CurrentOwner == "RedPost")
				{
					if (CPhealth.RedHP < CPhealth.MaxHP)
					{
						CPhealth.ReplenishRedHP();
					}

					else if (CPhealth.RedHP >= CPhealth.MaxHP)
					{
						//DoNothing();
					}
				}

				else if (CurrentOwner == "GreenPost")
				{
					if (CPhealth.GreenHP > CPhealth.MinHP)
					{
						CPhealth.DepleteGreenHP();
					}

					else if (CPhealth.GreenHP <= CPhealth.MinHP)
					{
						CurrentOwner = "BlankPost";
						tag = CurrentOwner;
						transform.parent.tag = CurrentOwner;
						CenterPoint.tag = CurrentOwner;

						//Remove from GreenPost list.
						GameManager.RemoveSpawnPostFromOwnerList(CPmanager.CPID, "GreenPost");

						CPColor.material = BlankMaterial;  //change CP color
					}
				}
			}

			else if (!RedPresent && GreenPresent)
			{
				if (CurrentOwner == "BlankPost")
				{
					if (CPhealth.RedHP > CPhealth.MinHP)
					{
						CPhealth.DepleteRedHP();
					}

					else if ((CPhealth.GreenHP >= CPhealth.MinHP && CPhealth.GreenHP < CPhealth.MaxHP) && CPhealth.RedHP <= CPhealth.MinHP)
					{
						CPhealth.ReplenishGreenHP();
					}

					else if (CPhealth.GreenHP >= CPhealth.MaxHP)
					{
						CurrentOwner = "GreenPost";
						tag = CurrentOwner;
						transform.parent.tag = CurrentOwner;
						CenterPoint.tag = CurrentOwner;

						//Add to GreenPost list.
						GameManager.AddSpawnPostToOwnerList(CPmanager.CPID, "GreenPost");

						CPColor.material = GreenMaterial;  //change CP color
					}
				}

				else if (CurrentOwner == "RedPost")
				{
					if (CPhealth.RedHP > CPhealth.MinHP)
					{
						CPhealth.DepleteRedHP();
					}

					else if (CPhealth.RedHP <= CPhealth.MinHP)
					{
						CurrentOwner = "BlankPost";
						tag = CurrentOwner;
						transform.parent.tag = CurrentOwner;
						CenterPoint.tag = CurrentOwner;

						//Remove from RedPost list.
						GameManager.RemoveSpawnPostFromOwnerList(CPmanager.CPID, "RedPost");

						CPColor.material = BlankMaterial;  //change CP color
					}
				}

				else if (CurrentOwner == "GreenPost")
				{
					if (CPhealth.GreenHP < CPhealth.MaxHP)
					{
						CPhealth.ReplenishGreenHP();
					}

					else if (CPhealth.GreenHP >= CPhealth.MaxHP)
					{
						//DoNothing();
					}
				}
			}

			else
			{
				//DoNothing();
			}
		}
	}



	public void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "RedTeam" || other.gameObject.tag == "GreenTeam" || other.gameObject.tag == "Player")
		{
			AddToList(other.gameObject);
		}

		CheckWhosInRegion();
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
			for(int i = 0; i < CharactersInRegionList.Count; i++)
			{
				if (CharactersInRegionList[i] != null)
				{
					try
					{
						if (CharactersInRegionList[i].CompareTag("RedTeam") && CharactersInRegionList[i].GetComponent<IMR_CharacterAttributes>().Alive)
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
						if ((CharactersInRegionList[i].CompareTag("GreenTeam") || CharactersInRegionList[i].CompareTag("Player")) && CharactersInRegionList[i].GetComponent<IMR_CharacterAttributes>().Alive)
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
	}



	//Removes characters who are not close enough to the CP to capture it.
	public void CharacterActuallyWithinRange()
	{
		float distance = 12f * 12f;  // use a square of the value you want or Mathf.Infinity.
		Vector3 position = transform.position;

		if (CharactersInRegionList.Count > 0 && CharactersInRegionList != null)
		{
			for (int i = 0; i < CharactersInRegionList.Count; i++)
			{
				Vector3 diff = CharactersInRegionList[i].transform.position - position;
				float curDistance = diff.sqrMagnitude;

				if (curDistance > distance)
				{
					//not in range
					RemoveFromList(CharactersInRegionList[i]);
				}
			}

			CheckWhosInRegion();
		}
	}
}
