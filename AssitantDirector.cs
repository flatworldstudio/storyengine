using UnityEngine;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;


#if !SOLO
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using StoryEngine.Network;
#endif


namespace StoryEngine
{
    public delegate bool TaskHandler(StoryTask theTask);
    public delegate void NewTasksEvent(object sender, TaskArgs e);

    /*!
     * \brief
     * Generates, distributes and synchronises StoryTask objects.
     * 
     * On Update the AssistantDirector first processes incoming lan StoryUpdate objects.
     * Secondly it goes over all StoryPointers to generate new StoryTask objects and distributes these locally.  
     * On LateUpdate it goes over all tasks and sends StoryUpdate objects over lan. These tasks will have executed their first frame by then.
     * 
     */    

    public class AssitantDirector : MonoBehaviour
    {

        public TextAsset script;/*!< \brief Set this value in Unity Editor */

        //public string scriptName;/*!< \brief Set this value in Unity Editor, will be deprecated. */
        public string launchOSX, launchWIN, launchIOS, launchAndroid;/*!< \brief Set this value in Unity Editor */

        public static AssitantDirector Instance;

        public LOGLEVEL DefaultLogLevel;/*!< \brief Set this value in Unity Editor */

        string ID = "AD";

        public event NewTasksEvent newTasksEvent;

        Director theDirector;
        string launchStoryline;

#if !SOLO

        List<StoryUpdate> StoryUpdateStack;

         GameObject NetworkObject;

        ExtendedNetworkManager networkManager;

        const short stringCode = 1002;
        const short storyCode = 1005;
        static public int BufferStatusOut = 0;
        static public int BufferStatusIn = 0;

#endif

        // Copy these into every class for easy debugging.
        void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

        void Awake()
		{
            Instance=this; 
		}


		void Start()
        {
          
            Verbose("Starting.");

            //UUID.setIdentity();
            //Verbose("Identity stamp " + UUID.identity);

            GENERAL.AUTHORITY = AUTHORITY.LOCAL;
            theDirector = new Director();
            GENERAL.ALLTASKS = new List<StoryTask>();

            #if !SOLO
            StoryUpdateStack = new List<StoryUpdate>();
#endif


#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

            Log("Running on OSX platform.");

            launchStoryline = launchOSX;

#endif


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            Log("Running on WINDOWS platform.");

            launchStoryline = launchWIN;

#endif

#if UNITY_IOS

		Log ("Running on IOS platform. ");

		launchStoryline = launchIOS;

#endif


#if UNITY_ANDROID

		Log ("Running on Android platform. ");

            launchOnStoryline = launchAndroid;

#endif


        }

        void OnApplicationPause(bool paused)
        {

            // IOS: first app leaves focus, then it pauzes. on return app enteres focus and then resumes

            if (paused)
                Verbose("pauzing ...");
            else
                Verbose("resuming ...");

        }

        void OnApplicationFocus(bool focus)
        {

            if (focus)
                Verbose("entering focus ...");
            else
                Verbose("leaving focus ...");

        }

#if !SOLO
        public void SetNetworkObject (GameObject _object)
        {

            if (_object == null)
                return;

            // get the networkmanager to call network event methods on the assistant director.
            // this should maybe move.
            NetworkObject = _object;

            //if (NetworkObject != null)
            //{

                networkManager = NetworkObject.GetComponent<ExtendedNetworkManager>();

                if (networkManager != null)
                {
                    networkManager.onStartServerDelegate = onStartServer;
                    networkManager.onStartClientDelegate = onStartClient;
                    networkManager.onServerConnectDelegate = OnServerConnect;
                    networkManager.onClientConnectDelegate = OnClientConnect;
                    networkManager.onClientDisconnectDelegate = OnClientDisconnect;
                    networkManager.onStopClientDelegate = OnStopClient;

                    //Log("Max Message Size: " + NetworkMessage.MaxMessageSize);

                }


            //}
        }
#endif





