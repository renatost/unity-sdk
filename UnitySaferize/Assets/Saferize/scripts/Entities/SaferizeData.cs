using System;
using SaferizeSDK;

[Serializable]
public class SaferizeData {
	public string token;
	public DateTime lastLogin;
	public string parentEmail;
	public string PINhash;
	public Approval approval;
	public AppFeature[] appFeatures;
}
