using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if NETWORKED

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

#endif

namespace StoryEngine
{

	#if NETWORKED

	public class PointerUpdate : MessageBase
	{

		public string pointerUuid;
		public string storyPoint;
		public bool killed;
		//	public int pointerStatus;

	}
	#endif


	public enum POINTERSTATUS
	{
		EVALUATE,
		NEWTASK,
		TASKUPDATED,
		KILLED,
		PAUSED
	}

	public class StoryPointer
	{
		public string ID;
		public StoryPoint currentPoint;

		public SCOPE scope;

		POINTERSTATUS status;

		public StoryTask currentTask;

		//	public StoryTask persistantData; // can hold generic data which will be passed onto new task.... WIP
		public string persistantData;

		public	Text deusText;
		public	Text deusTextSuper;
		public	GameObject pointerObject, pointerTextObject;
		public int position;

		#if NETWORKED
		public bool modified;

	#endif

		string me = "Storypointer";

		public StoryPointer ()
		{


		}


		public StoryPointer (StoryPoint startingPoint)
		{

			currentPoint = startingPoint;
			status = POINTERSTATUS.EVALUATE;
			ID = UUID.getGlobalID ();
			scope = SCOPE.LOCAL;
			GENERAL.ALLPOINTERS.Add (this);

//		persistantData = new StoryTask ();

		}

		public StoryPointer (StoryPoint startingPoint, string setID)
		{

			currentPoint = startingPoint;
			status = POINTERSTATUS.EVALUATE;
			ID = setID;
			scope = SCOPE.GLOBAL;
			GENERAL.ALLPOINTERS.Add (this);

//		persistantData = new StoryTask ();

		}

        #if NETWORKED
		public PointerUpdate getUpdateMessage ()
		{

			PointerUpdate r = new PointerUpdate ();
			r.pointerUuid = ID;
			r.storyPoint = currentPoint.ID;

			if (status == POINTERSTATUS.KILLED) {

				r.killed = true;

			} else {

				r.killed = false;

			}

//		r.pointerStatus = (int)status;

			return r;

		}

        #endif

		public void loadPersistantData ()
		{

			// load carry over value from task into pointer.

			if (currentTask != null)
				currentTask.getStringValue ("persistantData", out persistantData);

		}

		//	public void applyUpdateMessage (string pointerUuid, string pointName, int pointerStatus)
		//	{
		//
		//		// get the story point
		//
		//		StoryPoint point = GENERAL.getStoryPointByID (pointName);
		//
		//		// see if the pointer exists, update or create new
		//
		//		StoryPointer sp = GENERAL.getPointer (pointerUuid);
		//
		//		if (sp == null) {
		//
		//			sp = new StoryPointer (point, pointerUuid);
		//
		//			Log.Message ("Created a new (remotely owned) pointer with ID: " + sp.ID);
		//
		//		}
		//
		//		sp.currentPoint = point;
		//
		//		//		sp.setStatus ((POINTERSTATUS)pointerStatus);
		//
		//		//		sp.setStatus (POINTERSTATUS.PAUSED); // overrule the status sent over the network, since global pointers aren't updated locally.
		//
		//	}
		public void killPointerOnly ()
		{

			setStatus (POINTERSTATUS.KILLED);

		}


		/*

	public void killPointerAndTask ()
	{

		// Ending the pointer. Set status to killed so it gets cleaned up.

		setStatus (POINTERSTATUS.KILLED);

//		modified = true;

		// If there was an active task, mark as completed so it gets cleaned up.

		if (currentTask!=null)
			currentTask.setStatus (TASKSTATUS.COMPLETE);

	}
	*/

		public POINTERSTATUS getStatus ()
		{
			return status;
		}





		public void setStatus (POINTERSTATUS theStatus)
		{
			//		if (scope == SCOPE.GLOBAL && GENERAL.SCOPE!=SCOPE.GLOBAL) {
			//
			//			Debug.LogWarning (me + "Setting status on a global pointer without global scope.");
			//
			//		}

			if (status != POINTERSTATUS.KILLED) {

				// The end task sets pointerstatus to killed, then calls this method when the end task is complete. If it was killed, keep it killed for removal. 

				//					setStatus (POINTERSTATUS.TASKUPDATED);

				status = theStatus;
			}

			#if NETWORKED

			//		if (GENERAL.SCOPE == SCOPE.GLOBAL && scope==SCOPE.GLOBAL) {

			// Only mark as changed (triggering network distribution) if our scope is global

			modified = true;


			//		}
			#endif

		}

		//	public void taskStatusChanged ()
		//
		//	{
		//
		//		if (status != POINTERSTATUS.KILLED) {
		//
		//			// The end task sets pointerstatus to killed, then calls this method when the end task is complete. If it was killed, keep it killed for removal.
		//
		//			setStatus (POINTERSTATUS.TASKUPDATED);
		//
		//		}
		//	}

		public Boolean moveToNextPoint ()
		{

			Boolean r = false;

			if (currentPoint.getNextStoryPoint () == null) {

				Log.Message ("No next point", me);

			} else {

				currentPoint = currentPoint.getNextStoryPoint ();

				r = true;
			}

			return r;

		}

	}

}

