using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentUserPanelController : MonoBehaviour {
	public UnityEngine.UI.Text emailControlText;

	void Start () {
		if (SaferizeService.Instance ().getSaferizeData () != null) {
			emailControlText.text = "This app is managed by: \n" + SaferizeService.Instance ().getSaferizeData ().parentEmail;		
		}
	}

	public void ExitPanel(){
		SaferizeService.Instance ().CloseSaferizeParents ();
	}
}
