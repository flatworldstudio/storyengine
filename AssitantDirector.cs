using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using Random = UnityEngine.Random;


#if NETWORKED
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
#endif


namespace StoryEngine
{

    public delegate void NewTasksEvent(object sender, TaskArgs e);

    public class AssitantDirector : MonoBehaviour
    {
        public string scriptName;
        public string launchOSX, launchWIN, launchIOS, launchAndroid;


        string ID = "AD";
        List<StoryUpdate> StoryUpdateStack;
        public event NewTasksEvent newTasksEvent;
        Director theDirector;
        string launchOnStoryline;


#if NETWORKED

        public ExtendedNetworkManager networkManager;
        const short stringCode = 1002;
        const short storyCode = 1005;
        static public int BufferStatusOut = 0;
        static public int BufferStatusIn = 0;

#endif

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.

        void Log(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.NORMAL);
        }
        void Warning(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.WARNINGS);
        }
        void Error(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.ERRORS);
        }
        void Verbose(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.VERBOSE);
        }

        void Start()
        {

            Verbose("Starting.");

            UUID.setIdentity();
            Verbose("Identity stamp " + UUID.identity);

            GENERAL.AUTHORITY = AUTHORITY.LOCAL;
            theDirector = new Director();
            GENERAL.ALLTASKS = new List<StoryTask>();
            StoryUpdateStack = new List<StoryUpdate>();


#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

            Log("Running on OSX platform.");

            launchOnStoryline = launchOSX;

#endif


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            Log("Running on WINDOWS platform.");

            launchOnStoryline = launchWIN;

#endif

#if UNITY_IOS

		Log ("Running on IOS platform. ");

		launchOnStoryline = launchIOS;

#endif


#if UNITY_ANDROID

		Log ("Running on Android platform. ");

            launchOnStoryline = launchAndroid;

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

        void OnApplicationPause(bool paused)
        {

            // IOS: first app leaves focus, then it pauzes. on return app enteres focus and then resumes

            if (paused)
                Warning("pauzing ...");
            else
                Warning("resuming ...");

        }

        void OnApplicationFocus(bool focus)
        {

            if (focus)
                Warning("entering focus ...");
            else
                Warning("leaving focus ...");

        }

        public static StoryTask FindTaskByByLabel(string id)
        {

            StoryTask r = null;
            r = GENERAL.GetTaskForPoint(id);
            return r;

        }

        void Update()
        {

            // Handle story updates, aiming for 1 per frame.

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

                    // Overflowing. Apply the oldest ones, keep the latest.

                    BufferStatusIn = 2;
                    for (int u = UpdateCount - 2; u >= 0; u--)
                    {

                        ApplyStoryUpdate(StoryUpdateStack[u]);
                        StoryUpdateStack.RemoveAt(u);

                    }

                    break;

            }

            switch (theDirector.status)
            {

                case DIRECTORSTATUS.ACTIVE:

                    Verbose ( "Director active .");

                    foreach (StoryTask task in GENERAL.ALLTASKS)
                    {

                        if (task.getCallBack() != "")
                        {

                            // if a callback was set (somewhere on the network) we act on it only if we are the server or if the task is local.

                            if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL || task.scope == SCOPE.LOCAL)
                            {

                                task.pointer.SetStatus(POINTERSTATUS.TASKUPDATED);

                            }

                        }

                    }

                    theDirector.evaluatePointers();

                    List<StoryTask> newTasks = new List<StoryTask>();

                    for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++)
                    {

                        StoryPointer pointer = GENERAL.ALLPOINTERS[p];

                        if (pointer.modified)
                        {

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

                                            Verbose("Creating and pushing global task " + task.description + " for pointer " + pointer.currentPoint.storyLineName);

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

                                        Verbose("Creating local task " + task.description + " for pointer " + pointer.currentPoint.storyLineName);

                                    }

                                    break;

                            }

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

                        Log("Starting storyline " + launchOnStoryline);

                        theDirector.beginStoryLine(launchOnStoryline);
                        theDirector.status = DIRECTORSTATUS.ACTIVE;

                    }

                    break;

                case DIRECTORSTATUS.NOTREADY:

                    theDirector.loadScript(scriptName);

                    // create globals by default.

                //    GENERAL.storyPoints.Add("GLOBALS", new StoryPoint("GLOBALS", "none", new string[] { "GLOBALS" }));
                 //   GENERAL.GLOBALS = new StoryTask("GLOBALS", SCOPE.GLOBAL);

                    break;

                default:
                    break;
            }

        }

