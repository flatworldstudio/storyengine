//#define LOGVERBOSE

using UnityEngine;
using System;
using System.Collections.Generic;

#if !SOLO
using UnityEngine.Networking;
#endif

namespace StoryEngine.Network
{
#if !SOLO

    /*!
* \brief
* Holds update data for changes in StoryPointer and StoryTask objects.
*/

    public class StoryUpdate : MessageBase
    {

#if LOGVERBOSE
        public string DebugLog;
#endif

        public List<PointerUpdateBundled> pointerUpdates;
        public List<TaskUpdateBundled> taskUpdates;

        public StoryUpdate() : base()
        {
            // Extended constructor.

#if LOGVERBOSE
            DebugLog = "Created storyupdate. \n";
#endif

            pointerUpdates = new List<PointerUpdateBundled>();
            taskUpdates = new List<TaskUpdateBundled>();

        }

        public bool AnythingToSend()
        {

            return (pointerUpdates.Count > 0 || taskUpdates.Count > 0);

        }

        public bool GetPointerUpdate(out PointerUpdateBundled pointerUpdate)
        {

            int count = pointerUpdates.Count;

            if (count > 0)
            {
                pointerUpdate = pointerUpdates[count - 1];
                pointerUpdates.RemoveAt(count - 1);
                return true;
            }
            pointerUpdate = null;
            return false;

        }

        public bool GetTaskUpdate(out TaskUpdateBundled taskUpdate)
        {

            int count = taskUpdates.Count;

            if (count > 0)
            {
                taskUpdate = taskUpdates[count - 1];
                taskUpdates.RemoveAt(count - 1);
                return true;
            }

            taskUpdate = null;
            return false;

        }

        public void AddStoryPointerUpdate(PointerUpdateBundled pointerUpdateMessage)
        {

            pointerUpdates.Add(pointerUpdateMessage);

#if LOGVERBOSE
            DebugLog += "Added pointer update. \n";
#endif

        }

        public void AddTaskUpdate(TaskUpdateBundled taskUpdateMessage)
        {

            taskUpdates.Add(taskUpdateMessage);

#if LOGVERBOSE
            DebugLog += "Added task update. \n";
#endif

        }

        public override void Deserialize(NetworkReader reader)
        {


            Int16 count = reader.ReadInt16();

            //Debug.Log("pointer updates "+count);

            for (int n = 0; n < count; n++)
            {

                pointerUpdates.Add(new PointerUpdateBundled());

#if LOGVERBOSE
                DebugLog += pointerUpdates[n].Deserialize(ref reader);
#else
                pointerUpdates[n].Deserialize(ref reader);
#endif

            }

            // Task updates next. First get the number of messages, then deserialise them.

            count = reader.ReadInt16();

            //Debug.Log("task updates "+count);

            for (int n = 0; n < count; n++)
            {

                taskUpdates.Add(new TaskUpdateBundled());

#if LOGVERBOSE
                DebugLog += taskUpdates[n].Deserialize(ref reader);
#else
                taskUpdates[n].Deserialize(ref reader);
#endif

            }

        }

        public override void Serialize(NetworkWriter writer)
        {

            // Pointer updates first. First write the number of messages, then serialise them.

            Int16 count = (Int16)pointerUpdates.Count;

            writer.Write(count);

            for (int i = 0; i < count; i++)
            {

#if LOGVERBOSE
                DebugLog += pointerUpdates[i].Serialize(ref writer);
#else
                pointerUpdates[i].Serialize(ref writer);
#endif

            }

            // Task updates next. First write the number of messages, then serialise them.
            count = (Int16)taskUpdates.Count;

            writer.Write(count);

            for (int i = 0; i < count; i++)
            {

#if LOGVERBOSE
                DebugLog += taskUpdates[i].Serialize(ref writer);
#else
                taskUpdates[i].Serialize(ref writer);
#endif

            }

            //Debug.Log("serialised " +DebugLog);

        }

    }

    /*!
* \brief
* Holds update data for changes in StoryPointer object
* 
* # Effectively, updates are only sent to kill a pointer.
*/

    public class PointerUpdateBundled
    {

        // only has a string, because we just telling clients kill this storyline.