        public static StoryTask FindTaskByByLabel(string id)
        {

            StoryTask r = null;
            r = GENERAL.GetTaskForPoint(id);
            return r;

        }

        void Update()
        {

#if !SOLO

            // Handle story updates, aiming for 1 per frame, assuming we're tyring to run in sync.
            // Lowest numbers are oldest.

            int UpdateCount = StoryUpdateStack.Count;

            switch (UpdateCount)
            {

                case 0:
                    BufferStatusIn = 0;
                    break;

                case 1:

                    // exactly one, so apply.

                    ApplyStoryUpdate(StoryUpdateStack[0]);
                    StoryUpdateStack.RemoveAt(0);
                    BufferStatusIn = 1;

                    break;

                case 2:

                    // Two, normally for different frames that happened to arrive in the same frame on this end. 
                    // Apply the oldest one, keep the other because we exact 0 updates during our next frame.

                    ApplyStoryUpdate(StoryUpdateStack[0]);
                    StoryUpdateStack.RemoveAt(0);
                    BufferStatusIn = 1;

                    break;

                default:

                    // More than 2. Apply all the older ones in order of arrival, keep latest one.

                    Warning("Update buffer >2");

                    BufferStatusIn = 2;
                                      
                    while (StoryUpdateStack.Count>1){
                        
                        ApplyStoryUpdate(StoryUpdateStack[0]);
                        StoryUpdateStack.RemoveAt(0);

                    }


                    break;

            }
#endif

            switch (theDirector.status)
            {

                case DIRECTORSTATUS.ACTIVE:

                 //   Verbose("Director active .");

                    foreach (StoryTask task in GENERAL.ALLTASKS)
                    {

                        if (task.getCallBack() != "")
                        {

                            // if a callback was set (somewhere on the network) we act on it only if we are the server or if the task is local.

                            if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL || task.scope == SCOPE.LOCAL)
                            {
                                task.Pointer.SetStatus(POINTERSTATUS.TASKUPDATED);
                                }
                            }
                        }

                    theDirector.EvaluatePointers();

                    List<StoryTask> newTasks = new List<StoryTask>();

                    for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++)
                    {

                        StoryPointer pointer = GENERAL.ALLPOINTERS[p];

                            switch (pointer.scope)
                            {

                                case SCOPE.GLOBAL:

                                    // If pointer scope is global, we add a task if our own scope is global as well. (If our scope is local, we'll be receiving the task over the network)

                                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                                    {

                                        if (pointer.GetStatus() == POINTERSTATUS.NEWTASK)
                                        {

                                            pointer.SetStatus(POINTERSTATUS.PAUSED);

                                            StoryTask task = new StoryTask(pointer, SCOPE.GLOBAL);
                                            task.LoadPersistantData(pointer);

                                            newTasks.Add(task);
                                            task.modified = true;

                                            Verbose("Created global task " + task.Instruction + " for pointer " + pointer.currentPoint.StoryLine);

                                        }

                                    }

                                    break;

                                case SCOPE.LOCAL:
                                default:

                                    // If pointer scope is local, check if new tasks have to be generated.

                                    if (pointer.GetStatus() == POINTERSTATUS.NEWTASK)
                                    {

                                        pointer.SetStatus(POINTERSTATUS.PAUSED);

                                        StoryTask task = new StoryTask(pointer, SCOPE.LOCAL);
                                        task.LoadPersistantData(pointer);

                                        newTasks.Add(task);

                                        Verbose("Created local task " + task.Instruction + " for pointer " + pointer.currentPoint.StoryLine);

                                    }

                                    break;

                            }
                                        

                    }

                    if (newTasks.Count > 0)
                    {

                        DistributeTasks(new TaskArgs(newTasks)); // if any new tasks call an event, passing on the list of tasks to any handlers listening
                    }

                    break;

                case DIRECTORSTATUS.READY:

                    GENERAL.SIGNOFFS = eventHandlerCount();

