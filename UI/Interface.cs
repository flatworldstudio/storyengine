﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;

namespace StoryEngine.UI
{

	/*!
     * \brief 
     * Describes a collection of interactive objects and how they are interacted with.
     * 
     * An interface references a Mapping that maps user actions to methods.
     * An interface is assigned to a Plane which in turn is part of a Layout.    
     */

	public class InterFace
	{

		public string name;
		public List<GameObject> selectedObjects;
		public Dictionary<string, Button> uiButtons;
		public Dictionary<GameObject, Button> uiButtonsObject;

		public Material editMat, defaultMat;
		public string tapNoneCallback = "";

		Mapping mapping;

		public GameObject canvasObject, gameObject;

		public Plane plane;
		public UiCam3D uiCam3D;
		//   public Vector2 anchorPosition;
		string ID = "Interface";

		#region LOG
		// Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
		void Log(string message) => StoryEngine.Log.Message(message, ID);
		void Warning(string message) => StoryEngine.Log.Warning(message, ID);
		void Error(string message) => StoryEngine.Log.Error(message, ID);
		void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);
		#endregion

		public InterFace(GameObject _canvasObject, string _name = "Unnamed")
		{
			canvasObject = _canvasObject;
			name = _name;

			mapping = Mapping.Empty;

			Initvars();

		}

		public InterFace(Canvas _canvas, string _name = "Unnamed")
		{

			canvasObject = _canvas.gameObject;
			name = _name;

			mapping = Mapping.Empty;

			Initvars();

		}

		void Initvars()
		{

			editMat = Resources.Load("materials/white", typeof(Material)) as Material;
			defaultMat = Resources.Load("materials/black", typeof(Material)) as Material;

			uiButtons = new Dictionary<string, Button>();
			uiButtonsObject = new Dictionary<GameObject, Button>();

			selectedObjects = new List<GameObject>();

			//     anchorPosition = Vector2.zero;

		}

		public void AddMapping(Mapping _mapping)
		{
			mapping = _mapping;
		}

		public void AddUiCam3D(UiCam3D _cam)
		{
			uiCam3D = _cam;
		}

		//public Vector2 GetAnchorOffset()
		//{
		//    return anchorPosition;
		//}

		public void none(object sender, UIArgs args)
		{
			mapping.none(sender, args);
		}


		public void tap_2d(object sender, UIArgs args)
		{
			mapping.tap_2d(sender, args);
		}

		public void tap_3d(object sender, UIArgs args)
		{
			mapping.tap_3d(sender, args);
		}

		public void doubletap_none(object sender, UIArgs args)
		{
			mapping.doubletap_none(sender, args);
		}

		public void doubletap_2d(object sender, UIArgs args)
		{
			mapping.doubletap_2d(sender, args);
		}

		public void doubletap_3d(object sender, UIArgs args)
		{
			mapping.doubletap_3d(sender, args);
		}

		public void tap_none(object sender, UIArgs args)
		{
			mapping.tap_none(sender, args);
		}

		public void single_2d(object sender, UIArgs args)
		{
			mapping.single_2d(sender, args);
		}

		public void single_3d(object sender, UIArgs args)
		{
			mapping.single_3d(sender, args);
		}

		public void single_none(object sender, UIArgs args)
		{
			mapping.single_none(sender, args);
		}

		public void double_2d(object sender, UIArgs args)
		{
			mapping.double_2d(sender, args);
		}

		public void double_3d(object sender, UIArgs args)
		{
			mapping.double_3d(sender, args);
		}

		public void double_none(object sender, UIArgs args)
		{

			mapping.double_none(sender, args);

		}


		public string[] getButtonNames()
		{

			return uiButtonsObject.Select(item => "" + item.Key.name).ToArray();

			////return test.ToArray<string>();

			//Dictionary<string, Button>.KeyCollection allButtons = uiButtons.Keys;
			//string[] allButtonNames = new string[allButtons.Count];
			//allButtons.CopyTo(allButtonNames, 0);

			//return allButtonNames;
		}



		public void addButton(Button button)
		{

            Verbose("Adding button with name " + button.name);

            //            add by name(which may not be unique in the scene)
            uiButtons.Remove(button.name);
			uiButtons.Add(button.name, button);

			// add by gameobjct (unique in scene)
			uiButtonsObject.Remove(button.gameObject);
			uiButtonsObject.Add(button.gameObject, button);

			button.InterFace = this;// cross ref.

			Verbose("Button count by object in interface: " + uiButtonsObject.Count);
			Verbose("Button count by name in interface: " + uiButtons.Count);


		}

		public void removeButtonByObject(GameObject go)
		{

            Verbose("Removing button with name " + go.name);

			Button but = GetButton(go);
			uiButtonsObject.Remove(go);

			foreach (var item in uiButtons.Where(kvp => kvp.Value == but).ToList())
			{
				uiButtons.Remove(item.Key);
			}

            Verbose("Button count by object in interface: " + uiButtonsObject.Count);
            Verbose("Button count by name in interface: " + uiButtons.Count);

            		}


		public StoryEngine.UI.Button GetButton(GameObject gameObject)
		{
			uiButtonsObject.TryGetValue(gameObject, out Button result);
			return result;
		}

		public StoryEngine.UI.Button GetButton(string _name)
		{
			uiButtons.TryGetValue(_name, out Button result);
			return result;
		}

	}













	// ------------------------------------------------------------------------------------------------------------------------------------------
	// turn physical mouse/touch interaction into a ui event






	//    /*!
	//* \brief
	//* Holds ui info to pass onto handlers.
	//* 
	//*/
	//public class UIArgs : EventArgs
	//{

	//    public Event uiEvent;
	//    public Vector3 delta;

	//    public UIArgs() : base() // extend the constructor 
	//    {

	//    }

	//}

}