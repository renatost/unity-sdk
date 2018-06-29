using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaferizeSDK;

public class SampleSaferizeActivator : MonoBehaviour
{
	void Awake()
	{
		SaferizeService.Instance ().ClearSaferizeData ();    
	}

	public void OpenSaferize(){
		SaferizeService.Instance ().OpenSaferizeParents ();
	}

	public void CloseSaferize(){
		SaferizeService.Instance ().CloseSaferizeParents ();
	}

	public void RemoveDatFile(){
		SaferizeService.Instance().ClearSaferizeData();
	}
}
