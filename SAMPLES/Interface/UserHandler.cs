
using UnityEngine;
using StoryEngine.UI;
using Plane = StoryEngine.UI.Plane;
using UnityEngine.SceneManagement;


namespace StoryEngine.Samples.Interface
{

    public class UserHandler : MonoBehaviour
    {

        public UserController userController;
        readonly string ID = "UserHandler: ";

        public Canvas UserCanvas;
        Controller Controller;
        Mapping MainMapping;
        Layout MainLayout;
        InterFace MainInterface, UpperInterface, LowerInterface;


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
                case "makeinterface2d":

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

                    // Create a free moving button and add it to the interface
                    Button button;
                    button = new Button("Freemoving");
                    button.AddCallback("freemovingcallback");
                    MainInterface.addButton(button);

                    // Create a free moving button and add it to the interface
                  button = new Button("Locked");
                    button.AddCallback("lockedcallback");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    MainInterface.addButton(button);
                            

                    // Create a constrained button and add it to the interface. Coordinates are local.

                    Constraint slideConstraint = new Constraint()
                    {
                        hardClamp = true,
                        hardClampMin = new Vector2(-250f, 350f),
                        hardClampMax = new Vector2(250f, 350f),
                        edgeSprings = true,
                        edgeSpringMin = new Vector2(-200f, 350f),
                        edgeSpringMax = new Vector2(200f, 350f)
                    };

                    button = new Button("Slide");
                    button.AddConstraint(slideConstraint);
                    MainInterface.addButton(button);

                    // Create a button with spring positions and add it to the interface.

                    Constraint springConstraint = new Constraint()
                    {
                        hardClamp = true,
                        hardClampMin = new Vector2(-250f, 250f),
                        hardClampMax = new Vector2(250f, 250f),
                        springs = true,
                        springPositions = new Vector2[]
                        {
                        new Vector2(-200f, 250f),
                        new Vector2(-100f, 250f),
                        new Vector2(0f, 250f),
                        new Vector2(100f, 250f),
                        new Vector2(200f, 250f)
                        }

                    };

                    button = new Button("Springs");
                    button.AddConstraint(springConstraint);
                    MainInterface.addButton(button);

                    // Create two buttons with the same drag target, so they work as a group.

                    button = new Button("Option1", GameObject.Find("MenuFree"));
                    MainInterface.addButton(button);
                    button = new Button("Option2", GameObject.Find("MenuFree"));
                    MainInterface.addButton(button);

                    // Create a button with orthogonal dragging (so either horizontal or vertical) and add it to the interface.

                    Constraint verticalConstraint = new Constraint()
                    {
                        hardClamp = true,
                        hardClampMin = new Vector2(0f, -200f),
                        hardClampMax = new Vector2(0f, 200f)
                    };
                    Constraint horizontalConstraint = new Constraint()
                    {
                        hardClamp = true,
                        hardClampMin = new Vector2(-200f, -350f),
                        hardClampMax = new Vector2(200f, -350f)
                    };


                    button = new Button("Ortho");
                    button.AddOrthoConstraints(GameObject.Find("Layer"), horizontalConstraint, GameObject.Find("Sublayer"), verticalConstraint);
                    //button.AddConstraint(circleConstraint);
                    MainInterface.addButton(button);

                    // Create a button with circular constraint  and add it to the interface.
                    // Works from 0,0 local position

                    Constraint circleConstraint = new Constraint()
                    {
                        radiusClamp = true,
                        radiusClampMin = 100f,
                        radiusClampMax = 100f

                    };

                    button = new Button("Circle");
                    button.AddConstraint(circleConstraint);
                    MainInterface.addButton(button);

                    // Create a free moving button and add it to the interface
                   button = new Button("Exit");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startmenu");
                    MainInterface.addButton(button);

                    // Just using single plane for demo, add the interface to it 
                    MainLayout.AddInterface(MainInterface);

                    done = true;

                    break;

                case "makeinterfaceplanes":


                    // Create a controller
                    Controller = new Controller();

                    // Create a layout (can hold multiple planes and interfaces)
                    MainLayout = new Layout();

                    // Create a plane
                    Plane UpperPlane = new Plane(GameObject.Find("UpperPlane"));
                    Plane LowerPlane = new Plane(GameObject.Find("LowerPlane"));

                    // Create an interface
                    UpperInterface = new InterFace(UserCanvas.gameObject, "upper");
                    LowerInterface = new InterFace(UserCanvas.gameObject, "lower");

                    // Create a mapping and add it to the interface
                    MainMapping = new Mapping();
                    MainMapping.ux_single_2d += Methods.Drag2D;
                    MainMapping.ux_tap_2d += Methods.tapButton2D;

                    // Add together.
                                       
                    UpperInterface.AddMapping(MainMapping);
                    UpperPlane.AddInterface(UpperInterface);
                    LowerInterface.AddMapping(MainMapping);
                    LowerPlane.AddInterface(LowerInterface);

                    // Create buttons

                    button = new Button("Button01");
                    UpperInterface.addButton(button);
                    button = new Button("Button02");
                    LowerInterface.addButton(button);

                    // Create an exit button and add it to the interface
                    button = new Button("Exit");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startmenu");
                    UpperInterface.addButton(button);

                    // Add to layout.

                    MainLayout.AddPlane(UpperPlane);
                    MainLayout.AddPlane(LowerPlane);

                    done = true;
                    break;

                case "makeinterfaceplanes3d":

                    // Create a controller
                    Controller = new Controller();

                    // Create a layout (can hold multiple planes and interfaces)
                    MainLayout = new Layout();

                    // Create a plane
                    Plane UpperPlane3d = new Plane(GameObject.Find("UpperPlane"));
                    Plane LowerPlane3d = new Plane(GameObject.Find("LowerPlane"));

                    // Create an interface
                    UpperInterface = new InterFace(UserCanvas.gameObject, "upper");
                    LowerInterface = new InterFace(UserCanvas.gameObject, "lower");

                    // Create a mapping and add it to the interface
                    MainMapping = new Mapping();
                    MainMapping.ux_single_none += Methods.OrbitCamera;
                    MainMapping.ux_double_none += Methods.LateralCamera;
                    MainMapping.ux_double_none += Methods.LongitudinalCamera;
                    MainMapping.ux_single_2d += Methods.Drag2D;
                    MainMapping.ux_tap_2d += Methods.tapButton2D;
                                       
                    // Create an orbit cam with a pitch constraint.
                    Constraint orbitConstraint = new Constraint()
                    {
                        pitchClamp = true,
                        pitchClampMin = 15f,
                        pitchClampMax = 85f

                    };

                    UiCam3D uppercam = new UiCam3D(GameObject.Find("CameraUpper"));
                    uppercam.AddContraint(orbitConstraint);
                  
                    UiCam3D lowercam = new UiCam3D(GameObject.Find("CameraLower"));
                    lowercam.AddContraint(orbitConstraint);

                    // Create an exit button and add it to the interface
                    button = new Button("Exit");
                    button.AddConstraint(Constraint.LockInPlace(button));
                    button.AddCallback("startmenu");
                    UpperInterface.addButton(button);

                    // Add together.

                    UpperInterface.AddUiCam3D(uppercam);
                    LowerInterface.AddUiCam3D(lowercam);

                    UpperInterface.AddMapping(MainMapping);
                    LowerInterface.AddMapping(MainMapping);

                    UpperPlane3d.AddInterface(UpperInterface);
                 LowerPlane3d.AddInterface(LowerInterface);

                    // Add to layout

                    MainLayout.AddPlane(UpperPlane3d);
                    MainLayout.AddPlane(LowerPlane3d);

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