#if NETWORKED

        void ApplyStoryUpdate(StoryUpdate storyUpdate)
        {
            
            PointerUpdateBundled pointerUpdateBundled;

            while (storyUpdate.GetPointerUpdate(out pointerUpdateBundled))
            {

                ApplyPointerUpdate(pointerUpdateBundled);

            }

            TaskUpdateBundled taskUpdateBundled;

            while (storyUpdate.GetTaskUpdate(out taskUpdateBundled))
            {

                ApplyTaskUpdate(taskUpdateBundled);

            }

        }

        void ApplyPointerUpdate(PointerUpdateBundled pointerUpdate)
        {

            // Right now the only update we send for pointers is when they are killed.

            //   StoryPointer pointer = GENERAL.GetStorylinePointerForPointID(pointerUpdate.storyPointID);

            StoryPointer pointer = GENERAL.GetPointerForStoryline(pointerUpdate.StoryLineName);

            Log("Server says kill pointer: " + pointerUpdate.StoryLineName);

            if (pointer != null)
            {

                // We remove it instantly. No need to mark it as deleted, nothing else to do with it.

         //       pointer.Kill();

                GENERAL.ALLPOINTERS.Remove(pointer);
                Log("Removing pointer: " + pointer.currentPoint.storyLineName);
                // Remove task associated with pointer. This is only one at all times, we just don't know which one.

                //if (GENERAL.ALLTASKS.Remove(pointer.currentTask)){
                //    Log.Message("Removing local task: " + pointer.currentTask.description);
                //}

                // Server passes tasks, so if it is faster, client may be processing more than one task for a storyline. (Even if the deus dash would show it)

                for (int i = GENERAL.ALLTASKS.Count - 1; i >= 0; i--)
                {

                    StoryTask task = GENERAL.ALLTASKS[i];

                    if (task.point!=null && task.point.storyLineName == pointerUpdate.StoryLineName)
                    {

                        Log("Removing task: " + task.description);

                        GENERAL.ALLTASKS.Remove(task);

                    }
                }


                // Need to remove tasks for storyline. Normally there should be only one, but we don't know at which point this storyline is.


                // On server, tasks are blocking, so the only task currently active would be currenttask.
                // But on client, tasks are asynchronous. We do not know which task is active.


                //if (GENERAL.ALLTASKS.Remove(pointer.currentTask))
                //{

                //    Log.Message("Removing task " + pointer.currentTask.description);

                //}
                //else
                //{

                //    Log.Warning("Failed removing task " + pointer.currentTask.description);

                //}

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

                    Log("Created an instance of global task " + updateTask.description + " id "+updateTask.pointID);

                    if (taskUpdate.pointID != "GLOBALS")
                    {

                        // Now find a pointer.

                        StoryPointer updatePointer = GENERAL.GetStorylinePointerForPointID(taskUpdate.pointID);

                        if (updatePointer == null)
                        {

                            updatePointer = new StoryPointer();

                            Log("Created a new pointer for task "+updateTask.description);

                        }

                        updatePointer.PopulateWithTask(updateTask);

                        Log("Populated pointer " + updatePointer.currentPoint.storyLineName + " with task "+updateTask.description);

                        DistributeTasks(new TaskArgs(updateTask));

                    }
                }


            }
            else
            {

                updateTask.ApplyUpdateMessage(taskUpdate);

                updateTask.scope = SCOPE.GLOBAL;

                Verbose("Applied update to existing task.");

            }

        }


