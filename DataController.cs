using UnityEngine;
using System.Collections.Generic;

#if !SOLO
using UnityEngine.UI;
using UnityEngine.Networking;
using StoryEngine.Network;

#endif


namespace StoryEngine
{


    /*!
 * \brief
 * Controls data and network operations.
 * 
 * Use addTaskHandler to attach your custom handler.
 * See samples for default tasks to create a LAN application.
 */

    public class DataController : MonoBehaviour
    {

        TaskHandler dataTaskHandler;

#if !SOLO
        GameObject NetworkObject;
        NetworkBroadcast networkBroadcast;
        ExtendedNetworkManager networkManager;
        List<string> TrackedAddresses, NewAddresses;

        public GameObject NetworkObjectRef;
        public GameObject NetworkStatusObjectRef;
        GameObject BufferStatusIn, BufferStatusOut;

        const short connectionMessageCode = 1001;

        public string ConnectionMessage = "default";
        public int ConnectionKey = 1111;
        bool WasConnected = false;

        bool listening = false;
        bool sending = false;

        bool serving = false;
         public bool Connected
        {
            get
            {
                return WasConnected;
            }
        }
#endif

        public static DataController Instance;
        bool handlerWarning = false;

        enum STATE
        {
            AWAKE,
            PAUSED,
            RESUMED
        }

        STATE state = STATE.AWAKE;

        public List<StoryTask> taskList;

        string ID = "DataController";

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        void Awake()
        {
            Instance = this;
            state = STATE.AWAKE;
        }
       

        void Start()
        {
            Verbose("Starting.");

            taskList = new List<StoryTask>();
            AssitantDirector.Instance.newTasksEvent += newTasksHandler;

#if !SOLO
            NetworkObjectInit();

            if (DeusController.Instance.DeusCanvas != null && NetworkStatusObjectRef != null)
            {
                // Instantiate network status object for debugging
                GameObject ns = Instantiate(NetworkStatusObjectRef);
                ns.transform.SetParent(DeusController.Instance.DeusCanvas.transform, false);
                BufferStatusIn = ns.transform.Find("BufferIn").gameObject;
                BufferStatusOut = ns.transform.Find("BufferOut").gameObject;

            }

#endif

        }

#if !SOLO
        private void OnApplicationQuit()
        {
            Log("Application stopping, shutting down network object.");

            NetworkObjectShutdown();

        }

        private void OnDestroy()
        {
            Log("Scene stopping, shutting down network object.");
            NetworkObjectShutdown();
        }

#endif

#if UNITY_IOS && !SOLO

        void OnApplicationPause(bool paused)
        {

            if (state == STATE.AWAKE && paused == false)
                return;// catching false call on startup

            state = paused ? STATE.PAUSED : STATE.RESUMED;

            switch (state)
            {
                case STATE.PAUSED:

                    Log("Application pausing.");
                    // networkManager.client.Send(connectionCode, "pausing");

                    //AssitantDirector.Instance.sendMessageToServer("I'm new.");

                    // var msg = new StringMessage("clientpause");
                    //networkManager.client.Send(stringCode, msg);
                    //Verbose("Sending message to server: " + value);

               //    NetworkTransport.s();
                   

                    NetworkObjectShutdown();

                    // This is a workaround. On IOS, when putting the app to sleep with the side button, 
                    // networking fails on resume. Home button is ok, side button isn't.

                    //Log("Shutting down networktransport.");
                    //NetworkTransport.Shutdown();

                    //if (listening)
                    //    stopBroadcast();

                    //if (networkManager != null && networkManager.client != null)
                    //    StopNetworkClient();

                    //if (serving || sending)
                    //{
                    //    if (serving)
                    //        stopNetworkServer(); // close ports

                    //    if (sending)
                    //        stopBroadcast();// close ports

                    //    // This is a workaround. On IOS, when putting the app to sleep with the side button, 
                    //    // networking fails on resume. Home button is ok, side button isn't.
                    //    NetworkTransport.Shutdown();

                    //    networkBroadcast = null;
                    //    networkManager = null;
                    //    Destroy(NetworkObject);
                    //    sending = false;
                    //    serving = false;
                    //}

                    break;

                case STATE.RESUMED:

                    Log("Application resuming.");

                    NetworkObjectInit();
                    //NetworkTransport.Init();
                    break;

            }

        }

#endif

#if !SOLO

