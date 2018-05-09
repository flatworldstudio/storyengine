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

        string me = "Director";

        public Director()
        {

            GENERAL.ALLPOINTERS = new List<StoryPointer>();
            status = DIRECTORSTATUS.NOTREADY;

        }

        public void evaluatePointers()
        {

            // Create a stack of pointers for processing.

            pointerStack = new List<StoryPointer>();

            for (int p = GENERAL.ALLPOINTERS.Count - 1; p >= 0; p--)
            {

                StoryPointer sp = GENERAL.ALLPOINTERS[p];

                if (sp.GetStatus() == POINTERSTATUS.KILLED)
                {

                    // if a pointer was killed, remove it now.

                    Log.Message("Removing pointer: " + sp.currentPoint.storyLineName);

                    GENERAL.ALLPOINTERS.RemoveAt(p);

                }

                if (sp.GetStatus() == POINTERSTATUS.EVALUATE || sp.GetStatus() == POINTERSTATUS.TASKUPDATED)
                {

                    // pointer needs evaluating. but we only do this if pointer is local OR if pointer is global and we are the server

                    if ((sp.scope == SCOPE.GLOBAL && GENERAL.AUTHORITY == AUTHORITY.GLOBAL) || (sp.scope == SCOPE.LOCAL))
                    {

                        pointerStack.Add(sp);

                    }

                }

            }

            if (pointerStack.Count > 0)
                Log.Message("Evaluating " + pointerStack.Count + " of " + GENERAL.ALLPOINTERS.Count + " storypointers.");

            while (pointerStack.Count > 0)
            {

                // Keep processing items on the stack untill empty.

                StoryPointer pointer;
                string targetPointerName, targetValue;
                StoryPointer newPointer, targetPointer;
                StoryPoint targetPoint;

                pointer = pointerStack[0];

                Log.Message("Evaluating pointer: " + pointer.currentPoint.storyLineName);

                pointer.LoadPersistantData();

                switch (pointer.currentPoint.taskType)
                {

                    case TASKTYPE.ROUTING:

                        string type = pointer.currentPoint.task[0];

                        switch (type)
                        {

                            case "hold":

                                // Put this pointer on hold. Remove from stack.

                                Log.Message("Pausing pointer.");

                                pointer.SetStatus(POINTERSTATUS.PAUSED);

                                pointerStack.RemoveAt(0);

                                break;

                            case "tell":

                                // Control another pointer. Finds a/the(!) pointer on the given storyline and moves it to the given storypoint, marking the pointer for evaluation.
                                // Progress this pointer, keeping it on the stack

                                targetPointerName = pointer.currentPoint.task[1];

                                targetValue = pointer.currentPoint.task[2];

                                targetPointer = GENERAL.GetPointerForStoryline(targetPointerName);

                                if (targetPointer != null)
                                {

                                    targetPoint = GENERAL.GetStoryPointByID(targetValue);

                                    if (targetPoint != null)
                                    {

                                        targetPointer.currentPoint = targetPoint;

                                    }
                                    else
                                    {

                                        Log.Warning("Tell was unable to find the indicated storypoint.");

                                    }

                                    targetPointer.SetStatus(POINTERSTATUS.EVALUATE);

                                    Log.Message("Telling pointer on storyline " + targetPointerName + " to move to point " + targetValue);

                                }
                                else
                                {

                                    Log.Warning("Tell was unable to find the indicated storypointer.");

                                }

                                moveToNextPoint(pointer);

                                break;

                            case "goto":

                                // Moves this pointer to another point anywhere in the script. Mark for evaluation, keep on stack.

                                targetValue = pointer.currentPoint.task[1];

                                targetPoint = GENERAL.GetStoryPointByID(targetValue);

                                if (targetPoint != null)
                                {

                                    pointer.currentPoint = targetPoint;

                                    Log.Message("Go to point " + targetValue);

                                }
                                else
                                {

                                    Log.Warning("Goto point not found.");

                                }

                                pointer.SetStatus(POINTERSTATUS.EVALUATE);

                                break;

                            case "start":

                                // Start a new pointer on the given storypoint.
                                // Create a new pointer, add it to the list of pointers and add it to the stack.
                                // Progress the current pointer, keeping it on the stack.

                                targetPointerName = pointer.currentPoint.task[1];

                                if (GENERAL.GetPointerForStoryline(targetPointerName) == null)
                                {

                                    Log.Message("Starting new pointer for storypoint: " + targetPointerName);

                                    newPointer = new StoryPointer(targetPointerName, pointer.scope);
                                    pointerStack.Add(newPointer);

                                }
                                else
                                {

                                    Log.Message("Storyline already active for storypoint " + targetPointerName);
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

                                            Log.Message("Stopping pointer " + stp.currentPoint.storyLineName);

                                            stp.Kill();

                                            if (GENERAL.ALLTASKS.Remove(stp.currentTask))
                                            {

                                                Log.Message("Removing task " + stp.currentTask.description);

                                            }
                                            else
                                            {

                                                Log.Warning("Failed removing task " + stp.currentTask.description);

                                            }

                                        }

                                    }

                                    // Remove all pointers from stack, re-adding the one we're on.

                                    pointerStack.Clear();
                                    pointerStack.Add(pointer);

                                }
                                else
                                {

                                    // Stop a single storypointer on given storyline.

                                    targetPointer = GENERAL.GetPointerForStoryline(targetPointerName);

                                    if (targetPointer != null)
                                    {

                                        Log.Message("Stopping pointer " + targetPointer.currentPoint.storyLineName);

                                        pointerStack.Remove(targetPointer);
                                        targetPointer.Kill();

                                        if (GENERAL.ALLTASKS.Remove(targetPointer.currentTask))
                                        {

                                            Log.Message("Removing task " + targetPointer.currentTask.description);

                                        }
                                        else
                                        {

                                            Log.Warning("Failed removing task " + targetPointer.currentTask.description);

                                        }


                                    }
                                    else
                                    {

                                        Log.Message("No pointer found for " + targetPointerName);

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
                            Log.Warning("Encountered end of storyline, but current task didn't complete?");

                        }

                        pointer.Kill();
                        pointerStack.RemoveAt(0);

                        break;

                    case TASKTYPE.BASIC:
                        //			case TASKTYPE.END:

                        if (pointer.GetStatus() == POINTERSTATUS.EVALUATE)
                        {

                            // A normal task to be executed. Assistant director will generate task.

                            Log.Message("Task to be executed: " + pointer.currentPoint.task[0]);

                            pointer.SetStatus(POINTERSTATUS.NEWTASK);

                            pointerStack.RemoveAt(0);

                        }

                        if (pointer.GetStatus() == POINTERSTATUS.TASKUPDATED)
                        {

                            // Something has happened in the task that we need to evaluate.

                            if (pointer.currentTask.getStatus() == TASKSTATUS.COMPLETE)
                            {

                                // Task was completed. Check if there's a callback before moving on.

                                checkForCallBack(pointer);

                                // Task was completed, progress to the next point.

                                Log.Message("task completed: " + pointer.currentTask.description);

                                pointer.SetStatus(POINTERSTATUS.EVALUATE);

                                moveToNextPoint(pointer);

                            }

                            if (pointer.currentTask.getStatus() == TASKSTATUS.ACTIVE)
                            {

                                // See if there's a callback.

                                checkForCallBack(pointer);

                                // Return pointerstatus to paused and stop evaluating it for now.

                                //						Debug.LogWarning (me + "Pointerstatus says taskupdated, but taskstatus for task " + pointer.currentTask.description + " is active.");

                                pointer.SetStatus(POINTERSTATUS.PAUSED);

                                pointerStack.RemoveAt(0);

                            }

                        }

                        break;

                    default:

                        // This shouldn't occur.

                        Log.Warning("Error: unkown storypoint type. ");

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

            if (GENERAL.GetPointerForStoryline(pointer.currentTask.getCallBack()) == null)
            {

                Log.Message("New callback storyline: " + callBackValue);

                StoryPointer newStoryPointer = new StoryPointer(callBackValue, pointer.scope);
                newStoryPointer.modified = true;
                pointerStack.Add(newStoryPointer);
                newStoryPointer.persistantData = pointer.persistantData; // inherit data, note that data network distribution is via task only. AD will load value into task.

            }
            else
            {

                Log.Message("Callback storyline already started: " + callBackValue);
            }

            return true;

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

            new StoryPointer(beginName, SCOPE.LOCAL); // constructor adds pointer to GENERAL.allpointers

        }

        void moveToNextPoint(StoryPointer thePointer)
        {
            if (!thePointer.ProgressToNextPoint())
            {

                Log.Warning("Error: killing pointer ");

                thePointer.SetStatus(POINTERSTATUS.KILLED);

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