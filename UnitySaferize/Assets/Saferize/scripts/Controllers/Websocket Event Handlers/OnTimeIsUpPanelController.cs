using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTimeIsUpPanelController : MonoBehaviour, ISaferizeOnTimeIsUp{

	public void OnTimeIsUp()
	{
		Time.timeScale = 0;
	}
}
