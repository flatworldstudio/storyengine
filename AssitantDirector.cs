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
        //List<PointerUpdate> PointerUpdateStack;
        List<StoryUpdate> StoryUpdateStack;

        public event NewTasksEvent newTasksEvent;

        int lastStoryUpdateAtFrame = -1;

        double[] frameDurationBuffer;
        double frameDurrationBufferSum;
        int frameDurationBufferIndex = 0;
        double frameDurationAverage;

        string me = "Assistant director";

        Director theDirector;
        public string scriptName;

        string launchOnStoryline;
        public string launchOSX, launchWIN, launchIOS;

#if NETWORKED

        public ExtendedNetworkManager networkManager;

        const short stringCode = 1002;
        //const short pointerCode = 1003;
        //const short taskCode = 1004;
        const short storyCode = 1005;
        static public double loadBalance = 1;
        static public bool BufferStatusOk = true;

#endif

        void Awake()
        {

            Log.Init();// this initialises the dictionary without entries, if not handled by developer.

        }

        void Start()
        {

            Log.Message("Starting.");

            UUID.setIdentity();

            Log.Message("Identity stamp " + UUID.identity);

            GENERAL.AUTHORITY = AUTHORITY.LOCAL;

            theDirector = new Director();

            GENERAL.ALLTASKS = new List<StoryTask>();
         //   PointerUpdateStack = new List<PointerUpdate>();

            StoryUpdateStack = new List<StoryUpdate>();

            frameDurationBuffer = new double[5];


#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

            Log.Message("Running on OSX platform.");

            launchOnStoryline = launchOSX;

#endif


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            Log.Message("Running on WINDOWS platform.");

            launchOnStoryline = launchWIN;

#endif

#if UNITY_IOS

		Log.Message ("Running on IOS platform. ");

		launchOnStoryline = launchIOS;

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
            {

                Log.Message("pauzing ...");

            }
            else
            {

                Log.Message("resuming ...");

            }

        }

        void OnApplicationFocus(bool focus)
        {

            if (focus)
            {

                Log.Message("entering focus ...");

            }
            else
            {

                Log.Message("leaving focus ...");

            }

        }


        void Update()
        {

            // Handle story updates, aiming for 1 per frame.

            int UpdateCount = StoryUpdateStack.Count;

            switch (UpdateCount)
            {

                case 0:
                    break;

                case 1:

                    // exactly one, so apply.

                    ApplyStoryUpdate(StoryUpdateStack[0]);
                    StoryUpdateStack.RemoveAt(0);
                    BufferStatusOk = true;

                    break;

                case 2:

                    // Two, normally for different frames that happened to arrive in the same frame on this end. 
                    // Apply the oldest one, keep the other because we exact 0 updates during our next frame.

                    ApplyStoryUpdate(StoryUpdateStack[0]);
                    StoryUpdateStack.RemoveAt(0);
                    BufferStatusOk = true;

                    break;

                default:

                    // Overflowing. Apply the oldest ones, keep the latest.

                    for (int u = UpdateCount - 2; u >= 0; u--)
                    {

                        ApplyStoryUpdate(StoryUpdateStack[u]);
                        StoryUpdateStack.RemoveAt(u);

                    }

                    BufferStatusOk = false;
                    break;

            }

            switch (theDirector.status)
            {

                case DIRECTORSTATUS.ACTIVE:

                    //			Log.Message ( "director active ...");

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

                                            Log.Message("Creating and distributing global task " + task.description + " for pointer " + pointer.currentPoint.storyLineName);

                                            //if (task.description=="moodon"){
                                            //    Debug.Log("moodon task created at "+Time.frameCount);
                                            //}
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

                                        Log.Message("Creating local task " + task.description + " for pointer " + pointer.currentPoint.storyLineName);

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

                        Log.Warning("No handlers registred. Pausing director.");
                        theDirector.status = DIRECTORSTATUS.PAUSED;

                    }
                    else
                    {

                        Log.Message("" + GENERAL.SIGNOFFS + " handlers registred.");

                        Log.Message("Starting storyline " + launchOnStoryline);

                        theDirector.beginStoryLine(launchOnStoryline);
                        theDirector.status = DIRECTORSTATUS.ACTIVE;

                    }

                    break;

                case DIRECTORSTATUS.NOTREADY:

                    theDirector.loadScript(scriptName);

                    // create globals by default.

                    GENERAL.storyPoints.Add("GLOBALS", new StoryPoint("GLOBALS", "none", new string[] { "GLOBALS" }));
                    GENERAL.GLOBALS = new StoryTask("GLOBALS", SCOPE.GLOBAL);

                    break;

                default:
                    break;
            }

        }

