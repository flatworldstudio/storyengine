using UnityEngine;
using System.Collections.Generic;
using System;

namespace StoryEngine
{
    public enum LOGLEVEL
    {

        ERRORS,
        WARNINGS,
        NORMAL,
        VERBOSE

    }

    public delegate void Echo(string message, string module);

    public static class Log
    {

        static Echo errorEcho;

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

        public static void AddErrorEcho(Echo _echo)
        {
            errorEcho = _echo;
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


        public static void Message(string message, string module = "Unkown", LOGLEVEL messageLevel = LOGLEVEL.NORMAL)
        {

#if LOGGING
            LOGLEVEL moduleLevel = LOGLEVEL.NORMAL; // normal by default.

			moduleLogStatus.TryGetValue (module, out moduleLevel);
				
			if (messageLevel <= moduleLevel)
				Debug.Log (module + ": " + message);

#endif
        }

        public static void Warning(string message, string module = "Unkown")
        {
#if LOGGING

            LOGLEVEL messageLevel = LOGLEVEL.WARNINGS;

            LOGLEVEL moduleLevel = LOGLEVEL.NORMAL; // normal by default

            moduleLogStatus.TryGetValue(module, out moduleLevel);

            if (messageLevel <= moduleLevel)
                Debug.LogWarning(module + ": " + message);

#endif

        }

        public static void Error(string message, string module = "Unkown")
        {
            LOGLEVEL messageLevel = LOGLEVEL.ERRORS;

            LOGLEVEL moduleLevel = LOGLEVEL.NORMAL; // normal by default

            moduleLogStatus.TryGetValue(module, out moduleLevel);

            if (messageLevel <= moduleLevel)
            {
                Debug.LogError(module + ": " + message);
                if (errorEcho != null)
                    errorEcho(message, module);

            }

        }

    }


}