
using StoryEngine;
using UnityEngine;

public class DataHandlerExample : MonoBehaviour
{
    
    // Copy this as a template.

    public DataController dataController;
    string me = "Data handler: ";


    void Awake()
    {

        // StoryEngine core.

        Log.SetModuleLevel("AssistantDirector", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("Director", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("Script", LOGLEVEL.WARNINGS);

        // StoryEngine Controllers

        Log.SetModuleLevel("DeusController", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("UserController", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("SetController", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("UiController", LOGLEVEL.WARNINGS);     

        // StoryEngine Data Objects
       
        Log.SetModuleLevel("StoryPointer", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("StoryTask", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("TaskUpdate", LOGLEVEL.WARNINGS);

        // Application Modules

        Log.SetModuleLevel("DataHandler", LOGLEVEL.NORMAL);

        #if NETWORKED
        // SET NETWORK VARS
              
        Log.SetModuleLevel("Network manager", LOGLEVEL.WARNINGS);
        Log.SetModuleLevel("Networkbroadcast", LOGLEVEL.WARNINGS);
        GENERAL.connectionKey = "key";

        #endif
    }


    void Start()
    {

        dataController.addTaskHandler(TaskHandler);

    }



    public bool TaskHandler(StoryTask task)
    {

        bool done = false;

        switch (task.description)
        {

            case "task":

                Log.Message("Executing task",me);

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


    }



}
