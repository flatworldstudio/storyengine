using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StoryEngine
{

    public delegate bool UserTaskHandler(StoryTask theTask);

    public class UserController : MonoBehaviour
    {

        GameObject StoryEngineObject;
        UserTaskHandler userTaskHandler;

        AssitantDirector ad;
        bool handlerWarning = false;

        public List<StoryTask> taskList;

        string ID = "UserController";

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

            StoryEngineObject = GameObject.Find("StoryEngineObject");

            if (StoryEngineObject == null)
            {

                Warning("StoryEngineObject not found.");

            }
            else
            {

                ad = StoryEngineObject.GetComponent<AssitantDirector>();
                ad.newTasksEvent += new NewTasksEvent(newTasksHandler); // registrer for task events

            }

        }

        public void addTaskHandler(UserTaskHandler theHandler)
        {
            userTaskHandler = theHandler;
            Log("Handler added");
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

                    Log("Removing task:" + task.description);

                    taskList.RemoveAt(t);

                }
                else
                {

                    if (userTaskHandler != null)
                    {

                        if (userTaskHandler(task))
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

                        if (!handlerWarning)
                        {
                            Debug.LogWarning(ID + "No handler available, blocking task while waiting.");
                            handlerWarning = true;
                           
                        }

                        t++;

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