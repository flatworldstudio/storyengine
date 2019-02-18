
using UnityEngine;
using StoryEngine.UI;

namespace StoryEngine.Samples.Networked
{

    public class UserHandler : MonoBehaviour
    {

        public UserController userController;
        readonly string ID = "UserHandler: ";

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
            userController.addTaskHandler(TaskHandler);

        }


        float wait;

        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.Instruction)
            {
                case "pingpong":

                    // Every device tries to switch the value every 5 seconds.

                    if (Time.time > wait)
                    {
                        wait = Time.time + 4f;

                        string pingpong;
                        task.GetStringValue("debug", out pingpong);

                        switch (pingpong)
                        {
                            case "ping":
                                task.SetStringValue("debug", "pong");
                                Log("pong");
                                break;
                            default:
                                task.SetStringValue("debug", "ping");
                                Log("ping");
                                break;

                        }

                    }

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

