﻿
using UnityEngine;
using System.Collections.Generic;

using System;




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

//	public static bool logToConsole = true;

	static Dictionary<string,LOGLEVEL> moduleLogStatus;

	public static void AddModule (string module, LOGLEVEL status)

	{

		Init ();

		moduleLogStatus.Add (module, status);

	}

	public static void Init ()
	{
		if (moduleLogStatus == null)
			moduleLogStatus = new Dictionary<string, LOGLEVEL> ();

	}

	public static void SetModuleLevel (string module, LOGLEVEL level)

	{

		LOGLEVEL current;

		Init ();

		if (moduleLogStatus.TryGetValue (module, out current)) {

			moduleLogStatus [module] = level;

		} else {

			AddModule (module, level);

		}

	}


	public static void Message (string message, string module = "Unkown", LOGLEVEL messageLevel=LOGLEVEL.NORMAL)

	{
		LOGLEVEL moduleLevel = LOGLEVEL.NORMAL; // normal by default.

		moduleLogStatus.TryGetValue (module, out moduleLevel);
				
		if (messageLevel>=moduleLevel) 
			Debug.Log (module + ": " + message);


	}

	public static void Warning (string message, string module = "Unkown")

	{
		LOGLEVEL messageLevel = LOGLEVEL.WARNINGS;

		LOGLEVEL moduleLevel = LOGLEVEL.NORMAL; // normal by default

		moduleLogStatus.TryGetValue (module, out moduleLevel);

		if (messageLevel>=moduleLevel) 
			Debug.LogWarning (module + ": " + message);
		


	}

	public static void Error (string message, string module = "Unkown")

	{
		LOGLEVEL messageLevel = LOGLEVEL.ERRORS;

		LOGLEVEL moduleLevel = LOGLEVEL.NORMAL; // normal by default

		moduleLogStatus.TryGetValue (module, out moduleLevel);

		if (messageLevel>=moduleLevel) 
			Debug.LogError (module + ": " + message);



	}



		
}


