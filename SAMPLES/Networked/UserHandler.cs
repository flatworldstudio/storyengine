
using UnityEngine;
using StoryEngine.UI;
using UnityEngine.SceneManagement;


namespace StoryEngine.Samples.Networked
{

    public class UserHandler : MonoBehaviour
    {

        public UserController userController;
        readonly string ID = "UserHandler: ";

        public Canvas UserCanvas;
        Controller Controller;
        Mapping MainMapping;
        Layout MainLayout;
        InterFace MainInterface;

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
                case "makeinterface":
                    // Create a controller
                    Controller = new Controller();

                    // Create a layout (can hold multiple planes and interfaces)
                    MainLayout = new Layout();

                    // Create an interface
                    MainInterface = new InterFace(UserCanvas.gameObject, "demo");

                    // Create a mapping and add it to the interface
                    MainMapping = new Mapping();
                    MainMapping.ux_tap_2d += Methods.tapButton2D;
                    MainInterface.AddMapping(MainMapping);

                    // Create an exit button and add it to the interface
                    Button button;
                    button = new Button("Exit");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startmenu");
                    MainInterface.addButton(button);

                    // Just using single plane for demo, add the interface to it 
                    MainLayout.AddInterface(MainInterface);

                    done = true;
                    break;

                case "interface":

                    // Update the interface(s) and get result.

                    UserCallBack result = Controller.updateUi(MainLayout);

                    if (result.trigger)
                    {
                        Log("User tapped " + result.sender + ", starting storyline " + result.label);
                        Director.Instance.NewStoryLine(result.label);
                    }

                    break;

                case "startallsamples":

                    SceneManager.LoadScene("AllSamples", LoadSceneMode.Single);
                    done = true;
                    break;


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

