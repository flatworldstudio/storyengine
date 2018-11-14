using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


#if NETWORKED
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
#endif


namespace StoryEngine
{


	public delegate void NewTasksEvent (object sender, TaskArgs e);

	public class AssitantDirector : MonoBehaviour
	{
	
		public event NewTasksEvent newTasksEvent;

		string me = "AssistantDirector";

		Director theDirector;
		public string scriptName;

		string launchOnStoryline;
		public string launchOSX, launchWIN, launchIOS;

		#if NETWORKED

		public ExtendedNetworkManager networkManager;

		const short stringCode = 1002;
		const short pointerCode = 1003;
		const short taskCode = 1004;

		#endif

		void Awake ()
		{

			Log.Init ();// this initialises the dictionary without entries, if not handled by developer.

		}

		void Start ()
		{

      


			Log.Message ("Starting.", me);

			UUID.setIdentity ();

			Log.Message ("Identity stamp " + UUID.identity, me);

			GENERAL.AUTHORITY = AUTHORITY.LOCAL;

			theDirector = new Director ();

			GENERAL.ALLTASKS = new List<StoryTask> ();

			#if UNITY_IOS

		Log.Message ("Running on IOS platform. ",me);

		launchOnStoryline = launchIOS;

			#endif

			#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

			Log.Message ("Running on OSX platform.", me);

			launchOnStoryline = launchOSX;

			#endif
				

			#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

		Log.Message ("Running on WINDOWS platform.",me);

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



		void OnApplicationPause (bool paused)
		{

			// IOS: first app leaves focus, then it pauzes. on return app enteres focus and then resumes
	
			if (paused) {

				Log.Message ("pauzing ...", me);

			} else {

				Log.Message ("resuming ...", me);

			}

		}

		void OnApplicationFocus (bool focus)
		{
		
			if (focus) {

				Log.Message ("entering focus ...", me);

			} else {

				Log.Message ("leaving focus ...", me);

			}

		}

		void Update ()
		{

			switch (theDirector.status) {

			case DIRECTORSTATUS.ACTIVE:

//			Log.Message ( "director active ...");

				foreach (StoryTask task in GENERAL.ALLTASKS) {

					if (task.getCallBack () != "") {

						// if a callback was set (somewhere on the network) we act on it only if we are the server or if the task is local.

						if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL || task.scope == SCOPE.LOCAL) {

							task.pointer.setStatus (POINTERSTATUS.TASKUPDATED);

							// wip, carry over value

//						string callBackValue="";
//
//						if (task.getStringValue ("callBackValue", out callBackValue))
//							task.pointer.carryOver = callBackValue;

						}

					}

				}

				theDirector.evaluatePointers ();

				List<StoryTask> newTasks = new List<StoryTask> ();

				for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++) {

					StoryPointer pointer = GENERAL.ALLPOINTERS [p];

				//	if (pointer.modified) {

						switch (pointer.scope) {

						case SCOPE.GLOBAL:

						// If pointer scope is global, we add a task if our own scope is global as well. (If our scope is local, we'll be receiving the task over the network)

							if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL) {

								if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

									pointer.setStatus (POINTERSTATUS.PAUSED);

									StoryTask task = new StoryTask (pointer, SCOPE.GLOBAL);

//								task.loadPersistantData (pointer);
									task.loadPersistantData (pointer);

									pointer.currentTask = task;
									newTasks.Add (task);

                                        #if NETWORKED
									task.modified = true;
                                        #endif

									Log.Message ("Creating and distributing global task " + task.description + " for pointer " + pointer.currentPoint.storyLineName, me);

								}

							}

							break;

						case SCOPE.LOCAL:
						default:

						// If pointer scope is local, check if new tasks have to be generated.

							if (pointer.getStatus () == POINTERSTATUS.NEWTASK) {

								pointer.setStatus (POINTERSTATUS.PAUSED);

								StoryTask task = new StoryTask (pointer, SCOPE.LOCAL);

//							task.loadPersistantData (pointer);
								task.loadPersistantData (pointer);


								pointer.currentTask = task;

								newTasks.Add (task);

								Log.Message ("Creating local task " + task.description + " for pointer " + pointer.currentPoint.storyLineName, me);

							}

							break;

						}

				//	}

				}

				if (newTasks.Count > 0) {

					DistributeTasks (new TaskArgs (newTasks)); // if any new tasks call an event, passing on the list of tasks to any handlers listening
				}

				break;

			case DIRECTORSTATUS.READY:

				GENERAL.SIGNOFFS = eventHandlerCount ();

				if (GENERAL.SIGNOFFS == 0) {

					Log.Warning ("No handlers registred. Pausing director.", me);
					theDirector.status = DIRECTORSTATUS.PAUSED;

				} else {

					Log.Message ("" + GENERAL.SIGNOFFS + " handlers registred.", me);

					Log.Message ("Starting storyline " + launchOnStoryline, me);

					theDirector.beginStoryLine (launchOnStoryline);
					theDirector.status = DIRECTORSTATUS.ACTIVE;

//				Log.Message ( "Started storyline " + launchOnStoryline);

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

					Log.Message ("Sending pointer update to clients. ID: " + pointer.ID, me);

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

						Log.MessageWarning ( "Global pointer " + pointer.ID + " | " + pointer.currentTask.description + " was changed locally.");

					}

					break;

				case AUTHORITY.GLOBAL:

					if (pointer.scope == SCOPE.GLOBAL) {

						Log.Message ( "Global pointer, global AD -> sending pointer update for " + pointer.ID);

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

					Log.Message ("Task " + task.description + " completed, removing from alltasks. ID: " + task.ID, me);

				}

