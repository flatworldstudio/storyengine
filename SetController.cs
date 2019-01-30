using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StoryEngine
{

    public delegate bool SetTaskHandler(StoryTask theTask);

    /*!
* \brief
* Controls set (eg. scenery, content) operations.
* 
* Use addTaskHandler to attach your custom handler.
*/

    public class SetController : MonoBehaviour
    {

        GameObject StoryEngineObject;
        SetTaskHandler setTaskHandler;

        //AssitantDirector ad;
        bool handlerWarning = false;

        public List<StoryTask> taskList;

        string ID = "SetController";

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.

        void Log(string message)
        {
            StoryEngine.Log.Message(message, ID);
        }
        void Warning(string message)
        {
            StoryEngine.Log.Warning(message, ID);
        }
        void Error(string message)
        {
            StoryEngine.Log.Error(message, ID);
        }
        void Verbose(string message)
        {
            StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);
        }

        void Start()
        {
            Log("Starting.");

            taskList = new List<StoryTask>();

            if (AssitantDirector.Instance == null)
            {
                Error("No Assistant Director instance.");
            }
            else
            {
                AssitantDirector.Instance.newTasksEvent += newTasksHandler;
            }

            //StoryEngineObject = GameObject.Find("StoryEngineObject");

            //if (StoryEngineObject == null)
            //{

            //    Warning("StoryEngineObject not found.");

            //}
            //else
            //{

            //    ad = StoryEngineObject.GetComponent<AssitantDirector>();
            //    ad.newTasksEvent += new NewTasksEvent(newTasksHandler); // registrer for task events

            //}

        }

        public void addTaskHandler(SetTaskHandler theHandler)
        {
            setTaskHandler = theHandler;
            Log("Handler added.");
        }


        void Update()
        {

            int t = 0;

            while (t < taskList.Count)
            {

                StoryTask task = taskList[t];

                //			if (task.pointer.getStatus () == POINTERSTATUS.KILLED && task.description != "end") {

                if (!GENERAL.ALLTASKS.Exists(at => at == task))
                {

                    Log("Removing task:" + task.Instruction);

                    taskList.RemoveAt(t);

                }
                else
                {

                    if (setTaskHandler != null)
                    {

                        if (setTaskHandler(task))
                        {

                            task.signOff(ID);
                            taskList.RemoveAt(t);

                        }
                        else
                        {
                            t++;

                        }

                    }
                    else
                    {
                        task.signOff(ID);
                        taskList.RemoveAt(t);

                        if (!handlerWarning)
                        {
                            Warning("No handler registered.");
                            handlerWarning = true;
                        }

                        //if (!handlerWarning)
                        //{
                        //    Warning("No handler available, blocking task while waiting.");
                        //    handlerWarning = true;

                        //}

                        //t++;
                    }

                }

            }

        }

        void newTasksHandler(object sender, TaskArgs e)
        {
            addTasks(e.theTasks);

        }

        public void addTasks(List<StoryTask> theTasks)
        {
            taskList.AddRange(theTasks);
        }

    }
}



