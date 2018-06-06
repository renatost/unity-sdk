using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentUserPanelController : MonoBehaviour {
	public UnityEngine.UI.Text emailControlText;
	public GameObject SupportMessage;
	public GameObject BackToSignUp;

	private SaferizeData _saferizeData;
    
	void Start () {
		_saferizeData = SaferizeService.Instance().getSaferizeData();
		if (_saferizeData != null) {
			emailControlText.text = "This app is managed by: \n" + _saferizeData.approval.ParentEmail;		
		}

		if(_saferizeData != null && _saferizeData.approval.Status == SaferizeSDK.Approval.StatusEnum.PENDING)
		{
			BackToSignUp.SetActive(true);
		}else{
			SupportMessage.SetActive(true);
		}
	}

    public void backToSignUp()
	{
		SaferizeService.Instance().OpenSignUpPanel();
	}

	public void ExitPanel(){
		SaferizeService.Instance ().CloseSaferizeParents ();
	}
}