        public string StoryLineName;

        public string Deserialize(ref NetworkReader reader)
        {

            StoryLineName = reader.ReadString();

#if LOGVERBOSE
            string DebugLog = "Deserialising pointer update.";
            DebugLog += "Storyline: " + StoryLineName;
            return DebugLog;
#else
            return "";
#endif

        }

        public string Serialize(ref NetworkWriter writer)
        {
            writer.Write(StoryLineName);

#if LOGVERBOSE
            string DebugLog = "Serialising pointer update.";
            DebugLog += "Storyline: " + StoryLineName;
 return DebugLog;
#else
            return "";
#endif

        }

    }


    /*!
* \brief
* Holds update data for changes in StoryTask object
*/

    public class TaskUpdateBundled
    {

        public string pointID;

        public List<string> updatedIntNames;
        public List<Int32> updatedIntValues;

        public List<string> updatedFloatNames;
        public List<float> updatedFloatValues;

        public List<string> updatedQuaternionNames;
        public List<Quaternion> updatedQuaternionValues;

        public List<string> updatedVector3Names;
        public List<Vector3> updatedVector3Values;

        public List<string> updatedStringNames;
        public List<string> updatedStringValues;

        public List<string> updatedUshortNames;
        public List<ushort[]> updatedUshortValues;

        public List<string> updatedByteNames;
        public List<byte[]> updatedByteValues;

        public List<string> updatedVector3ArrayNames;
        public List<Vector3[]> updatedVector3ArrayValues;

        public List<string> updatedBoolArrayNames;
        public List<bool[]> updatedBoolArrayValues;

        public List<string> updatedStringArrayNames;
        public List<string[]> updatedStringArrayValues;

#if LOGVERBOSE
        public string debug;
#endif

        public TaskUpdateBundled() : base()
        {

            updatedIntNames = new List<string>();
            updatedIntValues = new List<Int32>();

            updatedFloatNames = new List<string>();
            updatedFloatValues = new List<float>();

            updatedQuaternionNames = new List<string>();
            updatedQuaternionValues = new List<Quaternion>();

            updatedVector3Names = new List<string>();
            updatedVector3Values = new List<Vector3>();

            updatedStringNames = new List<string>();
            updatedStringValues = new List<string>();

            updatedUshortNames = new List<string>();
            updatedUshortValues = new List<ushort[]>();

            updatedByteNames = new List<string>();
            updatedByteValues = new List<byte[]>();

            updatedVector3ArrayNames = new List<string>();
            updatedVector3ArrayValues = new List<Vector3[]>();

            updatedBoolArrayNames = new List<string>();
            updatedBoolArrayValues = new List<bool[]>();

            updatedStringArrayNames = new List<string>();
            updatedStringArrayValues = new List<string[]>();

        }

