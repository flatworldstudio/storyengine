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

        List<StoryPointer> pointerStack;
        public DIRECTORSTATUS status;
       static public Director Instance;

        string me = "Director";

        public Director()
        {

            GENERAL.ALLPOINTERS = new List<StoryPointer>();
            status = DIRECTORSTATUS.NOTREADY;
            Instance = this;
        }

        public void evaluatePointers()
        {

            // Create a stack of pointers for processing.

            pointerStack = new List<StoryPointer>();

            for (int p = GENERAL.ALLPOINTERS.Count - 1; p >= 0; p--)
            {

                StoryPointer sp = GENERAL.ALLPOINTERS[p];

                if (sp.getStatus() == POINTERSTATUS.KILLED)
                {

                    // if a pointer was killed, remove it now.

                    Log.Message("Removing pointer uuid: " + sp.ID, me);

                    GENERAL.ALLPOINTERS.RemoveAt(p);

                }

                if (sp.getStatus() == POINTERSTATUS.EVALUATE || sp.getStatus() == POINTERSTATUS.TASKUPDATED)
                {

                    // pointer needs evaluating. but we only do this if pointer is local OR if pointer is global and we are the server

                    if ((sp.scope == SCOPE.GLOBAL && GENERAL.AUTHORITY == AUTHORITY.GLOBAL) || (sp.scope == SCOPE.LOCAL))
                    {

                        pointerStack.Add(sp);

                    }

                }

            }

            if (pointerStack.Count > 0)
                Log.Message("Evaluating " + pointerStack.Count + " of " + GENERAL.ALLPOINTERS.Count + " storypointers.", me);

            while (pointerStack.Count > 0)
            {

                // Keep processing items on the stack untill empty.

                StoryPointer pointer;
                string targetPointerName, targetValue;
                StoryPointer newPointer, targetPointer;
                StoryPoint targetPoint;

                pointer = pointerStack[0];

                Log.Message("Evaluating pointer uid: " + pointer.ID + " on storyline " + pointer.currentPoint.storyLineName, me);

                pointer.loadPersistantData();

                switch (pointer.currentPoint.taskType)
                {

                    case TASKTYPE.ROUTING:

                        string type = pointer.currentPoint.task[0];

                        switch (type)
                        {

                            case "hold":

                                // Put this pointer on hold. Remove from stack.

                                Log.Message("Pausing pointer.", me);

                                pointer.setStatus(POINTERSTATUS.PAUSED);

                                pointerStack.RemoveAt(0);

                                break;

                            case "tell":

                                // Control another pointer. Finds a/the(!) pointer on the given storyline and moves it to the given storypoint, marking the pointer for evaluation.
                                // Progress this pointer, keeping it on the stack

                                targetPointerName = pointer.currentPoint.task[1];

                                targetValue = pointer.currentPoint.task[2];

                                targetPointer = GENERAL.getPointerOnStoryline(targetPointerName);

                                if (targetPointer != null)
                                {

                                    targetPoint = GENERAL.getStoryPointByID(targetValue);

                                    if (targetPoint != null)
                                    {

                                        targetPointer.currentPoint = targetPoint;

                                    }
                                    else
                                    {

                                        Log.Warning("Tell was unable to find the indicated storypoint.", me);

                                    }

                                    targetPointer.setStatus(POINTERSTATUS.EVALUATE);

                                    Log.Message("Telling pointer on storyline " + targetPointerName + " to move to point " + targetValue, me);

                                }
                                else
                                {

                                    Log.Warning("Tell was unable to find the indicated storypointer.", me);

                                }

                                moveToNextPoint(pointer);

                                break;

                            case "goto":

                                // Moves this pointer to another point anywhere in the script. Mark for evaluation, keep on stack.

                                targetValue = pointer.currentPoint.task[1];

                                targetPoint = GENERAL.getStoryPointByID(targetValue);

                                if (targetPoint != null)
                                {

                                    pointer.currentPoint = targetPoint;

                                    Log.Message("Go to point " + targetValue, me);

                                }
                                else
                                {

                                    Log.Warning("Goto point not found.", me);

                                }

                                pointer.setStatus(POINTERSTATUS.EVALUATE);

                                break;

                            case "start":

                                // Start a new pointer on the given storypoint.
                                // Create a new pointer, add it to the list of pointers and add it to the stack.
                                // Progress the current pointer, keeping it on the stack.

                                targetPointerName = pointer.currentPoint.task[1];

                                targetPoint = GENERAL.getStoryPointByID(targetPointerName);

                                if (GENERAL.getPointerOnStoryline(targetPointerName) == null)
                                {

                                    Log.Message("Starting new pointer for storypoint: " + targetPointerName, me);

                                    newPointer = new StoryPointer(targetPoint);

                                    pointerStack.Add(newPointer);

                                }
                                else
                                {

                                    Log.Message("Storyline already active for storypoint " + targetPointerName, me);
                                }

                                moveToNextPoint(pointer);

                                break;

                            case "stop":

                                // Stop another storypointer by storyline name, or all other storylines with 'all'.

                                targetPointerName = pointer.currentPoint.task[1];

                                if (targetPointerName == "all")
                                {

                                    foreach (StoryPointer stp in GENERAL.ALLPOINTERS)
                                    {

                                        if (stp != pointer)
                                        {

                                            Log.Message("Stopping pointer " + pointer.ID + " on " + stp.currentPoint.storyLineName, me);

                                            //								targetPointer.killPointerAndTask ();

                                            stp.killPointerOnly();

                                            if (GENERAL.ALLTASKS.Remove(stp.currentTask))
                                            {

                                                Log.Message("Removing task " + stp.currentTask.description, me);

                                            }
                                            else
                                            {

                                                Log.Warning("Failed removing task " + stp.currentTask.description, me);

                                            }


                                            //								GENERAL.ALLTASKS.Remove (stp.currentTask);

                                        }

                                    }

                                    // Remove all pointers from stack, re-adding the one we're on.

                                    pointerStack.Clear();
                                    pointerStack.Add(pointer);

                                }
                                else
                                {

                                    // Stop a single storypointer on given storyline.

                                    targetPointer = GENERAL.getPointerOnStoryline(targetPointerName);

                                    if (targetPointer != null)
                                    {

                                        Log.Message("Stopping pointer " + targetPointer.ID + " on " + targetPointer.currentPoint.storyLineName, me);

                                        pointerStack.Remove(targetPointer);

                                        //							targetPointer.killPointerAndTask ();

                                        targetPointer.killPointerOnly();

                                        //							GENERAL.removeTask (targetPointer.currentTask);



                                        if (GENERAL.ALLTASKS.Remove(targetPointer.currentTask))
                                        {

                                            Log.Message("Removing task " + targetPointer.currentTask.description, me);

                                        }
                                        else
                                        {

                                            Log.Warning("Failed removing task " + targetPointer.currentTask.description, me);

                                        }


                                    }
                                    else
                                    {

                                        Log.Message("No pointer found for " + targetPointerName, me);

                                    }

                                }

                                moveToNextPoint(pointer);

                                break;

                            default:

                                break;

                        }

                        break;


                    case TASKTYPE.END:

                        // Ends the storyline, kills the pointer.

                        checkForCallBack(pointer);

                        if (pointer.currentTask != null && pointer.currentTask.getStatus() != TASKSTATUS.COMPLETE)
                        {
                            Log.Warning("Encountered end of storyline, but current task didn't complete?", me);

                        }

                        pointer.killPointerOnly();



                        //				pointer.killPointerAndTask ();

                        //				targetPointer

                        pointerStack.RemoveAt(0);



                        break;

                    case TASKTYPE.BASIC:
                        //			case TASKTYPE.END:

                        if (pointer.getStatus() == POINTERSTATUS.EVALUATE)
                        {

                            // A normal task to be executed. Assistant director will generate task.

                            Log.Message("Task to be executed: " + pointer.currentPoint.task[0], me);

                            pointer.setStatus(POINTERSTATUS.NEWTASK);

                            pointerStack.RemoveAt(0);

                        }

                        if (pointer.getStatus() == POINTERSTATUS.TASKUPDATED)
                        {

                            // Something has happened in the task that we need to evaluate.

                            if (pointer.currentTask.getStatus() == TASKSTATUS.COMPLETE)
                            {

                                // Task was completed. Check if there's a callback before moving on.

                                checkForCallBack(pointer);

                                // Task was completed, progress to the next point.

                                Log.Message("task completed: " + pointer.currentTask.description, me);

                                pointer.setStatus(POINTERSTATUS.EVALUATE);

                                moveToNextPoint(pointer);

                            }

                            if (pointer.currentTask.getStatus() == TASKSTATUS.ACTIVE)
                            {

                                // See if there's a callback.

                                checkForCallBack(pointer);

                                // Return pointerstatus to paused and stop evaluating it for now.

                                //						Debug.LogWarning (me + "Pointerstatus says taskupdated, but taskstatus for task " + pointer.currentTask.description + " is active.");

                                pointer.setStatus(POINTERSTATUS.PAUSED);

                                pointerStack.RemoveAt(0);

                            }



                        }

                        break;

                    default:

                        // This shouldn't occur.

                        Log.Warning("Error: unkown storypoint type. ", me);

                        pointerStack.RemoveAt(0);

                        break;

                }

            }

        }

        bool checkForCallBack(StoryPointer pointer)
        {

            // checks and trigger callback on the current task for given pointer. does not touch the pointer itself.

            if (pointer.currentTask == null)
                return false;

            string callBackValue = pointer.currentTask.getCallBack();

            if (callBackValue == "")
                return false;

            pointer.currentTask.clearCallBack(); // clear value

            // A callback is equivalent to 'start name', launching a new storypointer on the given point.

            StoryPoint targetPoint = GENERAL.getStoryPointByID(callBackValue);

            if (targetPoint != null)
            {

                if (GENERAL.getPointerOnStoryline(callBackValue) == null)
                {

                    Log.Message("New callback storyline: " + callBackValue, me);

                    StoryPointer newStoryPointer = new StoryPointer(targetPoint);

                    newStoryPointer.scope = pointer.scope; // INHERIT SCOPE...

                    #if NETWORKED
                    newStoryPointer.modified = true;
                    #endif

                    pointerStack.Add(newStoryPointer);

                    newStoryPointer.persistantData = pointer.persistantData; // inherit data, note that data network distribution is via task only. AD will load value into task.

                }
                else
                {

                    Log.Message("Callback storyline already started: " + callBackValue, me);
                }

                return true;


            }
            else
            {

                return false;

            }


            //		} else {
            //
            //			return false;
            //		}

        }


        public void loadScript(string fileName)
        {
            Script theScript = new Script(fileName);

            while (!theScript.isReady)
            {

            }

            status = DIRECTORSTATUS.READY;

        }

        public void beginStoryLine(string beginName)
        {

            StoryPoint begin = GENERAL.getStoryPointByID(beginName);

            if (begin!=null)
            new StoryPointer(begin); // constructor adds pointer to GENERAL.allpointers

        }

        void moveToNextPoint(StoryPointer thePointer)
        {
            if (!thePointer.moveToNextPoint())
            {

                Log.Warning("Error: killing pointer ", me);
                thePointer.setStatus(POINTERSTATUS.KILLED);

                pointerStack.RemoveAt(0);

            }
        }

        float getValue(string[] instructions, string var)
        {
            string r = "0";
            Char delimiter = '=';
            foreach (string e in instructions)
            {

                string[] splitElement = e.Split(delimiter);
                if (splitElement[0] == var && splitElement.Length > 1)
                {
                    r = splitElement[1];
                }

            }

            return float.Parse(r);

        }

    }






}