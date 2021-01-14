using UnityEngine;
using System.Collections.Generic;


using UnityEngine.UI;
using UnityEngine.Networking;
using StoryEngine.Network;
using System.Net.NetworkInformation;
using System.Linq;
using UnityEngine.Networking.NetworkSystem;
using JimEngine.ConfigHandler;
using System.Net;
using System.Net.Sockets;

namespace StoryEngine
{


    /*!
 * \brief
 * Controls data and network operations.
 * 
 * Use addTaskHandler to attach your custom handler.
 * See samples for default tasks to create a LAN application.
 */

    public class NetworkHandler : MonoBehaviour
    {

        //[Header("Logging")]
        //public LOGLEVEL LogLevel = LOGLEVEL.WARNINGS;
        //public LOGLEVEL LogLevelNetworkManager = LOGLEVEL.WARNINGS;

        GameObject NetworkObject;
        NetworkBroadcast networkBroadcast;
        ExtendedNetworkManager networkManager;
        List<string> TrackedAddresses, NewAddresses;

        public GameObject NetworkObjectRef;
        //   public GameObject NetworkStatusObjectRef;
        //       GameObject BufferStatusIn, BufferStatusOut;

        const short connectionMessageCode = 1001;

        string BroadcastMessageString = "";
        int BroadcastKey = 0;


        const short stringCode = 1002;
        const short storyCode = 1005;

        public List<StoryUpdate> StoryUpdateStack;

        bool WasConnected = false;

        bool Listening = false;
        bool Broadcasting = false;

        bool Serving = false;

        public bool Connected
        {
            get
            {
                return WasConnected;
            }
        }

        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static NetworkHandler Instance;

        //  static string __addr="";
        public static string Addr
        {
            get
            {
                if (Instance != null)
                {
                    if (Instance.IsServer())
                    {
                        return LocalIPAddress();
                    }
                    if (Instance.IsClient())
                    {
                        return Instance.networkManager.networkAddress;
                    }

                }

                return "local";

                //       if (Instance != null && Instance.networkManager != null) return Instance.networkManager.networkAddress;
                //     Instance.networkManager.NetworkServer;
                // return      NetworkManager.singleton.networkAddress;



            }

        }

        enum STATE
        {
            AWAKE,
            PAUSED,
            RESUMED
        }

        STATE state = STATE.AWAKE;

        public List<StoryTask> taskList;

        string ID = "NetworkHandler";
        bool Mounted = false;
        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        void Awake()
        {
            TrackedAddresses = new List<string>();
            NewAddresses = new List<string>();
            StoryUpdateStack = new List<StoryUpdate>();
            Instance = this;
            state = STATE.AWAKE;
        }

        public void Mount(MountPrefab prefab)
        {
            // NOTE: not implemented yet mounted vs unmounted logic. a lot happening in start and awake etc.
            Mounted = true;
        }

        public void UnMount()
        {
            Log("Shutting down network object.");
            NetworkObjectShutdown();

            for (int t = 0; t < taskList.Count; t++) taskList[t].signOff(ID);
            taskList.Clear();
            Mounted = false;
        }

        void OnDestroy()
        {
            if (Mounted)
            {
                Warning("Destroying while mounted, unmount first.");
                UnMount();
            }
        }


        public void SetBroadcastInfo(int key, string message)
        {
            BroadcastKey = key;
            BroadcastMessageString = message;
        }

        public void SetBroadcastKey(int key)
        {
            BroadcastKey = key;

        }

        public void SetBroadcastMessage(string message)
        {

            BroadcastMessageString = message;
        }

        public void ResetBroadcastInfo()
        {
            BroadcastKey = 0;
            BroadcastMessageString = "";
        }

