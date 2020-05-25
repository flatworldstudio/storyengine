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
               
        string ID = "Storypointer";

        // Copy these into every class for easy debugging.
        protected void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        protected void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        protected void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        protected void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

        #region CONSTRUCTOR
        public StoryPointer()
        {

            // Empty pointer, set to pause just to be safe. To be populated from task.

            status = POINTERSTATUS.PAUSED;
           GENERAL.ALLPOINTERS.Add(this);
                       
        }
        #endregion

        #region FLOW

        public void SetStoryPointByID(string pointID)
        {
            currentPoint = GENERAL.GetStoryPointByID(pointID);
            status = POINTERSTATUS.EVALUATE;

        }

        public void SetScope(SCOPE setScope)
        {
            scope = setScope;
        }


        public StoryTask SpawnTask()
        {
            currentTask = new StoryTask(this);
            //status = POINTERSTATUS.EVALUATE;
            return currentTask;
        }


        public void SetLocal()
        {
            scope = SCOPE.LOCAL;
            status = POINTERSTATUS.EVALUATE;
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


        #endregion

        #region GETSET

        public void LoadPersistantData()
        {

            // load carry over value from task into pointer.

            if (currentTask != null)
                currentTask.GetStringValue("persistantData", out persistantData);

        }


        #endregion


        #region UPDATE
        public StoryPointerUpdate GetUpdate()

        {
            // bundled approach.
            // Generate a network update message for this pointer. Only case is KILL, so only a name is needed.


            StoryPointerUpdate updateMessageSend = new StoryPointerUpdate();
            updateMessageSend.StoryLineName = currentPoint.StoryLine;

            return updateMessageSend;

        }


        #endregion





    }



}