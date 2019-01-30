﻿using System;
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

    public class Script
    {
        string ID = "Script";


        List<String> manuscript;
        public Boolean isReady;

        // Copy these into every class for easy debugging.
        void Log(string _m) => StoryEngine.Log.Message(_m, ID);
        void Warning(string _m) => StoryEngine.Log.Warning(_m, ID);
        void Error(string _m) => StoryEngine.Log.Error(_m, ID);
        void Verbose(string _m) => StoryEngine.Log.Message(_m, ID, LOGLEVEL.VERBOSE);


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

        void parse(string _text)
        {

            manuscript = new List<string>();
            string[] lines = _text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                if (!isComment(lines[i]))
                    manuscript.Add(lines[i]);
            }

            if (manuscript.Count == 0)
                Error("Script has no lines.");

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
                    //				Log.Message ("storyPoint: "+l+" " + storyPoint);
                    string[] task = getTask(manuscript[l]);
                    //				Log.Message ("task: " + task[0]);

                    //				point = new StoryPoint (storyPoint, currentStoryline, task);
                    point = new StoryPoint(UUID.getID(), currentStoryline, task);

                    //				if (point.taskType != TASKTYPE.BASIC) {
                    //					storyPoint = UUID.getId ();
                    //					point.ID = storyPoint;
                    //				}


                    //				storyPoints.Add (storyPoint, point);
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

        string l;

        string isStoryLine(int i)
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

        string isStoryLabel(int i)
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

        string isStoryPoint(int i)
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

        private bool isComment(string line)
        {

            string comment = "//";

            if (line.IndexOf(comment) == -1)
            {
                return false;
            }

            return true;
        }



        private string getElementFromLine(string line, int i)
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

        private string[] getTask(string line)
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





        private string[] splitLine(string line)
        {
            Char delimiter = ' ';
            string[] r = line.Split(delimiter);
            return r;
        }


        private string Load(string fileName)
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