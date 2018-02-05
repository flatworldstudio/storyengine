using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if NETWORKED
using UnityEngine.Networking;

#endif

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

	string me = "Data controller: ";

	void Start ()
	{
		Debug.Log (me + "Starting...");

		taskList = new List <StoryTask> ();

		#if NETWORKED

		NetworkObject = GameObject.Find ("NetworkObject");

		if (NetworkObject == null) {

			Debug.LogWarning (me + "NetworkObject not found.");

		} else {

			networkBroadcast = NetworkObject.GetComponent<NetworkBroadcast> ();
			networkManager = NetworkObject.GetComponent<ExtendedNetworkManager> ();

		}

		#endif

		StoryEngineObject = GameObject.Find ("StoryEngineObject");

		if (StoryEngineObject == null) {

			Debug.LogWarning (me + "StoryEngineObject not found.");

		} else {
			
			ad = StoryEngineObject.GetComponent <AssitantDirector> ();
			ad.newTasksEvent += new NewTasksEvent (newTasksHandler); // registrer for task events

		}

	}
		

	#if UNITY_IOS && NETWORKED
	

	void OnApplicationPause(bool paused){


		if (paused) {

			Debug.Log (me + "pauzing ...");
			Debug.Log (me + "Disconnecting client ...");


			if (networkManager.client != null) {

	StopNetworkClient();

//				networkManager.client.Disconnect ();

//	networkManager.client.Shutdown ();

//	Network.CloseConnection(Network.connections[0], true);

//	NetworkManager.InformServerOnDisconnect();




	//or maybe better use  Shutdown(); 



			}


		} else {

			Debug.Log (me + "resuming ...");

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

		Debug.Log (me + "Starting broadcast client.");

		networkBroadcast.StartClient ();

	}

	public void startBroadcastServer ()
	{

		Debug.Log (me + "Starting broadcast server.");

		networkBroadcast.StartServer ();

	}

	public void stopBroadcast ()
	{

		Debug.Log (me + "Stopping broadcast server.");

		networkBroadcast.Stop ();

	}

	public void startNetworkClient (string server)
	{
		
		if (server == "") {
			Debug.LogError (me + "trying to start client without server address");
			return;
		}

		Debug.Log (me + "Starting client for remote server " + server);

		networkManager.StartNetworkClient (server);

	}

	public void StopNetworkClient(){

		Debug.Log (me + "Stopping network client.");

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
		
		if (networkBroadcast.serverMessage != "") {
			
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
		Debug.Log (me + "Handler added");
	}

	void Update ()
	{
		
		int t = 0;

		while (t < taskList.Count) {

			StoryTask task = taskList [t];

//			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {
				
			if (!GENERAL.ALLTASKS.Exists(at => at==task )) {
					
				Debug.Log (me + "Removing task:" + task.description);

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
						
						Debug.LogWarning (me + "No handler available, blocking task while waiting.");
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