        public string Deserialize(ref NetworkReader reader)
        {
#if LOGVERBOSE
            debug = "Task update deserialing.";
#endif
            // Custom deserialisation.

            pointID = reader.ReadString();

#if LOGVERBOSE
            debug += "/ pointid: " + pointID;
#endif

            // Deserialise updated int values.

            int intCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated ints: " + intCount;
#endif

            for (int i = 0; i < intCount; i++)
            {

                string intName = reader.ReadString();
                Int32 intValue = reader.ReadInt32();

                updatedIntNames.Add(intName);
                updatedIntValues.Add(intValue);

            }

            // Deserialise updated float values.

            int floatCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated floats: " + floatCount;
#endif

            for (int i = 0; i < floatCount; i++)
            {

                string floatName = reader.ReadString();
                float floatValue = reader.ReadSingle();

                updatedFloatNames.Add(floatName);
                updatedFloatValues.Add(floatValue);

            }

            // Deserialise updated quaternion values.

            int quaternionCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated quaternions: " + quaternionCount;
#endif

            for (int i = 0; i < quaternionCount; i++)
            {

                string quaternionName = reader.ReadString();
                Quaternion quaternionValue = reader.ReadQuaternion();

                updatedQuaternionNames.Add(quaternionName);
                updatedQuaternionValues.Add(quaternionValue);

            }

            // Deserialise updated vector3 values.

            int vector3Count = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated vector3s: " + vector3Count;
#endif

            for (int i = 0; i < vector3Count; i++)
            {

                string vector3Name = reader.ReadString();
                Vector3 vector3Value = reader.ReadVector3();

                updatedVector3Names.Add(vector3Name);
                updatedVector3Values.Add(vector3Value);

            }

            // Deserialise updated string values.

            int stringCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated strings: " + stringCount;
#endif

            for (int i = 0; i < stringCount; i++)
            {

                string stringName = reader.ReadString();
                string stringValue = reader.ReadString();

                updatedStringNames.Add(stringName);
                updatedStringValues.Add(stringValue);

            }


            // Deserialise updated ushort values.

            int ushortCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated ushort arrays: " + ushortCount;
#endif

            for (int i = 0; i < ushortCount; i++)
            {

                string ushortName = reader.ReadString();
                updatedUshortNames.Add(ushortName);

                int ushortArrayLength = reader.ReadInt32();

                ushort[] ushortArray = new ushort[ushortArrayLength];

                for (int j = 0; j < ushortArrayLength; j++)
                {

                    ushortArray[j] = reader.ReadUInt16();

                }

                updatedUshortValues.Add(ushortArray);

            }

            // Deserialise updated byte values.

            int byteCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated ushort arrays: " + byteCount;
#endif

            for (int i = 0; i < byteCount; i++)
            {

                string byteName = reader.ReadString();
                updatedByteNames.Add(byteName);

                int byteArrayLength = reader.ReadInt32();

                byte[] byteArray = new byte[byteArrayLength];

                for (int j = 0; j < byteArrayLength; j++)
                {

                    byteArray[j] = reader.ReadByte();

                }

                updatedByteValues.Add(byteArray);

            }

            // Deserialise updated vector 3 array values.

            int vector3ArrayCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated vector3 arrays: " + vector3ArrayCount;
#endif

            for (int i = 0; i < vector3ArrayCount; i++)
            {

                string vector3ArrayName = reader.ReadString();
                updatedVector3ArrayNames.Add(vector3ArrayName);

                int vector3ArrayLength = reader.ReadInt32();

                Vector3[] vector3Array = new Vector3[vector3ArrayLength];

                for (int j = 0; j < vector3ArrayLength; j++)
                {

                    vector3Array[j] = reader.ReadVector3();

                }

                updatedVector3ArrayValues.Add(vector3Array);

            }

            // Deserialise updated bool array values.

            int boolArrayCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated bool arrays: " + boolArrayCount;
#endif

            for (int i = 0; i < boolArrayCount; i++)
            {

                string boolArrayName = reader.ReadString();
                updatedBoolArrayNames.Add(boolArrayName);

                int boolArrayLength = reader.ReadInt32();

                bool[] boolArray = new bool[boolArrayLength];

                for (int j = 0; j < boolArrayLength; j++)
                {

                    boolArray[j] = reader.ReadBoolean();

                }

                updatedBoolArrayValues.Add(boolArray);

            }

            // Deserialise updated string array values.

            int stringArrayCount = reader.ReadInt32();

#if LOGVERBOSE
            debug += "/ updated string arrays: " + stringArrayCount;
#endif

            for (int i = 0; i < stringArrayCount; i++)
            {

                string stringArrayName = reader.ReadString();
                updatedStringArrayNames.Add(stringArrayName);

                int stringArrayLength = reader.ReadInt32();

                string[] stringArray = new string[stringArrayLength];

                for (int j = 0; j < stringArrayLength; j++)
                {

                    stringArray[j] = reader.ReadString();

                }

                updatedStringArrayValues.Add(stringArray);

            }

#if LOGVERBOSE
            return debug;
#else
            return "";
#endif

        }

