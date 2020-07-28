
using UnityEngine;
using System.Collections.Generic;

namespace StoryEngine
{

    public class Handler : MonoBehaviour
    {


        readonly string ID = "Handler";
        List<StoryTask> taskList;

        #region LOG
        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);
        #endregion

        #region AWAKESTART
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


        }
        #endregion

        #region TASKHANDLING
        public bool HandleTask(StoryTask task)
        {

            bool done = false;

            switch (task.Instruction)
            {

                case "helloworld":
                    Log("Hello world");
                    done = true;
                    break;
                case "unknowntask":

                    break;

                default:
                    done = true;

                    break;
            }

            return done;

        }
        #endregion

        #region ENGINE
        void newTasksHandlerUnity(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);
            foreach (StoryTask task in theTasks)
            {
                task.signOn(ID);
            }
        }
        #endregion  

        #region UPDATE
        void Update()
        {

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
        }
        #endregion

    }
}

