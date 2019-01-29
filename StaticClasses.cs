
using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
using System;
//using System.Linq;


//#if NETWORKED
//using UnityEngine.Networking;
//using UnityEngine.Networking.NetworkSystem;
//#endif

namespace StoryEngine
{

    public enum SCOPE
    {
        LOCAL,
        GLOBAL

    }

    public enum AUTHORITY
    {
        LOCAL,
        GLOBAL

    }

    public class UserCallBack
    {

        public bool trigger = false;
        public string label = "";
        public GameObject sender;

        public UserCallBack()

        {
        }

    }


    public static class GENERAL
    {
  
        public static AUTHORITY AUTHORITY = AUTHORITY.LOCAL;

        public static float pointerScreenScalar = -0.5f;
        public static float pointerRectScalar = 0.5f;

        public static int SIGNOFFS;

        public static Dictionary<string, StoryPoint> storyPoints;
        public static List<StoryPointer> ALLPOINTERS;
        public static List<StoryTask> ALLTASKS;

        public static bool isPauzed = false;
        public static bool hasFocus = true;

        public static string broadcastServer, networkServer;

        public static string ID = "General";

        // Copy these into every class for easy debugging.
        static void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        static void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        static void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        static void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);

        public static void AddPointer(StoryPointer pointer)
        {

            if (pointer != null)
                ALLPOINTERS.Add(pointer);

        }

        public static StoryPoint GetStoryPointByID(string pointID)
        {
            StoryPoint r;

            if (!storyPoints.TryGetValue(pointID, out r))
            {
                Warning("Storypoint " + pointID + " not found.");
            }

            return r;

        }

        static void flagPointerOverflow()
        {

            if (ALLPOINTERS.Count > 25)
            {
                Warning("Potential pointer overflow.");
            }

        }

        static void flagTaskOverflow()
        {

            if (ALLTASKS.Count > 25)
            {
                Warning("Potential task overflow.");


                //			foreach (Task t in ALLTASKS) {
                //				Debug.Log (t.pointer.ID + " " + t.description);
                //
                //			}

            }

        }

        public static StoryPointer GetStorylinePointerForPointID(string pointID)
        {

            string storyline = GetStoryPointByID(pointID).StoryLine;

            flagPointerOverflow();

            StoryPointer r = null;

            foreach (StoryPointer p in GENERAL.ALLPOINTERS)
            {

                if (p.currentPoint.StoryLine == storyline)
                {
                    r = p;
                    break;
                }

            }

            return r;
        }



        public static StoryPointer GetPointerForStoryline(string theStoryLine)
        {

            flagPointerOverflow();

            StoryPointer r = null;

            foreach (StoryPointer p in GENERAL.ALLPOINTERS)
            {

                if (p.currentPoint.StoryLine == theStoryLine)
                {
                    r = p;
                    break;
                }

            }

            return r;
        }

        public static StoryPointer GetPointerForPoint(string pointID)
        {

            flagPointerOverflow();

            for (int i = 0; i < ALLPOINTERS.Count; i++)
            {

                if (ALLPOINTERS[i].currentPoint.ID.Equals(pointID))
                {

                    return ALLPOINTERS[i];
                }

            }

            return null;

        }

        public static StoryTask GetTaskForPoint(string pointID)
        {

            flagTaskOverflow();

            for (int t = 0; t < ALLTASKS.Count; t++)
            {

                if (ALLTASKS[t].PointID == pointID)
                {

                    return ALLTASKS[t];

                }

            }

            return null;

        }

        //public static int CONNECTIONINDEX;

        //static int NEWCONNECTION;


        //public static void SETNEWCONNECTION(int value)
        //{
        //    NEWCONNECTION = value;

        //}

        //public static int GETNEWCONNECTION()
        //{

        //    int r = NEWCONNECTION;
        //    NEWCONNECTION = -1;
        //    return r;

        //}

    }

    public static class UUID
    {
        static int uid { get; set; }

        public static string identity;

        public static string getGlobalID()
        {

            uid++;

            return identity + uid.ToString("x8");

        }

        public static string getID()
        {

            uid++;

            return uid.ToString("x8");

        }

        public static void setIdentity()
        {

            string stamp = System.DateTime.UtcNow.ToString("yyyyMMddhhmmss");

            Int64 num = Int64.Parse(stamp);
            identity = num.ToString("x8");

        }
    }
}