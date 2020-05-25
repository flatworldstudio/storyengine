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

    /*!
* \brief
* Holds a range of serialisable variables. Serves as base for storytask and storystate.

*/


        /*
    public enum DATASTATE

    {
        NEUTRAL,
        AUTHORITY,
        NEEDSUPDATE
    }
    */

    public class StoryData
    {

        
        string _ID = "";
        
        public string ID
        {
            get
            {
                return _ID;
            }
            set
            {
                if (_ID == "")
                    _ID = value;
                else
                    Warning("Cannot change ID, are you sure that is your intention?");
            }
        }

    
     //   readonly string ID = "StoryData";

        public SCOPE scope;

        protected Dictionary<string, Int32> taskIntValues;
        protected Dictionary<string, float> taskFloatValues;
        protected Dictionary<string, Quaternion> taskQuaternionValues;
        protected Dictionary<string, Vector3> taskVector3Values;
        protected Dictionary<string, string> taskStringValues;
        protected Dictionary<string, ushort[]> taskUshortValues;
        protected Dictionary<string, byte[]> taskByteValues;
        protected Dictionary<string, Vector3[]> taskVector3ArrayValues;
        protected Dictionary<string, bool[]> taskBoolArrayValues;
        protected Dictionary<string, string[]> taskStringArrayValues;

        //       public Dictionary<string, DATASTATE> Participants; // ip address


        public bool modified = false;
        //   protected bool allModified = false;

        protected Dictionary<string, string> taskValuesChangeMask;

        List<string> changeLog;
        //    protected List<string> changedTaskValue;


        // Copy these into every class for easy debugging.
        void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

        // ----------------

        public void MarkAllAsModified()
        {
            //     allModified = true;
            modified = true;
        }


        public StoryData(string _id, SCOPE _scope = SCOPE.LOCAL)
        {

            //      GUID = Guid.NewGuid(); // to be used over network
            ID = _id;
            scope = _scope;
            PopulateData();

        }
        //public void AddParticipant(string _ip)
        //{
        //    Participants.Add(_ip, DATASTATE.NEUTRAL);
        //}

        //public string GetParticipants()
        //{
        //    string r = "";
        //    string[] ips = Participants.Keys.ToArray<string>();
        //    for (int p = 0; p < ips.Length; p++)
        //    {
        //        r += ips[p] + " ";
        //        switch (Participants[ips[p]])
        //        {
        //            case DATASTATE.NEUTRAL:
        //                r += "neutral";
        //                break;
        //            case DATASTATE.AUTHORITY:
        //                r += "authority";
        //                break;
        //            case DATASTATE.NEEDSUPDATE:
        //                r += "needsupdate";
        //                break;
        //            default:
        //                break;

        //        }
        //        r += "\n";
        //    }

        //    return r;



        //}

        void PopulateData()
        {
            //    Participants = new Dictionary<string, DATASTATE>();

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

            taskValuesChangeMask = new Dictionary<string, string>();
            changeLog = new List<string>();
        }

        public StoryDataUpdate GetDataUpdateFor(string target)
        {
            // Get changes for a target, so any changes NOT made by said target.

            StoryDataUpdate msg = new StoryDataUpdate();

            string[] intNames = taskIntValues.Keys.ToArray();

            foreach (string intName in intNames)
            {
                // If mask isn't void and isn't the target, then it is changed but not by the target, implying it should be told about it.

                if (taskValuesChangeMask[intName] != "" && taskValuesChangeMask[intName] != target)
                {

                    msg.updatedIntNames.Add(intName);

                    taskValuesChangeMask[intName] = "";

                    int intValue;

                    if (taskIntValues.TryGetValue(intName, out intValue))
                        msg.updatedIntValues.Add(intValue);

                }

            }

            string[] floatNames = taskFloatValues.Keys.ToArray();

            foreach (string floatName in floatNames)
            {

                if (taskValuesChangeMask[floatName] != target && taskValuesChangeMask[floatName] != target)
                {

                    msg.updatedFloatNames.Add(floatName);

                    taskValuesChangeMask[floatName] = "";

                    float floatValue;

                    if (taskFloatValues.TryGetValue(floatName, out floatValue))
                        msg.updatedFloatValues.Add(floatValue);

                }

            }

            string[] quaternionNames = taskQuaternionValues.Keys.ToArray();

            foreach (string quaternionName in quaternionNames)
            {

                if (taskValuesChangeMask[quaternionName] != "" && taskValuesChangeMask[quaternionName] != target)
                {

                    msg.updatedQuaternionNames.Add(quaternionName);

                    taskValuesChangeMask[quaternionName] = "";

                    Quaternion quaternionValue;

                    if (taskQuaternionValues.TryGetValue(quaternionName, out quaternionValue))
                        msg.updatedQuaternionValues.Add(quaternionValue);

                }

            }

            string[] vector3Names = taskVector3Values.Keys.ToArray();

            foreach (string vector3Name in vector3Names)
            {

                if (taskValuesChangeMask[vector3Name] != "" && taskValuesChangeMask[vector3Name] != target)
                {

                    msg.updatedVector3Names.Add(vector3Name);

                    taskValuesChangeMask[vector3Name] = "";

                    Vector3 vector3Value;

                    if (taskVector3Values.TryGetValue(vector3Name, out vector3Value))
                        msg.updatedVector3Values.Add(vector3Value);

                }

            }

            string[] stringNames = taskStringValues.Keys.ToArray();

            foreach (string stringName in stringNames)
            {

                if (taskValuesChangeMask[stringName] != "" && taskValuesChangeMask[stringName] != target)
                {

                    msg.updatedStringNames.Add(stringName);

                    taskValuesChangeMask[stringName] = "";

                    string stringValue;

                    if (taskStringValues.TryGetValue(stringName, out stringValue))
                        msg.updatedStringValues.Add(stringValue);

                }

            }

            string[] ushortNames = taskUshortValues.Keys.ToArray();

            foreach (string ushortName in ushortNames)
            {

                if (taskValuesChangeMask[ushortName] != "" && taskValuesChangeMask[ushortName] != target)
                {

                    msg.updatedUshortNames.Add(ushortName);

                    taskValuesChangeMask[ushortName] = "";

                    ushort[] ushortValue;

                    if (taskUshortValues.TryGetValue(ushortName, out ushortValue))
                        msg.updatedUshortValues.Add(ushortValue);

                }

            }

            string[] byteNames = taskByteValues.Keys.ToArray();

            foreach (string byteName in byteNames)
            {

                if (taskValuesChangeMask[byteName] != "" && taskValuesChangeMask[byteName] != target)
                {

                    msg.updatedByteNames.Add(byteName);

                    taskValuesChangeMask[byteName] = "";

                    byte[] byteValue;

                    if (taskByteValues.TryGetValue(byteName, out byteValue))
                        msg.updatedByteValues.Add(byteValue);

                }

            }

            string[] vector3ArrayNames = taskVector3ArrayValues.Keys.ToArray();

            foreach (string vector3ArrayName in vector3ArrayNames)
            {

                if (taskValuesChangeMask[vector3ArrayName] != "" && taskValuesChangeMask[vector3ArrayName] != "")
                {

                    msg.updatedVector3ArrayNames.Add(vector3ArrayName);

                    taskValuesChangeMask[vector3ArrayName] = "";

                    Vector3[] vector3ArrayValue;

                    if (taskVector3ArrayValues.TryGetValue(vector3ArrayName, out vector3ArrayValue))
                        msg.updatedVector3ArrayValues.Add(vector3ArrayValue);

                }

            }

            string[] boolArrayNames = taskBoolArrayValues.Keys.ToArray();

            foreach (string boolArrayName in boolArrayNames)
            {

                if (taskValuesChangeMask[boolArrayName] != "" && taskValuesChangeMask[boolArrayName] != target)
                {

                    msg.updatedBoolArrayNames.Add(boolArrayName);

                    taskValuesChangeMask[boolArrayName] = "";

                    bool[] boolArrayValue;

                    if (taskBoolArrayValues.TryGetValue(boolArrayName, out boolArrayValue))
                        msg.updatedBoolArrayValues.Add(boolArrayValue);

                }

            }

            string[] stringArrayNames = taskStringArrayValues.Keys.ToArray();

            foreach (string stringArrayName in stringArrayNames)
            {

                if (taskValuesChangeMask[stringArrayName] != "" && taskValuesChangeMask[stringArrayName] != target)
                {

                    msg.updatedStringArrayNames.Add(stringArrayName);

                    taskValuesChangeMask[stringArrayName] = "";

                    string[] stringArrayValue;

                    if (taskStringArrayValues.TryGetValue(stringArrayName, out stringArrayValue))
                        msg.updatedStringArrayValues.Add(stringArrayValue);

                }

            }

            //     allModified = false;

            return msg;

        }
   

        public StoryDataUpdate GetDataUpdate()
        {
            // Bundled approach.

            StoryDataUpdate msg = new StoryDataUpdate();

            string[] intNames = taskIntValues.Keys.ToArray();

            foreach (string intName in intNames)
            {

                if (taskValuesChangeMask[intName] != "")
                {

                    msg.updatedIntNames.Add(intName);

                    taskValuesChangeMask[intName] = "";

                    int intValue;

                    if (taskIntValues.TryGetValue(intName, out intValue))
                        msg.updatedIntValues.Add(intValue);

                }

            }

            string[] floatNames = taskFloatValues.Keys.ToArray();

            foreach (string floatName in floatNames)
            {

                if (taskValuesChangeMask[floatName] != "")
                {

                    msg.updatedFloatNames.Add(floatName);

                    taskValuesChangeMask[floatName] = "";

                    float floatValue;

                    if (taskFloatValues.TryGetValue(floatName, out floatValue))
                        msg.updatedFloatValues.Add(floatValue);

                }

            }

            string[] quaternionNames = taskQuaternionValues.Keys.ToArray();

            foreach (string quaternionName in quaternionNames)
            {

                if (taskValuesChangeMask[quaternionName] != "")
                {

                    msg.updatedQuaternionNames.Add(quaternionName);

                    taskValuesChangeMask[quaternionName] = "";

                    Quaternion quaternionValue;

                    if (taskQuaternionValues.TryGetValue(quaternionName, out quaternionValue))
                        msg.updatedQuaternionValues.Add(quaternionValue);

                }

            }

            string[] vector3Names = taskVector3Values.Keys.ToArray();

            foreach (string vector3Name in vector3Names)
            {

                if (taskValuesChangeMask[vector3Name] != "")
                {

                    msg.updatedVector3Names.Add(vector3Name);

                    taskValuesChangeMask[vector3Name] = "";

                    Vector3 vector3Value;

                    if (taskVector3Values.TryGetValue(vector3Name, out vector3Value))
                        msg.updatedVector3Values.Add(vector3Value);

                }

            }

            string[] stringNames = taskStringValues.Keys.ToArray();

            foreach (string stringName in stringNames)
            {

                if (taskValuesChangeMask[stringName] != "")
                {

                    msg.updatedStringNames.Add(stringName);

                    taskValuesChangeMask[stringName] = "";

                    string stringValue;

                    if (taskStringValues.TryGetValue(stringName, out stringValue))
                        msg.updatedStringValues.Add(stringValue);

                }

            }

            string[] ushortNames = taskUshortValues.Keys.ToArray();

            foreach (string ushortName in ushortNames)
            {

                if (taskValuesChangeMask[ushortName] != "")
                {

                    msg.updatedUshortNames.Add(ushortName);

                    taskValuesChangeMask[ushortName] = "";

                    ushort[] ushortValue;

                    if (taskUshortValues.TryGetValue(ushortName, out ushortValue))
                        msg.updatedUshortValues.Add(ushortValue);

                }

            }

            string[] byteNames = taskByteValues.Keys.ToArray();

            foreach (string byteName in byteNames)
            {

                if (taskValuesChangeMask[byteName] != "")
                {

                    msg.updatedByteNames.Add(byteName);

                    taskValuesChangeMask[byteName] = "";

                    byte[] byteValue;

                    if (taskByteValues.TryGetValue(byteName, out byteValue))
                        msg.updatedByteValues.Add(byteValue);

                }

            }

            string[] vector3ArrayNames = taskVector3ArrayValues.Keys.ToArray();

            foreach (string vector3ArrayName in vector3ArrayNames)
            {

                if (taskValuesChangeMask[vector3ArrayName] != "")
                {

                    msg.updatedVector3ArrayNames.Add(vector3ArrayName);

                    taskValuesChangeMask[vector3ArrayName] = "";

                    Vector3[] vector3ArrayValue;

                    if (taskVector3ArrayValues.TryGetValue(vector3ArrayName, out vector3ArrayValue))
                        msg.updatedVector3ArrayValues.Add(vector3ArrayValue);

                }

            }

            string[] boolArrayNames = taskBoolArrayValues.Keys.ToArray();

            foreach (string boolArrayName in boolArrayNames)
            {

                if (taskValuesChangeMask[boolArrayName] != "")
                {

                    msg.updatedBoolArrayNames.Add(boolArrayName);

                    taskValuesChangeMask[boolArrayName] = "";

                    bool[] boolArrayValue;

                    if (taskBoolArrayValues.TryGetValue(boolArrayName, out boolArrayValue))
                        msg.updatedBoolArrayValues.Add(boolArrayValue);

                }

            }

            string[] stringArrayNames = taskStringArrayValues.Keys.ToArray();

            foreach (string stringArrayName in stringArrayNames)
            {

                if (taskValuesChangeMask[stringArrayName] != "")
                {

                    msg.updatedStringArrayNames.Add(stringArrayName);

                    taskValuesChangeMask[stringArrayName] = "";

                    string[] stringArrayValue;

                    if (taskStringArrayValues.TryGetValue(stringArrayName, out stringArrayValue))
                        msg.updatedStringArrayValues.Add(stringArrayValue);

                }

            }

            //   allModified = false;

            return msg;

        }


        public void ApplyDataUpdate(StoryDataUpdate update, string changeMask = "")
        {



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

        void AddChangeToLog(string valueName, string changer)
        {
            changeLog.Insert(0, changer + " changed " + valueName);

            if (changeLog.Count > 5)
            {
                changeLog.RemoveAt(changeLog.Count-1);
                //Log("log size " + changeLog.Count);
            }

        }

        public string GetChangeLog()
        {
            string log = "";

            for (int l = 0; l < changeLog.Count; l++)
            {
                log += changeLog[l] + "\n";
            }
            return log;
        }

        public void SetIntValue(string valueName, Int32 value, string changer = "local")
        {

            taskIntValues[valueName] = value;

            taskValuesChangeMask[valueName] = changer;
            modified = true;

            AddChangeToLog(valueName, changer);


        }

        public bool GetIntValue(string valueName, out Int32 value)
        {

            if (!taskIntValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetStringValue(string valueName, string value, string changer = "local")
        {

            //Log("changing " + valueName + "  " + value); 
            taskStringValues[valueName] = value;


            taskValuesChangeMask[valueName] = changer;
            modified = true;


        }

        public bool GetStringValue(string valueName, out string value)
        {

            if (!taskStringValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetFloatValue(string valueName, float value, string changer = "local")
        {

            taskFloatValues[valueName] = value;

            taskValuesChangeMask[valueName] = changer;
            modified = true;

            AddChangeToLog(valueName, changer);

        }

        public bool GetFloatValue(string valueName, out float value)
        {

            if (!taskFloatValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetUshortValue(string valueName, ushort[] value, string changer = "local")
        {

            taskUshortValues[valueName] = value;

            taskValuesChangeMask[valueName] = changer;
            modified = true;


        }

        public bool GetUshortValue(string valueName, out ushort[] value)
        {

            if (!taskUshortValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetByteValue(string valueName, byte[] value, string changer = "local")
        {

            taskByteValues[valueName] = value;

            taskValuesChangeMask[valueName] = changer;
            modified = true;

        }

        public bool GetByteValue(string valueName, out byte[] value)
        {

            if (!taskByteValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }


        public void SetVector3Value(string valueName, Vector3 value, string changer = "local")
        {

            taskVector3Values[valueName] = value;


            taskValuesChangeMask[valueName] = changer;
            modified = true;

        }

        public bool GetVector3Value(string valueName, out Vector3 value)
        {

            if (!taskVector3Values.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetVector3ArrayValue(string valueName, Vector3[] value, string changer = "local")
        {

            taskVector3ArrayValues[valueName] = value;

            taskValuesChangeMask[valueName] = changer;
            modified = true;

        }

        public bool GetVector3ArrayValue(string valueName, out Vector3[] value)
        {

            if (!taskVector3ArrayValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetBoolArrayValue(string valueName, bool[] value, string changer = "local")
        {

            taskBoolArrayValues[valueName] = value;
            taskValuesChangeMask[valueName] = changer;
            modified = true;

        }

        public bool GetBoolArrayValue(string valueName, out bool[] value)
        {

            if (!taskBoolArrayValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetStringArrayValue(string valueName, string[] value, string changer = "local")
        {
          
            taskStringArrayValues[valueName] = value;
            taskValuesChangeMask[valueName] = changer;
            modified = true;

        }

        public bool GetStringArrayValue(string valueName, out string[] value)
        {

            if (!taskStringArrayValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

        public void SetQuaternionValue(string valueName, Quaternion value, string changer = "local")
        {

            taskQuaternionValues[valueName] = value;
            taskValuesChangeMask[valueName] = changer;
            modified = true;

        }

        public bool GetQuaternionValue(string valueName, out Quaternion value)
        {

            if (!taskQuaternionValues.TryGetValue(valueName, out value))
            {
                return false;
            }

            return true;

        }

    }






}