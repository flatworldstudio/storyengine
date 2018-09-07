using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if NETWORKED

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

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

#if NETWORKED
        public bool modified;
    //    PointerUpdateBundled updateMessageSend;
#endif

        string ID = "Storypointer";

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

            currentPoint = GENERAL.GetStoryPointByID(task.pointID);
            currentTask = task;
            task.pointer = this;
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
       

        public PointerUpdateBundled GetUpdate()

        {
            // bundled approach.
            // Generate a network update message for this pointer. Only case is KILL.


            PointerUpdateBundled updateMessageSend=new PointerUpdateBundled();
            updateMessageSend.StoryLineName=currentPoint.storyLineName;


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

#if NETWORKED

            modified = true;

#endif

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