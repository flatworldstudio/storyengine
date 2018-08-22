using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if NETWORKED
using UnityEngine.Networking;

#endif

namespace StoryEngine
{
	
	public delegate bool DataTaskHandler (StoryTask theTask);
	
	public class DataController : MonoBehaviour
	{
	
		GameObject StoryEngineObject;
		DataTaskHandler dataTaskHandler;

		#if NETWORKED
		GameObject NetworkObject;
		NetworkBroadcast networkBroadcast;
		ExtendedNetworkManager networkManager;
		#endif

		AssitantDirector ad;

		bool handlerWarning = false;

		public List <StoryTask> taskList;

		string me = "Data controller";

		void Start ()
		{
			Log.Message ("Starting.", me);

			taskList = new List <StoryTask> ();

			#if NETWORKED

			NetworkObject = GameObject.Find ("NetworkObject");

			if (NetworkObject == null) {

				Log.Warning ("NetworkObject not found.", me);

			} else {

				networkBroadcast = NetworkObject.GetComponent<NetworkBroadcast> ();
				networkManager = NetworkObject.GetComponent<ExtendedNetworkManager> ();

			}

			#endif

			StoryEngineObject = GameObject.Find ("StoryEngineObject");

			if (StoryEngineObject == null) {

				Log.Warning ("StoryEngineObject not found.", me);

			} else {
			
				ad = StoryEngineObject.GetComponent <AssitantDirector> ();
				ad.newTasksEvent += new NewTasksEvent (newTasksHandler); // registrer for task events

			}

		}
		

		#if UNITY_IOS && NETWORKED
	

	void OnApplicationPause(bool paused){


		if (paused) {

			Log.Message ("pauzing ...",me);
			Log.Message ("Disconnecting client ...",me);


			if (networkManager.client != null) {

	stopNetworkClient();

//				networkManager.client.Disconnect ();

//	networkManager.client.Shutdown ();

//	Network.CloseConnection(Network.connections[0], true);

//	NetworkManager.InformServerOnDisconnect();




	//or maybe better use  Shutdown(); 



			}


		} else {

			Log.Message ("resuming ...",me);

		}
	}

	#endif

		#if NETWORKED

		// These are networking methods to be called from datahandler to establish connections.
		// Once connected, handling is done internally by the assistant directors.

		public int serverConnections ()
		{

			// Get a count for the number of (active) connections.

			NetworkConnection[] connections = new NetworkConnection[NetworkServer.connections.Count];

			NetworkServer.connections.CopyTo (connections, 0);

			int c = 0;

			foreach (NetworkConnection nc in connections) {

//			if (nc.isConnected) {

				if (nc != null) {
				
					c++;

				}

			}
			return c;

		}

		public void startBroadcastClient ()
		{

			Log.Message ("Starting broadcast client.", me);

			networkBroadcast.StartClient ();

		}

		public void startBroadcastServer ()
		{

			Log.Message ("Starting broadcast server.", me);

			networkBroadcast.StartServer ();

		}

		public void stopBroadcastServer ()
		{

			Log.Message ("Stopping broadcast server.", me);

			networkBroadcast.Stop ();

		}

		public void startNetworkClient (string server)
		{
		
			if (server == "") {
				Log.Error ("trying to start client without server address", me);
				return;
			}

			Log.Message ("Starting client for remote server " + server, me);

			networkManager.StartNetworkClient (server);

		}

		public void stopNetworkClient ()
		{

			Log.Message ("Stopping network client.", me);

			networkManager.StopClient ();

		}

		public void startNetworkServer ()
		{

			networkManager.StartNetworkServer ();

		}

		public void stopNetworkServer ()
		{

			networkManager.StopNetworkServer ();

		}

		public bool foundServer ()
		{
		
            // Checks if the message from the server is the one we're looking for. If so, returns true.
            // This allows for different environments - a dev intance cannot connect to a production instance.
            // Could include versions, but would have to generate user feedback.

            // We use a search because the message is 512 chars

                if (networkBroadcast.serverMessage.IndexOf(GENERAL.connectionKey)!=-1) 
            {
			
				return true;

			} else {
			
				return false;
			}

		}

		public bool clientIsConnected ()
		{

			if (networkManager.client == null)
				return false;

			if (!networkManager.client.isConnected)
				return false;

			return true;

		}

		#endif

		public void addTaskHandler (DataTaskHandler theHandler)
		{
			dataTaskHandler = theHandler;
			Log.Message ("Handler added.", me);
		}

		void Update ()
		{
		
			int t = 0;

			while (t < taskList.Count) {

				StoryTask task = taskList [t];

//			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {
				
				if (!GENERAL.ALLTASKS.Exists (at => at == task)) {
					
					Log.Message ("Removing task:" + task.description, me);

					// Task was removed, so stop executing it.

					taskList.RemoveAt (t);

				} else {

					if (dataTaskHandler != null) {

						if (dataTaskHandler (task)) {

							task.signOff (me);
							taskList.RemoveAt (t);

						} else {
						
							t++;

						}

					} else {
					
						if (!handlerWarning) {
						
							Log.Warning ("No handler available, blocking task while waiting.", me);

							handlerWarning = true;
							t++;

						} 

					}

				}

			}

		}

		//	public void taskDone (StoryTask theTask){
		//		theTask.signOff (me);
		//		taskList.Remove (theTask);
		//
		//	}

		void newTasksHandler (object sender, TaskArgs e)
		{
	
			addTasks (e.theTasks);

		}

		public void addTasks (List<StoryTask> theTasks)
		{
		
			taskList.AddRange (theTasks);

		}

		//

		#if NETWORKED

		public bool displayNetworkGUIState ()
		{


			return		networkBroadcast.showGUI;


		}


		public void displayNetworkGUI (bool status)
		{


			NetworkObject.GetComponent<NetworkManagerHUD> ().showGUI = status;
			networkBroadcast.showGUI = status;


		}

		#endif
	}




}