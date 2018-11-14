using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StoryEngine
{

    public delegate bool SetTaskHandler(StoryTask theTask);

    public class SetController : MonoBehaviour
    {

        GameObject StoryEngineObject;
        SetTaskHandler setTaskHandler;
        AssitantDirector ad;
        bool handlerWarning = false;
        public List<StoryTask> taskList;
        string me = "SetController";


        void Start()
        {
            Log.Message("Starting.", me);

            taskList = new List<StoryTask>();

            StoryEngineObject = GameObject.Find("StoryEngineObject");

            if (StoryEngineObject == null)
            {

                Log.Warning("StoryEngineObject not found.", me);

            }
            else
            {

                ad = StoryEngineObject.GetComponent<AssitantDirector>();
                ad.newTasksEvent += new NewTasksEvent(newTasksHandler); // registrer for task events

            }
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

                    Log.Message("Removing task:" + task.description, me);

                    taskList.RemoveAt(t);

                }
                else
                {

                    if (setTaskHandler != null)
                    {

                        if (setTaskHandler(task))
                        {

                            task.signOff(me);
                            taskList.RemoveAt(t);

                        }
                        else
                        {
                            t++;

                        }

                    }
                    else
                    {

                        task.signOff(me);
                        taskList.RemoveAt(t);

                        if (!handlerWarning)
                        {

                            Log.Warning("No handler available, blocking task while waiting.", me);

                            handlerWarning = true;

                        }

                    }

                }

            }

        }


        public void addTaskHandler(SetTaskHandler _handler)
        {
            setTaskHandler = _handler;
            Log.Message("Handler added.", me);
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