                    if (GENERAL.SIGNOFFS == 0)
                    {

                        Error("No handlers registred. Pausing director.");
                        theDirector.status = DIRECTORSTATUS.PAUSED;

                    }
                    else
                    {

                        Verbose("" + GENERAL.SIGNOFFS + " handlers registred.");

                        Log("Starting storyline " + launchStoryline);

                        theDirector.NewStoryLine(launchStoryline);
                        theDirector.status = DIRECTORSTATUS.ACTIVE;

                    }

                    break;

                case DIRECTORSTATUS.NOTREADY:

                    if (script != null)
                    {
                        theDirector.loadScript(script);
                        break;
                    }

                    //if (scriptName != "")
                    //{
                    //    Warning("Please use the TextAsset field for your script.");
                    //    theDirector.loadScript(scriptName);
                    //    break;
                    //}
                   
                    Error("No script reference found.");
                  
                    break;

                default:
                    break;
            }

        }

#if !SOLO

        void ApplyStoryUpdate(StoryUpdate storyUpdate)
        {

            PointerUpdateBundled pointerUpdateBundled;

            while (storyUpdate.GetPointerUpdate(out pointerUpdateBundled))
                 ApplyPointerUpdate(pointerUpdateBundled);

            TaskUpdateBundled taskUpdateBundled;

            while (storyUpdate.GetTaskUpdate(out taskUpdateBundled))
            ApplyTaskUpdate(taskUpdateBundled);
                       
        }

        void ApplyPointerUpdate(PointerUpdateBundled pointerUpdate)
        {

            // Right now the only update we send for pointers is when they are killed.

            StoryPointer pointer = GENERAL.GetPointerForStoryline(pointerUpdate.StoryLineName);

            Log("Server says kill pointer: " + pointerUpdate.StoryLineName);

            if (pointer != null)
            {

                // We remove it instantly. No need to mark it as deleted, nothing else to do with it.

                GENERAL.ALLPOINTERS.Remove(pointer);
                Log("Removing pointer: " + pointer.currentPoint.StoryLine);

                // Server passes tasks, so if it is faster, client may be processing more than one task for a storyline. (Even if the deus dash would show it)

                for (int i = GENERAL.ALLTASKS.Count - 1; i >= 0; i--)
                {

                    StoryTask task = GENERAL.ALLTASKS[i];

                    if (task.Point != null && task.Point.StoryLine == pointerUpdate.StoryLineName)
                    {

                        Log("Removing task: " + task.Instruction);

                        GENERAL.ALLTASKS.Remove(task);

                    }
                }

            }

        }

        void ApplyTaskUpdate(TaskUpdateBundled taskUpdate)
        {

            // See if we have a task on this storypoint.

            StoryTask updateTask = GENERAL.GetTaskForPoint(taskUpdate.pointID);


            if (updateTask == null)
            {

                // If not, and we're a client, we create the task.
                // If we're the server, we ignore updates for task we no longer know about.

                if (GENERAL.AUTHORITY == AUTHORITY.LOCAL)
                {

                    updateTask = new StoryTask(taskUpdate.pointID, SCOPE.GLOBAL);
                    updateTask.ApplyUpdateMessage(taskUpdate);

                    Log("Created an instance of global task " + updateTask.Instruction + " id " + updateTask.PointID);

                    if (taskUpdate.pointID != "GLOBALS")
                    {

                        // Now find a pointer.

                        StoryPointer updatePointer = GENERAL.GetStorylinePointerForPointID(taskUpdate.pointID);

                        if (updatePointer == null)
                        {

                            updatePointer = new StoryPointer();

                            Log("Created a new pointer for task " + updateTask.Instruction);

                        }

                        updatePointer.PopulateWithTask(updateTask);

                        Log("Populated pointer " + updatePointer.currentPoint.StoryLine + " with task " + updateTask.Instruction);

                        DistributeTasks(new TaskArgs(updateTask));

                    }
                }


            }
            else
            {

                updateTask.ApplyUpdateMessage(taskUpdate);

                updateTask.scope = SCOPE.GLOBAL;//?? 

                Verbose("Applied update to existing task "+updateTask.Instruction);

            }

        }