#if NETWORKED

        void ApplyStoryUpdate(StoryUpdate storyUpdate)
        {

            //Debug.Log("Applying storyupdate \n"+storyUpdate.DebugLog);

            // load balance

            loadBalance = frameDurationAverage / storyUpdate.frameDuration;

            //Debug.Log("load balance ours/theirs (>1 they run faster) "+ loadBalance);

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

            StoryPointer pointer = GENERAL.GetStorylinePointerForPointID(pointerUpdate.storyPointID);

            Log.Message("Server update for pointer " + pointerUpdate.storyPointID);

            if (pointer != null && pointerUpdate.killed)
            {
                pointer.Kill();
                
                if (GENERAL.ALLTASKS.Remove(pointer.currentTask))
                {

                    Log.Message("Removing task " + pointer.currentTask.description);

                }
                else
                {

                    Log.Warning("Failed removing task " + pointer.currentTask.description);

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

                    Log.Message("Created an instance of global task " + updateTask.description);

                    if (taskUpdate.pointID != "GLOBALS")
                    {

                        // Now find a pointer.

                        StoryPointer updatePointer = GENERAL.GetStorylinePointerForPointID(taskUpdate.pointID);

                        if (updatePointer == null)
                        {

                            updatePointer = new StoryPointer();

                            Log.Message("Created a new pointer for new task.");

                        }

                        updatePointer.PopulateWithTask(updateTask);

                        Log.Message("Populated pointer from task. " + updatePointer.currentPoint.storyLineName);

                        DistributeTasks(new TaskArgs(updateTask));

                    }
                }


            }
            else
            {

                updateTask.ApplyUpdateMessage(taskUpdate);

                updateTask.scope = SCOPE.GLOBAL;

                Log.Message("Applied update to existing task.");

            }

        }


#endif


