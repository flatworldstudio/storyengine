using UnityEngine;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;
using UnityEngine.Events;
using JimEngine.Basemodule;

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using StoryEngine.Network;


namespace StoryEngine
{


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

        [Header("Log levels")]
        public LOGLEVEL DefaultLogLevel;/*!< \brief Set this value in Unity Editor */
        public LOGLEVEL DirectorLogLevel;
        public LOGLEVEL ADLogLevel;
        public LOGLEVEL DataControllerLogLevel;
        public LOGLEVEL ControllerLogLevel;
        public LOGLEVEL EventLogLevel;

        string ID = "AD";
        private NewTasksEventUnity newTasksEventUnity;

        Director theDirector;
        string launchStoryline;

        static public int BufferStatusOut = 0;
        static public int BufferStatusIn = 0;

        // Copy these into every class for easy debugging.
        void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

        #region LIFECYCLE
        void Awake()
        {
            Instance = this;
            if (newTasksEventUnity == null)
            {
                newTasksEventUnity = new NewTasksEventUnity();
            }

            theDirector = new Director();
            GENERAL.ALLTASKS = new List<StoryTask>();
            //  GENERAL.ALLDATA = new List<StoryData>();
        }


        void Start()
        {

            Verbose("Starting.");

            StoryEngine.Log.SetModuleLevel("Director", DirectorLogLevel);
            StoryEngine.Log.SetModuleLevel("AD", ADLogLevel);
            //       StoryEngine.Log.SetModuleLevel("DataController", DataControllerLogLevel);

            StoryEngine.Log.SetModuleLevel("Controller", ControllerLogLevel);
            StoryEngine.Log.SetModuleLevel("Event", EventLogLevel);




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

        #endregion

        void Update()
        {
            #region APPLYUPDATES

            if (NetworkHandler.Instance != null)
            {

                // Handle story updates, aiming for 1 per frame, assuming we're tyring to run in sync.
                // Lowest numbers are oldest.

                int UpdateCount = NetworkHandler.Instance.StoryUpdateStack.Count;

                switch (UpdateCount)
                {

                    case 0:
                        BufferStatusIn = 0;
                        break;

                    case 1:

                        // exactly one, so apply.

                        ApplyStoryUpdate(NetworkHandler.Instance.StoryUpdateStack[0]);
                        NetworkHandler.Instance.StoryUpdateStack.RemoveAt(0);
                        BufferStatusIn = 1;

                        break;

                    case 2:

                        // Two, normally for different frames that happened to arrive in the same frame on this end. 
                        // Apply the oldest one, keep the other because we expect to get 0 updates during our next frame
                        // (If we're on similar fps)

                        ApplyStoryUpdate(NetworkHandler.Instance.StoryUpdateStack[0]);
                        NetworkHandler.Instance.StoryUpdateStack.RemoveAt(0);
                        BufferStatusIn = 1;

                        break;

                    default:

                        // More than 2. Apply all the older ones in order of arrival, keep latest one because we may get 0 next frame.

                        Warning("Update buffer >2");
                        BufferStatusIn = 2;

                        while (NetworkHandler.Instance.StoryUpdateStack.Count > 1)
                        {

                            ApplyStoryUpdate(NetworkHandler.Instance.StoryUpdateStack[0]);
                            NetworkHandler.Instance.StoryUpdateStack.RemoveAt(0);

                        }

                        break;

                }


            }

            #endregion

            #region DIRECTOR

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
                            //  if (true || task.scope == SCOPE.LOCAL)
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
                                //      if (true)
                                {

                                    if (pointer.GetStatus() == POINTERSTATUS.NEWTASK)
                                    {

                                        pointer.SetStatus(POINTERSTATUS.PAUSED);

                                        StoryTask task = new StoryTask(pointer, SCOPE.GLOBAL);
                                        task.LoadPersistantData(pointer);

                                        newTasks.Add(task);
                                        task.modified = true;

                                        Verbose("Created global task " + task.Instruction + " with pointid " + task.PointID + " for pointer " + pointer.currentPoint.StoryLine);

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


                                    Verbose("Created local task " + task.Instruction + " with pointid " + task.PointID + " for pointer " + pointer.currentPoint.StoryLine);

                                }

                                break;

                        }


                    }

                    if (newTasks.Count > 0)
                    {
                        if (newTasksEventUnity != null) newTasksEventUnity.Invoke(newTasks);
                        //     if (newTasksEvent != null) newTasksEvent(this, new TaskArgs(newTasks)); // trigger the event, if there are any listeners

                        //  DistributeTasks(new TaskArgs(newTasks)); // if any new tasks call an event, passing on the list of tasks to any handlers listening
                    }

                    break;

                case DIRECTORSTATUS.READY:

                    int SIGNOFFS = eventHandlerCount();

                    if (SIGNOFFS == 0)
                    {

                        Error("No handlers registred. Pausing director.");
                        theDirector.status = DIRECTORSTATUS.PAUSED;

                    }
                    else
                    {

                        Verbose("" + SIGNOFFS + " handlers registred at this time.");

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

            #endregion
        }



        #region SENDUPDATES


        void LateUpdate()
        {
            // Check if any data changed during this frame, and send updates across network.

            StoryUpdate storyUpdate = new StoryUpdate(); // Contains a collection of task and pointer updates.


            // Is there a networkhandler.

            if (NetworkHandler.Instance != null)
            {

                int Queue = NetworkHandler.Instance.GetQueueSize();

                if (Queue == -1)
                {
                    //  Verbose("no network active");
                }
                else
                {

                    switch (Queue)
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

                    if (Queue < 3 || true)
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

                                Verbose("Task " + task.Instruction + " on storyline " + task.Pointer.currentPoint.StoryLine + " completed, removed from alltasks. ");

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

                                                Verbose("Global task " + task.Instruction +  " with id "+ task.PointID + " changed, adding to update for server.");

                                                storyUpdate.AddTaskUpdate(task.GetUpdateBundled()); // bundled

                                            }

                                            break;

                                        case AUTHORITY.GLOBAL:

                                            if (task.scope == SCOPE.GLOBAL)
                                            {

                                                Verbose("Global task " + task.Instruction + " with id " + task.PointID + " changed, adding to update for clients.");

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

                                    if (NetworkHandler.Instance != null) NetworkHandler.Instance.SendStoryUpdateToServer(storyUpdate);
                                    Log("Sending story update to server. \n" + storyUpdate.DebugLog);

                                    break;
                                case AUTHORITY.GLOBAL:

                                    if (NetworkHandler.Instance != null) NetworkHandler.Instance.SendStoryUpdateToClients(storyUpdate);

                                    Log("Sending story update to clients. \n" + storyUpdate.DebugLog);
                                    //Debug.Log(storyUpdate.ToString());

                                    break;
                                default:
                                    break;


                            }

                        }


                    }


                }











