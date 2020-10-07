using UnityEngine;
using System.Collections.Generic;


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

    /*!
   * \brief
   * Log messages supporting different log levels for different modules.
   * 
   * Default level can be set in inspector on the AssistantDirector
   * By setting a loglevel for a module, logging can be set to verbose, normal, warnings or errors.
   * An echo method can be added to errors, ie. for logging them to a server.  
   */

    public static class Log
    {

        static Echo errorEcho;

        static Dictionary<string, LOGLEVEL> moduleLogStatus;

        static void AddModule(string module, LOGLEVEL status)
        {

            Init();

            moduleLogStatus.Add(module, status);

        }

        static void Init()
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

            if (module == "") return;
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
        static LOGLEVEL GetDefaultLevel()
        {
            return AssitantDirector.Instance == null ? LOGLEVEL.NORMAL : AssitantDirector.Instance.DefaultLogLevel;
        }

        public static void Force(string message)
        {
            Debug.Log(message);
        }

        public static void Message(string message, string module = "Unkown", LOGLEVEL messageLevel = LOGLEVEL.NORMAL)
        {

#if !NOLOGGING
            Init();

            LOGLEVEL moduleLevel;

            if (!moduleLogStatus.TryGetValue(module, out moduleLevel))
                moduleLevel = GetDefaultLevel();

            if (messageLevel <= moduleLevel)
                Debug.Log(module + ": " + message);

#endif
        }

        public static void Warning(string message, string module = "Unkown")
        {
#if !NOLOGGING
            Init();

            LOGLEVEL messageLevel = LOGLEVEL.WARNINGS;

            LOGLEVEL moduleLevel;

            if (!moduleLogStatus.TryGetValue(module, out moduleLevel))
                moduleLevel = GetDefaultLevel();

            if (messageLevel <= moduleLevel)
            {
                Debug.LogWarning(module + ": " + message);
                DeusHandler.Instance?.AddLogLine(module + ": " + message);
            }


#endif

        }

        public static void Error(string message, string module = "Unkown")
        {
            Init();

            LOGLEVEL messageLevel = LOGLEVEL.ERRORS;

            LOGLEVEL moduleLevel;

            if (!moduleLogStatus.TryGetValue(module, out moduleLevel))
                moduleLevel = GetDefaultLevel();

            if (messageLevel <= moduleLevel)
            {
                Debug.LogError(module + ": " + message);
                DeusHandler.Instance?.AddLogLine(module + ": " + message);
                if (errorEcho != null)
                    errorEcho(message, module);

            }

        }

    }


}