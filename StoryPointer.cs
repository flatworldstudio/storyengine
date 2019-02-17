using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if !SOLO

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
//using Amazon.S3.Model;
using StoryEngine.Network;
#endif

namespace StoryEngine
{




    public enum POINTERSTATUS
    {
        EVALUATE,
        NEWTASK,
        TASKUPDATED,
        KILLED,
        PAUSED
    }
    /*!
* \brief
* Holds a pointer which progresses along a Script 
* 
* The Director progresses the pointer, AssistantDirector generates StoryTask objects from the pointer.
*/

    public class StoryPointer
    {

        public StoryPoint currentPoint;
        public StoryTask currentTask;
        public SCOPE scope;

        POINTERSTATUS status;
        public string persistantData;

        public Text deusText;
        public Text deusTextSuper;
        public GameObject pointerObject, pointerTextObject;
        public int position;

        //public bool modified;

//#if !SOLO
//        public bool modified;
//#endif

        string ID = "Storypointer";

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

        public StoryPointer()
        {

            // Empty pointer, set to pause just to be safe. To be populated from task.

            status = POINTERSTATUS.PAUSED;
           GENERAL.ALLPOINTERS.Add(this);

        //    updateMessageSend = new PointerUpdateBundled(); // we'll reuse.


        }

        public StoryPointer(string pointID, SCOPE setScope)
        {

            // Create a pointer from a given point. Task to be added later.

            currentPoint = GENERAL.GetStoryPointByID(pointID);
         
            currentTask = null;
            status = POINTERSTATUS.EVALUATE;
            scope = setScope;

         //   GENERAL.AddPointer(this);

            GENERAL.ALLPOINTERS.Add(this);

          //  updateMessageSend = new PointerUpdateBundled(); // we'll reuse.


        }

        public void PopulateWithTask(StoryTask task)
        {

            // Filling an empty pointer with task info. Used for network task and pointer creation.

            currentPoint = GENERAL.GetStoryPointByID(task.PointID);
            currentTask = task;
            task.Pointer = this;
            scope = task.scope;

        }

        public void LoadPersistantData()
        {

            // load carry over value from task into pointer.

            if (currentTask != null)
                currentTask.GetStringValue("persistantData", out persistantData);

        }

        //public PointerUpdate GetUpdateMessage()
        //{

//    // Generate a network update message for this pointer. (In effect: if it was killed.)

//    PointerUpdate message = new PointerUpdate();

//    message.storyPointID = currentPoint.ID;

//    if (status == POINTERSTATUS.KILLED)
//    {
//        message.killed = true;
//    }

//    return message;

//}
#if !SOLO

        public PointerUpdateBundled GetUpdate()

        {
            // bundled approach.
            // Generate a network update message for this pointer. Only case is KILL.


            PointerUpdateBundled updateMessageSend=new PointerUpdateBundled();
            updateMessageSend.StoryLineName=currentPoint.StoryLine;


        //updateMessageSend =;

        //updateMessageSend.storyPointID= currentPoint.ID;

            /*
            updateMessageSend.storyPointID = currentPoint.ID;

            if (status == POINTERSTATUS.KILLED)
            {
                updateMessageSend.killed = true;

            } else{
                
                updateMessageSend.killed = false;

            }
*/
            return updateMessageSend;

        }

#endif

        public void SetLocal (){
            scope = SCOPE.LOCAL;
            status=POINTERSTATUS.EVALUATE;
        }

        public void Kill()
        {

            SetStatus(POINTERSTATUS.KILLED);

        }

        public POINTERSTATUS GetStatus()
        {
            return status;
        }

        public void SetStatus(POINTERSTATUS theStatus)
        {

            if (status != POINTERSTATUS.KILLED)
            {

                status = theStatus;
            }

//#if !SOLO

            //modified = true;

//#endif

        }

        public Boolean ProgressToNextPoint()
        {

            Boolean r = false;

            if (currentPoint.getNextStoryPoint() == null)
            {

                Log("No next point");

            }
            else
            {

                currentPoint = currentPoint.getNextStoryPoint();

                r = true;
            }

            return r;

        }

    }



}