        public List<string> TrackedConnectedAddresses()
        {
            return TrackedAddresses;
        }

        public List<string> NewConnectedAddresses()
        {
            return NewAddresses;
        }
        public void RemoveTrackedAddress(string address)
        {

            TrackedAddresses.Remove(address);

        }
        public bool NewTrackedAddresses()
        {


            if (NewAddresses == null)
                NewAddresses = new List<string>();

            NewAddresses.Clear();

            if (TrackedAddresses == null)
                TrackedAddresses = new List<string>();

            foreach (string address in networkManager.ConnectedAddresses)
            {
                if (!TrackedAddresses.Contains(address))
                {
                    TrackedAddresses.Add(address);
                    NewAddresses.Add(address);

                }
            }

            int i = TrackedAddresses.Count - 1;

            while (i >= 0)
            {
                if (!networkManager.ConnectedAddresses.Contains(TrackedAddresses[i]))
                {
                    TrackedAddresses.RemoveAt(i);

                }
                i--;
            }

            return NewAddresses.Count > 0;

        }

        public void startBroadcastClient()
        {
            networkBroadcast.StartClient();
            listening = true;
        }

        public void startBroadcastClient(int _key)
        {

            networkBroadcast.broadcastKey = _key;
            startBroadcastClient();
            listening = true;
        }

        public void startBroadcastServer()
        {
            //if (networkBroadcast == null)
            //return;

            networkBroadcast.StartServer();
            sending = true;
        }

        public void startBroadcastServer(int _key, string _message)
        {
            //if (networkBroadcast == null)
            //return;

            networkBroadcast.broadcastKey = _key;
            networkBroadcast.broadcastData = _message;
            startBroadcastServer();
            sending = true;

        }

        public void stopBroadcast()
        {
            networkBroadcast.Stop();

            //networkBroadcast.
            //networkBroadcast.StopBroadcast();
            listening = false;
            sending = false;
        }

        public void startNetworkClient(string server)
        {

            if (server == "")
            {
                Error("No server address.");
                return;
            }

            Log("Starting client for remote server " + server);

            networkManager.StartNetworkClient(server);

        }

        public void StopNetworkClient()
        {

            //Log("Stopping network client.");
            NetworkClient client = networkManager.client;

            if (client != null)
            {
                networkManager.client.Disconnect();
                networkManager.StopClient();
            }

        }

        public void startNetworkServer()
        {

            networkManager.StartNetworkServer();
            serving = true;
        }

        public void stopNetworkServer()
        {

            networkManager.StopNetworkServer();
            serving = false;
        }

        public bool foundServer()
        {

            if (networkBroadcast.serverMessage != "")
            {

                return true;

            }
            else
            {

                return false;
            }

        }

        public bool clientIsConnected()
        {

            if (networkManager.client == null)
                return false;

            if (!networkManager.client.isConnected)
                return false;

            //  !networkManager.client.

            return true;

        }

#endif

        public void addTaskHandler(TaskHandler theHandler)
        {
            dataTaskHandler = theHandler;
            Verbose("Handler added.");
        }

