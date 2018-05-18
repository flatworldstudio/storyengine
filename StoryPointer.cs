﻿using System;
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
        PointerUpdateMessage updateMessage;
#endif

        string me = "Storypointer";

        public StoryPointer()
        {

            // Empty pointer, set to pause just to be safe. To be populated from task.

            status = POINTERSTATUS.PAUSED;
           GENERAL.ALLPOINTERS.Add(this);

           updateMessage = new PointerUpdateMessage(); // we'll reuse.


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

            updateMessage = new PointerUpdateMessage(); // we'll reuse.


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
                currentTask.getStringValue("persistantData", out persistantData);

        }

        public PointerUpdate GetUpdateMessage()
        {

            // Generate a network update message for this pointer. (In effect: if it was killed.)

            PointerUpdate message = new PointerUpdate();

            message.storyPointID = currentPoint.ID;

            if (status == POINTERSTATUS.KILLED)
            {
                message.killed = true;
            }

            return message;

        }

        public PointerUpdateMessage GetUpdate()
        {
            // bundled approach.
            // Generate a network update message for this pointer. (In effect: if it was killed.)

            updateMessage.storyPointID = currentPoint.ID;

            if (status == POINTERSTATUS.KILLED)
            {
                updateMessage.killed = true;

            } else{
                
                updateMessage.killed = false;

            }

            return updateMessage;

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

                Log.Message("No next point");

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