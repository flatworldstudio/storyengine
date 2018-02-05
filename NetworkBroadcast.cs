using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkBroadcast : NetworkDiscovery
{
	
	public string serverAddress, serverMessage;

	string me = "Networkbroadcast: ";

	bool resumeClient = false;
	bool resumeServer = false;

	public void ResetMessage ()
	{

		serverAddress = "";
		serverMessage = "";

	}

	public void StartClient ()
	{

		Initialize ();
		ResetMessage (); // just to be sure.
		StartAsClient ();

	}

	void OnApplicationPause (bool paused)
	{

		if (paused) {
			
			if (isClient) {

				resumeClient = true;
				Stop ();

				Debug.Log (me + "Pausing broadcast client.");

			}

			if (isServer) {

				resumeServer = true;
				Stop ();

				Debug.Log (me + "Pausing broadcast server.");

			}

		} else {
			
			if (resumeClient) {
				
				resumeClient = false;
				StartClient ();

				Debug.Log (me + "Resuming broadcast client.");

			}

			if (resumeServer) {

				resumeServer = false;
				StartServer ();

				Debug.Log (me + "Resuming broadcast server.");

			}
							
		}

	}

	public void StartServer ()
	{

		Initialize ();
		ResetMessage (); // just to be sure.
		StartAsServer ();

	}

	public void Stop ()
	{
		
		StopBroadcast ();
		ResetMessage ();

	}

	public override void OnReceivedBroadcast (string fromAddress, string data)
	{

		// Handler to respond to received broadcast message event.
		// Since our engine is loop based, we just store the info for the loop to pick up on.

		Debug.Log (me + "Received broadcast: " + data + " from " + fromAddress);

		serverMessage = data;
		serverAddress = fromAddress;

		GENERAL.broadcastServer = fromAddress;

	}

}
