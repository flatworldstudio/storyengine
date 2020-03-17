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

        public GameObject DeusCanvas, PointerBlock;

        //public int Width;
         //GameObject StoryEngineObject;

        List<StoryTask> taskList;
        List<StoryPointer> pointerList;
        StoryPointer[] pointerPositions;

        public int PointerDisplayCols;
        public int PointerDisplayRows;

        //   public bool storyBoard;

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
                        case "debugtoggle":

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



        


        


        void Update()
        {

            if (Input.GetKey("escape"))
            {
                Warning("Quitting application.");
                Application.Quit();
            }


            handleTasks();
          

        }
    }
}