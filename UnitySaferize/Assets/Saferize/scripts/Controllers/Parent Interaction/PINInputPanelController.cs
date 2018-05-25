using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PINInputPanelController : MonoBehaviour, ISaferizeOnOfflineStart, ISaferizeOnOfflineEnd {
	// Use this for initialization
	public GameObject PINInputPanel;
	public GameObject NoPINInputPanel;
	public InputField PIN1;
	public InputField PIN2;
	public InputField PIN3;
	public InputField PIN4;
	public GameObject wrongPinErrorText;

	private string storedPINhash;

	void Start () {
		//check to see if there is a dat file and if the dat file contains information relating to a pin
		storedPINhash = SaferizeService.Instance().getSaferizeData().PINhash;
		if(storedPINhash != null){
			PINInputPanel.SetActive(true);
		}else{
			NoPINInputPanel.SetActive(true);
		}
	}

	public void OnOfflineStart()
	{
		Time.timeScale = 0;
	}

    public void OnOfflineEnd()
	{
		Time.timeScale = 1;
		Destroy(gameObject);
	}
    
    public void ComparePIN()
	{
		wrongPinErrorText.SetActive(false);

		var inputedPin = PIN1.text + PIN2.text + PIN3.text + PIN4.text;
		Debug.Log(inputedPin);
		Debug.Log(storedPINhash);

		// NEED TO HASH THE INPUTED PIN BEFORE COMPARING
		if(storedPINhash != null && storedPINhash == inputedPin){
			OnOfflineEnd();
		}else{
			wrongPinErrorText.SetActive(true);
		}
	}
}
