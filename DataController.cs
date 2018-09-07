using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if NETWORKED
using UnityEngine.Networking;

#endif


namespace StoryEngine
{

    public delegate bool DataTaskHandler(StoryTask theTask);

    public class DataController : MonoBehaviour
    {

        GameObject StoryEngineObject;
        DataTaskHandler dataTaskHandler;

#if NETWORKED
        GameObject NetworkObject;
        NetworkBroadcast networkBroadcast;
        ExtendedNetworkManager networkManager;
#endif

        AssitantDirector ad;

        bool handlerWarning = false;

        public List<StoryTask> taskList;

        string ID = "DataController";

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
            Log("Starting.");

            taskList = new List<StoryTask>();

#if NETWORKED

            NetworkObject = GameObject.Find("NetworkObject");

            if (NetworkObject == null)
            {

                Warning("NetworkObject not found.");

            }
            else
            {

                networkBroadcast = NetworkObject.GetComponent<NetworkBroadcast>();
                networkManager = NetworkObject.GetComponent<ExtendedNetworkManager>();

            }

#endif

            StoryEngineObject = GameObject.Find("StoryEngineObject");

            if (StoryEngineObject == null)
            {

                Warning("StoryEngineObject not found.");

            }
            else
            {

                ad = StoryEngineObject.GetComponent<AssitantDirector>();
                ad.newTasksEvent += new NewTasksEvent(newTasksHandler); // registrer for task events

            }

        }


#if UNITY_IOS && NETWORKED
	

	void OnApplicationPause(bool paused){


		if (paused) {

			Log.Message ("pauzing ...");
			Log.Message ("Disconnecting client ...");


			if (networkManager.client != null) {

	StopNetworkClient();

//				networkManager.client.Disconnect ();

//	networkManager.client.Shutdown ();

//	Network.CloseConnection(Network.connections[0], true);

//	NetworkManager.InformServerOnDisconnect();




	//or maybe better use  Shutdown(); 



			}


		} else {

			Log.Message ("resuming ...");

		}
	}

#endif

#if NETWORKED

        // These are networking methods to be called from datahandler to establish connections.
        // Once connected, handling is done internally by the assistant directors.

        public int serverConnections()
        {

            // Get a count for the number of (active) connections.

            NetworkConnection[] connections = new NetworkConnection[NetworkServer.connections.Count];

            NetworkServer.connections.CopyTo(connections, 0);

            int c = 0;

            foreach (NetworkConnection nc in connections)
            {

                //			if (nc.isConnected) {

                if (nc != null)
                {

                    c++;

                }

            }
            return c;

        }

        public void startBroadcastClient()
        {

            Log("Starting broadcast client.");

            networkBroadcast.StartClient();

        }

        public void startBroadcastServer()
        {

            Log("Starting broadcast server.");

            networkBroadcast.StartServer();

        }

        public void stopBroadcast()
        {

            Log("Stopping broadcast server.");

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

            networkManager.StopNetworkServer();

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

        public void addTaskHandler(DataTaskHandler theHandler)
        {
            dataTaskHandler = theHandler;
            Log("Handler added.");
        }

        void Update()
        {

            int t = 0;

            while (t < taskList.Count)
            {

                StoryTask task = taskList[t];

                //			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {

                if (!GENERAL.ALLTASKS.Exists(at => at == task))
                {

                    Log("Removing task:" + task.description);

                    // Task was removed, so stop executing it.

                    taskList.RemoveAt(t);

                }
                else
                {

                    if (dataTaskHandler != null)
                    {

                        if (dataTaskHandler(task))
                        {

                            task.signOff(ID);
                            taskList.RemoveAt(t);

                        }
                        else
                        {

                            t++;

                        }

                    }
                    else
                    {

                        if (!handlerWarning)
                        {

                            Warning("No handler available, blocking task while waiting.");

                            handlerWarning = true;
                          

                        }
                        t++;
                    }

                }

            }

        }

        //	public void taskDone (StoryTask theTask){
        //		theTask.signOff (me);
        //		taskList.Remove (theTask);
        //
        //	}

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


            return networkBroadcast.showGUI;


        }


        public void displayNetworkGUI(bool status)
        {


            NetworkObject.GetComponent<NetworkManagerHUD>().showGUI = status;
            networkBroadcast.showGUI = status;


        }

#endif
    }




}