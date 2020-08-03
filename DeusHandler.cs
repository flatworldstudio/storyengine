
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

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
            //  Debug.Log("creating item");

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



    }


    public class DeusHandler : MonoBehaviour
    {

        readonly string ID = "Deus";
    //    public LOGLEVEL LogLevel = LOGLEVEL.WARNINGS;

        public GameObject DebugCanvas, PointerBlock;

        public GameObject MessageCanvas;

        GameObject[] MessageCanvases;

        List<StoryTask> taskList;

        public Text FrameRate;
        public GameObject BufferStatusIn, BufferStatusOut;

        public Text Logs;
        //List<StoryPointer> pointerList;
        //List<StoryData> dataList;

        List<Item> itemList;

        public int PointerDisplayCols;
        public int PointerDisplayRows;

        public static DeusHandler Instance;
        //  int PointerdisplayBuffer = 24;

     

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        void Awake()
        {
            Instance = this;

        }

        void OnEnable()
        {
            Application.logMessageReceivedThreaded += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error)
            {
                AddLogLine("echo: "+logString);
            }
              
            
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

            if (DebugCanvas == null)
            {
                Warning("DeusCanvas not found.");
            }
            else
            {

                DebugCanvas.SetActive(false);

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

                case "pause1":

                    float timeOut1;

                    if (!task.GetFloatValue("timeOut", out  timeOut1))
                    {
                        task.SetFloatValue("timeOut",  Time.time + 1f);

                    }
                    else
                    {
                        if (Time.time > timeOut1)
                        {
                            done = true;
                        }

                    }
                    break;

                case "pause5":

                    float timeOut5;

                    if (!task.GetFloatValue("timeOut", out timeOut5))
                    {
                        task.SetFloatValue("timeOut", Time.time + 5f);

                    }
                    else
                    {
                        if (Time.time > timeOut5)
                        {
                            done = true;
                        }

                    }
                    break;




                case "debugon":

                    GENERAL.Debugging = true;
                    //DeusCanvas.SetActive(true);

                    done = true;
                    break;


                case "debugoff":
                    GENERAL.Debugging = false;
                    //DeusCanvas.SetActive(false);
                    done = true;
                    break;


                case "toggledebug":
                case "debugtoggle":
                    GENERAL.Debugging = !GENERAL.Debugging;

                    //DeusCanvas.SetActive(!DeusCanvas.activeSelf);
                    done = true;
                    break;

                default:
                    done = true;

                    break;
            }

            return done;

        }

        float MessageTimeStamp;

        public void AddLogLine (string logLine)
        {
            Logs.text = logLine + "\n" + Logs.text;
        }


        public static void DeusMessage(string message, float duration = 5f)
        {
            if (Instance != null) Instance.ShowDeusMessage(message, duration);
        }

        public void ShowDeusMessage(string message, float duration = 5f)
        {


            if (MessageCanvas == null) return;

            Log("Deus message: " + message);

            if (MessageCanvases == null || MessageCanvases.Length == 0)
            {
                // just populate with 8 for 8 potential displays.
                MessageCanvases = new GameObject[8];

                for (int d = 0; d < 8; d++)
                {
                    GameObject clone = Instantiate(this.MessageCanvas);
                    clone.SetActive(true);
                    clone.GetComponent<Canvas>().targetDisplay = d;
                    clone.GetComponentInChildren<Text>().text = message;
                    clone.transform.SetParent(this.gameObject.transform);
                    this.MessageCanvases[d] = clone;

                }
                MessageTimeStamp = Time.time + duration;
            }
            else
            {
                foreach (GameObject o in MessageCanvases) o.GetComponentInChildren<Text>().text = message;
            }


        }

        public void DeusMessageTimeOut()
        {
            // if anything to time out
            if (MessageTimeStamp > float.Epsilon && Time.time > MessageTimeStamp)
            {
                // time out and destroy canvases
                MessageTimeStamp = 0f;
                if (MessageCanvases != null)
                {
                    foreach (GameObject o in MessageCanvases) Destroy(o);
                }
                MessageCanvases = null;

            }
        }


        void updateTaskDisplays()
        {

            if (PointerBlock == null || DebugCanvas == null)
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
                    ni.displayObject.transform.SetParent(DebugCanvas.transform, false);

                    itemList.Add(ni);

                }

            }

            //   Log("datacount " + GENERAL.ALLDATA.Count);

            // Go over all data objects and plot them into our diplay

            /*
            for (int i = 0; i < GENERAL.ALLDATA.Count; i++)
            {

                StoryData data = GENERAL.ALLDATA[i];

                if (!itemList.Exists(x => x.StoryData == data))
                {
                    //    Log("addin gitem");
                    Item ni = new Item(data, PointerBlock);
                    ni.displayObject.transform.SetParent(DeusCanvas.transform, false);

                    itemList.Add(ni);

                }

            }
            */


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

