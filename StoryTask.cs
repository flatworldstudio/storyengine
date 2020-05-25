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

        // is END still a thign?
        BASIC,
        ROUTING,
        END

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

        string ID = "Storytask";


        StoryPoint __point;
        public StoryPointer Pointer;
        List<string> signedOn;

        TASKSTATUS status;

        StoryTaskUpdate updateSend, updateReceive;

        int LastUpdateFrame = -1;
        int UpdatesPerFrame = 0;
        public int LastUpdatesPerFrame = 0;
        public int MaxUpdatesPerFrame = 0;


        // Copy these into every class for easy debugging.
        void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

        // ----------------

        public void ApplyUpdateMessage(StoryTaskUpdate update, bool changeMask = false)
        {

            // apply data changes. changemask isn't really in use right now.

            ApplyDataUpdate(update, "");

        }

        public StoryTaskUpdate GetUpdateBundled()
        {
            // just returning data update now.
            // in fact, our point id is still there and should go here

            return new StoryTaskUpdate(GetDataUpdate());
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


        public StoryTask(string _fromPointID, SCOPE _scope) : base("StoryTask", _scope)
        {

            // Creating a task from a storypoint -> pointer to be created from this task.

            __point = GENERAL.GetStoryPointByID(_fromPointID);
            //    scope = _scope;
            Pointer = null;

            setDefaults();
            GENERAL.AddTask(this);


        }

        public StoryTask(StoryPointer _fromPointer, SCOPE _scope) : base("StoryTask", _scope)
        {

            // Create a task based on the current storypoint of the pointer.
            // Note that setting scope is explicit, but in effect the scope of the task is the same as the scope of the pointer.

            Pointer = _fromPointer;
            __point = Pointer.currentPoint;

            _fromPointer.currentTask = this;
            //       scope = _scope;

            setDefaults();
            GENERAL.AddTask(this);

        }

        void setDefaults()
        {

            signedOn = new List<string>();

            updateSend = new StoryTaskUpdate();
            updateReceive = new StoryTaskUpdate();

            setStatus(TASKSTATUS.ACTIVE);

        }

        public void LoadPersistantData(StoryPointer referencePointer)
        {

            SetStringValue("persistantData", referencePointer.persistantData);

        }










        void SetPointerToUpdated()
        {
            Pointer.SetStatus(POINTERSTATUS.TASKUPDATED);

            /*
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
            */


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
            //signoffs = GENERAL.SIGNOFFS;
            complete();



            //Warning("Force complete, signoffs still required was " + (GENERAL.SIGNOFFS - signoffs));

            //signoffs = GENERAL.SIGNOFFS;
            //complete();



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
                Warning(Instruction + " trying to sign off more than once: " + fromMe);


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
                Verbose(Instruction + " signing off " + fromMe);

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

            /*
            if (GENERAL.SIGNOFFS == 0)
            {
                Warning("Trying to signoff on a task with 0 required signoffs.");
            }

            signoffs++;

            //			Debug.Log ("SIGNOFFS "+fromMe + description + " signoffs: " + signoffs + " of " + signoffsRequired);

            if (signoffs == GENERAL.SIGNOFFS)
            {

                complete();

            }
            */


        }

    }






}