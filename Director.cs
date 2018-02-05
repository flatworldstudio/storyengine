using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

#if NETWORKED

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

#endif

public enum DIRECTORSTATUS
{
	NOTREADY,
	READY,
	ACTIVE,
	PASSIVE,
	PAUSED
	//	ASSISTANT
}

public class Director
{

	List <StoryPointer> pointerStack;
	public DIRECTORSTATUS status;
	string me = "Director: ";

	public Director ()
	{

		GENERAL.ALLPOINTERS = new List <StoryPointer> ();
		status = DIRECTORSTATUS.NOTREADY;

	}

	public void evaluatePointers ()
	{

		// Create a stack of pointers for processing.

		pointerStack = new List <StoryPointer> ();

		for (int p = GENERAL.ALLPOINTERS.Count - 1; p >= 0; p--) {
				
			StoryPointer sp = GENERAL.ALLPOINTERS [p];

			if (sp.getStatus () == POINTERSTATUS.KILLED) {
					
				// if a pointer was killed, remove it now.

				Debug.Log (me + "Removing pointer uuid: " + sp.ID);

				GENERAL.ALLPOINTERS.RemoveAt (p);

			}

			if (sp.getStatus () == POINTERSTATUS.EVALUATE || sp.getStatus () == POINTERSTATUS.TASKUPDATED) {

				// pointer needs evaluating. but we only do this if pointer is local OR if pointer is global and we are the server


				if ((sp.scope == SCOPE.GLOBAL && GENERAL.AUTHORITY == AUTHORITY.GLOBAL) || (sp.scope == SCOPE.LOCAL)) {

					pointerStack.Add (sp);

				}

	


			}

		}

		if (pointerStack.Count > 0)
			Debug.Log (me + "Evaluating " + pointerStack.Count + " of " + GENERAL.ALLPOINTERS.Count + " storypointers.");
		
		while (pointerStack.Count > 0) {

			// Keep processing items on the stack untill empty.
								
			StoryPointer pointer;
			string targetPointerName, targetValue;
			StoryPointer newPointer, targetPointer;
			StoryPoint targetPoint;

			pointer = pointerStack [0];

			Debug.Log (me + "Evaluating pointer uid: " + pointer.ID + " on storyline " + pointer.currentPoint.storyLineName);

			switch (pointer.currentPoint.taskType) {

			case TASKTYPE.ROUTING:

				string type = pointer.currentPoint.task [0];

				switch (type) {

				case "hold":

					// Put this pointer on hold. Remove from stack.

					Debug.Log (me + "Pausing pointer.");

					pointer.setStatus (POINTERSTATUS.PAUSED);

					pointerStack.RemoveAt (0);

					break;

				case "tell":
						
					// Control another pointer. Finds a/the(!) pointer on the given storyline and moves it to the given storypoint, marking the pointer for evaluation.
					// Progress this pointer, keeping it on the stack

					targetPointerName = pointer.currentPoint.task [1];

					targetValue = pointer.currentPoint.task [2];

					targetPointer = GENERAL.getPointerOnStoryline (targetPointerName);

					if (targetPointer != null) {
							
						targetPoint = GENERAL.getStoryPointByID (targetValue);

						if (targetPoint != null) {
							
							targetPointer.currentPoint = targetPoint;

						} else {

							Debug.LogWarning (me + "Tell was unable to find the indicated storypoint.");

						}

						targetPointer.setStatus (POINTERSTATUS.EVALUATE);

						Debug.Log (me + "Telling pointer on storyline " + targetPointerName + " to move to point " + targetValue);

					} else {
							
						Debug.LogWarning (me + "Tell was unable to find the indicated storypointer.");

					}

					moveToNextPoint (pointer);

					break;

				case "goto":
						
					// Moves this pointer to another point anywhere in the script. Mark for evaluation, keep on stack.

					targetValue = pointer.currentPoint.task [1];

					targetPoint = GENERAL.getStoryPointByID (targetValue);

					if (targetPoint != null) {

						pointer.currentPoint = targetPoint;

						Debug.Log (me + "Go to point " + targetValue);

					} else {
							
						Debug.LogWarning (me + "Goto point not found.");

					}

					pointer.setStatus (POINTERSTATUS.EVALUATE);

					break;

				case "start":
						
					// Start a new pointer on the given storypoint.
					// Create a new pointer, add it to the list of pointers and add it to the stack.
					// Progress the current pointer, keeping it on the stack.

					targetPointerName = pointer.currentPoint.task [1];

					targetPoint = GENERAL.getStoryPointByID (targetPointerName);

					if (GENERAL.getPointerOnStoryline (targetPointerName) == null) {
														
						Debug.Log (me + "Starting new pointer for storypoint: " + targetPointerName);

						newPointer = new StoryPointer (targetPoint);

						pointerStack.Add (newPointer); 

					} else {

						Debug.Log (me + "Storyline already active for storypoint " + targetPointerName);
					}

					moveToNextPoint (pointer);

					break;

				case "stop":
						
					// Stop another storypointer by storyline name, or all other storylines with 'all'.
													
					targetPointerName = pointer.currentPoint.task [1];

					if (targetPointerName == "all") {

						foreach (StoryPointer stp in GENERAL.ALLPOINTERS) {

							if (stp != pointer) {
									
								Debug.Log (me + "Stopping pointer " + pointer.ID + " on " + stp.currentPoint.storyLineName);

//								targetPointer.killPointerAndTask ();

								stp.killPointerOnly ();

								if (GENERAL.ALLTASKS.Remove (stp.currentTask)) {

									Debug.Log (me + "Removing task " + stp.currentTask.description);

								} else {

									Debug.LogWarning (me + "Failed removing task " + stp.currentTask.description);

								}


//								GENERAL.ALLTASKS.Remove (stp.currentTask);

							}

						}

						// Remove all pointers from stack, re-adding the one we're on.

						pointerStack.Clear ();
						pointerStack.Add (pointer);

					} else {

						// Stop a single storypointer on given storyline.

						targetPointer = GENERAL.getPointerOnStoryline (targetPointerName);

						if (targetPointer != null) {
												
							Debug.Log (me + "Stopping pointer " + targetPointer.ID + " on " + targetPointer.currentPoint.storyLineName);

							pointerStack.Remove (targetPointer);

//							targetPointer.killPointerAndTask ();

							targetPointer.killPointerOnly ();

//							GENERAL.removeTask (targetPointer.currentTask);



							if (GENERAL.ALLTASKS.Remove (targetPointer.currentTask)) {
								
								Debug.Log (me + "Removing task " + targetPointer.currentTask.description);

							} else {
								
								Debug.LogWarning (me + "Failed removing task " + targetPointer.currentTask.description);

							}


						} else {

							Debug.Log (me + "No pointer found for " + targetPointerName);
							
						}

					}
										
					moveToNextPoint (pointer);

					break;

				default:
					
					break;

				}

				break;


			case TASKTYPE.END:

				// Ends the storyline, kills the pointer.

				checkForCallBack (pointer);

				if (pointer.currentTask.getStatus () != TASKSTATUS.COMPLETE) {
					Debug.LogWarning (me + "Encountered end of storyline, but current task didn't complete?");

				}

								pointer.killPointerOnly ();



//				pointer.killPointerAndTask ();

//				targetPointer

				pointerStack.RemoveAt (0);



				break;

			case TASKTYPE.BASIC:
//			case TASKTYPE.END:
									
				if (pointer.getStatus () == POINTERSTATUS.EVALUATE) {

					// A normal task to be executed. Assistant director will generate task.

					Debug.Log (me + "Task to be executed: " + pointer.currentPoint.task [0]);

					pointer.setStatus (POINTERSTATUS.NEWTASK);

					pointerStack.RemoveAt (0);

				}

				if (pointer.getStatus () == POINTERSTATUS.TASKUPDATED) {

					// Something has happened in the task that we need to evaluate.
						
					if (pointer.currentTask.getStatus () == TASKSTATUS.COMPLETE) {

						// Task was completed. Check if there's a callback before moving on.

						checkForCallBack (pointer);

						// Task was completed, progress to the next point.

						Debug.Log (me + "task completed: " + pointer.currentTask.description);

						pointer.setStatus (POINTERSTATUS.EVALUATE);

						moveToNextPoint (pointer);

					}

					if (pointer.currentTask.getStatus () == TASKSTATUS.ACTIVE) {

						// See if there's a callback.

						checkForCallBack (pointer);

						// Return pointerstatus to paused and stop evaluating it for now.

//						Debug.LogWarning (me + "Pointerstatus says taskupdated, but taskstatus for task " + pointer.currentTask.description + " is active.");

						pointer.setStatus (POINTERSTATUS.PAUSED);

						pointerStack.RemoveAt (0);

					}



				}

				break;

			default:

				// This shouldn't occur.

				Debug.LogWarning (me + "Error: unkown storypoint type. ");

				pointerStack.RemoveAt (0);

				break;

			}

		} 

	}

