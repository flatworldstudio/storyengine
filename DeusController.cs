using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace StoryEngine
{
    public class DeusController : MonoBehaviour
    {

        public GameObject DeusCanvas, PointerBlock;
        public int Width;
         GameObject StoryEngineObject;
       AssitantDirector ad;

        string ID = "DeusController";

        List<StoryTask> taskList;
        List<StoryPointer> pointerList;
        StoryPointer[] pointerPositions;
        public bool storyBoard;

        int PointerdisplayBuffer = 24;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.

        void Log(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.NORMAL);
        }
        void Warning(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.WARNINGS);
        }
        void Error(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.ERRORS);
        }
        void Verbose(string message)
        {
            Logger.Output(message, ID, LOGLEVEL.VERBOSE);
        }

        void Start()
        {

            Log("Starting...");

            taskList = new List<StoryTask>();
            pointerList = new List<StoryPointer>();
            pointerPositions = new StoryPointer[PointerdisplayBuffer];

            //		smoothMouseX = 0;
            //		smoothMouseY = 0;

            //       StoryEngineObject = GameObject.Find("StoryEngineObject");
            StoryEngineObject = this.transform.gameObject;

            if (StoryEngineObject == null)
            {

                Warning("StoryEngineObject with central command script not found.");

            }
            else
            {
                ad = StoryEngineObject.GetComponent<AssitantDirector>();
                ad.newTasksEvent += new NewTasksEvent(newTasksHandler); // registrer for task events
            }

      //      DeusCanvas = GameObject.Find("DeusCanvas");

            if (DeusCanvas == null)
            {
                Warning("DeusCanvas not found.");
            }

     //       DeusCanvas = GameObject.Find("DeusCanvas");

            if (DeusCanvas == null)
            {
                Warning("DeusCanvas not found.");
            }
            else
            {
               DeusCanvas.SetActive(false);
            }

        //    PointerBlock = GameObject.Find("PointerBlock");


            if (PointerBlock == null)
            {
                Warning("PointerBlock not found.");
            }
            else
            {
                //			PointerBlock.SetActive (false);
            }


        }

        void newTasksHandler(object sender, TaskArgs e)
        {

            addTasks(e.theTasks);

            //		if (e.storyMode != 0) {
            //		
            //			storyMode = e.storyMode;
            //			Debug.Log (me+"storymode "+e.storyMode);
            //
            //		}

        }

        public void addTasks(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);
        }

        void handleTasks()
        {

            int t = 0;

            while (t < taskList.Count)
            {

                StoryTask task = taskList[t];

                //			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {
                //				if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {

                if (!GENERAL.ALLTASKS.Exists(at => at == task))
                {

                    Log("Removing task:" + task.description);

                    taskList.RemoveAt(t);

                }
                else
                {

                    switch (task.description)
                    {

                        case "debugon":
                            DeusCanvas.SetActive(true);

                            task.signOff(ID);
                            taskList.RemoveAt(t);
                            break;


                        case "debugoff":

                            DeusCanvas.SetActive(false);
                            task.signOff(ID);
                            taskList.RemoveAt(t);
                            break;


                        case "toggledebug":

                            DeusCanvas.SetActive(!DeusCanvas.activeSelf);
                            task.signOff(ID);
                            taskList.RemoveAt(t);
                            break;


                        /*
                        case "end":

                            // after finishing the end task, we mark the pointer as killed, so it gets removed.

                            task.pointer.setStatus (POINTERSTATUS.KILLED);



        //					Log.Message ("Destroying ui for pointer for storyline " + task.pointer.currentPoint.storyLineName);
        //
        //					// update first. some pointers reach end in a single go - we want those to be aligned.
        //
        //					updateTaskDisplay (task);
        //
        //					pointerList.Remove (task.pointer);
        //
        //					pointerPositions [task.pointer.position] = null;
        //
        //					Destroy (task.pointer.pointerObject);
        //
        //					// after finishing the end task, we mark the pointer as killed, so it gets removed.
        //
        //					task.pointer.setStatus (POINTERSTATUS.KILLED);

        //					updateTaskDisplay (task);


                            task.signOff (me);
                            taskList.RemoveAt (t);
                            break;

        */

                        default:

                            //					updateTaskDisplay (task);

                            task.signOff(ID);
                            taskList.RemoveAt(t);
                            break;

                    }

                }

            }

        }




        void updateTaskDisplays()
        {

            // Go over all pointers and plot them into our diplay

            foreach (StoryPointer pointer in GENERAL.ALLPOINTERS)
            {

                if (!pointerList.Contains(pointer))
                {

                    Log("Pointer is new, added display for storyline " + pointer.currentPoint.storyLineName);

                    pointerList.Add(pointer);

                    createNewPointerUi(pointer);

                }

                updateTaskInfo(pointer);

            }

            // Go over all display pointers and see if they're still alive.

            for (int i = pointerList.Count - 1; i >= 0; i--)
            {

                //		foreach (StoryPointer pointer in pointerList) {

                StoryPointer pointer = pointerList[i];


                if (!GENERAL.ALLPOINTERS.Contains(pointer))
                {

                    Log("Destroying ui for pointer for storyline " + pointer.currentPoint.storyLineName);

                    // update first. some pointers reach end in a single go - we want those to be aligned.

                    //				updateTaskDisplay (task);

                    pointerList.Remove(pointer);

                    pointerPositions[pointer.position] = null;

                    Destroy(pointer.pointerObject);


                }

            }







        }





        void updateTaskInfo(StoryPointer pointer)
        {


            StoryTask theTask = pointer.currentTask;

            if (theTask == null)
                return;


            if (theTask.description != "wait")
            {

                string displayText;

                // If global

                if (theTask.scope == SCOPE.GLOBAL)
                {
                    displayText = "G | ";
                }
                else
                {
                    displayText = "L | ";

                }

                displayText = displayText + theTask.description + " | ";

                // If a task has a value for "debug" we display it along with task description.

                string debugText;

                if (theTask.GetStringValue("debug", out debugText))
                {

                    displayText = displayText + debugText;

                }

                theTask.pointer.deusText.text = displayText;
                theTask.pointer.deusTextSuper.text = theTask.pointer.currentPoint.storyLineName + " " + GENERAL.ALLPOINTERS.Count;

            }
            else
            {

                theTask.pointer.deusTextSuper.text = theTask.pointer.currentPoint.storyLineName;

            }

        }

        void createNewPointerUi(StoryPointer targetPointer)
        {
            GameObject newPointerUi;

            newPointerUi = Instantiate(PointerBlock);
            newPointerUi.transform.SetParent(DeusCanvas.transform,false);

            targetPointer.pointerObject = newPointerUi;
            targetPointer.pointerTextObject = newPointerUi.transform.Find("textObject").gameObject;

            targetPointer.deusText = newPointerUi.transform.Find("textObject/Text").GetComponent<Text>();
            targetPointer.deusTextSuper = newPointerUi.transform.Find("textObject/TextSuper").GetComponent<Text>();

            // find empty spot

            int p = 0;
            while (pointerPositions[p] != null)
            {
                p++;
                if (p==PointerdisplayBuffer)
                {
                    Error("To many pointers for display, crashing now.");

                }
            }

            //		Debug.Log ("found point position: " + p);

            pointerPositions[p] = targetPointer;
            targetPointer.position = p;

            int maxPosition = 0;

            for (int i = 0; i < PointerdisplayBuffer; i++)
            {
                if (pointerPositions[i] != null)
                {
                    maxPosition = i;
                }
            }
            maxPosition++;

          //  maxPosition = Mathf.Max(maxPosition, 4);

            maxPosition = Mathf.Clamp(maxPosition, 4, 6);


            float xSize = (float) Width / maxPosition;
            float xAnchor = xSize / 2f;
            float scalar = xSize / 320f;
            float ySize = xSize / 320f * 160f;


            for (int i = 0; i < PointerdisplayBuffer; i++)
            {
                if (pointerPositions[i] != null)
                {
                    //pointerPositions[i].pointerObject.GetComponent<RectTransform>().localPosition = new Vector3((-640f + xAnchor + i * xSize) * screenCorrection, yAnchor, 0);
                    //pointerPositions[i].pointerObject.GetComponent<RectTransform>().localScale = new Vector3(scalar * screenCorrection, scalar * screenCorrection, 1);
                    int row = i / 6;
                    int col = i % 6;

                    pointerPositions[i].pointerObject.GetComponent<RectTransform>().localPosition = new Vector3((-Width/2 + xAnchor + col * xSize) , row*ySize, 0);
                    pointerPositions[i].pointerObject.GetComponent<RectTransform>().localScale = new Vector3(scalar , scalar, scalar);
                }
            }

        }

        void handleUi()
        {

            string inputString = Input.inputString;
            if (inputString.Length > 0)
            {

                int storyPointerIndex = -1;

                switch (inputString)
                {

                    case "1":
                        storyPointerIndex = 0;
                        break;
                    case "2":
                        storyPointerIndex = 1;
                        break;
                    case "3":
                        storyPointerIndex = 2;
                        break;
                    case "4":
                        storyPointerIndex = 3;
                        break;
                    case "5":
                        storyPointerIndex = 4;
                        break;
                    case "6":
                        storyPointerIndex = 5;
                        break;

                    case "7":
                        storyPointerIndex = 6;
                        break;
                    case "8":
                        storyPointerIndex = 7;
                        break;
                    case "9":
                        storyPointerIndex = 8;
                        break;


                    default:
                        break;
                }



                if (pointerPositions.Length > storyPointerIndex && storyPointerIndex != -1)
                {
                    if (pointerPositions[storyPointerIndex] != null)
                    {
                        Log("Progressing storyline" + pointerPositions[storyPointerIndex].currentPoint.storyLineName);

                        //					cc.progressPointer (pointerPositions [storyPointerIndex].uid);

                        pointerPositions[storyPointerIndex].currentTask.setStatus(TASKSTATUS.COMPLETE);

                        pointerPositions[storyPointerIndex].SetStatus(POINTERSTATUS.TASKUPDATED);


                    }
                }

            }


        }


        void Update()
        {

            if (Input.GetKey("escape"))
            {
                Warning("Quitting application.");
                Application.Quit();
            }


            handleTasks();
            //		handleUi ();
            updateTaskDisplays();

        }
    }
}