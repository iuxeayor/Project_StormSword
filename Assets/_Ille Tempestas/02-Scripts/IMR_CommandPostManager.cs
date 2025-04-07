//IMR_CommandPostManager.cs
/*
 * 
 */



using UnityEngine;
using System.Collections;



public class IMR_CommandPostManager : MonoBehaviour
{
	public int CPID;  //Command Post ID
	public int TeamID;  //ID of command Post within its Team list - easier to access when removing.
	//Health CPintegrity;  //CP = CommandPost
	//Bonus bonusScript;



	void Start()
    {
        //CPintegrity = GetComponentInChildren<Health>();
		//bonusScript = GetComponentInChildren<Bonus>();
	}



	void Update()
	{
		/*if(CPintegrity.HP <= CPintegrity.MinHP)
		{
			if (transform.FindChild("Command Post") != null)
			{
				bonusScript.AddPointsToPlayerScore();
				bonusScript.AddPlayerCPPoints();

				Destroy(transform.FindChild("Command Post").gameObject);
			}
		}*/
	}
}