using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace StoryEngine
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



    public class ExtendedNetworkManager : NetworkManager
    {
        string ID = "Network manager";

        const short connectionMessageCode = 1001;


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


        void OnApplicationQuit()
        {
            if (client != null)
                StopNetworkClient();

            if (NetworkServer.active)
                StopNetworkServer();

        }

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
            Log("Client has started.");

            if (onStartClientDelegate != null)
                onStartClientDelegate(theClient);

        }

        public override void OnClientConnect(NetworkConnection conn)
        {

            Log("Client connected to server.");

            if (onClientConnectDelegate != null)
                onClientConnectDelegate(conn);

        }

        public override void OnClientDisconnect(NetworkConnection connection)
        {

            Log("Client disconnected from server.");

            if (onClientDisconnectDelegate != null)
                onClientDisconnectDelegate(connection);

        }

        public void StopNetworkClient()
        {

            StopClient();

        }

        public override void OnStopClient()
        {

            Log("Client has stopped.");

            if (onStopClientDelegate != null)
                onStopClientDelegate();

        }

        // Server methods.

        public OnStartServerDelegate onStartServerDelegate;
        public OnStopServerDelegate onStopServerDelegate;

        public OnServerConnectDelegate onServerConnectDelegate;
        public OnServerDisconnectDelegate onServerDisconnectDelegate;


        public void StartNetworkServer()
        {

            StartServer();

        }

        public void StopNetworkServer()
        {

            StopServer();

        }

        public override void OnStartServer()
        {
            Log("Server started.");

            if (onStartServerDelegate != null)
                onStartServerDelegate();

        }

        public override void OnStopServer()
        {
            Log("Server stopped.");

            if (onStopServerDelegate != null)
                onStopServerDelegate();

        }

        public override void OnServerConnect(NetworkConnection connection)
        {
            Log("Remote client connected.");

            if (onServerConnectDelegate != null)
                onServerConnectDelegate(connection);

        }

        public override void OnServerDisconnect(NetworkConnection connection)
        {
            Log("Remote client disconnected.");

            if (onServerDisconnectDelegate != null)
                onServerDisconnectDelegate(connection);

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