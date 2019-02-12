using UnityEngine;
using System.Collections.Generic;

#if NETWORKED
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
 */

    public class DataController : MonoBehaviour
    {


        //GameObject StoryEngineObject;
        TaskHandler dataTaskHandler;

#if NETWORKED
        GameObject NetworkObject;
        NetworkBroadcast networkBroadcast;
        ExtendedNetworkManager networkManager;
        List<string> TrackConnectedAddresses;
        public GameObject NetworkStatusObject;
        GameObject BufferStatusIn, BufferStatusOut;


        public string ConnectionMessage = "default";
        public int ConnectionKey = 1111;
        bool WasConnected = false;
        float startListening = 0f;
        bool listening = false;

        //   public string RemoteServerAddress,RemoteBroadcastServerAddress;
#endif

        //AssitantDirector ad;

        public static DataController Instance;

        bool handlerWarning = false;

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
        }
        void Start()
        {
            Verbose("Starting.");

            taskList = new List<StoryTask>();

            if (AssitantDirector.Instance == null)
            {
                Error("No Assistant Director instance.");
            }
            else
            {
                AssitantDirector.Instance.newTasksEvent += newTasksHandler;
#if NETWORKED

                NetworkObject = AssitantDirector.Instance.NetworkObject;

                if (NetworkObject == null)
                {
                    Warning("NetworkObject not found.");
                }
                else
                {
                    networkBroadcast = NetworkObject.GetComponent<NetworkBroadcast>();
                    networkManager = NetworkObject.GetComponent<ExtendedNetworkManager>();
                }

                if (DeusController.Instance != null && DeusController.Instance.DeusCanvas != null)
                {
                    // Create a network status prefab for visual debugging
                    GameObject ns = Instantiate(NetworkStatusObject);
                    ns.transform.SetParent(DeusController.Instance.DeusCanvas.transform, false);
                    BufferStatusIn = ns.transform.Find("BufferIn").gameObject;
                    BufferStatusOut = ns.transform.Find("BufferOut").gameObject;


                }

#endif
            }
        }

#if UNITY_IOS && NETWORKED


        void OnApplicationPause(bool paused)
        {


            if (paused)
            {

                Log("pauzing ...");
                Log("Disconnecting client ...");

                if (networkManager != null && networkManager.client != null)
                {
                    StopNetworkClient();
                }

            }
            else
            {
                Log("resuming ...");

            }
        }

#endif

#if NETWORKED

        // These are networking methods to be called from datahandler to establish connections.
        // Once connected, handling is done internally by the assistant directors.

        public List<string> ConnectedAddresses()
        {

            return networkManager.ConnectedAddresses;

        }

        public bool NewClientsConnected()
        {

            // Returns if new clients connected since the last call.

            List<string> NewConnectedAddresses = networkManager.ConnectedAddresses;

            if (TrackConnectedAddresses == null)
            {
                TrackConnectedAddresses = new List<string>();

                foreach (string s in NewConnectedAddresses)
                    TrackConnectedAddresses.Add(s);

                return NewConnectedAddresses.Count > 0 ? true : false;
            }

            bool NewClient = false;

            foreach (string address in NewConnectedAddresses)
            {
                if (!TrackConnectedAddresses.Contains(address))
                {
                    NewClient = true;
                    break;
                }
            }

            TrackConnectedAddresses.Clear();

            foreach (string s in NewConnectedAddresses)
                TrackConnectedAddresses.Add(s);

            return NewClient;

        }

        public int ConnectedClientsCount()
        {

            return networkManager.ConnectedAddresses.Count;

        }

        public void startBroadcastClient()
        {

            Log("Starting broadcast client, key: " + networkBroadcast.broadcastKey);
            networkBroadcast.StartClient();

        }

        public void startBroadcastClient(int _key)
        {

            networkBroadcast.broadcastKey = _key;
            startBroadcastClient();
        }

        public void startBroadcastServer()
        {

            Log("Starting broadcast server, key: " + networkBroadcast.broadcastKey);

            networkBroadcast.StartServer();

        }

        public void startBroadcastServer(int _key, string _message)
        {

            networkBroadcast.broadcastKey = _key;
            networkBroadcast.broadcastData = _message;
            startBroadcastServer();

        }

        public void stopBroadcast()
        {

            Log("Stopping broadcast server.");
            //if (networkBroadcast.isServer)
            networkBroadcast.Stop();

        }

        public void startNetworkClient(string server)
        {

            if (server == "")
            {
                Error("trying to start client without server address");
                return;
            }

            Log("Starting client for remote server " + server);

            networkManager.StartNetworkClient(server);

        }

        public void StopNetworkClient()
        {

            Log("Stopping network client.");

            networkManager.StopClient();

        }

        public void startNetworkServer()
        {

            networkManager.StartNetworkServer();

        }

        public void stopNetworkServer()
        {

            //networkManager.isNetworkActive
            networkManager.StopNetworkServer();

        }


        public string RemoteBroadcastServerAddress
        {
            get
            {
                return networkBroadcast.serverAddress;
            }
            set
            {
                Warning("Can't set remote broadcast server address directly");
            }

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

#if NETWORKED
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
#if NETWORKED

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

                        // ---------------------------- SERVER SIDE ----------------------------

                        case "startserver":

                            // Start broadcast and network server

                            Log("Starting broadcast server, key " + ConnectionKey + " message " + ConnectionMessage);
                            startBroadcastServer(ConnectionKey, ConnectionMessage);

                            Log("Starting network server.");
                            startNetworkServer();

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

                        case "monitorclients":

                            // Watch for new clients. Stays live indefinitely.

                            if (NewClientsConnected())
                            {
                                // new clients or are we getting all clients???

                                foreach (string add in ConnectedAddresses())
                                {
                                    Log("New client at " + add);
                                }

                                task.SetStringValue("debug", "clients: " + ConnectedAddresses().Count);
                                task.setCallBack("addclient");

                            }

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

                            if (listening)
                            {
                                if (foundServer())
                                {
                                    Log("Found broadcast server.");

                                    stopBroadcast();
                                    task.setCallBack("serverfound");
                                    listening = false;

                                    done = true;
                                }
                            }
                            else
                            {
                                startBroadcastClient(ConnectionKey);
                                Log("Starting broadcast listening for key " + ConnectionKey);
                                listening = true;
                            }

                            break;



                        case "startclient":

                            string ServerAddress = RemoteBroadcastServerAddress;

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
                                    task.setCallBack("serversearch");

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

        //    public void taskDone (StoryTask theTask){
        //        theTask.signOff (me);
        //        taskList.Remove (theTask);
        //
        //    }

        void newTasksHandler(object sender, TaskArgs e)
        {
            addTasks(e.theTasks);
        }

        public void addTasks(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);
        }

        //

#if NETWORKED

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