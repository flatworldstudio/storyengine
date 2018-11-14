using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace StoryEngine
{
	
	public class NetworkBroadcast : NetworkDiscovery
	{
	
		public string serverAddress, serverMessage;

		string me = "Networkbroadcast";

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

					Log.Message ("Pausing broadcast client.", me);

				}

				if (isServer) {

					resumeServer = true;
					Stop ();

					Log.Message ("Pausing broadcast server.", me);

				}

			} else {
			
				if (resumeClient) {
				
					resumeClient = false;
					StartClient ();

					Log.Message ("Resuming broadcast client.", me);

				}

				if (resumeServer) {

					resumeServer = false;
					StartServer ();

					Log.Message ("Resuming broadcast server.", me);

				}
							
			}

		}

		public void StartServer ()
		{

            broadcastData = GENERAL.connectionKey; // get message string. default is HELLO.
            
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

			Log.Message ("Received broadcast: " + data + " from " + fromAddress, me);

			serverMessage = data;
			serverAddress = fromAddress;

			GENERAL.broadcastServer = fromAddress;
            GENERAL.receivedMessage = data; // store message string. allow for comparing messages, eg for different versions or environments.

		}

	}

}