        public string Serialize(ref NetworkWriter writer)
        {
            // Custom serialisation.

#if LOGVERBOSE
            debug = "Serialising: ";
#endif

            writer.Write(pointID);

#if LOGVERBOSE
            debug += "/ pointid: " + pointID;
#endif

            // Serialise updated int values.
            writer.Write(updatedIntNames.Count);

#if LOGVERBOSE
            debug += "/ updated ints: " + updatedIntNames.Count;
#endif

            for (int i = 0; i < updatedIntNames.Count; i++)
            {
                writer.Write(updatedIntNames[i]);
                writer.Write(updatedIntValues[i]);
            }

            // Serialise updated float values.
            writer.Write(updatedFloatNames.Count);

#if LOGVERBOSE
            debug += "/ updated floats: " + updatedFloatNames.Count;
#endif

            for (int i = 0; i < updatedFloatNames.Count; i++)
            {
                writer.Write(updatedFloatNames[i]);
                writer.Write(updatedFloatValues[i]);
            }

            // Serialise updated quaternion values.
            writer.Write(updatedQuaternionNames.Count);

#if LOGVERBOSE
            debug += "/ updated quaternions: " + updatedQuaternionNames.Count;
#endif

            for (int i = 0; i < updatedQuaternionNames.Count; i++)
            {
                writer.Write(updatedQuaternionNames[i]);
                writer.Write(updatedQuaternionValues[i]);
            }

            // Serialise updated vector3 values.
            writer.Write(updatedVector3Names.Count);

#if LOGVERBOSE
            debug += "/ updated vector3's: " + updatedVector3Names.Count;
#endif

            for (int i = 0; i < updatedVector3Names.Count; i++)
            {
                writer.Write(updatedVector3Names[i]);
                writer.Write(updatedVector3Values[i]);
            }

            // Serialise updated string values.
            writer.Write(updatedStringNames.Count);

#if LOGVERBOSE
            debug += "/ updated strings: " + updatedStringNames.Count;
#endif

            for (int i = 0; i < updatedStringNames.Count; i++)
            {
                writer.Write(updatedStringNames[i]);
                writer.Write(updatedStringValues[i]);
            }

            // Serialise updated ushort values.
            writer.Write(updatedUshortNames.Count);

#if LOGVERBOSE
            debug += "/ updated ushorts: " + updatedUshortNames.Count;
#endif

            for (int i = 0; i < updatedUshortNames.Count; i++)
            {
                writer.Write(updatedUshortNames[i]); // name
                writer.Write(updatedUshortValues[i].Length); // length

                for (int j = 0; j < updatedUshortValues[i].Length; j++)
                    writer.Write(updatedUshortValues[i][j]); // data

            }

            // Serialise updated byte values.
            writer.Write(updatedByteNames.Count);

#if LOGVERBOSE
            debug += "/ updated bytes: " + updatedByteNames.Count;
#endif

            for (int i = 0; i < updatedByteNames.Count; i++)
            {
                writer.Write(updatedByteNames[i]); // name
                writer.Write(updatedByteValues[i].Length); // length

                for (int j = 0; j < updatedByteValues[i].Length; j++)
                    writer.Write(updatedByteValues[i][j]); // data

            }

            // Serialise updated vector3 array values.
            writer.Write(updatedVector3ArrayNames.Count);

#if LOGVERBOSE
            debug += "/ updated vector3 arrays: " + updatedVector3ArrayNames.Count;
#endif

            for (int i = 0; i < updatedVector3ArrayNames.Count; i++)
            {
                writer.Write(updatedVector3ArrayNames[i]); // name
                writer.Write(updatedVector3ArrayValues[i].Length); // length

                for (int j = 0; j < updatedVector3ArrayValues[i].Length; j++)
                    writer.Write(updatedVector3ArrayValues[i][j]); // data

            }

            // Serialise updated bool array values.
            writer.Write(updatedBoolArrayNames.Count);

#if LOGVERBOSE
            debug += "/ updated bool arrays: " + updatedBoolArrayNames.Count;
#endif

            for (int i = 0; i < updatedBoolArrayNames.Count; i++)
            {
                writer.Write(updatedBoolArrayNames[i]); // name
                writer.Write(updatedBoolArrayValues[i].Length); // length

                for (int j = 0; j < updatedBoolArrayValues[i].Length; j++)
                    writer.Write(updatedBoolArrayValues[i][j]); // data

            }

            // Serialise updated string array values.
            writer.Write(updatedStringArrayNames.Count);

#if LOGVERBOSE
            debug += "/ updated string arrays: " + updatedStringArrayNames.Count;
#endif

            for (int i = 0; i < updatedStringArrayNames.Count; i++)
            {
                writer.Write(updatedStringArrayNames[i]); // name
                writer.Write(updatedStringArrayValues[i].Length); // length

                for (int j = 0; j < updatedStringArrayValues[i].Length; j++)
                    writer.Write(updatedStringArrayValues[i][j]); // data

            }

#if LOGVERBOSE
            return debug;
#else 
            return "";
#endif

        }
    }