                /*



                            if (NetworkHandler.Instance.IsClient())
                            {
                                StoryUpdate storyUpdate = new StoryUpdate(); // Contains a collection of dataupdates.

                                // Iterate over all data.

                                for (int i = GENERAL.ALLTASKS.Count - 1; i >= 0; i--)
                                {
                                    StoryTask data = GENERAL.ALLTASKS[i];

                                    if (data.modified)
                                    {
                                        // Data was modified locally, if global, compile an update for the server.

                                        if (data.scope == SCOPE.GLOBAL)
                                        {
                                            Verbose("Data was modified locally as client, creating update for server for " + data.ID);
                                            storyUpdate.AddDataUpdate(data.GetDataUpdate()); // bundled
                                        }

                                        data.modified = false;
                                    }
                                }

                                // If anything to send, send. 

                                if (storyUpdate.AnythingToSend())

                                {

                                   NetworkHandler.Instance.SendStoryUpdateToServer(storyUpdate);

                                    Verbose("Sending story update to server. \n" + storyUpdate.DebugLog);



                                }



                            }




                            // If server, messages may differ for clients.


                            if (NetworkHandler.Instance.IsServer())
                            {

                                string[] clients = NetworkHandler.Instance.TrackedConnectedAddresses().ToArray();

                                for (int c = 0; c < clients.Length; c++)
                                {

                                    StoryUpdate storyUpdate = new StoryUpdate(); // Contains a collection of dataupdates.

                                    // Iterate over all data.

                                    for (int i = GENERAL.ALLTASKS.Count - 1; i >= 0; i--)
                                    {
                                        StoryTask data = GENERAL.ALLTASKS[i];

                                        if (data.modified)
                                        {
                                            // Data was modified locally, if global, compile an update for the server.

                                            if (data.scope == SCOPE.GLOBAL)
                                            {
                                                Verbose("Data was modified locally as client, creating update for server for " + data.ID);
                                                storyUpdate.AddDataUpdate(data.GetDataUpdateFor(clients[c])); // bundled
                                            }

                                            data.modified = false;
                                        }
                                    }

                                    // If anything to send, send. 

                                    if (storyUpdate.AnythingToSend())

                                    {
                                        NetworkHandler.Instance.SendStoryUpdateToClient(storyUpdate, clients[c]);

                                        Verbose("Sending story update to client. \n" + storyUpdate.DebugLog);



                                    }


                                }




                            }


        */

            }


        }



        #endregion





        #region UPDATEMETHODS

        //void ApplyStoryUpdate(StoryUpdate storyUpdate)
        //{

        //    DataUpdate dataUpdate;
        //    while (storyUpdate.GetDataUpdate(out dataUpdate))
        //        ApplyDataUpdate(dataUpdate);

        //}

        //void ApplyDataUpdate(DataUpdate dataUpdate)
        //{

        //    // See if we have an instance of this.

        //    //   GENERAL.ALLDATA.Contains




        //}

        void ApplyStoryUpdate(StoryUpdate storyUpdate)
        {
            // apply all pointerupdates

            StoryPointerUpdate pointerUpdateBundled;

            while (storyUpdate.GetPointerUpdate(out pointerUpdateBundled))
                ApplyPointerUpdate(pointerUpdateBundled);

            // apply all taskupdates

            StoryTaskUpdate taskUpdateBundled;

            while (storyUpdate.GetTaskUpdate(out taskUpdateBundled))
                ApplyTaskUpdate(taskUpdateBundled);

        }

        void ApplyPointerUpdate(StoryPointerUpdate pointerUpdate)
        {

            // Right now the only update we send for pointers is when they are killed.

            StoryPointer pointer = GENERAL.GetPointerForStoryline(pointerUpdate.StoryLineName);

            Log("Server says kill pointer: " + pointerUpdate.StoryLineName);

            if (pointer != null)
            {

                // We remove it instantly. No need to mark it as deleted, nothing else to do with it.

                GENERAL.ALLPOINTERS.Remove(pointer);
                Log("Removing pointer: " + pointer.currentPoint.StoryLine);


                // Remove tasks for pointer
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

        void ApplyTaskUpdate(StoryTaskUpdate taskUpdate)
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

                        //     DistributeTasks(new TaskArgs(updateTask));

                        List<StoryTask> newTasks = new List<StoryTask>();
                        newTasks.Add(updateTask);

                        //     if (newTasksEvent != null) newTasksEvent(this, new TaskArgs(updateTask)); // trigger the event, if there are any listeners

                        //    if (newTasksEvent != null) newTasksEvent(this, new TaskArgs(newTasks)); // trigger the event, if there are any listeners

                        if (newTasksEventUnity != null) newTasksEventUnity.Invoke(newTasks);


                    }
                }


            }
            else
            {

                updateTask.ApplyUpdateMessage(taskUpdate);

                updateTask.scope = SCOPE.GLOBAL;//?? 

                Verbose("Applied update to existing task " + updateTask.Instruction);

            }

        }
        #endregion



        public int eventHandlerCount()
        {

            int count = 0;

            if (newTasksEventUnity != null)
            {
                count += newTasksListenerUnityCount;
            }

            return count;
        }

        int newTasksListenerUnityCount = 0;

        public void AddNewTasksListenerUnity(UnityAction<List<StoryTask>> call)
        {
            Verbose("Adding unity event listener.");
            newTasksEventUnity.AddListener(call);
            newTasksListenerUnityCount++;
            // it seems unity can't count its event listeners, so we'll have to do that here.

            Verbose("Unity event listener count " + newTasksListenerUnityCount);

        }

        public void RemoveNewTasksListenerUnity(UnityAction<List<StoryTask>> call)
        {
            Verbose("Removing unity event listener.");
            newTasksEventUnity.RemoveListener(call);
            newTasksListenerUnityCount--;
            // it seems unity can't count its event listeners, so we'll have to do that here.

            Verbose("Unity event listener count " + newTasksListenerUnityCount);
        }

    }

    [System.Serializable]
    public class NewTasksEventUnity : UnityEvent<List<StoryTask>>
    {


    }



}