#if NETWORKED

        void LateUpdate()
        {

            // New approach: bundled updates.

            StoryUpdate storyUpdate = new StoryUpdate();

            // Calculate average framerate.

            float current = Time.deltaTime;

            frameDurrationBufferSum -= frameDurationBuffer[frameDurationBufferIndex];
            frameDurationBuffer[frameDurationBufferIndex] = current;
            frameDurrationBufferSum += current;
            frameDurationBufferIndex = (frameDurationBufferIndex + 1) % frameDurationBuffer.Length;
            frameDurationAverage = (frameDurrationBufferSum / frameDurationBuffer.Length);

            storyUpdate.frameDuration = frameDurationAverage;

            // Check our loadbalance. If we're sending too many updates we'll randomly drop frames.
            // All changes will be sent but if values are updated in the meantime the previous value will never be sent.
            // This is ok for running values but not ok for status values etc.

            if (loadBalance > 0.90f || Random.value < loadBalance)
            {

                // Iterate over all pointers to see if any were killed. Clients do not kill pointers themselves.

                for (int p = 0; p < GENERAL.ALLPOINTERS.Count; p++)
                {

                    StoryPointer pointer = GENERAL.ALLPOINTERS[p];

                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL && pointer.scope == SCOPE.GLOBAL && pointer.modified && pointer.GetStatus() == POINTERSTATUS.KILLED)
                    {

                        Log.Message("Sending pointer (killed) update to clients: " + pointer.currentPoint.storyLineName);

                        storyUpdate.AddStoryPointerUpdate(pointer.GetUpdate()); // bundled

                        //SendPointerUpdateToClients(pointer.GetUpdateMessage()); // individual

                        pointer.modified = false;


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

                        Log.Message("Task " + task.description + " completed, removing from alltasks. ");

                        //if (task.description=="moodon"){
                        //    Debug.Log("moodon task removed at "+Time.frameCount);
                        //}
                    }

                    if (task.modified)
                    {

                        // Check if we need to send network updates.

                        switch (GENERAL.AUTHORITY)
                        {

                            case AUTHORITY.LOCAL:

                                if (task.scope == SCOPE.GLOBAL)
                                {

                                    Log.Message("Global task " + task.description + " changed, sending update to server.");

                                    storyUpdate.AddTaskUpdate(task.GetUpdateBundled()); // bundled

                                    //sendTaskUpdateToServer(task.GetUpdateMessage());

                                }

                                break;

                            case AUTHORITY.GLOBAL:

                                if (task.scope == SCOPE.GLOBAL)
                                {

                                    Log.Message("Global task " + task.description + " changed, sending update to clients.");

                                    storyUpdate.AddTaskUpdate(task.GetUpdateBundled()); // bundled

                                    //sendTaskUpdateToClients(task.GetUpdateMessage());

                                    //if (task.description=="moodon"){
                                    //    Debug.Log("moodon task update sent at "+Time.frameCount);
                                    //}
                                }

                                break;

                            default:

                                break;

                        }

                        task.modified = false;
                    }

                }

                // If anything to send, send. 
                // If not, and buffer is overflowing, send updates randomly once a second.

                if (storyUpdate.AnythingToSend() || (!AssitantDirector.BufferStatusOk && Random.value < frameDurationAverage))
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

            Log.Message("Registering server message handlers.");

            NetworkServer.RegisterHandler(stringCode, OnMessageFromClient);
            NetworkServer.RegisterHandler(storyCode, OnStoryUpdateFromClient);

        }

        void onStopServer()
        {

            revertAllToLocal();

        }

        void onStartClient(NetworkClient theClient)
        {

            Log.Message("Registering client message handlers.");

            theClient.RegisterHandler(stringCode, OnMessageFromServer);
            theClient.RegisterHandler(storyCode, OnStoryUpdateFromServer);

        }

        void OnStopClient()
        {

            Log.Message("Client stopped. Resetting scope to local.");

            revertAllToLocal();

        }


        void OnServerConnect(NetworkConnection conn)
        {

            Log.Message("incoming server connection delegate called");

            GENERAL.SETNEWCONNECTION(conn.connectionId);

        }

        void OnClientConnect(NetworkConnection conn)
        {

            Log.Message("Client connection delegate called");

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

            Log.Message("Lost client connection. Resetting scope to local.");

            revertAllToLocal();

        }

        // Handle basic string messages.

        void OnMessageFromClient(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<StringMessage>();

            Log.Message("Message received from client: " + message.value);

        }

        void OnMessageFromServer(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<StringMessage>();

            Log.Message("Message received from server: " + message.value);

            if (message.value == "suspending")
            {

                Log.Message("Client will be suspending, closing their connection.");

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

            NetworkServer.SendToAll(storyCode, message);
            
        }

        void SendStoryUpdateToServer(StoryUpdate message)
        {

            networkManager.client.Send(storyCode, message);

        }
        
        void OnStoryUpdateFromClient(NetworkMessage netMsg)
        {

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
            Log.Message("Sending message to server: " + value);
        }

        public void sendMessageToClients(string value)
        {
            var msg = new StringMessage(value);
            NetworkServer.SendToAll(stringCode, msg);
            Log.Message("Sending message to all clients: " + value);
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