        void Start()
        {

            taskList = new List<StoryTask>();

            // Add ourselves to the ad's task distribution event.
            if (AssitantDirector.Instance != null)
            {
                AssitantDirector.Instance.AddNewTasksListenerUnity(newTasksHandlerUnity);

            }

            Verbose("Starting.");

            NetworkObjectInit();

            //if (DeusHandler.Instance.DeusCanvas != null && NetworkStatusObjectRef != null)
            //{
            //    // Instantiate network status object for debugging
            //    GameObject ns = Instantiate(NetworkStatusObjectRef);
            //    ns.transform.SetParent(DeusHandler.Instance.DeusCanvas.transform, false);
            //    BufferStatusIn = ns.transform.Find("BufferIn").gameObject;
            //    BufferStatusOut = ns.transform.Find("BufferOut").gameObject;

            //}

        }

        //private void OnApplicationQuit()
        //{
        //    Log("Application stopping, shutting down network object.");

        //    NetworkObjectShutdown();

        //}




#if UNITY_IOS

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

        #region UNITY

        void NetworkObjectInit()
        {

            if (NetworkObject != null)
            {
                Warning("Already initialised");
                return;
            }


            if (NetworkObjectRef == null)
            {
                Warning("No networkobject reference.");
                return;
            }

            Verbose("Instantiating network object.");
            NetworkObject = Instantiate(NetworkObjectRef);
            networkBroadcast = NetworkObject.GetComponent<NetworkBroadcast>();
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

            if (state == STATE.RESUMED)
            {
                // on state awake this is already active.
                NetworkTransport.Init();
            }


        }

        #endregion

        #region BROADCAST

        public void startBroadcastClient()
        {

            networkBroadcast.StartClient();
            Listening = true;
        }

        public void startBroadcastClient(int _key)
        {
            networkBroadcast.broadcastKey = _key;
            startBroadcastClient();
        }

        public void startBroadcastServer()
        {
            //      Verbose("Starting broadcast server.\nKey: " + networkBroadcast.broadcastKey + "\nMessage: " + networkBroadcast.broadcastData);
            networkBroadcast.StartServer();
            Broadcasting = true;
        }

        public void startBroadcastServer(int _key, string _message)
        {
            networkBroadcast.broadcastKey = _key;
            networkBroadcast.broadcastData = _message;
            startBroadcastServer();
        }

        public void stopBroadcast()
        {
            networkBroadcast.Stop();
            Listening = false;
            Broadcasting = false;
        }

        #endregion

        #region SERVER

        public void startNetworkServer()
        {
            //  Verbose("Starting network server.");
            networkManager.StartNetworkServer();
            Serving = true;
        }

        void onStartServer()
        {
            Verbose("Network server started, adding message handlers.");
            NetworkServer.RegisterHandler(stringCode, OnMessageFromClient);
            NetworkServer.RegisterHandler(storyCode, OnStoryUpdateFromClient);
        }

        void OnServerConnect(NetworkConnection conn)
        {

            Verbose("Remote client connected from " + conn.address);

            Verbose(networkManager.ConnectedClientList());

            //     GENERAL.SETNEWCONNECTION(conn.connectionId);

        }

        public void stopNetworkServer()
        {
            networkManager.StopNetworkServer();
            Serving = false;
        }


        void onStopServer()
        {

        }

        #endregion

        #region CLIENT



        void onStartClient(NetworkClient theClient)
        {

            //      Logger.Message("Registering client message handlers.");

            theClient.RegisterHandler(stringCode, OnMessageFromServer);
            theClient.RegisterHandler(storyCode, OnStoryUpdateFromServer);

        }

        void OnStopClient()
        {

            Warning("Client stopped. ");

            //   revertAllToLocal();

        }

        void OnClientConnect(NetworkConnection conn)
        {

            Verbose("Client connection delegate called");

            //  GENERAL.AUTHORITY = AUTHORITY.LOCAL;
            //   sendMessageToServer("REGISTER");

        }


        void OnClientDisconnect(NetworkConnection conn)
        {

            Warning("Lost client connection.");

            //    revertAllToLocal();

        }

        // Handle basic string messages.

