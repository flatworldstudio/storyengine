using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace StoryEngine
{
    /*!
* \brief
* Holds chains of StoryPoint objects and routing between them.
* 
* Takes a text document and parses it into connected StoryPoint objects.
*/

   static public class Script
    {
      static  string ID = "Script";

        static List<string> manuscript;

        static public string flattened = ""; // this is stripped of line endings, for checking purposes (eg hash)

        // Copy these into every class for easy debugging.
        static void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        static void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        static void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        static void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);


        static public void Mount(TextAsset _asset)
        {
            parse(_asset.text);
            }

        static public void Mount(string _filename)
        {
            string text = Load(_filename);
            parse(text);

        }

        /*
        public Script(TextAsset _asset)
        {

            //isReady = false;
            parse(_asset.text);
            //isReady = true;

        }

        public Script(string fileName)
        {

            //isReady = false;
            string text = Load(fileName);
            parse(text);
            //isReady = true;

        }
        */

        static int __idcount;

        static string GetId()
        {
            __idcount++;

            return __idcount.ToString("x8"); ;
        }

        static void parse(string _text)
        {
            __idcount = 0;
            flattened = "";

            manuscript = new List<string>();
            string[] lines = _text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                flattened += " "+lines[i];
                if (!isComment(lines[i]))
                    manuscript.Add(lines[i]);
            }

            if (manuscript.Count == 0)
                Error("Script has no lines.");

            Log(flattened);

            GENERAL.storyPoints = new Dictionary<string, StoryPoint>();
            Dictionary<string, StoryPoint> storyLines = new Dictionary<string, StoryPoint>();
            StoryPoint point, previousPoint;

            string currentStoryline = "default";

            previousPoint = null;

            int l = 0;

            string storyLine, storyLabel, storyPoint;

            while (l < manuscript.Count)
            {

                storyLine = isStoryLine(l);

                if (storyLine != null)
                {
                    l++;

                    storyPoint = isStoryPoint(l);

                    if (storyPoint != null)
                    {

                        //					Log.Message ("new storyline: " + storyLine);

                        string[] task = getTask(manuscript[l]);
                        //					Log.Message ("task: " + task[0]);

                        point = new StoryPoint(storyLine, storyLine, task);

                        //					point.name = storyLine;

                        storyLines.Add(storyLine, point);
                        GENERAL.storyPoints.Add(storyLine, point);

                        currentStoryline = storyLine;

                        previousPoint = point;


                    }
                    else
                    {

                        Warning("#storyline should be followed by storypoint");

                    }

                }

                storyLabel = isStoryLabel(l);

                if (storyLabel != null)
                {
                    l++;

                    storyPoint = isStoryPoint(l);

                    if (storyPoint != null)
                    {

                        //					Log.Message ("new storylabel: " + storyLabel);

                        string[] task = getTask(manuscript[l]);
                        //					Log.Message ("task: " + task[0]);

                        point = new StoryPoint(storyLabel, currentStoryline, task);


                        //					point.name = storyLabel;
                        //					if (point.taskType != TASKTYPE.BASIC)
                        //						storyPoint = getUniqueName ();

                        //					storyLabels.Add (storyLabel, point);

                        StoryPoint exists;

                        if (GENERAL.storyPoints.TryGetValue(storyLabel, out exists))
                        {

                            Debug.LogError("Storylabel exists: " + storyLabel + ". Adding as a non functional duplicate.");
                            storyLabel = storyLabel + "_DUPLICATE";
                        }

                        GENERAL.storyPoints.Add(storyLabel, point);

                        if (previousPoint != null)
                            previousPoint.setNextStoryPoint(point);

                        previousPoint = point;

                    }
                    else
                    {

                        Warning("@storylabel should be followed by storypoint");

                    }

                }

                storyPoint = isStoryPoint(l);

                if (storyLine == null && storyLabel == null && storyPoint != null)
                {
                    string[] task = getTask(manuscript[l]);
                 
                    point = new StoryPoint(GetId(), currentStoryline, task);

                    GENERAL.storyPoints.Add(point.ID, point);

                    if (previousPoint != null)
                        previousPoint.setNextStoryPoint(point);

                    previousPoint = point;

                }




                l++;

            }
            //		string[] keys = storyLines.Keys.Select(x => x.ToString()).ToArray();

            //		Enumerable storyLineKeys = storyLines.Keys;


            string[] keys = storyLines.Keys.ToArray();


            for (int sl = 0; sl < keys.Count(); sl++)
            {

                string key = keys[sl];
                //			Debug.Log ("Storyline key: " + key);


                StoryPoint thePoint;

                if (!storyLines.TryGetValue(key, out thePoint))
                {
                    //				Log.Message ("Can't find point");
                }
                else
                {

                    while (thePoint.getNextStoryPoint() != null)
                    {

                        //					Debug.Log (thePoint.ID + " | " + string.Join (" ", thePoint.task));
                        thePoint = thePoint.getNextStoryPoint();
                    }

                    //				Debug.Log (thePoint.ID + " | " + string.Join (" ", thePoint.task));




                }




                //			storyPoint point = storyLines[keys[sl]];



                //			getNextStoryPointName





            }





            //		string s = string.Join(";", storyLines.Select(x => x.Key + " : " + x.Value.task[0]+ " -> " + x.Value.getNextStoryPointName()).ToArray());
            //
            //		Debug.Log (s);
            //
            //		s = string.Join(";", storyLabels.Select(x => x.Key + " : " + x.Value.task[0]+ " -> " + x.Value.getNextStoryPointName()).ToArray());
            //
            //		Debug.Log (s);
            //
            //		s = string.Join(";", storyPoints.Select(x => x.Key + " : " + x.Value.task[0]+ " -> " + x.Value.getNextStoryPointName()).ToArray());
            //
            //		Debug.Log (s);

        }

        static string l;

        static string isStoryLine(int i)
        {
            l = manuscript[i];

            //if (isComment(l))
            //return null;

            string r = null;

            Char delimiter = '#';
            string[] s = manuscript[i].Split(delimiter);

            if (s.Length > 1)
            {
                r = s[1];
            }

            return r;

        }

        static string isStoryLabel(int i)
        {

            l = manuscript[i];


            //if (isComment(l))
            //return null;

            string r = null;

            Char delimiter = '@';
            string[] s = manuscript[i].Split(delimiter);

            if (s.Length > 1)
            {
                r = s[1];
            }

            return r;

        }

        static string isStoryPoint(int i)
        {
            l = manuscript[i];

            //if (isComment(l))
            //return null;

            Char space = ' ';
            //			string comment = "//";
            //
            //			if (l.IndexOf (comment) != -1) {
            //				return null;
            //			}
            string r = null;

            string[] s = l.Split(space);

            r = s[0];

            if (r.Equals(""))
            {
                r = null;
                //			Debug.Log ("empty!!");
            }

            return r;


        }

        static private bool isComment(string line)
        {

            string comment = "//";

            if (line.IndexOf(comment) == -1)
            {
                return false;
            }

            return true;
        }



        static private string getElementFromLine(string line, int i)
        {
            string[] split = splitLine(line);
            if (split.Length > i)
            {
                return split[i];
            }
            else
            {
                //				Debug.Log ("Empty line");
                return "null";
            }
        }

        static private string[] getTask(string line)
        {
            string[] s;

            s = splitLine(line);

            //			Debug.Log (s [1]);
            //		if (s.Length > 1) {
            //			r = new string[s.Length - 1];
            //
            //			Array.Copy (s, 1, r, 0, s.Length - 1);
            //			//				Debug.Log (r [0]);
            //			return r;
            //		} else {
            //			r = new string[0];
            //			return r;
            //		}

            return s;
        }





        static private string[] splitLine(string line)
        {
            Char delimiter = ' ';
            string[] r = line.Split(delimiter);
            return r;
        }


        static private string Load(string fileName)
        {

            TextAsset mytxtData = (TextAsset)Resources.Load(fileName);

            if (mytxtData != null)
            {
                return mytxtData.text;
            }
            else
            {
                return "";
            }
        }
    }

   

}