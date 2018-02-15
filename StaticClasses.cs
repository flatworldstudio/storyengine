
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


#if NETWORKED
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
#endif

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

	public bool trigger=false;
	public  string label="";
	public  GameObject sender;

	public UserCallBack ()

	{
	}
		




}


public static class GENERAL
{
//	public static SCOPE SCOPE=SCOPE.LOCAL;

	public static AUTHORITY AUTHORITY=AUTHORITY.LOCAL;

	//GENERAL.pointerScreenScalar

	public static float pointerScreenScalar= -0.5f;
	public static float pointerRectScalar = 0.5f;



	public static int SIGNOFFS;
	public static Dictionary <string,StoryPoint> storyPoints;
	public static List <StoryPointer> ALLPOINTERS;
	public static List <StoryTask> ALLTASKS;
	public static STORYMODE STORYMODE;

	public static bool isPauzed=false;
	public static bool hasFocus=true;

	public static bool wasConnected = false;

	public static string broadcastServer,networkServer;

	public static string me = "General";

	public static StoryPoint getStoryPointByID (string pointID)
	{
		StoryPoint r;

		if (!storyPoints.TryGetValue (pointID, out r)) {
			Log.Warning ("Storypoint " + pointID + " not found.",me);
		}

		return r;

	}

	static void flagPointerOverflow ()
	{

		if (ALLPOINTERS.Count > 10) {
			Log.Warning ("Potential pointer overflow.",me);
		}

	}

	static void flagTaskOverflow ()
	{

		if (ALLTASKS.Count > 10) {
			Log.Warning ("Potential task overflow.",me);


			//			foreach (Task t in ALLTASKS) {
			//				Debug.Log (t.pointer.ID + " " + t.description);
			//
			//			}

		}

	}

	public static StoryPointer getPointerOnStoryline (string theStoryLine)
	{

		flagPointerOverflow ();

		StoryPointer r = null;

		foreach (StoryPointer p in GENERAL.ALLPOINTERS) {

			if (p.currentPoint.storyLineName == theStoryLine) {
				r = p;
				break;
			}

		}

		return r;
	}

	public static StoryPointer getPointer (string pointerUuid)
	{

		flagPointerOverflow ();

		for (int i = 0; i < ALLPOINTERS.Count; i++) {

			if (ALLPOINTERS [i].ID.Equals (pointerUuid)) {
				//				Debug.Log ("found storypoint with id " + storyPointerUid);
				return ALLPOINTERS [i];
			}

		}

		return null;

	}

	public static StoryTask getTask (string taskID)
	{

		flagTaskOverflow ();

		for (int t = 0; t < ALLTASKS.Count; t++) {

			if (ALLTASKS [t].ID == taskID) {

				return ALLTASKS [t];

			}

		}

		return null;

	}

	public static int CONNECTIONINDEX;

	static int NEWCONNECTION;

//	public static bool LOSTCONNECTION;

	public static void SETNEWCONNECTION (int value)
	{
		NEWCONNECTION = value;

	}

	public static int GETNEWCONNECTION ()
	{

		int r = NEWCONNECTION;
		NEWCONNECTION = -1;
		return r;

	}

}

public static class UUID
{
	static int uid { get; set; }

	public static string identity;

	public static string getGlobalID ()
	{

		uid++;

		return identity + uid.ToString ("x8");

	}

	public static string getID ()
	{

		uid++;

		return uid.ToString ("x8");

	}

	public static void setIdentity ()
	{

		string stamp = System.DateTime.UtcNow.ToString ("yyyyMMddhhmmss");

		Int64 num = Int64.Parse (stamp);
		identity = num.ToString ("x8");

	}
}