#endif


#if !SOLO

        void LateUpdate()
        {

            StoryUpdate storyUpdate = new StoryUpdate(); // Contains a collection of task and pointer updates.

            // Check our loadbalance. If we're sending too many updates we'll randomly drop frames. 
            // All changes will be sent but if values are updated in the meantime the previous value will never be sent.
            // This is ok for running values but not ok for status values etc.

            int QueueSize = 0;

            if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL && NetworkServer.active)
            {

                // We're an active server.

                byte error;
                QueueSize = NetworkTransport.GetOutgoingMessageQueueSize(NetworkServer.serverHostId, out error);
                //  debugValue.text = "queued out server: " + QueueSize;

                if ((NetworkError)error != NetworkError.Ok)
                    Error("Networktransport error: " + (NetworkError)error);


            }
            if (GENERAL.AUTHORITY == AUTHORITY.LOCAL && networkManager != null && networkManager.client != null)
            {

                // We're an active client.

                byte error=(byte)NetworkError.Ok;

                if (networkManager.client.connection != null)
                {
                    QueueSize = NetworkTransport.GetOutgoingMessageQueueSize(networkManager.client.connection.hostId, out error);
                }
                else
                {
                    Warning("Can't get queue size (yet)");
                }

                //    debugValue.text = "queued out client: " + QueueSize;

                if ((NetworkError)error != NetworkError.Ok)
                    Error("Networktransport error: " + (NetworkError)error);

            }

            switch (QueueSize)
            {
                case 0:
                    BufferStatusOut = 0;
                    break;
                case 1:
                case 2:
                    BufferStatusOut = 1;
                    break;
                default:
                    BufferStatusOut = 2;
                    break;

            }

            // forcing always send.
            // this means that a storyupdate is sent for every frame and they all arrive in order.
            // they get executed in order as well, but they may be batched together.
            // so multiple tasks might get executed in a single frame. they will be executed in the correct order.


            if (QueueSize < 3 || true)

            {

                // Iterate over all pointers to see if any were killed. Clients do not kill pointers themselves.
                // For consistency of network logic, local pointers that were killed are disposed by the director.
                // Global pointers are disposed here, after updating clients about them.

                for (int p = GENERAL.ALLPOINTERS.Count - 1; p >= 0; p--)
                {

                    StoryPointer pointer = GENERAL.ALLPOINTERS[p];

                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL && pointer.scope == SCOPE.GLOBAL && pointer.GetStatus() == POINTERSTATUS.KILLED)

                        //if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL && pointer.scope == SCOPE.GLOBAL && pointer.modified && pointer.GetStatus() == POINTERSTATUS.KILLED)
                    {

                        Log("Sending pointer kill update to clients: " + pointer.currentPoint.StoryLine);

                        storyUpdate.AddStoryPointerUpdate(pointer.GetUpdate()); // bundled

                        //pointer.modified = false;


                        Log("Removing pointer " + pointer.currentPoint.StoryLine);

                        GENERAL.ALLPOINTERS.Remove(pointer);


                    }

                }

                // Iterate over all tasks.

                for (int i = GENERAL.ALLTASKS.Count - 1; i >= 0; i--)
                {

                    StoryTask task = GENERAL.ALLTASKS[i];

                    // Cleanup completed tasks.

                    if (task.getStatus() == TASKSTATUS.COMPLETE)
                    {

                        GENERAL.ALLTASKS.RemoveAt(i);

                        Verbose("Task " + task.Instruction + " on storyline " + task.Pointer.currentPoint.StoryLine +" completed, removed from alltasks. ");
                                         
                    }

                    if (task.modified)
                    {

                        // Debugging: if a pointer is in the process of being killed, we may want to not send task updates
                        // as they might result in the task being recreated clientside.

                        if (task.Pointer.GetStatus() == POINTERSTATUS.KILLED)
                        {

                            Warning("Supressing sending task update for task with pointer that is dying. " + task.Instruction);

                        }
                        else
                        {

                            // Check if we need to send network updates.

                            switch (GENERAL.AUTHORITY)
                            {

                                case AUTHORITY.LOCAL:

                                    if (task.scope == SCOPE.GLOBAL)
                                    {

                                        Verbose("Global task " + task.Instruction + " changed, adding to update for server.");

                                        storyUpdate.AddTaskUpdate(task.GetUpdateBundled()); // bundled

                                    }

                                    break;

                                case AUTHORITY.GLOBAL:

                                    if (task.scope == SCOPE.GLOBAL)
                                    {

                                        Verbose("Global task " + task.Instruction + " changed, adding to update for clients.");

                                        storyUpdate.AddTaskUpdate(task.GetUpdateBundled()); // bundled

                                    }

                                    break;

                                default:

                                    break;

                            }

                            task.modified = false;

                        }
                    }

                }

                // If anything to send, send. 

                if (storyUpdate.AnythingToSend())

                {

                    switch (GENERAL.AUTHORITY)
                    {
                        case AUTHORITY.LOCAL:
                            SendStoryUpdateToServer(storyUpdate);
                            //Debug.Log("Sending story update to server. \n" + storyUpdate.DebugLog);

                            break;
                        case AUTHORITY.GLOBAL:

                            SendStoryUpdateToClients(storyUpdate);

                            //Debug.Log("Sending story update to clients. \n" + storyUpdate.DebugLog);
                            //Debug.Log(storyUpdate.ToString());

                            break;
                        default:
                            break;


                    }

                }

            }
            else
            {

               Warning("Dropping update.");

            }

        }

