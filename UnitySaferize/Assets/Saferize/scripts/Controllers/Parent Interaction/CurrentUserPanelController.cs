using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentUserPanelController : MonoBehaviour {
	public UnityEngine.UI.Text emailControlText;

	private SaferizeData _saferizeData;
    
	void Start () {
		_saferizeData = SaferizeService.Instance().getSaferizeData();
		if (_saferizeData != null) {
			emailControlText.text = "This app is managed by: \n" + _saferizeData.approval.ParentEmail;		
		}
	}

	public void ExitPanel(){
		SaferizeService.Instance ().CloseSaferizeParents ();
	}
}
