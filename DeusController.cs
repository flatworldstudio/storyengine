using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace StoryEngine
{
    /*!
* \brief
* Controls superuser operations, including debugging and storyboarding.
* 
* Use debugon and debugoff in your script to visualise StoryPointer status.
*/

    public class DeusController : MonoBehaviour
    {
        string ID = "DeusController";
        public static DeusController Instance;

        public GameObject DeusPointers, PointerBlock;

        //public int Width;
         //GameObject StoryEngineObject;

        List<StoryTask> taskList;
        List<StoryPointer> pointerList;
        StoryPointer[] pointerPositions;

        public int PointerDisplayCols;
        public int PointerDisplayRows;

        //   public bool storyBoard;

        float scale = 1;
        int PointerdisplayBuffer = 24;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);


        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {

            Verbose("Starting...");

            taskList = new List<StoryTask>();
            pointerList = new List<StoryPointer>();

            if (PointerDisplayCols==0 || PointerDisplayRows == 0)
            {
                Warning("Set number of rows and columns for pointer display.");
                PointerDisplayCols = 1;
                PointerDisplayRows = 1;
            }
         
            PointerdisplayBuffer = PointerDisplayCols * PointerDisplayRows;
            pointerPositions = new StoryPointer[PointerdisplayBuffer];

            //		smoothMouseX = 0;
            //		smoothMouseY = 0;

            //       StoryEngineObject = GameObject.Find("StoryEngineObject");
            if (AssitantDirector.Instance == null)
            {
                Error("No Assistant Director instance.");
            }
            else
            {
                AssitantDirector.Instance.newTasksEvent += newTasksHandler;
            }


            if (DeusPointers == null)
            {
                Warning("DeusCanvas not found.");
            }
            else
            {

                DeusPointers.SetActive(false);

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

                    Log("Removing task:" + task.Instruction);

                    taskList.RemoveAt(t);

                }
                else
                {

                    switch (task.Instruction)
                    {

                        case "debugon":

                            DeusPointers.SetActive(true);

                            task.signOff(ID);
                            taskList.RemoveAt(t);
                            break;


                        case "debugoff":

                            DeusPointers.SetActive(false);
                            task.signOff(ID);
                            taskList.RemoveAt(t);
                            break;


                        case "toggledebug":
                        case "debugtoggle":

                            DeusPointers.SetActive(!DeusPointers.activeSelf);
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





            PositionPointerBlocks();

        }





        void updateTaskInfo(StoryPointer pointer)
        {

            StoryTask theTask = pointer.currentTask;

            if (theTask == null || theTask.Pointer.pointerObject==null)
                return;

            if (theTask.Instruction != "wait")
            {

                //string displayText;

                string displayText = theTask.scope == SCOPE.GLOBAL ? "G| " : "L| ";

                // if (theTask.scope == SCOPE.GLOBAL)
                //{
                //    displayText = "G | ";
                //}
                //else
                //{
                //    displayText = "L | ";

                //}

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
            if (PointerBlock == null || DeusPointers == null)
            {
                Warning("Can't make pointerblock, null reference.");
                return;
            }
           
            GameObject newPointerUi;

            newPointerUi = Instantiate(PointerBlock);
            newPointerUi.transform.SetParent(DeusPointers.transform,false);

            targetPointer.pointerObject = newPointerUi;
            targetPointer.pointerTextObject = newPointerUi.transform.Find("textObject").gameObject;

            targetPointer.deusText = newPointerUi.transform.Find("textObject/Text").GetComponent<Text>();
            targetPointer.deusTextSuper = newPointerUi.transform.Find("textObject/TextSuper").GetComponent<Text>();

            // find empty spot

            int p = 0;
            while (p < PointerdisplayBuffer && pointerPositions[p] != null  )
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
            /*
           int row = p / PointerDisplayCols;
            int col = p % PointerDisplayCols;

        //    float scale = 1f / PointerDisplayCols;

            //DeusPointers.GetComponent<RectTransform>().localScale= new Vector3(scale, scale, scale);

           targetPointer.pointerObject.GetComponent<RectTransform>().localPosition = new Vector3(col*Screen.width,-row*384,0);
    */
        }


        void PositionPointerBlocks()
        {

            scale = 1f / PointerDisplayCols;

            DeusPointers.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);

            if (PointerBlock == null || DeusPointers == null)
            {
                Warning("Can't make pointerblock, null reference.");
                return;
            }

            for (int p = 0; p < PointerdisplayBuffer; p++)
            {

                if (pointerPositions[p] != null)
                {
                    int row = p / PointerDisplayCols;
                    int col = p % PointerDisplayCols;
                    pointerPositions[p].pointerObject.GetComponent<RectTransform>().localPosition = new Vector3(col * Screen.width, -row * 384, 0);

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
                        Log("Progressing storyline" + pointerPositions[storyPointerIndex].currentPoint.StoryLine);

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