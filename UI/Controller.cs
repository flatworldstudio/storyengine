using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace StoryEngine.UI
{

    /*!
* \brief
* Handles user interaction, to be called every frame.
* 
* Returns a callback if user clicked on a button.
* Takes a Layout as argument, which can be contain Plane objects which can each hold an Interface.
* Maintains a stack of Event objects.
*/

    public class Controller
    {

        string ID = "Controller";

        Event activeUiEvent;

        int cycle = 0; // for debugging

        List<Event> uiEventStack;
        List<Button> AnimatingButtons;

        //   float currentScale = 1; // Not implemented right now...

        public static Controller instance;

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        void Log(string message) => StoryEngine.Log.Message(message, ID);
        void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        void Error(string message) => StoryEngine.Log.Error(message, ID);
        void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        public Controller()
        {
            instance = this;
            reset();
        }

        public void reset()
        {

            activeUiEvent = new Event();
            uiEventStack = new List<Event>();
            uiEventStack.Add(activeUiEvent);
            AnimatingButtons = new List<Button>();

        }

        // ------------------------------------ MAIN UPDATE METHOD -------------------------------------------------------------------

        // Checks and applies user interaction. 

        /*!\brief Main method to be called to run user interaction, returns callback.
         * 
        
         * Always has 1 active ui Event that contains current user activity
         * inertia and springing is handled by buttons themselves.
                   */




        public UserCallBack updateUi(Layout _layout)
        {
            cycle = (cycle + 1) % 1000;

            // Move all buttons.

            foreach (Plane plane in _layout.GetPlanes())
            {
                foreach (Button button in plane.interFace.GetButtons())
                {
                    if (!activeUiEvent.IsActiveDragTarget(button)) button.Move();

                }
            }


            UserCallBack callBack = new UserCallBack();
                  

            activeUiEvent.GetUserActivity(); // get (unscaled) mouse movement, touches, taps


            // If a user touch/click just began, set targets. 

            if (activeUiEvent.touch == TOUCH.BEGAN)
            {
                activeUiEvent.plane = _layout.FindPlaneByPoint(activeUiEvent.position); // get the plane the user is active in by screen coordinates

                //Debug.Log("targeting "+activeUiEvent.plane.address);

                activeUiEvent.SetTargets();
                                
            }

            // When we get user activity from mouse/touch we get screen coordinates.
            // Before we start applying drag etc to object, we need to scale the delta values.

            processUiEvent(activeUiEvent);
                        

            if (activeUiEvent.callback != "")
            {

                callBack.label = activeUiEvent.callback;
                callBack.sender = activeUiEvent.target2D;
                callBack.trigger = true;

                //Debug.Log ("callbackResult: " +  callBack.label);

            }
            
            if (activeUiEvent.touch == TOUCH.ENDED)
            {

                // the active event just ended. set inert and springing to true.

                activeUiEvent.touch = TOUCH.NONE;
                
                if (activeUiEvent.targetButton != null)
                {

                    activeUiEvent.targetButton.BeginInertia(activeUiEvent.scaledSpeed, activeUiEvent.direction);
                  
                }

                
                // Create a new uievent to catch the next interaction 

                activeUiEvent = new Event();

                // Verbose("added new event");
            }
                       

            AnimateButtons(); 

            return callBack;

        }


        void AnimateButtons()
        {
            int i = AnimatingButtons.Count - 1;

            while (i >= 0)
            {

                Button button = AnimatingButtons[i];
                button.ApplyBrightness();

                i--;
            }

        }

        public void AddAnimatingButton(Button _button)
        {

            if (!AnimatingButtons.Contains(_button))
                AnimatingButtons.Add(_button);

        }

        public void RemoveAnimatingButton(Button _button)
        {

            AnimatingButtons.Remove(_button);

        }

        
        /*!\brief Move a Button to a given spring position.*/
        /*
        public void setSpringTarget(Button _button, int index, DIRECTION dir = DIRECTION.FREE)

        //public void setSpringTarget(Button button, int index, DIRECTION dir = DIRECTION.FREE)
        {

            // Moves an interface segment (dragtarget, so a parent object) to a given spring. 
            // it uses a button as a hook. checks if any event is targeting the same dragtarget to prevent interference.
            // If there is we just take over. Could delete and replace it as well...

            int i = uiEventStack.Count - 1;

            while (i >= 0)
            {

                Event uie = uiEventStack[i];

                if (uie.targetButton != null && uie.targetButton.GetDragTarget(uie.direction) == _button.GetDragTarget(dir))
                {

                    // the event explicitly targets the (explicit) target of the passed in button
                    //remove the event
                    uiEventStack.RemoveAt(i);
                    Log("removing event");
                }

                i--;

            }

            Log("Adding a springing event for spring, button " + _button.name);

            Event springEvent = new Event();

            springEvent.targetButton = _button;
            springEvent.target2D = _button.gameObject;
            springEvent.plane = _button.InterFace.plane;
            springEvent.action = ACTION.SINGLEDRAG;
            springEvent.isSpringing = true;
            springEvent.springIndex = index;
            springEvent.direction = dir;

            uiEventStack.Add(springEvent);

        }
        */
        // ------------------------------------------------------------------------------------------------------------------------------------------

        void processUiEvent(Event ui)
        {

            if (ui.plane == null || ui.plane.interFace == null)
                return;

            ui.callback = "";

            UIArgs args = new UIArgs();

            args.delta = Vector3.zero;
            args.uiEvent = ui;

            InterFace interFace = ui.plane.interFace;

            switch (ui.action)
            {

                case ACTION.TAP:

                    ui.action = ACTION.SINGLEDRAG;

                    if (ui.target2D != null)
                        interFace.tap_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.tap_3d(this, args);
                    else
                        interFace.tap_none(this, args);

                    break;

                case ACTION.SINGLEDRAG:

                    // If this is an orthodrag button, we need to set and lock the initial direction.
                    // We also get the appropriate targets and constraints for that direction and load them into the uievent.

                    if (ui.targetButton != null && ui.targetButton.orthoDragging && ui.direction == DIRECTION.FREE)
                    {
                        if (Mathf.Abs(ui.scaledDx) > Mathf.Abs(ui.scaledDy))
                        {
                            ui.direction = DIRECTION.HORIZONTAL;
                            Log("Locking to drag horizontally");
                        }
                        if (Mathf.Abs(ui.scaledDx) < Mathf.Abs(ui.scaledDy))
                        {
                            ui.direction = DIRECTION.VERTICAL;
                            Log("Locking to drag vertically");
                        }

                    }
                    //      Verbose("singledrag");
                    args.delta = new Vector3(ui.scaledDx, ui.scaledDy, 0);

                    if (ui.target2D != null)
                        interFace.single_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.single_3d(this, args);
                    else
                        interFace.single_none(this, args);

                    // interFace.AddMapping

                    //      ui.isInert = false;
                    //    ui.isSpringing = false;

                    break;

                case ACTION.DOUBLEDRAG:

                    args.delta = new Vector3(ui.scaledDx, ui.scaledDy, ui.scaledDd);

                    if (ui.target2D != null)
                        interFace.double_2d(this, args);
                    else if (ui.target3D != null)
                        interFace.double_3d(this, args);
                    else
                        interFace.double_none(this, args);

                    break;

                default:

                    interFace.none(this, args);

                    break;
            }


            //   ui.Inertia();

        }


        // ------------------------------------------------------------------------------------------------------------------------------------------


        void setRaycastActive(string name, bool value, InterFace activeInterface)
        {
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
                theButton.image.raycastTarget = value;

        }

        bool brightnessIsChanging(string name, InterFace activeInterface)
        {
            //bool result = false;
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
            {
                return theButton.BrightnessChanging();

            }

            return false;
        }

    }

}