	bool checkForCallBack (StoryPointer pointer){

		// checks and trigger callback on the current task for given pointer. does not touch the pointer itself.


		if (pointer.currentTask == null )
			return false;

		string callBackValue = pointer.currentTask.getCallBack ();


		if (callBackValue == "")
			return false;




//		string callBackValue = pointer.currentTask.getCallBack ();

//		if (callBackValue != "") {

			pointer.currentTask.clearCallBack (); // clear value

			// A callback is equivalent to 'start name', launching a new storypointer on the given point.

			StoryPoint targetPoint = GENERAL.getStoryPointByID (callBackValue);

			if (GENERAL.getPointerOnStoryline (pointer.currentTask.getCallBack ()) == null) {

				Debug.Log (me + "New callback storyline: " + callBackValue);

				StoryPointer newStoryPointer = new StoryPointer (targetPoint);

				newStoryPointer.scope = pointer.scope; // INHERIT SCOPE...
				newStoryPointer.modified=true;

				pointerStack.Add (newStoryPointer);

//				#if NETWORKED
//
//				if (pointer.scope==SCOPE.GLOBAL){
//					
//					// if pointer was global, new pointer should be too
//
//					newStoryPointer.scope=SCOPE.GLOBAL;
//					newStoryPointer.hasChanged=true;
//
//					Debug.Log (me + "Callback: new pointer also set to global");
//
//
//				}
//
//				#endif





			} else {

				Debug.Log (me + "Callback storyline already started: " + callBackValue);
			}

			return true;

//		} else {
//
//			return false;
//		}

	}


	public void loadScript (string fileName)
	{
		Script theScript = new Script (fileName);

		while (!theScript.isReady) {

		}

		status = DIRECTORSTATUS.READY;

	}

	public void beginStoryLine (string beginName)
	{

		StoryPointer newStoryPointer = new StoryPointer (GENERAL.getStoryPointByID (beginName));

	}

	void moveToNextPoint (StoryPointer thePointer)
	{
		if (!thePointer.moveToNextPoint ()) {
			
			Debug.Log (me + "Error: killing pointer ");
			thePointer.setStatus (POINTERSTATUS.KILLED);

			pointerStack.RemoveAt (0);

		}
	}

	float getValue (string[] instructions, string var)
	{
		string r = "0";
		Char delimiter = '=';
		foreach (string e in instructions) {

			string[] splitElement = e.Split (delimiter);
			if (splitElement [0] == var && splitElement.Length > 1) {
				r = splitElement [1];
			}

		}

		return float.Parse (r);

	}

}






