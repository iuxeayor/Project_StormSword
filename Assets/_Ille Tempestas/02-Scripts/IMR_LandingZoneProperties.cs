//IMR_LandingZoneProperties.cs
/* 
 * The Landing Zone Game Mode is basically Combat Zone with sequential
 *	zones, as opposed to randomly selected ones.
 *	
 * The player plays through the game in a mostly linear fashion.
 * Also, instead of capturing the zone via regions, players must destroy targets.
 * Once all designated targets are destroyed, the region is captured and the players
 *	must move onto the next Landing Zone area.
 *	
 * Contains lists of SpawnPoints, lists of Waypoints (or Wander Regions),
 *  and an Active status.
 *  
 *  To use, set the GameMode to LandingZone in the GameSetup component.
 *  Place this script on a Landing Zone object (empty GameObject).
*/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class IMR_LandingZoneProperties : MonoBehaviour
{
	public int LandingZoneID = 777;
	public bool LandingZoneActive = false;
	public bool AlreadyUsed = false;  //true when used once. Set back to false if all CZs have already had a turn and game still continues.

	public List<GameObject> LocalRedCPs = new List<GameObject>();
	public List<GameObject> LocalGreenCPs = new List<GameObject>();
	public List<GameObject> LocalWaypoints = new List<GameObject>();
}
