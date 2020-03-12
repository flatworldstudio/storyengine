
using UnityEngine;

using System.Collections.Generic;
using UnityEngine.UI;

namespace StoryEngine
{

    public class DeusHandler : MonoBehaviour
    {

        readonly string ID = "Deus";


        public GameObject DeusCanvas, PointerBlock;


        List<StoryTask> taskList;
        List<StoryPointer> pointerList;
        StoryPointer[] pointerPositions;

        public int PointerDisplayCols;
        public int PointerDisplayRows;


        int PointerdisplayBuffer = 24;



        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        void Awake()
        {


        }


        void Start()
        {

            taskList = new List<StoryTask>();

            // Add ourselves to the ad's task distribution event.
            if (AssitantDirector.Instance != null)
            {
                AssitantDirector.Instance.AddNewTasksListenerUnity(newTasksHandlerUnity);

            }

            pointerList = new List<StoryPointer>();

            if (PointerDisplayCols == 0 || PointerDisplayRows == 0)
            {
                Warning("Set number of rows and columns for pointer display.");
                PointerDisplayCols = 1;
                PointerDisplayRows = 1;
            }

            PointerdisplayBuffer = PointerDisplayCols * PointerDisplayRows;
            pointerPositions = new StoryPointer[PointerdisplayBuffer];

            if (DeusCanvas == null)
            {
                Warning("DeusCanvas not found.");
            }
            else
            {

                DeusCanvas.SetActive(false);

            }

            if (PointerBlock == null)
            {
                Warning("PointerBlock not found.");
            }
            else
            {
                //
            }


        }


        public bool HandleTask(StoryTask task)
        {

            bool done = false;

            switch (task.Instruction)
            {

                case "debugon":

                    DeusCanvas.SetActive(true);

                    done = true;
                    break;


                case "debugoff":

                    DeusCanvas.SetActive(false);
                    done = true;
                    break;


                case "toggledebug":
                case "debugtoggle":

                    DeusCanvas.SetActive(!DeusCanvas.activeSelf);
                    done = true;
                    break;

                default:
                    done = true;

                    break;
            }

            return done;

        }


        void updateTaskDisplays()
        {

            // Go over all pointers and plot them into our diplay

            foreach (StoryPointer pointer in GENERAL.ALLPOINTERS)
            {

                if (!pointerList.Contains(pointer))
                {

                    Verbose("Pointer is new, added display for storyline " + pointer.currentPoint.StoryLine);

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

                    Verbose("Destroying ui for pointer for storyline " + pointer.currentPoint.StoryLine);

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

            if (theTask == null || theTask.Pointer.pointerObject == null)
                return;

            if (theTask.Instruction != "wait")
            {

           

                string displayText = theTask.scope == SCOPE.GLOBAL ? "G| " : "L| ";

            

                displayText = displayText + theTask.Instruction + " | ";

                // If a task has a value for "debug" we display it along with task description.

                string debugText;

                if (theTask.GetStringValue("debug", out debugText))
                {

                    displayText = displayText + debugText;

                }

                theTask.Pointer.deusText.text = displayText;
                theTask.Pointer.deusTextSuper.text = theTask.Pointer.currentPoint.StoryLine + " " + GENERAL.ALLPOINTERS.Count;

            }
            else
            {

                theTask.Pointer.deusTextSuper.text = theTask.Pointer.currentPoint.StoryLine;

            }

        }

        void createNewPointerUi(StoryPointer targetPointer)
        {
            if (PointerBlock == null || DeusCanvas == null)
            {
                Warning("Can't make pointerblock, null reference.");
                return;
            }

            GameObject newPointerUi;

            newPointerUi = Instantiate(PointerBlock);
            newPointerUi.transform.SetParent(DeusCanvas.transform, false);

            targetPointer.pointerObject = newPointerUi;
            targetPointer.pointerTextObject = newPointerUi.transform.Find("textObject").gameObject;

            targetPointer.deusText = newPointerUi.transform.Find("textObject/Text").GetComponent<Text>();
            targetPointer.deusTextSuper = newPointerUi.transform.Find("textObject/TextSuper").GetComponent<Text>();

            // find empty spot

            int p = 0;
            while (p < PointerdisplayBuffer && pointerPositions[p] != null)
            {
                p++;
            }

            if (p == PointerdisplayBuffer)
            {
                Warning("Too many pointers to display.");
                p = 0;
            }

            pointerPositions[p] = targetPointer;
            targetPointer.position = p;

            // Place it on the canvas

            int row = p / PointerDisplayCols;
            int col = p % PointerDisplayCols;
            float scale = 1f / PointerDisplayCols;

            targetPointer.pointerObject.GetComponent<RectTransform>().localPosition = scale * new Vector3(PointerDisplayCols / 2f * -1024f + 512f + col * 1024f, PointerDisplayRows / 2f * 512f - 256f - row * 512f, 0);
            targetPointer.pointerObject.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);

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
                        Log("Progressing storyline" + pointerPositions[storyPointerIndex].currentPoint.StoryLine);

                        //					cc.progressPointer (pointerPositions [storyPointerIndex].uid);

                        pointerPositions[storyPointerIndex].currentTask.setStatus(TASKSTATUS.COMPLETE);

                        pointerPositions[storyPointerIndex].SetStatus(POINTERSTATUS.TASKUPDATED);


                    }
                }

            }


        }


        void newTasksHandlerUnity(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);

            foreach (StoryTask task in theTasks)
            {
                task.signOn(ID);
            }

        }


        void Update()


        {


            if (Input.GetKey("escape"))
            {
                Warning("Quitting application.");
                Application.Quit();
            }

    
            int t = 0;

            while (t < taskList.Count)
            {

                StoryTask task = taskList[t];

                if (!GENERAL.ALLTASKS.Exists(at => at == task))
                {

                    Log("Removing task:" + task.Instruction);

                    taskList.RemoveAt(t);

                }
                else
                {

                    if (HandleTask(task))
                    {

                        task.signOff(ID);
                        taskList.RemoveAt(t);

                    }
                    else
                    {
                        t++;

                    }

                }

            }


            updateTaskDisplays();

        }

        //



    }
}

