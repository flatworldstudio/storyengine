using UnityEngine;
using System;
using System.Collections.Generic;

#if NETWORKED
using UnityEngine.Networking;
#endif

namespace StoryEngine
{
#if NETWORKED

    // Combined single message class.

    public class StoryUpdate : MessageBase
    {

        public string DebugLog;
        List<PointerUpdateMessage> pointerUpdateMessages;
        List<TaskUpdateMessage> taskUpdateMessages;


        public StoryUpdate() : base()
        {
            // Extended constructor.
            pointerUpdateMessages = new List<PointerUpdateMessage>();
            taskUpdateMessages = new List<TaskUpdateMessage>();

        }

        public bool AnythingToSend()
        {

            return (pointerUpdateMessages.Count > 0 || taskUpdateMessages.Count > 0);

        }

        public void Clear()
        {
            // Clear for reuse.

            DebugLog = "Created storyupdate. \n";
            pointerUpdateMessages.Clear();
            taskUpdateMessages.Clear();

        }

        public void AddStoryPointerUpdate(PointerUpdateMessage pointerUpdateMessage)
        {

            pointerUpdateMessages.Add(pointerUpdateMessage);
            DebugLog += "Added pointer update. \n";

        }

        public void AddTaskUpdate(TaskUpdateMessage taskUpdateMessage)
        {

            taskUpdateMessages.Add(taskUpdateMessage);
            DebugLog += "Added task update. \n";

        }



        public override void Deserialize(NetworkReader reader)
        {

        }

        public override void Serialize(NetworkWriter writer)
        {
            // Pointer updates first. First write the number of messages, then serialise them.

            writer.Write(pointerUpdateMessages.Count);

            for (int m = 0; m < pointerUpdateMessages.Count; m++)
            {
                DebugLog += pointerUpdateMessages[m].Serialize(ref writer);
            }

            // Task updates next. First write the number of messages, then serialise them.

            writer.Write(taskUpdateMessages.Count);

            for (int m = 0; m < taskUpdateMessages.Count; m++)
            {

                DebugLog += pointerUpdateMessages[m].Serialize(ref writer);
            }

        }

    }




    // bundled


    public class PointerUpdate : MessageBase
    {

        public string storyPointID;
        public bool killed = false;

    }


    public class PointerUpdateMessage
    {

        public string storyPointID;
        public bool killed = false;


        public string Deserialize(ref NetworkReader reader)
        {

            string DebugLog = "Deserialising pointer update.";

            storyPointID = reader.ReadString();
            killed = reader.ReadBoolean();

            DebugLog += "Storypoint ID: " + storyPointID;
            DebugLog += "Killed: " + killed.ToString();

            return DebugLog;

        }

        public string Serialize(ref NetworkWriter writer)
        {

            string DebugLog = "Serialising pointer update.";

            DebugLog += "Storypoint ID: " + storyPointID;
            DebugLog += "Killed: " + killed.ToString();

            writer.Write(storyPointID);
            writer.Write(killed);

            return DebugLog;

        }

    }




    public class TaskUpdateMessage
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

        public string debug;

        public TaskUpdateMessage() : base()
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
        }

        public void Clear()
        {

            updatedIntNames.Clear();
            updatedIntValues.Clear();

            updatedFloatNames.Clear();
            updatedFloatValues.Clear();

            updatedQuaternionNames.Clear();
            updatedQuaternionValues.Clear();

            updatedVector3Names.Clear();
            updatedVector3Values.Clear();

            updatedStringNames.Clear();
            updatedStringValues.Clear();

            updatedUshortNames.Clear();
            updatedUshortValues.Clear();

            updatedByteNames.Clear();
            updatedByteValues.Clear();

        }

        public string Deserialize(ref NetworkReader reader)
        {
            // When deserialising we handle clearing here.

            Clear();

            debug = "Task update deserialing.";

            // Custom deserialisation.

            pointID = reader.ReadString();

            debug += "/ pointid: " + pointID;

            // Deserialise updated int values.

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

            return debug;

        }

        public string Serialize(ref NetworkWriter writer)
        {

            debug = "Serialising: ";

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

            return debug;

        }

    }




    // individual

    //public class PointerUpdate : MessageBase
    //{

    //    public string storyPointID;
    //    public bool killed = false;

    //}


    //public class PointerUpdateMessage
    //{

    //    public string storyPointID;
    //    public bool killed = false;


    //    public string Deserialize(ref NetworkReader reader){

    //        string DebugLog ="Deserialising pointer update.";

    //        storyPointID=reader.ReadString();
    //        killed=reader.ReadBoolean();

    //        DebugLog+="Storypoint ID: "+storyPointID;
    //        DebugLog+="Killed: "+killed.ToString();

    //        return DebugLog;

    //    }

    //    public string Serialize(ref NetworkWriter writer){

    //        string DebugLog ="Serialising pointer update.";

    //        DebugLog+="Storypoint ID: "+storyPointID;
    //        DebugLog+="Killed: "+killed.ToString();

    //        writer.Write(storyPointID);
    //        writer.Write(killed);

    //        return DebugLog;

    //    }

    //}



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

#endif



}
