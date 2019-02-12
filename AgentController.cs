﻿using UnityEngine;
using System.Collections.Generic;

namespace StoryEngine
{

    /*!
* \brief
* Controls agents, objects that have behaviour.
* 
* Use addTaskHandler to attach your custom handler.
*/

    public class AgentController : MonoBehaviour
    {
        string ID = "AgentController";
        TaskHandler setTaskHandler;
        List<StoryTask> taskList;

        public static AgentController Instance;

        bool handlerWarning = false;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            Verbose("Starting.");

            taskList = new List<StoryTask>();

            if (AssitantDirector.Instance == null)
            {
                Error("No Assistant Director instance.");
            }
            else
            {
                AssitantDirector.Instance.newTasksEvent += newTasksHandler;
            }

        }

        public void addTaskHandler(TaskHandler theHandler)
        {
            setTaskHandler = theHandler;
            Verbose("Handler added.");
        }


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



