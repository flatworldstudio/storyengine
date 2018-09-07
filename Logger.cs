using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace StoryEngine
{

    public static class Logger
    {

        static Dictionary<string, LOGLEVEL> Modules;

        static void CheckDictionary()
        {
            if (Modules == null)
                Modules = new Dictionary<string, LOGLEVEL>();

        }

        public static void AddModule(string module, LOGLEVEL status)
        {

            CheckDictionary();
            Modules.Add(module, status);

        }

        public static void SetLogLevel(string module, LOGLEVEL level)

        {

            LOGLEVEL current;

            CheckDictionary();

            if (Modules.TryGetValue(module, out current))
                Modules[module] = level;
            else
                AddModule(module, level);

        }

        public static void Output(string message, string callerId = "Unkown", LOGLEVEL messageLevel = LOGLEVEL.NORMAL)

        {
            // Use this to supress all log output.

            #if DISABLELOG
            return;
            #endif

            // If in editor, retrieve line number.

            string line="";

            #if UNITY_EDITOR

            StackFrame callStack = new StackFrame(2, true);

            Char[] delimiter = { '/', '.', '\\' };
          //  string[] caller = callStack.GetFileName().Split(delimiter);
           // string callerId = caller[caller.Length - 2];

             line = "" + callStack.GetFileLineNumber();

            #endif


            LOGLEVEL moduleLevel;

            if (!Modules.TryGetValue(callerId, out moduleLevel))
                moduleLevel = LOGLEVEL.NORMAL;

            if (messageLevel <= moduleLevel)
            {

                switch (messageLevel)
                {

                    case LOGLEVEL.ERRORS:
                        UnityEngine.Debug.LogError(callerId + ": " + message + ((line!="") ? " at line: "+line : ""));
                        break;

                    case LOGLEVEL.WARNINGS:
                        UnityEngine.Debug.LogWarning(callerId + ": " +message+ ((line!="") ? " at line: "+line : ""));
                        break;

                    default:
                        UnityEngine.Debug.Log(callerId + ": " +message+ ((line!="") ? " at line: "+line : ""));
                        break;

                }
            }
        }

    }


    public enum LOGLEVEL
    {

        OFF,
        ERRORS,
        WARNINGS,
        NORMAL,
        VERBOSE

    }

}