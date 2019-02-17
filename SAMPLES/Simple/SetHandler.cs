
using UnityEngine;

namespace StoryEngine.Samples.Simple
{

    public class SetHandler : MonoBehaviour
    {

        public SetController setController;
        readonly string ID = "SetHandler: ";

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
            setController.addTaskHandler(TaskHandler);

        }


        float wait;

        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.Instruction)
            {

                case "task1":
                case "task2":
                case "task3":
                case "repeatingtask":
              
                    if (task.GetFloatValue("wait", out wait))
                        done |= Time.time > wait;
                    else
                    {
                        task.SetFloatValue("wait", Time.time + 3f);
                        Log("Executing task " + task.Instruction);
                    }
                    break;

                case "wait1":

                    if (task.GetFloatValue("wait", out wait))
                        done |= Time.time > wait;
                    else
                        task.SetFloatValue("wait", Time.time + 1f);
                    break;


                default:
                    done = true;

                    break;
            }

            return done;

        }

        void Update()
        {
            // 
        }
    }
}

