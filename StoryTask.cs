using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

#if !SOLO

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using StoryEngine.Network;

#endif

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
* Task variables are updated across the network by the AssistantDirector.
*/

    public class StoryTask
    {
        
        string ID = "Storytask";


         StoryPoint __point;
         StoryPointer __pointer;

        public SCOPE scope;

        int signoffs;

        TASKSTATUS status;

        public Dictionary<string, Int32> taskIntValues;
        public Dictionary<string, float> taskFloatValues;
        public Dictionary<string, Quaternion> taskQuaternionValues;
        public Dictionary<string, Vector3> taskVector3Values;
        public Dictionary<string, string> taskStringValues;
        public Dictionary<string, ushort[]> taskUshortValues;
        public Dictionary<string, byte[]> taskByteValues;
        public Dictionary<string, Vector3[]> taskVector3ArrayValues;
        public Dictionary<string, bool[]> taskBoolArrayValues;
        public Dictionary<string, string[]> taskStringArrayValues;

        public bool modified = false;
        bool allModified = false;

#if !SOLO

        TaskUpdateBundled updateSend, updateReceive;

        public Dictionary<string, bool> taskValuesChangeMask;
        List<string> changedTaskValue;
      
        int LastUpdateFrame = -1;
        int UpdatesPerFrame = 0;
        public int LastUpdatesPerFrame = 0;
        public int MaxUpdatesPerFrame = 0;

#endif

        // Copy these into every class for easy debugging.
        void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

        // ----------------

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

        public StoryPointer Pointer
        {
            get
            {
                return __pointer;
            }
            set
            {
                __pointer = value;
               // Warning("Can't set Pointer value directly.");
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
                if (__point==null)
                    return "";
                else
                    return __point.ID;
            }
            set
            {
                Warning("Can't set PointID value directly.");
            }
        }
          

        public void MarkAllAsModified()
        {
            allModified = true;
            modified = true;
        }

        public StoryTask(string _fromPointID, SCOPE _scope)
        {

            // Creating a task from a storypoint -> pointer to be created from this task.

            //pointID = storyPointID;
            __point = GENERAL.GetStoryPointByID(_fromPointID);
            //description = point.task[0];
            scope = _scope;
            __pointer = null;

            setDefaults();

            GENERAL.ALLTASKS.Add(this);

        }

        public StoryTask(StoryPointer _fromPointer, SCOPE _scope)
        {

            // Create a task based on the current storypoint of the pointer.
            // Note that setting scope is explicit, but in effect the scope of the task is the same as the scope of the pointer.

            __pointer = _fromPointer;
            //description = pointer.currentPoint.task[0];
            __point = __pointer.currentPoint;
            //pointID = pointer.currentPoint.ID;

            _fromPointer.currentTask = this;
            scope = _scope;

            setDefaults();
            GENERAL.ALLTASKS.Add(this);

        }

        void setDefaults()
        {

            signoffs = 0;

            taskIntValues = new Dictionary<string, int>();
            taskFloatValues = new Dictionary<string, float>();
            taskQuaternionValues = new Dictionary<string, Quaternion>();
            taskVector3Values = new Dictionary<string, Vector3>();
            taskStringValues = new Dictionary<string, string>();
            taskUshortValues = new Dictionary<string, ushort[]>();
            taskByteValues = new Dictionary<string, byte[]>();
            taskVector3ArrayValues = new Dictionary<string, Vector3[]>();
            taskBoolArrayValues = new Dictionary<string, bool[]>();
            taskStringArrayValues = new Dictionary<string, string[]>();


#if !SOLO
            taskValuesChangeMask = new Dictionary<string, bool>();
            updateSend = new TaskUpdateBundled();
            updateReceive = new TaskUpdateBundled();

#endif

            setStatus(TASKSTATUS.ACTIVE);

        }

        public void LoadPersistantData(StoryPointer referencePointer)
        {

            SetStringValue("persistantData", referencePointer.persistantData);

        }

#if !SOLO


        public TaskUpdateBundled GetUpdateBundled()
        {
            // Bundled approach.

            TaskUpdateBundled msg = new TaskUpdateBundled();

            msg.pointID = PointID;

            string[] intNames = taskIntValues.Keys.ToArray();

            foreach (string intName in intNames)
            {

                if (taskValuesChangeMask[intName] || allModified)
                {

                    msg.updatedIntNames.Add(intName);

                    taskValuesChangeMask[intName] = false;

                    int intValue;

                    if (taskIntValues.TryGetValue(intName, out intValue))
                        msg.updatedIntValues.Add(intValue);

                }

            }

            string[] floatNames = taskFloatValues.Keys.ToArray();

            foreach (string floatName in floatNames)
            {

                if (taskValuesChangeMask[floatName] || allModified)
                {

                    msg.updatedFloatNames.Add(floatName);

                    taskValuesChangeMask[floatName] = false;

                    float floatValue;

                    if (taskFloatValues.TryGetValue(floatName, out floatValue))
                        msg.updatedFloatValues.Add(floatValue);

                }

            }

            string[] quaternionNames = taskQuaternionValues.Keys.ToArray();

            foreach (string quaternionName in quaternionNames)
            {

                if (taskValuesChangeMask[quaternionName] || allModified)
                {

                    msg.updatedQuaternionNames.Add(quaternionName);

                    taskValuesChangeMask[quaternionName] = false;

                    Quaternion quaternionValue;

                    if (taskQuaternionValues.TryGetValue(quaternionName, out quaternionValue))
                        msg.updatedQuaternionValues.Add(quaternionValue);

                }

            }

            string[] vector3Names = taskVector3Values.Keys.ToArray();

            foreach (string vector3Name in vector3Names)
            {

                if (taskValuesChangeMask[vector3Name] || allModified)
                {

                    msg.updatedVector3Names.Add(vector3Name);

                    taskValuesChangeMask[vector3Name] = false;

                    Vector3 vector3Value;

                    if (taskVector3Values.TryGetValue(vector3Name, out vector3Value))
                        msg.updatedVector3Values.Add(vector3Value);

                }

            }

            string[] stringNames = taskStringValues.Keys.ToArray();

            foreach (string stringName in stringNames)
            {

                if (taskValuesChangeMask[stringName] || allModified)
                {

                    msg.updatedStringNames.Add(stringName);

                    taskValuesChangeMask[stringName] = false;

                    string stringValue;

                    if (taskStringValues.TryGetValue(stringName, out stringValue))
                        msg.updatedStringValues.Add(stringValue);

                }

            }

            string[] ushortNames = taskUshortValues.Keys.ToArray();

            foreach (string ushortName in ushortNames)
            {

                if (taskValuesChangeMask[ushortName] || allModified)
                {

                    msg.updatedUshortNames.Add(ushortName);

                    taskValuesChangeMask[ushortName] = false;

                    ushort[] ushortValue;

                    if (taskUshortValues.TryGetValue(ushortName, out ushortValue))
                        msg.updatedUshortValues.Add(ushortValue);

                }

            }

            string[] byteNames = taskByteValues.Keys.ToArray();

            foreach (string byteName in byteNames)
            {

                if (taskValuesChangeMask[byteName] || allModified)
                {

                    msg.updatedByteNames.Add(byteName);

                    taskValuesChangeMask[byteName] = false;

                    byte[] byteValue;

                    if (taskByteValues.TryGetValue(byteName, out byteValue))
                        msg.updatedByteValues.Add(byteValue);

                }

            }

            string[] vector3ArrayNames = taskVector3ArrayValues.Keys.ToArray();

            foreach (string vector3ArrayName in vector3ArrayNames)
            {

                if (taskValuesChangeMask[vector3ArrayName] || allModified)
                {

                    msg.updatedVector3ArrayNames.Add(vector3ArrayName);

                    taskValuesChangeMask[vector3ArrayName] = false;

                    Vector3[] vector3ArrayValue;

                    if (taskVector3ArrayValues.TryGetValue(vector3ArrayName, out vector3ArrayValue))
                        msg.updatedVector3ArrayValues.Add(vector3ArrayValue);

                }

            }

            string[] boolArrayNames = taskBoolArrayValues.Keys.ToArray();

            foreach (string boolArrayName in boolArrayNames)
            {

                if (taskValuesChangeMask[boolArrayName] || allModified)
                {

                    msg.updatedBoolArrayNames.Add(boolArrayName);

                    taskValuesChangeMask[boolArrayName] = false;

                    bool[] boolArrayValue;

                    if (taskBoolArrayValues.TryGetValue(boolArrayName, out boolArrayValue))
                        msg.updatedBoolArrayValues.Add(boolArrayValue);

                }

            }

            string[] stringArrayNames = taskStringArrayValues.Keys.ToArray();

            foreach (string stringArrayName in stringArrayNames)
            {

                if (taskValuesChangeMask[stringArrayName] || allModified)
                {

                    msg.updatedStringArrayNames.Add(stringArrayName);

                    taskValuesChangeMask[stringArrayName] = false;

                    string[] stringArrayValue;

                    if (taskStringArrayValues.TryGetValue(stringArrayName, out stringArrayValue))
                        msg.updatedStringArrayValues.Add(stringArrayValue);

                }

            }

            allModified = false;

            return msg;

        }



        /*
        public TaskUpdate GetUpdateMessage()
        {

            TaskUpdate msg = new TaskUpdate();

            msg.pointID = pointID;

            msg.updatedIntNames = new List<string>();
            msg.updatedIntValues = new List<Int32>();
            msg.updatedFloatNames = new List<string>();
            msg.updatedFloatValues = new List<float>();
            msg.updatedQuaternionNames = new List<string>();
            msg.updatedQuaternionValues = new List<Quaternion>();
            msg.updatedVector3Names = new List<string>();
            msg.updatedVector3Values = new List<Vector3>();
            msg.updatedStringNames = new List<string>();
            msg.updatedStringValues = new List<string>();
            msg.updatedUshortNames = new List<string>();
            msg.updatedUshortValues = new List<ushort[]>();

            msg.updatedByteNames = new List<string>();
            msg.updatedByteValues = new List<byte[]>();

            string[] intNames = taskIntValues.Keys.ToArray();

            foreach (string intName in intNames)
            {

                if (taskValuesChangeMask[intName] || allModified)
                {

                    msg.updatedIntNames.Add(intName);

                    taskValuesChangeMask[intName] = false;

                    int intValue;

                    if (taskIntValues.TryGetValue(intName, out intValue))
                        msg.updatedIntValues.Add(intValue);

                }

            }

            string[] floatNames = taskFloatValues.Keys.ToArray();

            foreach (string floatName in floatNames)
            {

                if (taskValuesChangeMask[floatName] || allModified)
                {

                    msg.updatedFloatNames.Add(floatName);

                    taskValuesChangeMask[floatName] = false;

                    float floatValue;

                    if (taskFloatValues.TryGetValue(floatName, out floatValue))
                        msg.updatedFloatValues.Add(floatValue);

                }

            }

            string[] quaternionNames = taskQuaternionValues.Keys.ToArray();

            foreach (string quaternionName in quaternionNames)
            {

                if (taskValuesChangeMask[quaternionName] || allModified)
                {

                    msg.updatedQuaternionNames.Add(quaternionName);

                    taskValuesChangeMask[quaternionName] = false;

                    Quaternion quaternionValue;

                    if (taskQuaternionValues.TryGetValue(quaternionName, out quaternionValue))
                        msg.updatedQuaternionValues.Add(quaternionValue);

                }

            }

            string[] vector3Names = taskVector3Values.Keys.ToArray();

            foreach (string vector3Name in vector3Names)
            {

                if (taskValuesChangeMask[vector3Name] || allModified)
                {

                    msg.updatedVector3Names.Add(vector3Name);

                    taskValuesChangeMask[vector3Name] = false;

                    Vector3 vector3Value;

                    if (taskVector3Values.TryGetValue(vector3Name, out vector3Value))
                        msg.updatedVector3Values.Add(vector3Value);

                }

            }

            string[] stringNames = taskStringValues.Keys.ToArray();

            foreach (string stringName in stringNames)
            {

                if (taskValuesChangeMask[stringName] || allModified)
                {

                    msg.updatedStringNames.Add(stringName);

                    taskValuesChangeMask[stringName] = false;

                    string stringValue;

                    if (taskStringValues.TryGetValue(stringName, out stringValue))
                        msg.updatedStringValues.Add(stringValue);

                }

            }

            string[] ushortNames = taskUshortValues.Keys.ToArray();

            foreach (string ushortName in ushortNames)
            {

                if (taskValuesChangeMask[ushortName] || allModified)
                {

                    msg.updatedUshortNames.Add(ushortName);

                    taskValuesChangeMask[ushortName] = false;

                    ushort[] ushortValue;

                    if (taskUshortValues.TryGetValue(ushortName, out ushortValue))
                        msg.updatedUshortValues.Add(ushortValue);

                }

            }

            string[] byteNames = taskByteValues.Keys.ToArray();

            foreach (string byteName in byteNames)
            {

                if (taskValuesChangeMask[byteName] || allModified)
                {

                    msg.updatedByteNames.Add(byteName);

                    taskValuesChangeMask[byteName] = false;

                    byte[] byteValue;

                    if (taskByteValues.TryGetValue(byteName, out byteValue))
                        msg.updatedByteValues.Add(byteValue);

                }

            }

            allModified = false;

            return msg;

        }
*/
        string UpdateFrequency = "";

        public void ApplyUpdateMessage(TaskUpdateBundled update, bool changeMask = false)
        {

            //		Log.Message ("Applying network task update.");

            //		if (update.updatedIntNames.Contains ("status")) {
            //			Log.Message ("incoming task status change, setting pointerstatus to taskupdated.", LOGLEVEL.VERBOSE);
            //
            //			pointer.SetStatus (POINTERSTATUS.TASKUPDATED);
            //		}

            //if (description == "userstream")
            //{

            //    int CurrentFrame = Time.frameCount;

            //    if (CurrentFrame - LastUpdateFrame==0)
            //    {

            //        UpdatesPerFrame++;


            //    }
            //    if (CurrentFrame - LastUpdateFrame==1){
            //        // we're in the next frame
            //        UpdateFrequency += "" + UpdatesPerFrame;

            //        UpdatesPerFrame = 1;

            //    }

            //    if (CurrentFrame - LastUpdateFrame>1){
            //        // we've skipped a frame

            //        for (int f=LastUpdateFrame+1 ;f<CurrentFrame;f++){
            //            UpdateFrequency += "" + 0;

            //        }



            //        UpdatesPerFrame = 1;

            //    }





            //    if (UpdateFrequency.Length > 30)
            //    {
            //        Debug.Log("update pattern:" + UpdateFrequency);
            //        setStringValue("debug",UpdateFrequency);
            //        UpdateFrequency = "";
            //    }
            //    LastUpdateFrame = CurrentFrame;
            //}
            //MaxUpdatesPerFrame = Mathf.Max(MaxUpdatesPerFrame, UpdatesPerFrame);




            for (int i = 0; i < update.updatedIntNames.Count; i++)
            {
                taskIntValues[update.updatedIntNames[i]] = update.updatedIntValues[i];
                taskValuesChangeMask[update.updatedIntNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedFloatNames.Count; i++)
            {
                taskFloatValues[update.updatedFloatNames[i]] = update.updatedFloatValues[i];
                taskValuesChangeMask[update.updatedFloatNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedQuaternionNames.Count; i++)
            {
                taskQuaternionValues[update.updatedQuaternionNames[i]] = update.updatedQuaternionValues[i];
                taskValuesChangeMask[update.updatedQuaternionNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedVector3Names.Count; i++)
            {
                taskVector3Values[update.updatedVector3Names[i]] = update.updatedVector3Values[i];
                taskValuesChangeMask[update.updatedVector3Names[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedStringNames.Count; i++)
            {
                taskStringValues[update.updatedStringNames[i]] = update.updatedStringValues[i];
                taskValuesChangeMask[update.updatedStringNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedUshortNames.Count; i++)
            {

                taskUshortValues[update.updatedUshortNames[i]] = update.updatedUshortValues[i];
                taskValuesChangeMask[update.updatedUshortNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedByteNames.Count; i++)
            {

                taskByteValues[update.updatedByteNames[i]] = update.updatedByteValues[i];
                taskValuesChangeMask[update.updatedByteNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedVector3ArrayNames.Count; i++)
            {

                taskVector3ArrayValues[update.updatedVector3ArrayNames[i]] = update.updatedVector3ArrayValues[i];
                taskValuesChangeMask[update.updatedVector3ArrayNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedBoolArrayNames.Count; i++)
            {

                taskBoolArrayValues[update.updatedBoolArrayNames[i]] = update.updatedBoolArrayValues[i];
                taskValuesChangeMask[update.updatedBoolArrayNames[i]] = changeMask;

            }

            for (int i = 0; i < update.updatedStringArrayNames.Count; i++)
            {

                taskStringArrayValues[update.updatedStringArrayNames[i]] = update.updatedStringArrayValues[i];
                taskValuesChangeMask[update.updatedStringArrayNames[i]] = changeMask;

            }

        }

#endif

        public void SetIntValue(string valueName, Int32 value)
        {

            taskIntValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetIntValue(string valueName, out Int32 value)
        {

            if (!taskIntValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetStringValue(string valueName, string value)
        {

            taskStringValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetStringValue(string valueName, out string value)
        {

            if (!taskStringValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetFloatValue(string valueName, float value)
        {

            taskFloatValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetFloatValue(string valueName, out float value)
        {

            if (!taskFloatValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetUshortValue(string valueName, ushort[] value)
        {

            taskUshortValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetUshortValue(string valueName, out ushort[] value)
        {

            if (!taskUshortValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetByteValue(string valueName, byte[] value)
        {

            taskByteValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetByteValue(string valueName, out byte[] value)
        {

            if (!taskByteValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }


        public void SetVector3Value(string valueName, Vector3 value)
        {

            taskVector3Values[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetVector3Value(string valueName, out Vector3 value)
        {

            if (!taskVector3Values.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetVector3ArrayValue(string valueName, Vector3[] value)
        {

            taskVector3ArrayValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetVector3ArrayValue(string valueName, out Vector3[] value)
        {

            if (!taskVector3ArrayValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetBoolArrayValue(string valueName, bool[] value)
        {

            taskBoolArrayValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetBoolArrayValue(string valueName, out bool[] value)
        {

            if (!taskBoolArrayValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetStringArrayValue(string valueName, string[] value)
        {

            taskStringArrayValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetStringArrayValue(string valueName, out string[] value)
        {

            if (!taskStringArrayValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetQuaternionValue(string valueName, Quaternion value)
        {

            taskQuaternionValues[valueName] = value;

#if !SOLO
            taskValuesChangeMask[valueName] = true;
            modified = true;
#endif

        }

        public bool GetQuaternionValue(string valueName, out Quaternion value)
        {

            if (!taskQuaternionValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        void SetPointerToUpdated()
        {

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



            Debug.Log("Force complete, signoffs still required was " + (GENERAL.SIGNOFFS - signoffs));

            signoffs = GENERAL.SIGNOFFS;
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

        public void signOff(String fromMe)
        {

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

        }

    }






}