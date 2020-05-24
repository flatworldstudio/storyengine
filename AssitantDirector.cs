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



    //    List<StoryUpdate> StoryUpdateStack;

        //    GameObject NetworkObject;

        //  ExtendedNetworkManager networkManager;

        static public int BufferStatusOut = 0;
        static public int BufferStatusIn = 0;
        

        // Copy these into every class for easy debugging.
        void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

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


        void Update()
        {

            if (NetworkHandler.Instance != null)
            {

             
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
                        // Apply the oldest one, keep the other because we exact 0 updates during our next frame.

                        ApplyStoryUpdate(NetworkHandler.Instance.StoryUpdateStack[0]);
                        NetworkHandler.Instance.StoryUpdateStack.RemoveAt(0);
                        BufferStatusIn = 1;

                        break;

                    default:

                        // More than 2. Apply all the older ones in order of arrival, keep latest one.

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
            // Handle story updates, aiming for 1 per frame, assuming we're tyring to run in sync.
            // Lowest numbers are oldest.

           

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

        }




        void ApplyStoryUpdate(StoryUpdate storyUpdate)
        {

            DataUpdate dataUpdate;
            while (storyUpdate.GetDataUpdate(out dataUpdate))
                ApplyDataUpdate(dataUpdate);

        }

        void ApplyDataUpdate(DataUpdate dataUpdate)
        {

            // See if we have an instance of this.

            //   GENERAL.ALLDATA.Contains




        }


        void LateUpdate()
        {
            // Check if any data changed during this frame, and send updates across network.

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




                    }
                    else
                    {

                        Warning("Dropping update.");

                    }
                }




            }





        }




        public int eventHandlerCount()
        {

            int count = 0;


            //if (newTasksEvent != null)
            //{

            //    count += newTasksEvent.GetInvocationList().Length;

            //}

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