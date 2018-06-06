using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using SaferizeSDK;

public class SignupPanelController : MonoBehaviour {

	public InputField parentEmailInputField;
	public Text errorLabel;
	public GameObject ThanksContainer;
	public GameObject InputContainer;

	private string token;

	private static Regex emailValidator = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[0-9a-zA-Z]+$");

	public void StartParentalControlClicked() {
		errorLabel.gameObject.SetActive(false);

		if(!emailValidator.IsMatch (parentEmailInputField.text)){
			errorLabel.gameObject.SetActive(true);
			return;
		}
			
		if(token == null) {
			token = SystemInfo.deviceUniqueIdentifier;
		};

		SaferizeService.Instance ().SignUp (parentEmailInputField.text, token);
		InputContainer.SetActive (false);
		ThanksContainer.SetActive (true);
	}

	public void CloseSaferize(){
		SaferizeService.Instance().CloseSaferizeParents();
	}

	public void ShowSignup(){
		ThanksContainer.SetActive(false);
		InputContainer.SetActive(true);
	}
    
}
