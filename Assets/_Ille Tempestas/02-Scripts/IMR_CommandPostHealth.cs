//IMR_CommandPostHealth.cs



using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class IMR_CommandPostHealth : MonoBehaviour 
{
	public float RedHP = 0;
	public float GreenHP = 0;
	public float MinHP = 0f;
	public float MaxHP = 100f;


	[Tooltip("Use to set how much HP is lost at each interval of time.")]
	public float LossAmount = 10f;
	[Tooltip("Use to set how much HP is gained at each interval of time.")]
	public float GainAmount = 10f;

	

	public void DepleteRedHP()
	{
		RedHP -= LossAmount;
	}


	public void DepleteGreenHP()
	{
		GreenHP -= LossAmount;
	}



	public void ReplenishRedHP()
	{
		RedHP += GainAmount;
	}



	public void ReplenishGreenHP()
	{
		GreenHP += GainAmount;
	}
}