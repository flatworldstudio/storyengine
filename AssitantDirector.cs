//#define NETWORKED

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


#if NETWORKED
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

#endif

public delegate void NewTasksEvent (object sender, TaskArgs e);

public class AssitantDirector : MonoBehaviour
{
	
	public event NewTasksEvent newTasksEvent;

	string me = "Assistant director: ";

	Director theDirector;
	public string scriptName;

	string launchOnStoryline;
	public string launchOSX, launchWIN, launchIOS;
//	public string main, remote;


	#if NETWORKED

	public ExtendedNetworkManager networkManager;

	const short stringCode = 1002;
	const short pointerCode = 1003;
	const short taskCode = 1004;

	#endif

	void Start ()
	{
		
		Debug.Log (me + "Starting ...");

		UUID.setIdentity ();

		Debug.Log (me + "Identity stamp " + UUID.identity);

		GENERAL.AUTHORITY = AUTHORITY.LOCAL;

		theDirector = new Director ();

		GENERAL.ALLTASKS = new List<StoryTask> ();


		/*
		#if OSX

		Debug.Log (me+"Running OSX storyline.");

		launchOnStoryline = launchOSX;

		#endif


		#if WIN

		Debug.Log (me+"Running WINDOWS storyline.");

		launchOnStoryline = launchWIN;

		#endif

		#if IOS

		Debug.Log (me+"Running IOS storyline.");

		launchOnStoryline = launchIOS;

		#endif
*/

//
//		launchOnStoryline = remote;
//
//		#endif



		#if UNITY_IOS

		Debug.Log (me+"Running on IOS platform");

		launchOnStoryline = launchIOS;

		#endif

		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

		Debug.Log (me + "Running on OSX platform");

		launchOnStoryline = launchOSX;

		#endif
				

		#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		Debug.Log (me+"Running on WINDOWS platform");

		launchOnStoryline = launchWIN;

		#endif


	


		#if NETWORKED

		// get the networkmanager to call network event methods on the assistant director.

		networkManager.onStartServerDelegate = onStartServer;
		networkManager.onStartClientDelegate = onStartClient;
		networkManager.onServerConnectDelegate = OnServerConnect;
		networkManager.onClientConnectDelegate = OnClientConnect;
		networkManager.onClientDisconnectDelegate = OnClientDisconnect;
		networkManager.onStopClientDelegate = OnStopClient;

		#endif

	}

	// IOS: first it leaves focus, then it pauzes
	// on return it enteres focus and then resumes

	void OnApplicationPause (bool paused)
	{


		if (paused) {

			Debug.Log (me + "pauzing ...");

		} else {

			Debug.Log (me + "resuming ...");

		}

	}

	void OnApplicationFocus (bool focus)
	{
		
		if (focus) {

			Debug.Log (me + "entering focus ...");

		} else {

			Debug.Log (me + "leaving focus ...");

		}

	}

	void Update ()
	{

//		// HACK
//		string inputString = Input.inputString;
//		if (inputString.Length > 0) {
//
//			if (inputString == "p") {
//
//				Debug.Log (me + "Simulating disconnect/pause ...");
//
//				theDirector.beginStoryLine ("disconnect");
//
//			}
//
//		}



		switch (theDirector.status) {

		case DIRECTORSTATUS.ACTIVE:

//			Debug.Log (me + "director active ...");

			foreach (StoryTask task in GENERAL.ALLTASKS) {

				if (task.getCallBack () != "") {

					// if a callback was set (somewhere on the network) we act on it only if we are the server or if the task is local.

					if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL || task.scope == SCOPE.LOCAL) {

						task.pointer.setStatus (POINTERSTATUS.TASKUPDATED);
					}

				}

			}

			theDirector.evaluatePointers ();

			List<StoryTask> newTasks = new List<StoryTask> ();

			for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++) {

				StoryPointer pointer = GENERAL.ALLPOINTERS [p];

				if (pointer.modified) {

					switch (pointer.scope) {

					case SCOPE.GLOBAL:

						// If pointer scope is global, we add a task if our own scope is global as well. (If our scope is local, we'll be receiving the task over the network)

						if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL) {

							if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

								pointer.setStatus (POINTERSTATUS.PAUSED);

								StoryTask task = new StoryTask (pointer, SCOPE.GLOBAL);

								task.loadPersistantData (pointer);

								pointer.currentTask = task;
								newTasks.Add (task);

								task.modified = true;

								Debug.Log (me + "Creating and distributing global task " + task.description + " for pointer " + pointer.currentPoint.storyLineName);

							}

						}

						break;

					case SCOPE.LOCAL:
					default:

						// If pointer scope is local, check if new tasks have to be generated.

						if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

							pointer.setStatus (POINTERSTATUS.PAUSED);

							StoryTask task = new StoryTask (pointer, SCOPE.LOCAL);

							task.loadPersistantData (pointer);

							pointer.currentTask = task;

							newTasks.Add (task);

							Debug.Log (me + "Creating local task " + task.description + " for pointer " + pointer.currentPoint.storyLineName);

						}