        void Update()
        {




#if UNITY_EDITOR && UNITY_IOS

            // Pause applicatin simulation for debugging.

            if (Input.GetKeyDown("p"))
            {
                OnApplicationPause(true);
            }

            if (Input.GetKeyDown("o"))
            {
                OnApplicationPause(false);
            }

            if (state == STATE.PAUSED)
                return;

#endif


#if !SOLO
            if (displayNetworkGUIState())
                SetNetworkIndicators();
#endif

            int t = 0;

            while (t < taskList.Count)
            {

                StoryTask task = taskList[t];

                if (task.Instruction == "end")
                {
                    Log("Encountered end task, removing pointer " + task.Pointer.currentPoint.StoryLine);

                    GENERAL.ALLPOINTERS.Remove(task.Pointer);

                }

                if (!GENERAL.ALLTASKS.Exists(at => at == task))
                {
                    // Task was removed, so stop executing it.
                    Log("Removing task:" + task.Instruction);
                    taskList.RemoveAt(t);
                }
                else
                {
                    // Task needs to be executed.
                    // first check for storyengine tasks.
                    bool done = false;
                    switch (task.Instruction)
                    {
#if !SOLO

                        // ---------------------------- VISUAL DEBUGGING ----------------------------
                        case "debugon":

                            // Show network info.

                            displayNetworkGUI(true);
                            done = true;
                            break;

                        case "debugoff":

                            // Hide network info.

                            displayNetworkGUI(false);
                            done = true;
                            break;

                        case "toggledebug":

                            // Toggle network info.

                            displayNetworkGUI(!displayNetworkGUIState());
                            done = true;
                            break;

                        // ---------------------------- SCOPE ----------------------------

                        case "isglobal":

                            if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                            {
                                task.Pointer.scope = SCOPE.GLOBAL;
                            }

                            done = true;
                            break;

                        case "islocal":

                            task.Pointer.SetLocal();

                            //Verbose("Setting pointer scope to local: " + task.Pointer.currentPoint.StoryLine);

                            done = true;
                            break;



                        // ---------------------------- SERVER SIDE ----------------------------

                        case "startserver":

                            // Start broadcast and network server if not already going.

                            if (!sending)
                            {
                                Log("Starting broadcast server, key " + ConnectionKey + " message " + ConnectionMessage);
                                startBroadcastServer(ConnectionKey, ConnectionMessage);
                            }

                            if (!serving)
                            {
                                Log("Starting network server.");
                                startNetworkServer();
                            }

                            done = true;
                            break;

                        case "stopserver":

                            // Stop broadcast and network server

                            Log("Stopping broadcast server.");
                            stopBroadcast();

                            Log("Stopping network server.");
                            stopNetworkServer();

                            done = true;
                            break;

                        case "monitorconnections":

                            if (NetworkObject == null || state == STATE.PAUSED)
                                break;

                            // Keep servers up on IOS (because of application pause)

#if UNITY_IOS
                            if (!sending)
                            {
                                Log("Starting broadcast server, key " + ConnectionKey + " message " + ConnectionMessage);
                                startBroadcastServer(ConnectionKey, ConnectionMessage);
                            }

                            if (!serving)
                            {
                                Log("Starting network server.");
                                startNetworkServer();
                            }

#endif

                            // Watch for new clients. Stays live indefinitely.

                            if (serving && NewTrackedAddresses())
                            {
                                // new addresses connected since last call

                                foreach (string add in NewConnectedAddresses())
                                {
                                    Log("New client at " + add);
                                }


                                task.setCallBack("addclient");

                            }

                            task.SetStringValue("debug", "clients: " + TrackedConnectedAddresses().Count);
                            WasConnected = TrackedConnectedAddresses().Count > 0;


                            break;

                        case "pushglobaltasks":

                            // go over all pointers  and mark everything as modified
                            // that way it'll get sent.
                            // with single client apps this is ok, but for multiple clients we need something more advanced

                            Log("Pushing global tasks.");

                            foreach (StoryTask theTask in GENERAL.ALLTASKS)
                            {
                                if (theTask.scope == SCOPE.GLOBAL)
                                {
                                    theTask.MarkAllAsModified();
                                }
                            }

                            done = true;
                            break;

                        // ---------------------------- CLIENT SIDE ----------------------------


                        case "serversearch":

                            if (NetworkObject == null || state == STATE.PAUSED)
                                break;

                            if (listening)
                            {
                                if (foundServer())
                                {
                                    Log("Found broadcast server " + networkBroadcast.serverAddress+" key " + networkBroadcast.broadcastKey);
                                   
                                    stopBroadcast();

                                    task.setCallBack("serverfound");
                                    listening = false;

                                    done = true;
                                }
                            }
                            else
                            {

                                startBroadcastClient(ConnectionKey);
                                Log("Restarting broadcast listening for key " + ConnectionKey);
                                listening = true;


                            }

                            break;



                        case "startclient":

                            string ServerAddress = networkBroadcast.serverAddress;

                            if (ServerAddress != "")
                            {
                                Log("Starting network client for remote server " + ServerAddress);
                                startNetworkClient(ServerAddress);
                            }
                            else
                            {
                                Warning("Trying to start client without server address.");
                            }

                            done = true;
                            break;

                        case "stopclient":

                            Log("Stopping network client.");

                            StopNetworkClient();
                            WasConnected = false;

                            done = true;

                            break;

                        case "monitorserver":

                            // Watches the connection, stays active until connection lost.

                            if (clientIsConnected())
                            {
                                WasConnected = true;
                            }
                            else
                            {
                                if (WasConnected)
                                {
                                    // Lost connection. Restart server search.

                                    Log("Lost server connection.");

                                    StopNetworkClient();
                                    WasConnected = false;
                                    task.setCallBack("client");

                                    done = true;

                                }
                                else
                                {
                                    //
                                }
                            }

                            break;



#endif
                        default:

                            // Not a controller task, pass it on.

                            if (dataTaskHandler != null)
                            {
                                done = dataTaskHandler(task);

                            }
                            else
                            {
                                // If no handler available we just sign off, but issue a warning once.
                                done = true;

                                if (!handlerWarning)
                                {
                                    Warning("No handler registered.");
                                    handlerWarning = true;
                                }

                            }

                            break;

                    }

                    if (done)
                    {
                        task.signOff(ID);
                        taskList.RemoveAt(t);
                    }
                    else
                    {
                        t++;
                    }



                }

            }

        }