				if (task.modified) {

					// Check if we need to send network updates.

					switch (GENERAL.AUTHORITY) {

					case AUTHORITY.LOCAL:

						if (task.scope == SCOPE.GLOBAL) {

							Log.Message ("Global task " + task.description + " changed, sending update to server. ID: " + task.ID, me);

							sendTaskUpdateToServer (task.getUpdateMessage ());

						}

						break;

					case AUTHORITY.GLOBAL:

						if (task.scope == SCOPE.GLOBAL) {

							Log.Message ("Global task " + task.description + " changed, sending update to clients. ID: " + task.ID, me);

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

			Log.Message ("Registering server message handlers.", me);

			NetworkServer.RegisterHandler (stringCode, onMessageFromClient);
			NetworkServer.RegisterHandler (taskCode, onTaskUpdateFromClient);

		}

		void onStopServer ()
		{

			revertAllToLocal ();

		}

		void onStartClient (NetworkClient theClient)
		{

			Log.Message ("Registering client message handlers.", me);
			theClient.RegisterHandler (stringCode, onMessageFromServer);
			theClient.RegisterHandler (pointerCode, onPointerUpdateFromServer);
			theClient.RegisterHandler (taskCode, onTaskUpdateFromServer);

		}

		void OnStopClient ()
		{

			Log.Message ("Client stopped. Resetting scope to local.", me);

			revertAllToLocal ();

		}


		void OnServerConnect (NetworkConnection conn)
		{

			Log.Message ("incoming server connection delegate called", me);

			GENERAL.SETNEWCONNECTION (conn.connectionId);

		}

		void OnClientConnect (NetworkConnection conn)
		{

			Log.Message ("Client connection delegate called", me);

			GENERAL.AUTHORITY = AUTHORITY.LOCAL; 

		}

		void revertAllToLocal ()
		{

			GENERAL.AUTHORITY = AUTHORITY.LOCAL;

			// set all pointers and tasks (back) to local. 
			// Disabled. Can work, but would need to also set/consider pointerstatus (now it defaults to 0=evaluate which isn't quite right).


			foreach (StoryPointer sp in GENERAL.ALLPOINTERS) {
				sp.scope = SCOPE.LOCAL;
				sp.setStatus (POINTERSTATUS.PAUSED);

			}

			foreach (StoryTask tsk in GENERAL.ALLTASKS) {
				tsk.scope = SCOPE.LOCAL;
			}



		}

		void OnClientDisconnect (NetworkConnection conn)
		{

			Log.Message ("Lost client connection. Resetting scope to local.", me);

			revertAllToLocal ();

		}

		// Handle basic string messages.

		void onMessageFromClient (NetworkMessage netMsg)
		{
			var message = netMsg.ReadMessage<StringMessage> ();

			Log.Message ("Message received from client: " + message.value, me);

		}

		void onMessageFromServer (NetworkMessage netMsg)
		{
			var message = netMsg.ReadMessage<StringMessage> ();

			Log.Message ("Message received from server: " + message.value, me);

			if (message.value == "suspending") {
			
				Log.Message ("Client will be suspending, closing their connection.", me);

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

			Log.Message ("Server update for pointer " + point.storyLineName + " ID: " + message.pointerUuid + " | " + message.storyPoint, me);
				
			StoryPointer sp = GENERAL.getPointer (message.pointerUuid);

			if (sp == null) {
		
				sp = new StoryPointer (point, message.pointerUuid);
		
				Log.Message ("Created an instance of global pointer: " + point.storyLineName + " ID: " + message.pointerUuid, me);



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
		//			Log.Message ( "Created a new (remotely owned) pointer with ID: " + sp.ID);
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

			Log.Message ("Sending pointer update to all clients: " + pointerMessage.pointerUuid + " " + pointerMessage.storyPoint, me);

		}

		// Handle task messages.

		void onTaskUpdateFromServer (NetworkMessage networkMessage)
		{

			var taskUpdate = networkMessage.ReadMessage<TaskUpdate> ();

			Log.Message ("Incoming task update for " + taskUpdate.description + " ID: " + taskUpdate.taskID, me);


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

			Log.Message (debug, me);

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

				Log.Message ("Created an instance of global task " + updateTask.description + " ID: " + taskUpdate.taskID, me);

				DistributeTasks (new TaskArgs (updateTask));

				if (updatePointer == null) {

					Log.Warning ("update pointer not found: " + taskUpdate.pointerID, me);

				} else {

					updatePointer.currentTask = updateTask;
//				updateTask.pointer = updatePointer;

//				Log.MessageWarning ( "Pointer existed but task did not." + taskUpdate.pointerID);


				}

			} else {


				updateTask.ApplyUpdateMessage (taskUpdate);

				//	Log.Message ( "Applied update to existing task.");

				if (updatePointer == null) {

					Log.Warning ("update pointer not found: " + taskUpdate.pointerID, me);

				} else {

					updatePointer.currentTask = updateTask;

				}



			}

		}

		void sendTaskUpdateToServer (TaskUpdate message)
		{
		
			networkManager.client.Send (taskCode, message);

//		Log.Message ( "Sending task update to server. ");
//		Log.Message (message.toString());

		}

		void sendTaskUpdateToClients (TaskUpdate message)
		{

			NetworkServer.SendToAll (taskCode, message);

//		Log.Message ( "Sending task update to all clients. ");
//		Log.Message (message.toString());

		}

		public void sendMessageToServer (string value)
		{
			var msg = new StringMessage (value);
			networkManager.client.Send (stringCode, msg);
			Log.Message ("Sending message to server: " + value, me);
		}

		public void sendMessageToClients (string value)
		{
			var msg = new StringMessage (value);
			NetworkServer.SendToAll (stringCode, msg);
			Log.Message ("Sending message to all clients: " + value, me);
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

}