						break;

					}

				}

			}

			if (newTasks.Count > 0) {

				DistributeTasks (new TaskArgs (newTasks)); // if any new tasks call an event, passing on the list of tasks to any handlers listening
			}

			break;

		case DIRECTORSTATUS.READY:

			GENERAL.SIGNOFFS = eventHandlerCount ();

			if (GENERAL.SIGNOFFS == 0) {

				Debug.LogWarning (me + "No handlers registred. Pausing director.");
				theDirector.status = DIRECTORSTATUS.PAUSED;

			} else {

				Debug.Log (me + GENERAL.SIGNOFFS + " handlers registred.");

				Debug.Log (me + "Starting storyline " + launchOnStoryline);

				theDirector.beginStoryLine (launchOnStoryline);
				theDirector.status = DIRECTORSTATUS.ACTIVE;

//				Debug.Log (me + "Started storyline " + launchOnStoryline);

			}

			break;

		case DIRECTORSTATUS.NOTREADY:

			theDirector.loadScript (scriptName);

			break;

		default:
			break;
		}

	}

	#if NETWORKED

	void LateUpdate ()
	{

		// Iterate over all pointers.

		for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++) {

			StoryPointer pointer = GENERAL.ALLPOINTERS [p];

			if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL && pointer.scope == SCOPE.GLOBAL && pointer.modified) {

				Debug.Log (me + "Sending pointer update to clients. ID: " + pointer.ID);

				sendPointerUpdateToClients (pointer.getUpdateMessage ());

				pointer.modified = false;

			}

		}

		/*

		for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++) {

			StoryPointer pointer = GENERAL.ALLPOINTERS [p];

			if (pointer.modified) {
								
				switch (GENERAL.AUTHORITY) {

				case AUTHORITY.LOCAL:

					if (pointer.scope == SCOPE.GLOBAL) {

						Debug.LogWarning (me + "Global pointer " + pointer.ID + " | " + pointer.currentTask.description + " was changed locally.");

					}

					break;

				case AUTHORITY.GLOBAL:

					if (pointer.scope == SCOPE.GLOBAL) {

						Debug.Log (me + "Global pointer, global AD -> sending pointer update for " + pointer.ID);

						sendPointerUpdateToClients (pointer.getUpdateMessage ());

					}

					break;

				default:

					break;

				}

				pointer.modified = false;

			}

		}
		*/

		// Iterate over all tasks.

		for (int i = GENERAL.ALLTASKS.Count - 1; i >= 0; i--) {

			StoryTask task = GENERAL.ALLTASKS [i];

			// Cleanup completed tasks.

			if (task.getStatus () == TASKSTATUS.COMPLETE) {
				
				GENERAL.ALLTASKS.RemoveAt (i);

				Debug.Log (me + "Task " + task.description + " completed, removing from alltasks. ID: " + task.ID);

			}

			if (task.modified) {

				// Check if we need to send network updates.

				switch (GENERAL.AUTHORITY) {

				case AUTHORITY.LOCAL:

					if (task.scope == SCOPE.GLOBAL) {

						Debug.Log (me + "Global task " + task.description + " changed, sending update to server. ID: " + task.ID);

						sendTaskUpdateToServer (task.getUpdateMessage ());

					}

					break;

				case AUTHORITY.GLOBAL:

					if (task.scope == SCOPE.GLOBAL) {

						Debug.Log (me + "Global task " + task.description + " changed, sending update to clients. ID: " + task.ID);

						sendTaskUpdateToClients (task.getUpdateMessage ());

					}

					break;

				default:

					break;

				}

				task.modified = false;
			}

		}

	}

	#endif

	#if NETWORKED

	// Network connectivity handling.

	void onStartServer ()
	{

		GENERAL.AUTHORITY = AUTHORITY.GLOBAL;

		GENERAL.SETNEWCONNECTION (-1);

		Debug.Log (me + "Registering server message handlers.");

		NetworkServer.RegisterHandler (stringCode, onMessageFromClient);
		NetworkServer.RegisterHandler (taskCode, onTaskUpdateFromClient);

	}

	void onStopServer ()
	{

		revertAllToLocal ();

	}

	void onStartClient (NetworkClient theClient)
	{

		Debug.Log (me + "Registering client message handlers.");
		theClient.RegisterHandler (stringCode, onMessageFromServer);
		theClient.RegisterHandler (pointerCode, onPointerUpdateFromServer);
		theClient.RegisterHandler (taskCode, onTaskUpdateFromServer);

	}

	void OnStopClient ()
	{

		Debug.Log (me + "Client stopped. Resetting scope to local.");

		revertAllToLocal ();

	}


	void OnServerConnect (NetworkConnection conn)
	{

		Debug.Log (me + "incoming server connection delegate called");

		GENERAL.SETNEWCONNECTION (conn.connectionId);

	}

	void OnClientConnect (NetworkConnection conn)
	{

		Debug.Log (me + "Client connection delegate called");

		GENERAL.AUTHORITY = AUTHORITY.LOCAL; 

	}

	void revertAllToLocal ()
	{

		GENERAL.AUTHORITY = AUTHORITY.LOCAL;

		// set all pointers and tasks (back) to local. 
		// Disabled. Can work, but would need to also set/consider pointerstatus (now it defaults to 0=evaluate which isn't quite right).


		foreach (StoryPointer sp in GENERAL.ALLPOINTERS) {
			sp.scope = SCOPE.LOCAL;
			sp.setStatus ( POINTERSTATUS.PAUSED);

		}

		foreach (StoryTask tsk in GENERAL.ALLTASKS) {
			tsk.scope = SCOPE.LOCAL;
		}



	}

	void OnClientDisconnect (NetworkConnection conn)
	{

		Debug.Log (me + "Lost client connection. Resetting scope to local.");

		revertAllToLocal ();

	}

	// Handle basic string messages.

	void onMessageFromClient (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<StringMessage> ();

		Debug.Log (me + "Message received from client: " + message.value);

	}

	void onMessageFromServer (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<StringMessage> ();

		Debug.Log (me + "Message received from server: " + message.value);

		if (message.value == "suspending") {
			
			Debug.Log (me + "Client will be suspending, closing their connection.");

			netMsg.conn.Disconnect ();

		}

	}

	// Handle pointer messages.

	void onPointerUpdateFromServer (NetworkMessage netMsg)
	{
		var message = netMsg.ReadMessage<PointerUpdate> ();

		StoryPoint point = GENERAL.getStoryPointByID (message.storyPoint);

		if (point == null)
			return; // Warning already logged.

		Debug.Log (me + "Server update for pointer " + point.storyLineName + " ID: " + message.pointerUuid + " | " + message.storyPoint);
				
		StoryPointer sp = GENERAL.getPointer (message.pointerUuid);

		if (sp == null) {
		
			sp = new StoryPointer (point, message.pointerUuid);
		
			Debug.Log (me + "Created an instance of global pointer: " + point.storyLineName + " ID: " + message.pointerUuid);



		} 

		sp.currentPoint = point;

		if (message.killed)
			sp.killPointerOnly ();



//		applyPointerUpdate (message.pointerUuid, message.storyPoint, message.pointerStatus);
			
	}

	//	void applyPointerUpdate (string pointerUuid, string pointName, int pointerStatus)
	//	{
	//
	//		// get the story point
	//
	//		StoryPoint point = GENERAL.getStoryPointByID (pointName);
	//
	//		// see if the pointer exists, update or create new
	//
	//		StoryPointer sp = GENERAL.getPointer (pointerUuid);
	//
	//		if (sp == null) {
	//
	//			sp = new StoryPointer (point, pointerUuid);
	//
	//			Debug.Log (me + "Created a new (remotely owned) pointer with ID: " + sp.ID);
	//
	//		}
	//
	//		sp.currentPoint = point;
	//
	////		sp.setStatus ((POINTERSTATUS)pointerStatus);
	//
	////		sp.setStatus (POINTERSTATUS.PAUSED); // overrule the status sent over the network, since global pointers aren't updated locally.
	//
	//	}

	public void sendPointerUpdateToClients (PointerUpdate pointerMessage)
	{
	
		NetworkServer.SendToAll (pointerCode, pointerMessage);

		Debug.Log (me + "Sending pointer update to all clients: " + pointerMessage.pointerUuid + " " + pointerMessage.storyPoint);

	}

	// Handle task messages.

	void onTaskUpdateFromServer (NetworkMessage networkMessage)
	{

		var taskUpdate = networkMessage.ReadMessage<TaskUpdate> ();

		Debug.Log (me + "Incoming task update for "+taskUpdate.description + " ID: "+taskUpdate.taskID);


		applyTaskUpdate (taskUpdate);

	}

	void onTaskUpdateFromClient (NetworkMessage netMsg)
	{

		var taskUpdate = netMsg.ReadMessage<TaskUpdate> ();

		string debug = "";

		debug += "Incoming task update on connection ID " + netMsg.conn.connectionId;

		applyTaskUpdate (taskUpdate);

		List <NetworkConnection> connections = new List<NetworkConnection> (NetworkServer.connections);

		int c = 0;

		for (int ci = 0; ci < connections.Count; ci++) {

			NetworkConnection nc = connections [ci];

			if (nc != null) {

				if (nc.connectionId != netMsg.conn.connectionId) {

					debug += " sending update to connection ID " + nc.connectionId;

					NetworkServer.SendToClient (ci, taskCode, taskUpdate);
					c++;

				} else {

					debug += " skipping client connection ID " + nc.connectionId;

				}

			} else {

				debug += (" skipping null connection ");

			}

		}

				Debug.Log (me+debug);

	}

	void applyTaskUpdate (TaskUpdate taskUpdate)
	{



		StoryPointer updatePointer = GENERAL.getPointer (taskUpdate.pointerID);

		// If we receive updates for a task for which we haven't spawned a pointer yet we ignore them.

		if (updatePointer == null)
			return;



		StoryTask updateTask = GENERAL.getTask (taskUpdate.taskID);

		if (updateTask == null) {
			
			updateTask = new StoryTask (taskUpdate.description, updatePointer, taskUpdate.taskID);

			updateTask.ApplyUpdateMessage (taskUpdate);

			Debug.Log (me + "Created an instance of global task " + updateTask.description + " ID: " + taskUpdate.taskID);

			DistributeTasks (new TaskArgs (updateTask));

			if (updatePointer == null) {

				Debug.LogWarning (me + "update pointer not found: " + taskUpdate.pointerID);

			} else {

				updatePointer.currentTask = updateTask;
//				updateTask.pointer = updatePointer;

//				Debug.LogWarning (me + "Pointer existed but task did not." + taskUpdate.pointerID);


			}

		} else {


			updateTask.ApplyUpdateMessage (taskUpdate);

			Debug.Log (me + "Applied update to existing task.");

			if (updatePointer == null) {

				Debug.LogWarning (me + "update pointer not found: " + taskUpdate.pointerID);

			} else {

				updatePointer.currentTask = updateTask;

			}



		}

	}

	void sendTaskUpdateToServer (TaskUpdate message)
	{
		
		networkManager.client.Send (taskCode, message);

//		Debug.Log (me + "Sending task update to server. ");
//		Debug.Log (message.toString());

	}

	void sendTaskUpdateToClients (TaskUpdate message)
	{

		NetworkServer.SendToAll (taskCode, message);

//		Debug.Log (me + "Sending task update to all clients. ");
//		Debug.Log (message.toString());

	}

	public void sendMessageToServer (string value)
	{
		var msg = new StringMessage (value);
		networkManager.client.Send (stringCode, msg);
		Debug.Log (me + "Sending message to server: " + value);
	}

	public void sendMessageToClients (string value)
	{
		var msg = new StringMessage (value);
		NetworkServer.SendToAll (stringCode, msg);
		Debug.Log (me + "Sending message to all clients: " + value);
	}


	#endif

	public int eventHandlerCount ()
	{

		if (newTasksEvent != null) {
			
			return newTasksEvent.GetInvocationList ().Length;

		} else {
			
			return 0;
		}

	}

	// Invoke event;

	protected virtual void DistributeTasks (TaskArgs e)
	{

		if (newTasksEvent != null)
			newTasksEvent (this, e); // trigger the event, if there are any listeners

	}

}

public class TaskArgs : EventArgs
{

	public List <StoryTask> theTasks;
//	public bool removeTask;


	public TaskArgs (List <StoryTask> tasks) : base () // extend the constructor 
	{ 
		theTasks = tasks;
	}

	public TaskArgs (StoryTask task) : base () // extend the constructor 
	{ 
		theTasks = new List <StoryTask> ();
		theTasks.Add (task);
	}

//	public TaskArgs (StoryTask task, bool remove) : base () // extend the constructor 
//	{ 
//		removeTask = remove;
//		theTasks = new List <StoryTask> ();
//		theTasks.Add (task);
//	}

}

