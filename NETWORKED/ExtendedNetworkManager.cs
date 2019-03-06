using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace StoryEngine.Network
{

    public delegate void OnClientMessageHandler(NetworkMessage netMessage);
    public delegate void messageHandler(NetworkMessage netMessage);

    // Client delegates.

    public delegate void OnStartClientDelegate(NetworkClient theClient);
    public delegate void OnStopClientDelegate();
    public delegate void OnClientConnectDelegate(NetworkConnection connection);
    public delegate void OnClientDisconnectDelegate(NetworkConnection connection);

    // Server delegates

    public delegate void OnStartServerDelegate();
    public delegate void OnStopServerDelegate();
    public delegate void OnServerConnectDelegate(NetworkConnection connection);
    public delegate void OnServerDisconnectDelegate(NetworkConnection connection);


    /*!
* \brief
* Extention of Unity Networkmanager to handle client-server connections.
* 
*/

    public class ExtendedNetworkManager : NetworkManager
    {
        string ID = "NetworkManager";
        //public GameObject NetworkInfoPrefab;

     //   const short connectionMessageCode = 1001;
        List<string> __connectedAddresses;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.


        void Log(string message)
        {
            StoryEngine.Log.Message(message, ID);
        }
        void Warning(string message)
        {
            StoryEngine.Log.Warning(message, ID);
        }
        void Error(string message)
        {
            StoryEngine.Log.Error(message, ID);
        }
        void Verbose(string message)
        {
            StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);
        }


        //void OnApplicationQuit()
        //{
        //    if (client != null)
        //        StopNetworkClient();

        //    if (NetworkServer.active)
        //        StopNetworkServer();

        //}

        // Client methods.

        public OnStartClientDelegate onStartClientDelegate;
        public OnStopClientDelegate onStopClientDelegate;

        public OnClientConnectDelegate onClientConnectDelegate;
        public OnClientDisconnectDelegate onClientDisconnectDelegate;

        public void StartNetworkClient(string server)
        {

            networkAddress = server;
            StartClient();

        }

        public override void OnStartClient(NetworkClient theClient)
        {
            Verbose("Started as Client.");

            if (onStartClientDelegate != null)
                onStartClientDelegate(theClient);

        }

        public void StopNetworkClient()
        {

            StopClient();

        }

        public override void OnStopClient()
        {

            Verbose("Stopped as Client.");

            if (onStopClientDelegate != null)
                onStopClientDelegate();

        }

        public override void OnClientConnect(NetworkConnection conn)
        {

            Verbose("Connected to remote Server as Client.");

            if (onClientConnectDelegate != null)
                onClientConnectDelegate(conn);

        }

        public override void OnClientDisconnect(NetworkConnection connection)
        {

            Verbose("Disconnected from remote Server as Client");

            if (onClientDisconnectDelegate != null)
                onClientDisconnectDelegate(connection);

        }



        // Server methods.

        public OnStartServerDelegate onStartServerDelegate;
        public OnStopServerDelegate onStopServerDelegate;

        public OnServerConnectDelegate onServerConnectDelegate;
        public OnServerDisconnectDelegate onServerDisconnectDelegate;


        public void StartNetworkServer()
        {
            Log("Starting as Server");
            StartServer();
            __connectedAddresses = new List<string>();

        }

        public void StopNetworkServer()
        {
            Verbose("Stopping as Server.");
            StopServer();
            __connectedAddresses.Clear();
        }

        public override void OnStartServer()
        {
            Verbose("Started as Server.");

            if (onStartServerDelegate != null)
                onStartServerDelegate();


        }

        public override void OnStopServer()
        {
            Verbose("Stopped as Server");

            if (onStopServerDelegate != null)
                onStopServerDelegate();

        }

        public override void OnServerConnect(NetworkConnection connection)
        {
            Verbose("Remote client connected.");
      //      Debug.Log("client connecct");

            GetConnectedAddresses();

            if (onServerConnectDelegate != null)
                onServerConnectDelegate(connection);

        }

        public override void OnServerDisconnect(NetworkConnection connection)
        {
            Verbose("Remote client disconnected.");

            GetConnectedAddresses();

            if (onServerDisconnectDelegate != null)
                onServerDisconnectDelegate(connection);

        }

        public List<string> ConnectedAddresses
        {
            get
            {
                GetConnectedAddresses();
                return __connectedAddresses;
            }
            set
            {
                Warning("Can't set connected addresses directly.");
            }


        }

        void GetConnectedAddresses()
        {

            NetworkConnection[] connections = new NetworkConnection[NetworkServer.connections.Count];
            NetworkServer.connections.CopyTo(connections, 0);

            __connectedAddresses.Clear();
        //    List<string> addresses = new List<string>();

            for (int c = 0; c < connections.Length; c++)
            {
                if (connections[c] != null && connections[c].isConnected)
                    __connectedAddresses.Add(connections[c].address);

            }

        //    __connectedAddresses    = addresses;

        }

        /*
    //	public void 

        public void InformServerOnDisconnect (){

            connectionMessageToServer ("disconnecting");

        }


        public void connectionMessageToServer (string value)
        {
            var msg = new StringMessage (value);
            client.Send (connectionMessageCode, msg);
            Log.Message ("Sending connection message to server: " + value);
        }

        public void connectionMessageToClients (string value)
        {
            var msg = new StringMessage (value);
            NetworkServer.SendToAll (connectionMessageCode, msg);
            Log.Message ("Sending connection message to all clients: " + value);
        }


        void onClientConnectionMessage (NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<StringMessage> ();
            Log.Message ("Connection message from server: " + message.value);
        }


        void onServerConnectionMessage (NetworkMessage netMsg)
        {
            var message = netMsg.ReadMessage<StringMessage> ();
            Log.Message ("Connection message from client: " + message.value);

            switch (message.value) {

            case "hello":

                connectionMessageToClients ("A new client was added.");

                break;

            case "disconnecting":

                Log.Message ("Client is disconnecting, dropping their connection." + message.value);

    //			Network.CloseConnection(netMsg.conn, true);

                break;

            default:

                break;

            }

        }
        */



    }
}