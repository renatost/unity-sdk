using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThanksContainerController : MonoBehaviour {
	public Text ThankYouText;

	void OnEnable () {
		ThankYouText.text = "Instructions have been sent to: \n\n" + SaferizeService.Instance ().getSaferizeData ().parentEmail + "\n\nPlease check your email for details!";
	}
}
