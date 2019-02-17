
using UnityEngine;
using StoryEngine.UI;

namespace StoryEngine.Samples.All
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
                    MainMapping.ux_single_2d += Methods.Drag2D;
                    MainMapping.ux_tap_2d += Methods.tapButton2D;

                    MainInterface.AddMapping(MainMapping);

                    // Create locked buttons and add them to the interface
                    Button button;

                    button = new Button("Simple");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startsimple");
                    MainInterface.addButton(button);

                    button = new Button("NetworkedServer");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startnetworkedserver");
                    MainInterface.addButton(button);

                    button = new Button("NetworkedClient");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startnetworkedclient");
                    MainInterface.addButton(button);

                    button = new Button("Interface2d");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startinterface2d");
                    MainInterface.addButton(button);

                    button = new Button("Interfaceplanes");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startinterfaceplanes");
                    MainInterface.addButton(button);

                    button = new Button("Interfaceplanes3d");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startinterfaceplanes3d");
                    MainInterface.addButton(button);

                    // Just add the interface directly to the layout, it will assign it to the root plane.
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