    /*
    public class TaskUpdate : MessageBase
    {

        public string pointID;

        Dictionary<string, Int32> intValues;
        Dictionary<string, float> floatValues;

        public List<string> updatedIntNames;
        public List<Int32> updatedIntValues;

        public List<string> updatedFloatNames;
        public List<float> updatedFloatValues;

        public List<string> updatedQuaternionNames;
        public List<Quaternion> updatedQuaternionValues;

        public List<string> updatedVector3Names;
        public List<Vector3> updatedVector3Values;

        public List<string> updatedStringNames;
        public List<string> updatedStringValues;

        public List<string> updatedUshortNames;
        public List<ushort[]> updatedUshortValues;

        public List<string> updatedByteNames;
        public List<byte[]> updatedByteValues;


        public string debug;

        string me = "Taskupdate";

        public override void Deserialize(NetworkReader reader)
        {

            debug = "Deserialised: ";

            // Custom deserialisation.

            pointID = reader.ReadString();

            debug += "/ pointid: " + pointID;

            // Deserialise updated int values.

            updatedIntNames = new List<string>();
            updatedIntValues = new List<Int32>();

            int intCount = reader.ReadInt32();

            debug += "/ updated ints: " + intCount;

            for (int i = 0; i < intCount; i++)
            {

                string intName = reader.ReadString();
                Int32 intValue = reader.ReadInt32();

                updatedIntNames.Add(intName);
                updatedIntValues.Add(intValue);

            }

            // Deserialise updated float values.

            updatedFloatNames = new List<string>();
            updatedFloatValues = new List<float>();

            int floatCount = reader.ReadInt32();

            debug += "/ updated floats: " + floatCount;

            for (int i = 0; i < floatCount; i++)
            {

                string floatName = reader.ReadString();
                float floatValue = reader.ReadSingle();

                updatedFloatNames.Add(floatName);
                updatedFloatValues.Add(floatValue);

            }

            // Deserialise updated quaternion values.

            updatedQuaternionNames = new List<string>();
            updatedQuaternionValues = new List<Quaternion>();

            int quaternionCount = reader.ReadInt32();

            debug += "/ updated quaternions: " + quaternionCount;

            for (int i = 0; i < quaternionCount; i++)
            {

                string quaternionName = reader.ReadString();
                Quaternion quaternionValue = reader.ReadQuaternion();

                updatedQuaternionNames.Add(quaternionName);
                updatedQuaternionValues.Add(quaternionValue);

            }

            // Deserialise updated vector3 values.

            updatedVector3Names = new List<string>();
            updatedVector3Values = new List<Vector3>();

            int vector3Count = reader.ReadInt32();

            debug += "/ updated vector3s: " + vector3Count;

            for (int i = 0; i < vector3Count; i++)
            {

                string vector3Name = reader.ReadString();
                Vector3 vector3Value = reader.ReadVector3();

                updatedVector3Names.Add(vector3Name);
                updatedVector3Values.Add(vector3Value);

            }

            // Deserialise updated string values.

            updatedStringNames = new List<string>();
            updatedStringValues = new List<string>();

            int stringCount = reader.ReadInt32();

            debug += "/ updated strings: " + stringCount;

            for (int i = 0; i < stringCount; i++)
            {

                string stringName = reader.ReadString();
                string stringValue = reader.ReadString();

                updatedStringNames.Add(stringName);
                updatedStringValues.Add(stringValue);

            }


            // Deserialise updated ushort values.

            updatedUshortNames = new List<string>();
            updatedUshortValues = new List<ushort[]>();

            int ushortCount = reader.ReadInt32();

            debug += "/ updated ushort arrays: " + ushortCount;

            for (int i = 0; i < ushortCount; i++)
            {

                string ushortName = reader.ReadString();
                updatedUshortNames.Add(ushortName);

                int ushortArrayLength = reader.ReadInt32();

                ushort[] ushortArray = new ushort[ushortArrayLength];

                for (int j = 0; j < ushortArrayLength; j++)
                {

                    ushortArray[j] = reader.ReadUInt16();

                }

                updatedUshortValues.Add(ushortArray);

            }

            // Deserialise updated byte values.

            updatedByteNames = new List<string>();
            updatedByteValues = new List<byte[]>();

            int byteCount = reader.ReadInt32();

            debug += "/ updated ushort arrays: " + byteCount;

            for (int i = 0; i < byteCount; i++)
            {

                string byteName = reader.ReadString();
                updatedByteNames.Add(byteName);

                int byteArrayLength = reader.ReadInt32();

                byte[] byteArray = new byte[byteArrayLength];

                for (int j = 0; j < byteArrayLength; j++)
                {

                    byteArray[j] = reader.ReadByte();

                }

                updatedByteValues.Add(byteArray);

            }

            Log.Message(debug, LOGLEVEL.VERBOSE);

            //      Debug.Log (debug);

        }

        public override void Serialize(NetworkWriter writer)
        {

            debug = "Serialised: ";

            // Custom serialisation.

            writer.Write(pointID);
            debug += "/ pointid: " + pointID;

            // Serialise updated int values.

            writer.Write(updatedIntNames.Count);

            debug += "/ updated ints: " + updatedIntNames.Count;

            for (int i = 0; i < updatedIntNames.Count; i++)
            {

                writer.Write(updatedIntNames[i]);
                writer.Write(updatedIntValues[i]);

            }

            // Serialise updated float values.

            writer.Write(updatedFloatNames.Count);

            debug += "/ updated floats: " + updatedFloatNames.Count;

            for (int i = 0; i < updatedFloatNames.Count; i++)
            {

                writer.Write(updatedFloatNames[i]);
                writer.Write(updatedFloatValues[i]);

            }

            // Serialise updated quaternion values.

            writer.Write(updatedQuaternionNames.Count);

            debug += "/ updated quaternions: " + updatedQuaternionNames.Count;

            for (int i = 0; i < updatedQuaternionNames.Count; i++)
            {

                writer.Write(updatedQuaternionNames[i]);
                writer.Write(updatedQuaternionValues[i]);

            }

            // Serialise updated vector3 values.

            writer.Write(updatedVector3Names.Count);

            debug += "/ updated vector3's: " + updatedVector3Names.Count;

            for (int i = 0; i < updatedVector3Names.Count; i++)
            {

                writer.Write(updatedVector3Names[i]);
                writer.Write(updatedVector3Values[i]);

            }

            // Serialise updated string values.

            writer.Write(updatedStringNames.Count);

            debug += "/ updated strings: " + updatedStringNames.Count;

            for (int i = 0; i < updatedStringNames.Count; i++)
            {

                writer.Write(updatedStringNames[i]);
                writer.Write(updatedStringValues[i]);

            }

            // Serialise updated ushort values.

            writer.Write(updatedUshortNames.Count);

            debug += "/ updated ushorts: " + updatedUshortNames.Count;

            for (int i = 0; i < updatedUshortNames.Count; i++)
            {

                writer.Write(updatedUshortNames[i]); // name

                writer.Write(updatedUshortValues[i].Length); // length

                for (int j = 0; j < updatedUshortValues[i].Length; j++)
                {

                    writer.Write(updatedUshortValues[i][j]); // data

                }


            }

            // Serialise updated byte values.

            writer.Write(updatedByteNames.Count);

            debug += "/ updated bytes: " + updatedByteNames.Count;

            for (int i = 0; i < updatedByteNames.Count; i++)
            {

                writer.Write(updatedByteNames[i]); // name

                writer.Write(updatedByteValues[i].Length); // length

                for (int j = 0; j < updatedByteValues[i].Length; j++)
                {

                    writer.Write(updatedByteValues[i][j]); // data

                }


            }

            Log.Message(debug, LOGLEVEL.VERBOSE);
            //      Debug.Log (debug);

        }

    }
    */
#endif



}
