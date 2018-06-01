using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using System;

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
		storedPINhash = SaferizeService.Instance().getSaferizeData().PINhash;
  
		if(storedPINhash != null){
			PINInputPanel.SetActive(true);
			PIN1.ActivateInputField();
		    SetupInputHandling();
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
		if(storedPINhash != null && storedPINhash == SHA256Hash(inputedPin)){
			OnOfflineEnd();
		}else{
			wrongPinErrorText.SetActive(true);
		}
	}

	private string SHA256Hash(string text)
    {
        SHA256 sha256 = new SHA256CryptoServiceProvider();
        sha256.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
        byte[] result = sha256.Hash;
      
		return Convert.ToBase64String(result);
    }

	private void SetupInputHandling(){
		PIN1.onValueChanged.AddListener((string value) =>
        {
            if (value.Length != 0)
            {
                PIN2.ActivateInputField();
            }
        });

        PIN2.onValueChanged.AddListener((string value) =>
        {
            if (value.Length != 0)
            {
                PIN3.ActivateInputField();
            }
        });

        PIN3.onValueChanged.AddListener((string value) =>
        {
            if (value.Length != 0)
            {
                PIN4.ActivateInputField();
            }
        });

        PIN4.onValueChanged.AddListener((string value) =>
        {
            if (value.Length != 0)
            {
                PIN4.DeactivateInputField();
                ComparePIN();
            }
        });
	}
}
    