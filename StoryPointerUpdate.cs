using UnityEngine.Networking;

namespace StoryEngine.Network
{

    /*!
    * \brief
    * Holds update data for changes in StoryPointer object
    * 
    * Effectively, updates are only sent to kill a pointer.
    */

    public class StoryPointerUpdate
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

}