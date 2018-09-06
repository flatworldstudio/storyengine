
using UnityEngine;
using System.Collections.Generic;

using System;
using System.Diagnostics;

namespace StoryEngine
{

    public enum LOGLEVEL
    {

        OFF,
        ERRORS,
        WARNINGS,
        NORMAL,
        VERBOSE

    }


    public static class Log
    {

        static Dictionary<string, LOGLEVEL> moduleLogStatus;

        public static void AddModule(string module, LOGLEVEL status)

        {

            Init();

            moduleLogStatus.Add(module, status);

        }

        public static void Init()
        {
            if (moduleLogStatus == null)
                moduleLogStatus = new Dictionary<string, LOGLEVEL>();

        }

        public static void SetModuleLevel(string module, LOGLEVEL level)

        {

            LOGLEVEL current;

            Init();

            if (moduleLogStatus.TryGetValue(module, out current))
            {

                moduleLogStatus[module] = level;

            }
            else
            {

                AddModule(module, level);

            }

        }


        public static void Message(string message, LOGLEVEL messageLevel = LOGLEVEL.NORMAL)

        {
#if UNITY_EDITOR

            StackFrame callStack = new StackFrame(1, true);

            Char[] delimiter = { '/', '.', '\\' };
            string[] caller = callStack.GetFileName().Split(delimiter);
            string callerId = caller[caller.Length - 2];

            string line = "" + callStack.GetFileLineNumber();

#else

		string callerId = "";
		string line = "";

#endif

            LOGLEVEL moduleLevel;

            if (!moduleLogStatus.TryGetValue(callerId, out moduleLevel))
            {

                moduleLevel = LOGLEVEL.ERRORS; // normal by default.

            }

            if (messageLevel <= moduleLevel)
            {

                UnityEngine.Debug.Log(callerId + ": " + message + ", Line: " + line);

            }

        }

        public static void Warning(string message)

        {
            LOGLEVEL messageLevel = LOGLEVEL.WARNINGS;

#if UNITY_EDITOR

            StackFrame callStack = new StackFrame(1, true);

            Char[] delimiter = { '/', '.' };
            string[] caller = callStack.GetFileName().Split(delimiter);
            string callerId = caller[caller.Length - 2];

            string line = "" + callStack.GetFileLineNumber();

#else

		string callerId = "";
		string line = "";

#endif

            LOGLEVEL moduleLevel;

            if (!moduleLogStatus.TryGetValue(callerId, out moduleLevel))
            {

                moduleLevel = LOGLEVEL.NORMAL; // normal by default.

            }

            if (messageLevel <= moduleLevel)
            {

                UnityEngine.Debug.LogWarning(callerId + ": " + message + ", Line: " + line);

            }

        }

        public static void Error(string message)

        {
            LOGLEVEL messageLevel = LOGLEVEL.WARNINGS;

#if UNITY_EDITOR

            StackFrame callStack = new StackFrame(1, true);

            Char[] delimiter = { '/', '.' };
            string[] caller = callStack.GetFileName().Split(delimiter);
            string callerId = caller[caller.Length - 2];

            string line = "" + callStack.GetFileLineNumber();

#else

		string callerId = "";
		string line = "";

#endif

            LOGLEVEL moduleLevel;

            if (!moduleLogStatus.TryGetValue(callerId, out moduleLevel))
            {

                moduleLevel = LOGLEVEL.NORMAL; // normal by default.

            }

            if (messageLevel <= moduleLevel)
            {

                UnityEngine.Debug.LogError(callerId + ": " + message + ", Line: " + line);

            }

        }

    }


}