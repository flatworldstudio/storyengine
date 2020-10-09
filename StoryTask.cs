using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using StoryEngine.Network;



namespace StoryEngine
{

    public enum TASKTYPE
    {


        BASIC,
        ROUTING


    }

    public enum TASKSTATUS
    {
        ACTIVE,
        COMPLETE,
    }

    /*!
* \brief
* Holds a task and variables associated with that task.
* 
*/

    public class StoryTask : StoryData
    {

        StoryPoint __point;
        public StoryPointer Pointer;
        List<string> signedOn;

        TASKSTATUS status;
        
        //int LastUpdateFrame = -1;
        //int UpdatesPerFrame = 0;
        //public int LastUpdatesPerFrame = 0;
        //public int MaxUpdatesPerFrame = 0;
        // ------------------------------------------------

        #region CONSTRUCTOR

        // Tasks are create by calling spawntask on a pointer. 

        public StoryTask(StoryPointer _fromPointer) : base("StoryTask", _fromPointer.scope)
        {

            // Create a task based on the current storypoint in the pointer.

            Pointer = _fromPointer;
            __point = _fromPointer.currentPoint;
            scope = _fromPointer.scope;

            setDefaults();
            GENERAL.AddTask(this);

        }


        void setDefaults()
        {

            signedOn = new List<string>();

            //updateSend = new StoryTaskUpdate();
            //updateReceive = new StoryTaskUpdate();

            setStatus(TASKSTATUS.ACTIVE);

        }

        #endregion

        #region FLOW

        void SetPointerToUpdated()
        {

            //Pointer.SetStatus(POINTERSTATUS.TASKUPDATED);


            switch (GENERAL.AUTHORITY)
            {

                case AUTHORITY.GLOBAL:
                    //		case SCOPE.SOLO:

                    // we're the global server or running solo so we can trigger the pointer. regardless of the task's scope.

                    Pointer.SetStatus(POINTERSTATUS.TASKUPDATED);

                    break;

                case AUTHORITY.LOCAL:

                    // we're a local client. only if the task is local do we trigger the pointer.

                    if (scope == SCOPE.LOCAL)
                    {

                        Pointer.SetStatus(POINTERSTATUS.TASKUPDATED);

                    }

                    break;

                default:


                    break;



            }



        }

        public void setStatus(TASKSTATUS theStatus)
        {

            status = theStatus;
            SetPointerToUpdated();

        }

        public TASKSTATUS getStatus()
        {

            return status;

        }


        public void ForceComplete()
        {
            Warning("Force complete, signoffs still required was " + signedOn.Count);
            signedOn.Clear();
            complete();

        }


        void complete()
        {

            if (getStatus() != TASKSTATUS.COMPLETE)
            {

                // make sure a task is only completed once.

                setStatus(TASKSTATUS.COMPLETE);

            }
            else
            {

                Warning("A task was completed more than once.");

            }

        }


        public void setCallBack(string theCallBackPoint)
        {

            SetStringValue("callBackPoint", theCallBackPoint);

        }

        public void clearCallBack()
        {

            SetStringValue("callBackPoint", "");

        }

        public string getCallBack()
        {

            string value;

            if (GetStringValue("callBackPoint", out value))
            {

                return value;

            }
            else
            {

                return ("");
            }

        }

        public void signOn(string fromMe)
        {
            if (signedOn.Exists(x => x == fromMe))
            {
                Warning(Instruction + " trying to sign on more than once: " + fromMe);

            }
            else
            {
                signedOn.Add(fromMe);
                Verbose(Instruction + " signing on " + fromMe);
            }

        }

        public void signOff(string fromMe)
        {

            if (signedOn.Exists(x => x == fromMe))
            {
                signedOn.Remove(fromMe);
                Verbose(Instruction + " signing off " + fromMe + " signees left " + signedOn.Count);

            }
            else
            {
                Warning(Instruction + " trying to sign off but never signed on: " + fromMe);
            }


            if (signedOn.Count == 0)
            {
                Verbose("No more signed on " + Instruction);
                complete();
            }




        }

        #endregion

        #region UPDATING

        public void ApplyUpdate(StoryTaskUpdate update, bool changeMask = false)
        {
            // apply data changes. changemask isn't really in use right now, to be used for having multiple clients...
            ApplyDataUpdate(update, "");
        }

        public StoryTaskUpdate GetUpdate()
        {
            // just returning data update now.
            // in fact, our point id is still there and should go here

            // get data update and expand into taskupdate
            StoryTaskUpdate update = new StoryTaskUpdate(GetDataUpdate());

            // add task update specifics that the dataobject doesn't know about.
            update.pointID = PointID;

            return update;

        }

        #endregion



        #region GETSET

        public void LoadPersistantData(StoryPointer referencePointer)
        {

            SetStringValue("persistantData", referencePointer.persistantData);

        }

        public StoryPoint Point
        {
            get
            {
                return __point;
            }
            set
            {
                Warning("Can't set Point value directly.");
            }
        }

        public string Instruction
        {
            get
            {
                if (__point == null || __point.Instructions == null || __point.Instructions.Length == 0)
                    return "";
                else
                    return __point.Instructions[0];
            }
            set
            {
                Warning("Can't set Instruction value directly.");
            }
        }

        public string PointID
        {
            get
            {
                if (__point == null)
                    return "";
                else
                    return __point.ID;
            }
            set
            {
                Warning("Can't set PointID value directly.");
            }
        }

        #endregion








    }






}