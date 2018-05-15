using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnResumePanelController : MonoBehaviour, ISaferizeOnResume{

	public void OnResume()
	{
		Time.timeScale = 1;
	}
}
