//IMR_CharacterAttributes.cs
/*
 * Formerly DeadOrAlive.cs. 
 * Was originally used to determine if the GameObject this is attached to is Dead or Alive.
 * Created to easily test HP from asset-system to determine if a bot within a trigger region
 *  was actually dead since triggers can't determine Exits based on destruction or deactivation.
 *  
 * Now, as CharacterAttributes, more character-defining traits will be added to the script.
 * Among the new attributes are tags, which can have multiple assignments (in the future).
 *  
 *  Be sure to set Enemies with RedTeam tags in the Editor,
 *   Allies with GreenTeam tags,
 *   and Players (not Bots) with Player tags.
 *  Tags are set manually.
*/



using UnityEngine;
using MalbersAnimations;



public class IMR_CharacterAttributes : MonoBehaviour
{
	public bool Alive = false;
	public Stats MyHPStat;
	public float CurrentHP = 0;

	public enum CharacterTags 
	{
		Blank
		,RedTeam
		,GreenTeam
		,Player
	};

	[Tooltip("Use to specify character-defining traits: Blank, RedTeam, GreenTeam, Player")]
	public CharacterTags MyCharacterTags;



	protected void Awake ()
	{
		MyHPStat = GetComponent<Stats>();
		CurrentHP = MyHPStat.Stat_Get("Health").Value;

	}



	void Update ()
	{
		CurrentHP = MyHPStat.Stat_Get("Health").Value;

		if (CurrentHP <= 0)
		{
			Alive = false;
		}

		else
		{
			Alive = true;
		}

	}
}