#endif

#if !SOLO

        // Network connectivity handling.

        void onStartServer()
        {

            GENERAL.AUTHORITY = AUTHORITY.GLOBAL;

     //       GENERAL.SETNEWCONNECTION(-1);

            //     Logger.Message("Registering server message handlers.");

            NetworkServer.RegisterHandler(stringCode, OnMessageFromClient);
            NetworkServer.RegisterHandler(storyCode, OnStoryUpdateFromClient);

        }

        void onStopServer()
        {

            revertAllToLocal();

        }

        void onStartClient(NetworkClient theClient)
        {

            //      Logger.Message("Registering client message handlers.");

            theClient.RegisterHandler(stringCode, OnMessageFromServer);
            theClient.RegisterHandler(storyCode, OnStoryUpdateFromServer);

        }

        void OnStopClient()
        {

            Warning("Client stopped. Resetting scope to local.");

            revertAllToLocal();

        }


        void OnServerConnect(NetworkConnection conn)
        {

            Verbose("Incoming server connection delegate called ");

       //     GENERAL.SETNEWCONNECTION(conn.connectionId);

        }

        void OnClientConnect(NetworkConnection conn)
        {

            Verbose("Client connection delegate called");

            GENERAL.AUTHORITY = AUTHORITY.LOCAL;
            AssitantDirector.Instance.sendMessageToServer("REGISTER");

        }

        void revertAllToLocal()
        {

            //WIP

            GENERAL.AUTHORITY = AUTHORITY.LOCAL;

            // set all pointers and tasks (back) to local. 
            // Disabled. Can work, but would need to also set/consider pointerstatus (now it defaults to 0=evaluate which isn't quite right).


            foreach (StoryPointer sp in GENERAL.ALLPOINTERS)
            {
                sp.scope = SCOPE.LOCAL;
                sp.SetStatus(POINTERSTATUS.PAUSED);

            }

            foreach (StoryTask tsk in GENERAL.ALLTASKS)
            {
                tsk.scope = SCOPE.LOCAL;
            }



        }

        void OnClientDisconnect(NetworkConnection conn)
        {

            Warning("Lost client connection. Resetting scope to local.");

            revertAllToLocal();

        }

        // Handle basic string messages.

        void OnMessageFromClient(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<StringMessage>();

            Verbose("Message received from client "+ netMsg.conn.address+" : " + message.value);

            if (message.value == "REGISTER")
            {
                DataController.Instance.RemoveTrackedAddress(netMsg.conn.address);
            }
                       

        }

        void OnMessageFromServer(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<StringMessage>();

            Verbose("Message received from server: " + message.value);

            if (message.value == "suspending")
            {

                Verbose("Client will be suspending, closing their connection.");

                netMsg.conn.Disconnect();

            }

        }

        // Keeping this for a bit. If there's multiple clients we need to solve what gets sent where. An update from a client to the server should be forwarded.
        // Other clients would receive the update first and any server updates on top of that.
        // But that would mean different messages which we haven't used so far.
        // Could also be that update messages get forwarded but all updates have sender labels, so the the sender can identify an update as their own and ignore it.
        // All clients and server would need unique id's.
        // Could already be implemented in unity. If not, server should pass around unique names.


        // 

        /*
        void onTaskUpdateFromClient(NetworkMessage netMsg)
        {

            var taskUpdate = netMsg.ReadMessage<TaskUpdate>();

            string debug = "";

            debug += "Incoming task update on connection ID " + netMsg.conn.connectionId;

            //		if (GENERAL.ALLTASKS

            applyTaskUpdate(taskUpdate);


            List<NetworkConnection> connections = new List<NetworkConnection>(NetworkServer.connections);

            int c = 0;

            for (int ci = 0; ci < connections.Count; ci++)
            {

                NetworkConnection nc = connections[ci];

                if (nc != null)
                {

                    if (nc.connectionId != netMsg.conn.connectionId)
                    {

                        debug += " sending update to connection ID " + nc.connectionId;

                        NetworkServer.SendToClient(ci, taskCode, taskUpdate);
                        c++;

                    }
                    else
                    {

                        debug += " skipping client connection ID " + nc.connectionId;

                    }

                }
                else
                {

                    debug += (" skipping null connection ");

                }

            }

            Log.Message(debug);

        }
*/


        // Send bundled story updates

        void SendStoryUpdateToClients(StoryUpdate message)
        {

          //  message.MaxMessageSize;


            // NetworkServer.SendUnreliableToAll(storyCode, message);
            NetworkServer.SendToAll(storyCode, message);

        }

        void SendStoryUpdateToServer(StoryUpdate message)
        {

            networkManager.client.Send(storyCode, message);
          //  sendMessageToServer("ping");
        }

        void OnStoryUpdateFromClient(NetworkMessage netMsg)
        {
            //  netMsg.MaxMess
            StoryUpdateStack.Add(netMsg.ReadMessage<StoryUpdate>());

        }

        void OnStoryUpdateFromServer(NetworkMessage netMsg)
        {

           


            StoryUpdateStack.Add(netMsg.ReadMessage<StoryUpdate>());


        }


        public void sendMessageToServer(string value)
        {
            var msg = new StringMessage(value);
            networkManager.client.Send(stringCode, msg);
            Log("Sending message to server: " + value);
        }

        public void sendMessageToClients(string value)
        {
            var msg = new StringMessage(value);
            NetworkServer.SendToAll(stringCode, msg);
            Log("Sending message to all clients: " + value);
        }


#endif

        public int eventHandlerCount()
        {

            if (newTasksEvent != null)
            {

                return newTasksEvent.GetInvocationList().Length;

            }
            else
            {

                return 0;
            }

        }

        // Invoke event;

        protected virtual void DistributeTasks(TaskArgs e)
        {

            if (newTasksEvent != null)
                newTasksEvent(this, e); // trigger the event, if there are any listeners

        }

    }

    /*!
* \brief
* Holds task info to pass onto handlers.
* 
*/

    public class TaskArgs : EventArgs
    {

        public List<StoryTask> theTasks;

        public TaskArgs(List<StoryTask> tasks) : base() // extend the constructor 
        {
            theTasks = tasks;
        }

        public TaskArgs(StoryTask task) : base() // extend the constructor 
        {
            theTasks = new List<StoryTask>();
            theTasks.Add(task);
        }

    }

}