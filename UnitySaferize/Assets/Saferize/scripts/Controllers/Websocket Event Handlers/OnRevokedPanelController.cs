using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnRevokedPanelController : MonoBehaviour, ISaferizeOnRevoke{

	public void OnRevoke()
	{
		gameObject.SetActive (true);
	}
}
