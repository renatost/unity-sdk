using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnPausePanelController : MonoBehaviour, ISaferizeOnPause {

	public void OnPause()
	{
		Time.timeScale = 0;
	}

}