#endif


#if NETWORKED

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

            }
            if (GENERAL.AUTHORITY == AUTHORITY.LOCAL && NetworkClient.active)
            {

                // We're an active client.

                byte error;
                QueueSize = NetworkTransport.GetOutgoingMessageQueueSize(networkManager.client.connection.hostId, out error);
                //    debugValue.text = "queued out client: " + QueueSize;

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

            if (QueueSize < 3)

            {

                // Iterate over all pointers to see if any were killed. Clients do not kill pointers themselves.
                // For consistency of network logic, local pointers that were killed are disposed by the director.
                // Global pointers are disposed here, after updating clients about them.

                for (int p = GENERAL.ALLPOINTERS.Count-1; p>=0; p--)
                {

                    StoryPointer pointer = GENERAL.ALLPOINTERS[p];
                    
                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL && pointer.scope == SCOPE.GLOBAL && pointer.modified && pointer.GetStatus() == POINTERSTATUS.KILLED)
                    {

                        Log("Sending pointer kill update to clients: " + pointer.currentPoint.storyLineName);

                        storyUpdate.AddStoryPointerUpdate(pointer.GetUpdate()); // bundled

                        pointer.modified = false;
                    

                        Log("Removing pointer " + pointer.currentPoint.storyLineName);

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

                        Verbose("Task " + task.description + " completed, removing from alltasks. ");

                        //if (task.description=="moodon"){
                        //    Debug.Log("moodon task removed at "+Time.frameCount);
                        //}
                    }

                    if (task.modified)
                    {

                        // Debugging: if a pointer is in the process of being killed, we may want to not send task updates
                        // as they might result in the task being recreated clientside.

                        if (task.pointer.GetStatus() == POINTERSTATUS.KILLED)
                        {

                            Warning("Supressing sending task update for task with pointer that is dying. " + task.description);

                        }
                        else
                        {
                            
                            // Check if we need to send network updates.

                            switch (GENERAL.AUTHORITY)
                            {

                                case AUTHORITY.LOCAL:

                                    if (task.scope == SCOPE.GLOBAL)
                                    {

                                        Verbose("Global task " + task.description + " changed, sending update to server.");

                                        storyUpdate.AddTaskUpdate(task.GetUpdateBundled()); // bundled

                                    }

                                    break;

                                case AUTHORITY.GLOBAL:

                                    if (task.scope == SCOPE.GLOBAL)
                                    {

                                        Verbose("Global task " + task.description + " changed, sending update to clients.");

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
                // If not, and buffer is overflowing, send updates randomly once a second.

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

                //  Debug.Log("Dropping update.");

            }

        }

#endif

#if NETWORKED

        // Network connectivity handling.

        void onStartServer()
        {

            GENERAL.AUTHORITY = AUTHORITY.GLOBAL;

            GENERAL.SETNEWCONNECTION(-1);

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

            GENERAL.SETNEWCONNECTION(conn.connectionId);

        }

        void OnClientConnect(NetworkConnection conn)
        {

            Verbose("Client connection delegate called");

            GENERAL.AUTHORITY = AUTHORITY.LOCAL;

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

            Verbose("Message received from client: " + message.value);

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


            // NetworkServer.SendUnreliableToAll(storyCode, message);
            NetworkServer.SendToAll(storyCode, message);

        }

        void SendStoryUpdateToServer(StoryUpdate message)
        {

            networkManager.client.Send(storyCode, message);

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
            Verbose("Sending message to server: " + value);
        }

        public void sendMessageToClients(string value)
        {
            var msg = new StringMessage(value);
            NetworkServer.SendToAll(stringCode, msg);
            Verbose("Sending message to all clients: " + value);
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