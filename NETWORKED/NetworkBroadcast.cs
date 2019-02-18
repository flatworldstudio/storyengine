using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace StoryEngine.Network
{

    /*!
* \brief
* Extention of Unity NetworkDiscovery to handle broadcast messages for server discovery.
* 
*/

    public class NetworkBroadcast : NetworkDiscovery
    {

        public string serverAddress, serverMessage;

        string ID = "Networkbroadcast";

        bool resumeClient = false;
        bool resumeServer = false;

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

         void ResetMessage()
        {

            serverAddress = "";
            serverMessage = "";

        }

        public void StartClient()
        {

            Initialize();
            ResetMessage(); // just to be sure.
            StartAsClient();

        }

        //void OnApplicationPause(bool paused)
        //{

        //    if (paused)
        //    {

        //        if (isClient)
        //        {

        //            resumeClient = true;
        //            Stop();

        //            Log("Pausing broadcast client.");

        //        }

        //        if (isServer)
        //        {

        //            resumeServer = true;
        //            Stop();

        //            Log("Pausing broadcast server.");

        //        }

        //    }
        //    else
        //    {

        //        if (resumeClient)
        //        {

        //            resumeClient = false;
        //            StartClient();

        //            Log("Resuming broadcast client.");

        //        }

        //        if (resumeServer)
        //        {

        //            resumeServer = false;
        //            StartServer();

        //            Log("Resuming broadcast server.");

        //        }

        //    }

        //}

        public void StartServer()
        {

            Initialize();
            ResetMessage(); // just to be sure.
            StartAsServer();

        }

        public void Stop()
        {

            StopBroadcast();
            //ResetMessage();

        }

        public override void OnReceivedBroadcast(string fromAddress, string data)
        {

            // Handler to respond to received broadcast message event.
            // Since our engine is loop based, we just store the info for the loop to pick up on.

            Log("Received broadcast: " + data + " from " + fromAddress);

            serverMessage = data;
            serverAddress = fromAddress;

       //     DataController.Instance.RemoteBroadcastServerAddress = fromAddress;

          //  GENERAL.broadcastServer = fromAddress;

        }

    }
}