        void OnMessageFromClient(NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<StringMessage>();

            Verbose("Message received from client " + netMsg.conn.address + " : " + message.value);

            /*
            if (message.value == "REGISTER")
            {
                //  Warning("tracking addresses method removed, replace");
                RemoveTrackedAddress(netMsg.conn.address);
            }
            */


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


        public void startNetworkClient(string server)
        {
            Verbose("Starting client for remote server " + server);

            if (server == "")
            {
                Error("No server address.");
                return;
            }

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




        #endregion



        #region CLIENTMANAGEMENT


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





        #endregion






        #region ENGINE
        void newTasksHandlerUnity(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);
            foreach (StoryTask task in theTasks)
            {
                task.signOn(ID);
            }
        }
        #endregion  

        #region UPDATE
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



            // Set debug visibility
            displayNetworkGUI(GENERAL.Debugging);



            int t = 0;

            while (t < taskList.Count)
            {
                StoryTask task = taskList[t];

                if (!GENERAL.ALLTASKS.Exists(at => at == task))
                {
                    Log("Removing task:" + task.Instruction);
                    taskList.RemoveAt(t);
                }
                else
                {
                    if (HandleTask(task))
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
        #endregion

        bool HandleTask(StoryTask task)
        {




            //#if UNITY_EDITOR && UNITY_IOS

            //            // Pause applicatin simulation for debugging.

            //            if (Input.GetKeyDown("p"))
            //            {
            //                OnApplicationPause(true);
            //            }

            //            if (Input.GetKeyDown("o"))
            //            {
            //                OnApplicationPause(false);
            //            }

            //            if (state == STATE.PAUSED)
            //                return;

            //#endif



            //if (displayNetworkGUIState())
            //    SetNetworkIndicators();



            // Task needs to be executed.
            // first check for storyengine tasks.
            bool done = false;
            switch (task.Instruction)
            {



                // ---------------------------- SCOPE ----------------------------

                case "isglobal":

                    if (GENERAL.AUTHORITY == AUTHORITY.GLOBAL)
                    {
                        task.Pointer.scope = SCOPE.GLOBAL;
                        Log("Setting pointer scope to global: " + task.Pointer.currentPoint.StoryLine);
                    }
                    else
                    {
                        Log("No global authority, not changing pointer scope. ");
                    }

                    done = true;
                    break;

                case "islocal":

                    task.Pointer.SetLocal();

                    Log("Setting pointer scope to local: " + task.Pointer.currentPoint.StoryLine);

                    done = true;
                    break;
                case "setglobalauth":
                case "amhub":

                    GENERAL.AUTHORITY = AUTHORITY.GLOBAL;
                    Log("Setting authority to global");
                    //     AlertHandler.Log("Launching environment " + SessionSettings.Environment);
                    //      Log("Launching " + Management.UserSettingsHandler.GetActiveModuleName() + ", environment " + StorageSettings.Environment);

                    done = true;
                    break;

                case "setlocalauth":
                case "amremote":

                    GENERAL.AUTHORITY = AUTHORITY.LOCAL;
                    Log("Setting authority to local");
                    //    AlertHandler.Log("Launching environment " + SessionSettings.Environment);
                    //  Log("Launching " + Management.UserSettingsHandler.GetActiveModuleName() + ", environment " + StorageSettings.Environment);

                    done = true;
                    break;


                // ---------------------------- SERVER SIDE ----------------------------

                case "startserver":

                    // Start broadcast and network server if not already going.

                    if (!Broadcasting)
                    {


                        if (BroadcastKey == 0 || BroadcastMessageString == null || BroadcastMessageString.Length == 0)
                        {
                            // no info on how to configure broadcasterver

                            Warning("No broadcast info, not starting broadcastserver.");
                        }
                        else
                        {
                            startBroadcastServer(BroadcastKey, BroadcastMessageString);
                            Log("Starting broadcast server, key: " + BroadcastKey + " message: " + BroadcastMessageString);
                        }

                        //ModuleInfo mi = SessionData.GetModuleInfo();

                        //if (mi != null)
                        //{
                        //    startBroadcastServer(mi.BroadcastKey, mi.ID);
                        //    Log("Starting broadcast server, key: " + mi.BroadcastKey + " message: " + mi.ID);
                        //}
                        //else
                        //{
                        //    Warning("No module info, starting generic broadcast server.");
                        //    Log("key: " + ConnectionKey + " message: " + ConnectionMessage);
                        //    startBroadcastServer(ConnectionKey, ConnectionMessage);
                        //}


                    }

                    if (!Serving)
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
                    if (!Broadcasting)
                    {
                        Log("Starting broadcast server, key " + BroadcastKey + " message " + BroadcastMessageString);
                        //startBroadcastServer(ConnectionKey, ConnectionMessage);
                        startBroadcastServer(BroadcastKey, BroadcastMessageString);
                    }

                    if (!Serving)
                    {
                        Log("Starting network server.");
                        startNetworkServer();
                    }

#endif



                    // Watch for new clients. Stays live indefinitely.

                    if (Serving && NewTrackedAddresses())
                    {
                        // new addresses connected since last call

                        foreach (string add in NewConnectedAddresses())
                        {
                            Log("New client at " + add);
                            // note: pass addresses on so we can send targeted messages...
                        }

                        task.setCallBack("addclient");

                    }

                    if (networkManager != null)
                    {
                        NetworkConnection[] connections = networkManager.GetConnectedClients();
                        task.SetStringValue("debug", "clients: " + connections.Length);
                        WasConnected = connections.Length > 0;
                    }
                    else
                    {
                        Warning("Network manager not available, cannot monitor connections");
                    }


                    //task.SetStringValue("debug", "clients: " + TrackedConnectedAddresses().Count);
                    //WasConnected = TrackedConnectedAddresses().Count > 0;

                    break;


                //case "pushglobaldata":




                //    done = true;
                //    break;


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

                    if (Listening)
                    {
                        if (foundServer())
                        {

                            stopBroadcast();

                            string message = networkBroadcast.serverMessage;
                            char[] param = { '\0' };
                            string[] split = message.Split(param);

                            task.SetStringValue("persistantData", split[0]);

                            task.setCallBack("serverfound");

                            Listening = false;

                            Log("Found broadcast server: " + networkBroadcast.serverAddress + " key: " + networkBroadcast.broadcastKey + " message: " + split[0]);


                            done = true;
                        }
                    }
                    else
                    {
                        /*
                        ModuleInfo mi = SessionData.GetModuleInfo();
                        if (mi != null)
                        {
                            startBroadcastClient(mi.BroadcastKey);
                            Log("(Re)starting broadcast listening for key " + mi.BroadcastKey);
                        }
                        else
                        {
                            startBroadcastClient(BroadcastKey);
                            Log("(Re)starting broadcast listening for key " + BroadcastKey);
                        }
                        */

                        startBroadcastClient(BroadcastKey);
                        Log("(Re)starting broadcast listening for key " + BroadcastKey);

                        Listening = true;

                    }

                    break;



                case "startclient":

                    string ServerAddress = networkBroadcast.serverAddress;

                    if (ServerAddress != "")
                    {
                        //        Log("Starting network client for remote server " + ServerAddress);
                        startNetworkClient(ServerAddress);
                    }
                    else
                    {
                        Warning("Trying to start client without server address.");
                    }

                    done = true;
                    break;

                case "stopclient":

                    //          Verbose("Stopping network client.");

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




                default:

                    done = true;

                    break;

            }
            return done;


        }

        //void newTasksHandler(object sender, TaskArgs e)
        //{
        //    addTasks(e.theTasks);
        //}

        public void addTasks(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);
        }
        public bool IsServer()
        {
            return NetworkServer.active;
        }
        public bool IsClient()
        {
            return networkManager != null && networkManager.client != null;
        }


        public int GetQueueSize()
        {
            int q = -1;

            if (NetworkServer.active)
            {

                // We're an active server.

                byte error;
                q = NetworkTransport.GetOutgoingMessageQueueSize(NetworkServer.serverHostId, out error);
                //  debugValue.text = "queued out server: " + QueueSize;

                if ((NetworkError)error != NetworkError.Ok)
                    Error("Networktransport error: " + (NetworkError)error);


            }

            if (networkManager != null && networkManager.client != null)
            {

                // We're an active client.

                byte error = (byte)NetworkError.Ok;

                if (networkManager.client.connection != null)
                {
                    q = NetworkTransport.GetOutgoingMessageQueueSize(networkManager.client.connection.hostId, out error);
                }
                else
                {
                    Warning("Can't get queue size (yet)");
                }

                //    debugValue.text = "queued out client: " + QueueSize;

                if ((NetworkError)error != NetworkError.Ok)
                    Error("Networktransport error: " + (NetworkError)error);

            }

            return q;
        }

        public string networkAddress()
        {

            if (networkManager != null)
            {
                return networkManager.networkAddress;
            }
            else
            {
                return "";
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

        public void SendStoryUpdateToClients(StoryUpdate message)
        {

            //  message.MaxMessageSize;


            // NetworkServer.SendUnreliableToAll(storyCode, message);
            NetworkServer.SendToAll(storyCode, message);
            Verbose("Sending: " + message.DebugLog);

        }




        public void SendStoryUpdateToClient(StoryUpdate message, string address)
        {
            int conn = -1;
            for (int i = 0; i < NetworkServer.connections.Count; i++)
            {
                if (NetworkServer.connections[i].address == address) { conn = i; break; }
            }


            if (conn == -1)
            {
                Warning("client not found " + address);
            };

            // NetworkServer.SendUnreliableToAll(storyCode, message);
            NetworkServer.SendToClient(conn, storyCode, message);
            //  Verbose("Sending: "+message.DebugLog);

        }

        public void SendStoryUpdateToServer(StoryUpdate message)
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


        // Network connectivity handling.








        void NetworkObjectShutdown()
        {
            // When pausing a server on ios using the sleep/side button, somehow network resources aren't properly disposed.
            // This why we destroy the object and stop start networkstransport.

            if (NetworkObject == null)
                return;

            // Client side shutdown.

            if (Listening)
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

            if (Serving || Broadcasting)
            {
                if (Serving)
                    stopNetworkServer(); // close ports

                if (Broadcasting)
                    stopBroadcast();// close ports

                networkBroadcast = null;
                networkManager = null;
                Destroy(NetworkObject);

                Log("Shutting down networktransport.");
            //    NetworkTransport.Shutdown();

            }

        }


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

        //void SetNetworkIndicators()
        //{
        //    if (BufferStatusIn == null || BufferStatusOut == null)
        //        return;

        //    BufferStatusIn.SetActive(WasConnected);
        //    BufferStatusOut.SetActive(WasConnected);

        //    switch (AssitantDirector.BufferStatusIn)
        //    {
        //        case 0:
        //            BufferStatusIn.GetComponent<Image>().color = Color.grey;
        //            break;
        //        case 1:
        //        case 2:
        //            BufferStatusIn.GetComponent<Image>().color = Color.green;
        //            break;
        //        default:
        //            BufferStatusIn.GetComponent<Image>().color = Color.cyan;
        //            break;
        //    }
        //    switch (AssitantDirector.BufferStatusOut)
        //    {
        //        case 0:
        //            BufferStatusOut.GetComponent<Image>().color = Color.grey;
        //            break;
        //        case 1:
        //        case 2:
        //            BufferStatusOut.GetComponent<Image>().color = Color.green;
        //            break;
        //        default:
        //            BufferStatusOut.GetComponent<Image>().color = Color.cyan;
        //            break;
        //    }
        //}


    }

}