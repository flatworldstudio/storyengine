
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StoryEngine.Samples.All
{

    public class DataHandler : MonoBehaviour
    {

        public DataController dataController;
        readonly string ID = "DataHandler: ";

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
            dataController.addTaskHandler(TaskHandler);

        }
              

        public bool TaskHandler(StoryTask task)
        {

            bool done = false;

            switch (task.Instruction)
            {

                case "startsimple":
                    LoadScene("Simple");
                    done = true;
                    break;

                case "startnetworkedserver":
                    LoadScene("NetworkedServer");
                    done = true;
                    break;

                case "startnetworkedclient":
                    LoadScene("NetworkedClient");
                    done = true;
                    break;

                case "startinterface2d":
                    LoadScene("Interface2d");
                    done = true;
                    break;

                case "startinterfaceplanes":
                    LoadScene("Interfaceplanes");
                    done = true;
                    break;

                case "startinterfaceplanes3d":
                    LoadScene("Interfaceplanes3d");
                    done = true;
                    break;

                default:
                    done = true;

                    break;
            }

            return done;

        }

        void LoadScene(string _name)
        {


            SceneManager.LoadScene(_name, LoadSceneMode.Single);
             
        }

        void Update()
        {
           
        }
    }
}

