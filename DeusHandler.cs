
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace StoryEngine
{
    public class Item
    {
        public enum TYPE
        {
            NONE,
            POINTER,
            DATA
        }

        TYPE __type;

        public TYPE Type
        {
            get
            {
                return __type;
            }
            // no setter
        }

        public StoryPointer StoryPointer
        {
            get
            {
                if (__type == TYPE.POINTER) return __sp;
                return null;
            }

            set
            {
                __sp = value;
                __type = TYPE.POINTER;
            }

        }

        public StoryData StoryData
        {
            get
            {
                if (__type == TYPE.DATA) return __sd;
                return null;
            }

            set
            {
                __sd = value;
                __type = TYPE.DATA;
            }

        }
        private StoryPointer __sp;
        public StoryData __sd;

        public GameObject displayObject;

        public Text deusText, deusTextSuper;

        public float TimeOut = 0;

        public Item(StoryPointer sp, GameObject prefab)
        {
            Debug.Log("creating item");

            __type = TYPE.POINTER;
            __sp = sp;
            //   __sd = null;
            //    TimeOut = 0f;
            //     remove = false;

            if (prefab != null)
            {
                displayObject = GameObject.Instantiate(prefab);
            }
            else
            {
                displayObject = null;
            }

            deusText = displayObject.transform.Find("textObject/Text").GetComponent<Text>();
            deusTextSuper = displayObject.transform.Find("textObject/TextSuper").GetComponent<Text>();



        }

        public Item(StoryData sd, GameObject prefab)
        {
            Debug.Log("creating item");

            __type = TYPE.DATA;
            __sd = sd;

            if (prefab != null)
            {
                displayObject = GameObject.Instantiate(prefab);
            }
            else
            {
                displayObject = null;
            }

            deusText = displayObject.transform.Find("textObject/Text").GetComponent<Text>();
            deusTextSuper = displayObject.transform.Find("textObject/TextSuper").GetComponent<Text>();



        }

        //public static Item EMPTY
        //{
        //    get
        //    {
        //        return new Item() { __type = TYPE.NONE };
        //    }
        //}






    }


    public class DeusHandler : MonoBehaviour
    {

        readonly string ID = "Deus";
        public LOGLEVEL LogLevel = LOGLEVEL.WARNINGS;

        public GameObject DeusCanvas, PointerBlock;

        List<StoryTask> taskList;



        //List<StoryPointer> pointerList;
        //List<StoryData> dataList;

        List<Item> itemList;

        public int PointerDisplayCols;
        public int PointerDisplayRows;


        //  int PointerdisplayBuffer = 24;



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

            //     pointerList = new List<StoryPointer>();
            //    dataList = new List<StoryData>();


            if (PointerDisplayCols == 0)
            {
                Warning("Set number of rows and columns for pointer display.");
                PointerDisplayCols = 2;
                //  PointerDisplayRows = 1;
            }

            itemList = new List<Item>();

            //     PointerdisplayBuffer = PointerDisplayCols * PointerDisplayRows;
            //  itemPositions = new Item[PointerdisplayBuffer];

            //    for (int i = 0; i < itemPositions.Length; i++) itemPositions[i] = Item.EMPTY;

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

            if (PointerBlock == null || DeusCanvas == null)
            {
                Warning("Set references for pointerblock and deuscanvas.");
                return;
            }

            // Go over all pointers and plot them into our diplay

            for (int i = 0; i < GENERAL.ALLPOINTERS.Count; i++)
            {

                StoryPointer pointer = GENERAL.ALLPOINTERS[i];

                if (!itemList.Exists(x => x.StoryPointer == pointer))
                {
                    Item ni = new Item(pointer, PointerBlock);
                    ni.displayObject.transform.SetParent(DeusCanvas.transform, false);

                    itemList.Add(ni);

                }

            }

         //   Log("datacount " + GENERAL.ALLDATA.Count);

            // Go over all data objects and plot them into our diplay
            
            for (int i = 0; i < GENERAL.ALLDATA.Count; i++)
            {

                StoryData data = GENERAL.ALLDATA[i];



                if (!itemList.Exists(x => x.StoryData == data))
                {
                   Log("addin gitem");
                   Item ni = new Item(data, PointerBlock);
                   ni.displayObject.transform.SetParent(DeusCanvas.transform, false);

                   itemList.Add(ni);

                }

            }
            
            // Go over all items backwards and remove any

            for (int i = itemList.Count - 1; i >= 0; i--)
            {

                if (itemList[i].TimeOut != 0 && itemList[i].TimeOut < Time.time)
                {
                    Destroy(itemList[i].displayObject);
                    itemList.RemoveAt(i);
                }


            }


            // Go over all display items and update them.

            for (int i = 0; i < itemList.Count; i++)
            {
                int row = i / PointerDisplayCols;
                int col = i % PointerDisplayCols;
                float scale = 1f / PointerDisplayCols;

                Item item = itemList[i];

                item.displayObject.GetComponent<RectTransform>().localPosition = scale * new Vector3(PointerDisplayCols / 2f * -1024f + 512f + col * 1024f, PointerDisplayRows / 2f * 512f - 256f - row * 512f, 0);
                item.displayObject.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);


                switch (item.Type)
                {
                    case Item.TYPE.POINTER:

                        if (item.TimeOut == 0 && !GENERAL.ALLPOINTERS.Contains(item.StoryPointer))
                        {
                            item.TimeOut = Time.time + 1f;
                        }

                        StoryTask theTask = item.StoryPointer.currentTask;

                        if (theTask == null || item.displayObject == null)
                            return;

                        if (theTask.Instruction != "wait" && theTask.Instruction != "end")
                        {

                            string displayText = theTask.scope == SCOPE.GLOBAL ? "G| " : "L| ";

                            displayText = displayText + theTask.Instruction + " | ";

                            // If a task has a value for "debug" we display it along with task description.

                            string debugText;

                            if (theTask.GetStringValue("debug", out debugText))
                            {

                                displayText = displayText + debugText;

                            }

                            item.deusText.text = displayText;
                            item.deusTextSuper.text = theTask.Pointer.currentPoint.StoryLine + " " + GENERAL.ALLPOINTERS.Count;

                        }
                        else
                        {
                            item.deusTextSuper.text = theTask.Pointer.currentPoint.StoryLine;
                        }

                        break;

                    case Item.TYPE.DATA:

                        item.deusText.text = "data";
                        item.deusTextSuper.text = "data objects" + GENERAL.ALLDATA.Count;

                        break;
                    default:
                        break;
                }




            }



            // Go over all display pointers and see if they're still alive.
            /*
            for (int i = pointerList.Count - 1; i >= 0; i--)
            {



                StoryPointer pointer = pointerList[i];

                if (!GENERAL.ALLPOINTERS.Contains(pointer))
                {

                    Verbose("Destroying ui for pointer for storyline " + pointer.currentPoint.StoryLine);

                    // update first. some pointers reach end in a single go - we want those to be aligned.

                    //				updateTaskDisplay (task);

                    pointerList.Remove(pointer);

                    itemList[pointer.position] = Item.EMPTY;

                    Destroy(pointer.pointerObject);


                }

            }
            */







        }

        /*
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
        */
        void createNewItemDisplay(Item item)
        {
            if (PointerBlock == null || DeusCanvas == null)
            {
                Warning("Can't make pointerblock, null reference.");
                return;
            }

            GameObject newPointerUi;

            newPointerUi = Instantiate(PointerBlock);
            newPointerUi.transform.SetParent(DeusCanvas.transform, false);

            item.displayObject = newPointerUi;
            //item.mainTextObject = newPointerUi.transform.Find("textObject").gameObject;

            //item.deusText = newPointerUi.transform.Find("textObject/Text").GetComponent<Text>();
            //item.deusTextSuper = newPointerUi.transform.Find("textObject/TextSuper").GetComponent<Text>();

            // find empty spot

            //int p = 0;
            //while (p < PointerdisplayBuffer && itemList[p].Type != Item.TYPE.NONE)
            //{
            //    p++;
            //}

            //if (p == PointerdisplayBuffer)
            //{
            //    Warning("Too many pointers to display.");
            //    p = 0;
            //}

            //itemList[p].SetPointer(targetPointer);
            //targetPointer.position = p;

            // Place it on the canvas

            //int row = p / PointerDisplayCols;
            //int col = p % PointerDisplayCols;
            //float scale = 1f / PointerDisplayCols;

            //targetPointer.pointerObject.GetComponent<RectTransform>().localPosition = scale * new Vector3(PointerDisplayCols / 2f * -1024f + 512f + col * 1024f, PointerDisplayRows / 2f * 512f - 256f - row * 512f, 0);
            //targetPointer.pointerObject.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);

        }



        /*
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

            //int p = 0;
            //while (p < PointerdisplayBuffer && itemList[p].Type != Item.TYPE.NONE)
            //{
            //    p++;
            //}

            //if (p == PointerdisplayBuffer)
            //{
            //    Warning("Too many pointers to display.");
            //    p = 0;
            //}

            ////itemList[p].SetPointer(targetPointer);
            //targetPointer.position = p;

            //// Place it on the canvas

            //int row = p / PointerDisplayCols;
            //int col = p % PointerDisplayCols;
            //float scale = 1f / PointerDisplayCols;

            //targetPointer.pointerObject.GetComponent<RectTransform>().localPosition = scale * new Vector3(PointerDisplayCols / 2f * -1024f + 512f + col * 1024f, PointerDisplayRows / 2f * 512f - 256f - row * 512f, 0);
            //targetPointer.pointerObject.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);

        }
        */


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



                if (itemList.Count > storyPointerIndex && storyPointerIndex != -1)
                {
                    if (itemList[storyPointerIndex].Type == Item.TYPE.POINTER)
                    {
                        Log("Progressing storyline" + itemList[storyPointerIndex].StoryPointer.currentPoint.StoryLine);

                        //					cc.progressPointer (pointerPositions [storyPointerIndex].uid);

                        itemList[storyPointerIndex].StoryPointer.currentTask.setStatus(TASKSTATUS.COMPLETE);

                        itemList[storyPointerIndex].StoryPointer.SetStatus(POINTERSTATUS.TASKUPDATED);


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

#if UNITY_EDITOR
            StoryEngine.Log.SetModuleLevel(ID, LogLevel);
#endif


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