//                item.displayObject.GetComponent<RectTransform>().localPosition = scale * new Vector3(PointerDisplayCols / 2f * -1024f + 512f + col * 1024f, PointerDisplayRows / 2f * 512f - 256f - row * 512f, 0);
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

                            //    string displayText = theTask.scope == SCOPE.GLOBAL ? "G| " : "L| ";

                            string displayText = theTask.Instruction;

                            // If a task has a value for "debug" we display it along with task description.

                            string debugText;

                            if (theTask.GetStringValue("debug", out debugText))
                            {

                                displayText = displayText + " | " + debugText;

                            }

                            item.deusText.text = displayText;
                            item.deusTextSuper.text = theTask.Pointer.currentPoint.StoryLine + " " + GENERAL.ALLPOINTERS.Count + (theTask.scope == SCOPE.GLOBAL ? " G" : " L");

                        }
                        else
                        {
                            item.deusTextSuper.text = theTask.Pointer.currentPoint.StoryLine;
                        }

                        break;

                    case Item.TYPE.DATA:

                        /*
                        item.deusText.text = item.StoryData.GetChangeLog();
                        item.deusTextSuper.text = "data: " + item.StoryData.ID + GENERAL.ALLDATA.Count;
                        */
                        break;
                    default:
                        break;
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
        //    StoryEngine.Log.SetModuleLevel(ID, LogLevel);
#endif


            DebugCanvas.SetActive(GENERAL.Debugging);

            if (MessageCanvases != null) foreach (GameObject o in MessageCanvases) o.SetActive(GENERAL.Debugging);

            DeusMessageTimeOut();


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
            SetIndicators();
            SetDebugCanvasSize();
        }

        //

            void SetDebugCanvasSize()
        {
            // Set the reference canvas size to the actual canvas size, because scale widht/height isn't always ideal

            Canvas canvas = DebugCanvas?.GetComponent<Canvas>();
            CanvasScaler canvasScaler = DebugCanvas?.GetComponent<CanvasScaler>();

            if (canvas!=null && canvasScaler != null)
            {

                int d = canvas.targetDisplay;
                int Horizontal = Display.displays[d].renderingWidth;
                int Vertical = Display.displays[d].renderingHeight;
                canvasScaler.referenceResolution = new Vector2(Horizontal, Vertical);

            }



        }

        void SetIndicators()
        {

            if (FrameRate != null)
            {
                FrameRate.text = "Framerate: " + (Mathf.Round(1f / Time.deltaTime));
            }

            if (BufferStatusIn != null && BufferStatusOut != null && NetworkHandler.Instance != null)
            {

                BufferStatusIn.SetActive(NetworkHandler.Instance.Connected);
                BufferStatusOut.SetActive(NetworkHandler.Instance.Connected);

                switch (AssitantDirector.BufferStatusIn)
                {
                    case 0:
                        BufferStatusIn.GetComponent<Image>().color = Color.grey;
                        break;
                    case 1:
                        BufferStatusIn.GetComponent<Image>().color = Color.green;
                        break;
                    default:
                        BufferStatusIn.GetComponent<Image>().color = Color.blue;
                        break;
                }
                switch (AssitantDirector.BufferStatusOut)
                {
                    case 0:
                        BufferStatusOut.GetComponent<Image>().color = Color.grey;
                        break;
                    case 1:
                        BufferStatusOut.GetComponent<Image>().color = Color.green;
                        break;
                    default:
                        BufferStatusOut.GetComponent<Image>().color = Color.blue;
                        break;
                }
            }
        }



    }
}