        void newTasksHandler(object sender, TaskArgs e)
        {
            addTasks(e.theTasks);
        }

        public void addTasks(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);
        }

      
#if !SOLO

        void NetworkObjectInit()
        {

            if (NetworkObject != null)
                return;

            if (NetworkObjectRef == null)
            {
                Log("No networkobject reference.");
                return;
            }

            Log("Creating network object.");
            NetworkObject = Instantiate(NetworkObjectRef);
            networkBroadcast = NetworkObject.GetComponent<NetworkBroadcast>();
            networkManager = NetworkObject.GetComponent<ExtendedNetworkManager>();
            AssitantDirector.Instance.SetNetworkObject(NetworkObject);

            //networkManager.onServerConnectDelegate = OnServerConnect;
            //networkManager.onServerDisconnectDelegate = OnServerDisconnect;



            if (state == STATE.RESUMED)
            {
                // on state awake this is already active.
                NetworkTransport.Init();
            }

        }

        void NetworkObjectShutdown()
        {
            // When pausing a server on ios using the sleep/side button, somehow network resources aren't properly disposed.
            // This why we destroy the object and stop start networkstransport.

            if (NetworkObject == null)
                return;

            // Client side shutdown.

            if (listening)
            {
                Verbose("Stopping broadcast");
                stopBroadcast();
            }
           

            if (networkManager != null && networkManager.client != null)
            {
                Verbose("Stopping networkclient.");
                StopNetworkClient();
            }
          

            // Server side shutdown.

            if (serving || sending)
            {
                if (serving)
                    stopNetworkServer(); // close ports

                if (sending)
                    stopBroadcast();// close ports

                networkBroadcast = null;
                networkManager = null;
                Destroy(NetworkObject);

                Log("Shutting down networktransport.");
                NetworkTransport.Shutdown();

            }

        }

        //void OnServerConnect(NetworkConnection conn)
        //{

        //    Warning("Incoming server connection delegate called ");

        //    //     GENERAL.SETNEWCONNECTION(conn.connectionId);

        //}

        //void OnServerDisconnect(NetworkConnection conn)
        //{

        //    Warning("Incoming server disconnection delegate called ");

        //    //     GENERAL.SETNEWCONNECTION(conn.connectionId);

        //}

        public bool displayNetworkGUIState()
        {
            return networkBroadcast != null && networkBroadcast.showGUI;

        }

        public void displayNetworkGUI(bool status)
        {
            NetworkManagerHUD hud = (NetworkObject == null ? null : NetworkObject.GetComponent<NetworkManagerHUD>());

            if (hud != null) hud.showGUI = status;
            if (networkBroadcast != null) networkBroadcast.showGUI = status;

        }

        void SetNetworkIndicators()
        {
            if (BufferStatusIn == null || BufferStatusOut == null)
                return;

            BufferStatusIn.SetActive(WasConnected);
            BufferStatusOut.SetActive(WasConnected);

            switch (AssitantDirector.BufferStatusIn)
            {
                case 0:
                    BufferStatusIn.GetComponent<Image>().color = Color.grey;
                    break;
                case 1:
                case 2:
                    BufferStatusIn.GetComponent<Image>().color = Color.green;
                    break;
                default:
                    BufferStatusIn.GetComponent<Image>().color = Color.cyan;
                    break;
            }
            switch (AssitantDirector.BufferStatusOut)
            {
                case 0:
                    BufferStatusOut.GetComponent<Image>().color = Color.grey;
                    break;
                case 1:
                case 2:
                    BufferStatusOut.GetComponent<Image>().color = Color.green;
                    break;
                default:
                    BufferStatusOut.GetComponent<Image>().color = Color.cyan;
                    break;
            }
        }

#endif
    }

}