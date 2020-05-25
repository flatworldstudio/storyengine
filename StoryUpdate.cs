//#define LOGVERBOSE

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace StoryEngine.Network
{


/*!
* \brief
* ROLLEDBACK: Holds update data for changes in storydata. An update is targeted at a client or server, and data changes can be packaged differently/.
* Holds update data for changes in StoryPointer and StoryTask objects.

 */

    public class StoryUpdate : MessageBase
    {

        public string DebugLog;

        //public List<DataUpdate> dataUpdates;

        public List<StoryPointerUpdate> pointerUpdates;
        public List<StoryTaskUpdate> taskUpdates;

        public StoryUpdate() : base()
        {
            // Extended constructor.

#if LOGVERBOSE
            DebugLog = "Created storyupdate. \n";
#endif
            //dataUpdates = new List<DataUpdate>();

            pointerUpdates = new List<StoryPointerUpdate>();
            taskUpdates = new List<StoryTaskUpdate>();

        }

        public bool AnythingToSend()
        {
            //return dataUpdates.Count > 0;

            return (pointerUpdates.Count > 0 || taskUpdates.Count > 0);

        }

        /*
        public bool GetDataUpdate(out DataUpdate dataUpdate)
        {

            int count = dataUpdates.Count;

            if (count > 0)
            {
                dataUpdate = dataUpdates[count - 1];
                dataUpdates.RemoveAt(count - 1);
                return true;
            }

            dataUpdate = null;
            return false;

        }

        public void AddDataUpdate(DataUpdate dataUpdate)
        {

            dataUpdates.Add(dataUpdate);

#if LOGVERBOSE
            DebugLog += "Added data update. \n";
            DebugLog += dataUpdate.debug;
#endif

        }
        */
        
        public bool GetPointerUpdate(out StoryPointerUpdate pointerUpdate)
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

        public bool GetTaskUpdate(out StoryTaskUpdate taskUpdate)
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

        public void AddStoryPointerUpdate(StoryPointerUpdate pointerUpdateMessage)
        {

            pointerUpdates.Add(pointerUpdateMessage);

#if LOGVERBOSE
            DebugLog += "Added pointer update. \n";
#endif

        }

        public void AddTaskUpdate(StoryTaskUpdate taskUpdateMessage)
        {

            taskUpdates.Add(taskUpdateMessage);

#if LOGVERBOSE
            DebugLog += "Added task update. \n";
#endif

        }


        /*
        public override void Deserialize(NetworkReader reader)
        {
            
            Int16 count = reader.ReadInt16();
                       
            for (int n = 0; n < count; n++)
            {

                dataUpdates.Add(new DataUpdate());

#if LOGVERBOSE
                DebugLog += dataUpdates[n].Deserialize(ref reader);
#else
                dataUpdates[n].Deserialize(ref reader);
#endif

            }
               
        }

        public override void Serialize(NetworkWriter writer)
        {

            // Pointer updates first. First write the number of messages, then serialise them.

            Int16 count = (Int16)dataUpdates.Count;

            writer.Write(count);

            for (int i = 0; i < count; i++)
            {

#if LOGVERBOSE
                DebugLog += dataUpdates[i].Serialize(ref writer);
#else
                dataUpdates[i].Serialize(ref writer);
#endif

            }

          

            //Debug.Log("serialised " +DebugLog);

        }
        */
        public override void Deserialize(NetworkReader reader)
        {


            Int16 count = reader.ReadInt16();

            //Debug.Log("pointer updates "+count);

            for (int n = 0; n < count; n++)
            {

                pointerUpdates.Add(new StoryPointerUpdate());

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

                taskUpdates.Add(new StoryTaskUpdate());

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
           
            Debug.Log("serialised " +DebugLog);

        }

    }



}
