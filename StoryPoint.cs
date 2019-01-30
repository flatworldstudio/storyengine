

namespace StoryEngine
{
    /*!
* \brief
* Holds information about a single point in a storyline.
* 
*/

    public class StoryPoint
    {
        public string ID;
        public string StoryLine;
        public string[] Instructions;

        public TASKTYPE taskType;
        StoryPoint nextStoryPoint;

        //public StoryPoint (string myName)
        //{
        //  ID = myName;
        //  storyLineName = "...";

        //  //if (ID.Equals ("end")) {

        //  //  task = new string[] { "end" };
        //  //  taskType = TASKTYPE.END;

        //  //} else {

        //      task = new string[] { "none" };
        //      taskType = TASKTYPE.BASIC;
        //  //}

        //}

        public StoryPoint(string myName, string myStoryLine, string[] myTask)
        {
            ID = myName;
            StoryLine = myStoryLine;

            Instructions = myTask;

            switch (Instructions[0])
            {

                case "start":
                case "stop":
                case "tell":
                case "goto":
                //      case "end":
                case "hold":

                    taskType = TASKTYPE.ROUTING;
                    break;

                //case "end":

                //  taskType = TASKTYPE.END;
                //  break;
                default:

                    taskType = TASKTYPE.BASIC;
                    break;

            }

        }

        public void setNextStoryPoint(StoryPoint myNextStoryPoint)
        {

            nextStoryPoint = myNextStoryPoint;

        }

        public StoryPoint getNextStoryPoint()
        {

            return nextStoryPoint;

        }

        public string getNextStoryPointName()
        {

            if (nextStoryPoint != null)
            {

                return nextStoryPoint.ID;

            }
            else
            {

                return ("no next point");

            }

        }

    }

}
