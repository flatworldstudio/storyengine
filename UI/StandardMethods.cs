﻿using UnityEngine;
using StoryEngine;

namespace StoryEngine.UI
{
    public static class Methods
    {

        // --------------------------------------------   2D    ----------------------------------------------------------------------


        // Drag a 2d interactive object (ie button). 

        public static void Drag2D(object sender, UIArgs uxArgs)
        {
            
            if (uxArgs.uiEvent.targetButton == null)
                return;
            
            Translate2D(uxArgs.uiEvent.targetButton.GetDragTarget(uxArgs.uiEvent.direction), uxArgs.delta, uxArgs.uiEvent.targetButton.GetConstraint(uxArgs.uiEvent.direction), uxArgs.uiEvent);
                       
        }

        static void Translate2D(GameObject target, Vector2 delta, Constraint constraint, Event ui)
        {
            Vector3 anchor = target.GetComponent<RectTransform>().anchoredPosition;
            anchor.y += delta.y;
            anchor.x += delta.x;

            if (constraint.hardClamp)
            {
                if (anchor.x < constraint.hardClampMin.x)
                {
                    anchor.x = constraint.hardClampMin.x;
                    ui.dx = 0;

                }
                else if (anchor.x > constraint.hardClampMax.x)
                {
                    anchor.x = constraint.hardClampMax.x;
                    ui.dx = 0;

                }
                if (anchor.y < constraint.hardClampMin.y)
                {
                    anchor.y = constraint.hardClampMin.y;
                    ui.dy = 0;

                }
                else if (anchor.y > constraint.hardClampMax.y)
                {
                    anchor.y = constraint.hardClampMax.y;
                    ui.dy = 0;
                }
            }

            if (constraint.radiusClamp)
            {
                Vector2 relativePosition = new Vector2(anchor.x, anchor.y) - constraint.anchor;

                float magnitude = relativePosition.magnitude;

                magnitude = Mathf.Clamp(magnitude, constraint.radiusClampMin, constraint.radiusClampMax);

                relativePosition = relativePosition.normalized * magnitude;

                //              relativePosition = Vector2.ClampMagnitude (relativePosition, constraint.radiusClampMax);

                anchor = constraint.anchor + relativePosition;

            }


            target.GetComponent<RectTransform>().anchoredPosition = anchor;

            //      if (ui.action == UIACTION.INERTIA) {


            //                      if (ui.isInert || ui.isSpringing) {
            if (ui.isSpringing)
            {

                bool springing = apply2Dsprings(target, constraint, ui.springIndex);

                if (ui.isSpringing && springing == false)
                {
                    //                  Debug.Log ("Springing stopped");
                    ui.isSpringing = false;
                    ui.springIndex = -1;
                }

                //  Debug.Log("am springing");
                // only apply springs when user not touching. reset 2d target only when springs have put object in place. 
                //              if (applyGUISprings (target, constraint)) {
                ////                    ui.target2D = null;
                ////                    ui.action = UIACTION.DELETE;
                //                  ui.isSpringing
                //                  Debug.Log ("DELETE ME");
                //              }
            }
        }


        static bool apply2Dsprings(GameObject target, Constraint constraint, int springIndex)
        {
            // apply springs to gui object. returns true if it did anything.
            bool result = false;
            Vector3 anchor = target.GetComponent<RectTransform>().anchoredPosition;

            if (constraint.edgeSprings)
            {
                result = true;
                float eVel0 = 0;
                float eVel1 = 0;
                float eVel2 = 0;
                float eVel3 = 0;

                if (anchor.x < constraint.edgeSpringMin.x)
                {
                    anchor.x = Mathf.SmoothDamp(anchor.x, constraint.edgeSpringMin.x, ref eVel0, 0.15f);
                }
                else if (anchor.x > constraint.edgeSpringMax.x)
                {
                    anchor.x = Mathf.SmoothDamp(anchor.x, constraint.edgeSpringMax.x, ref eVel1, 0.15f);
                }

                if (anchor.y < constraint.edgeSpringMin.y)
                {
                    anchor.y = Mathf.SmoothDamp(anchor.y, constraint.edgeSpringMin.y, ref eVel2, 0.15f);
                }
                else if (anchor.y > constraint.edgeSpringMax.y)
                {
                    anchor.y = Mathf.SmoothDamp(anchor.y, constraint.edgeSpringMax.y, ref eVel3, 0.15f);
                }

                float maxVel = Mathf.Max(Mathf.Abs(eVel0), Mathf.Abs(eVel1), Mathf.Abs(eVel2), Mathf.Abs(eVel3));

                if (maxVel < 1f)
                {
                    // maxvel 0 implies springs not actually doing anything, so return should be false
                    result = false;

                }
            }

            if (constraint.springs)
            {

                result = true;
                int closestSpring = -1;
                float closest = 9999999999f;



                if (springIndex >= 0)
                {

                    closestSpring = springIndex;

                }
                else
                {

                    for (int i = 0; i < constraint.springPositions.Length; i++)
                    {

                        // find nearest spring

                        Vector3 theSpringPosition = constraint.springPositions[i];
                        float distance = Vector2.Distance(anchor, theSpringPosition);
                        if (distance < closest)
                        {
                            closest = distance;
                            closestSpring = i;
                        }
                    }

                }

                float eVel0 = 0;
                float eVel1 = 0;

                anchor.x = Mathf.SmoothDamp(anchor.x, constraint.springPositions[closestSpring].x, ref eVel0, 0.15f);
                anchor.y = Mathf.SmoothDamp(anchor.y, constraint.springPositions[closestSpring].y, ref eVel1, 0.15f);

                float maxVel = Mathf.Max(Mathf.Abs(eVel0), Mathf.Abs(eVel1));

                //              float maxVel = Mathf.Max (eVel0, eVel1);
                //              if (maxVel > 0f && maxVel < 1f) {

                if (maxVel < 1f)
                {
                    // Note that speed is per second. So 1 pixel per second. Maybe look at dx dy since last time. 
                    // Also, last is not per se deltatime ago.

                    // maxvel 0 implies springs not actually doing anything, so return should be false
                    result = false;
                }
            }
            target.GetComponent<RectTransform>().anchoredPosition = anchor;

            return result;
        }

        // --------------------------------------------   3D    ----------------------------------------------------------------------


        // Dolly a 3d camera in and out

        public static void LongitudinalCamera(object sender, UIArgs uxArgs)
        {
            GameObject cameraObject, cameraInterest;
            cameraObject = uxArgs.activeInterface.uiCam3D.cameraObject;
            cameraInterest = uxArgs.activeInterface.uiCam3D.cameraInterest;

            float delta = uxArgs.delta.z;

            if (cameraInterest != null && cameraObject != null)
            {
                float unitsPerPixel = 10f / Screen.height;

                Quaternion cameraOrientation = cameraObject.transform.rotation;
                Vector3 forward = cameraOrientation * Vector3.forward;
                uxArgs.delta = unitsPerPixel * delta * forward;

                TranslateCamera(sender, uxArgs);

            }
        }

        // Translate a 3D camera by a vector3.

        static void TranslateCamera(object sender, UIArgs uxArgs)
        {

            GameObject cameraObject;
            cameraObject = uxArgs.activeInterface.uiCam3D.cameraObject;
            //cameraInterest = uxArgs.activeInterface.uiCam3D.cameraInterest;
            Constraint constraint = uxArgs.activeInterface.uiCam3D.constraint;
            Vector3 delta = uxArgs.delta;

            //Vector3 cameraPositionIn = cameraObject.transform.position;
            Vector3 cameraPositionOut = cameraObject.transform.position;

            cameraPositionOut += delta;

            // Constraint is applied to camera object.

            if (constraint != null && constraint.hardClamp)
            {
                cameraPositionOut.x = Mathf.Clamp(cameraPositionOut.x, constraint.hardClampMin.x, constraint.hardClampMax.x);
                cameraPositionOut.y = Mathf.Clamp(cameraPositionOut.y, constraint.hardClampMin.y, constraint.hardClampMax.y);
                cameraPositionOut.z = Mathf.Clamp(cameraPositionOut.z, constraint.hardClampMin.z, constraint.hardClampMax.z);
            }

            // Translation is applied to camera object.

            cameraObject.transform.position = cameraPositionOut;

        }


        // Pan a 3D camera;

        public static void LateralCamera(object sender, UIArgs uxArgs)
        {

            GameObject cameraObject, cameraInterest;
            cameraObject = uxArgs.activeInterface.uiCam3D.cameraObject;
            cameraInterest = uxArgs.activeInterface.uiCam3D.cameraInterest;
            Constraint constraint = uxArgs.activeInterface.uiCam3D.constraint;
            Vector3 delta = -1f * uxArgs.delta;

            Vector3 cameraPositionIn = cameraObject.transform.position;
            Vector3 cameraPositionOut = cameraPositionIn;

            Quaternion cameraOrientation = cameraInterest.transform.rotation;
            Vector3 horizontal = cameraOrientation * Vector3.right;
            Vector3 vertical = cameraOrientation * Vector3.up;

            float unitsPerPixel = 10f / Screen.height; // feels random

            cameraPositionOut += (delta.x * unitsPerPixel * 1f) * horizontal;
            cameraPositionOut += (delta.y * unitsPerPixel * 1f) * vertical;

            // Constraint is applied to camera object.

            if (constraint != null && constraint.hardClamp)
            {
                cameraPositionOut.x = Mathf.Clamp(cameraPositionOut.x, constraint.hardClampMin.x, constraint.hardClampMax.x);
                cameraPositionOut.y = Mathf.Clamp(cameraPositionOut.y, constraint.hardClampMin.y, constraint.hardClampMax.y);
                cameraPositionOut.z = Mathf.Clamp(cameraPositionOut.z, constraint.hardClampMin.z, constraint.hardClampMax.z);
            }

            // Translation is applied to interest object.

            cameraInterest.transform.position += (cameraPositionOut - cameraPositionIn);

        }

        // Technically, it's more of a YPR camera, not an orbit camera.

        static public void OrbitCamera(object sender, UIArgs uxArgs)
        {

            GameObject cameraObject, cameraInterest;
            cameraObject = uxArgs.activeInterface.uiCam3D.cameraObject;
            cameraInterest = uxArgs.activeInterface.uiCam3D.cameraInterest;

            if (cameraInterest != null && cameraObject != null)
            {

                float degreesPerPixel = 360f / Screen.height;

                Quaternion orientation = cameraInterest.transform.rotation;

                Vector3 euler = orientation.eulerAngles;
                euler.y += uxArgs.delta.x * degreesPerPixel;
                euler.x -= uxArgs.delta.y * degreesPerPixel;
                euler.z = 0;

                Constraint orbitConstraint = uxArgs.activeInterface.uiCam3D.constraint;

                if (orbitConstraint != null && orbitConstraint.pitchClamp)
                {

                    // transform to plus minus range, clamp, transform back
                    float pitch = euler.x <= 180f ? euler.x : euler.x - 360f;
                    pitch = Mathf.Clamp(pitch, orbitConstraint.pitchClampMin, orbitConstraint.pitchClampMax);
                    euler.x = pitch >= 0f ? pitch : pitch + 360f;

                }

                orientation = Quaternion.Euler(euler);
                cameraInterest.transform.rotation = orientation;

            }
        }

        // --------------------------------------------   DEBUG    ----------------------------------------------------------------------


        public static void someAction(object sender, UIArgs uxArgs)
        {

            Log.Message("SOME ACTION TRIGGERRED");
            //uxArgs.uiEvent.target2D

        }



        // ------------------------------------------------------------------------------------------------------------------
        // SALVAGED CODE

        /*


        void TerrainClamp{

            // this clamps the camera to a lowest point (from fabric). based on geometry. however, it's not generic right now -> refactor.

            //              Vector3 newCameraPosition = state.cameraInterest.transform.position + orientation * new Vector3 (0, 0, -distanceToInterest);

            Vector3 newCameraPosition = ci.transform.position + orientation * new Vector3(0, 0, -distanceToInterest);


            float hh = 0;
            //          float hh = cc.getActiveLandscape ().getHeight (cp.x, cp.z) + minimumHeight;

            if (newCameraPosition.y < hh)
            {
                //                  float pitch = Mathf.Asin ((hh - TPObject.transform.position.y) / distanceToInterest) * Mathf.Rad2Deg;
                float pitch = Mathf.Asin((hh - 0) / distanceToInterest) * Mathf.Rad2Deg;

                //Debug.Log ("orbitcam under hull, locking pitch to: " + pitch);
                euler.x = pitch;
            }

        }

void HullClamp{
       // control for camera hitting the hull. camera will be pushed up and when possible it'll smoothly lower back along the hull towards its original y.

            // !!!!!!!!!!!!!!!!!!!!!
            //          float hullHeight = cc.getActiveLandscape ().getHeight (cameraPositionOut.x, cameraPositionOut.z) + minimumHeight;

            
                if (false)
                {

                    float lockValueY = 0;
                    bool lockY = false;
                    float hullHeight = 0;




                    bool yUnlocked = false;
                    bool yIsTooLow = false;
                    bool hullHigherThanLockvalue = false;
                    float modifY = 0f;

                    if (cameraPositionOut.y > lockValueY)
                        yUnlocked = true;

                    if (cameraPositionOut.y <= hullHeight)
                        yIsTooLow = true;

                    if (lockValueY <= hullHeight)
                        hullHigherThanLockvalue = true;

                    if (yIsTooLow)
                    {
                        // find out how much y needs to go up and create a correction vector along the vertical axis to do it. (thus controlling x and z as well)
                        modifY = hullHeight - cameraPositionOut.y;
                        float factor = modifY / vertical.y;
                        Vector3 yCorrection = factor * vertical;
                        cameraPositionOut += yCorrection;

                        if (!yUnlocked)
                        {
                            // store unmodified y value. effectively this stores the lock value only the first time the cam gets bumped up
                            lockValueY = cameraPositionIn.y;
                        }
                    }

                    if (!yIsTooLow && yUnlocked && hullHigherThanLockvalue && lockY)
                    {

                        // smooth towards hull y
                        float cameraVel = 0f;
                        cameraPositionOut.y = Mathf.SmoothDamp(cameraPositionIn.y, hullHeight, ref cameraVel, 0.25f);
                    }

                    if (!yIsTooLow && yUnlocked && !hullHigherThanLockvalue && lockY)
                    {
                        // smooth towards unmodified y value
                        float cameraVel = 0f;
                        cameraPositionOut.y = Mathf.SmoothDamp(cameraPositionIn.y, lockValueY, ref cameraVel, 0.25f);
                        if (Mathf.Abs(cameraPositionOut.y - lockValueY) < 0.1f)
                        {
                            // snap value 
                            cameraPositionOut.y = lockValueY;
                            yUnlocked = false;// this is redundant
                        }
                    }

                    //          Log.Message ( "ytoolow " + yIsTooLow.ToString () + " y modded " + yUnlocked.ToString ());
                    //          Log.Message ( "y: " + cameraPositionOut.y + " unmod y: " + state.lockValueY);

                }
    
}
*/

        // ------------------------------------------------------------------------------------------------------------------




        public static void clearSelectedObjects(object sender, UIArgs uxArgs)
        {
            foreach (GameObject go in uxArgs.activeInterface.selectedObjects)
            {

                go.GetComponent<MeshRenderer>().material = uxArgs.activeInterface.defaultMat;

            }
            uxArgs.activeInterface.selectedObjects.Clear();
        }

        public static void select3dObject(object sender, UIArgs uxArgs)
        {

            Log.Message("3d target: " + uxArgs.uiEvent.target3D.transform.name);

            if (uxArgs.activeInterface.selectedObjects.IndexOf(uxArgs.uiEvent.target3D) != -1)
            {

                // object was selected, deselect
                uxArgs.uiEvent.target3D.GetComponent<MeshRenderer>().material = uxArgs.activeInterface.defaultMat;
                uxArgs.activeInterface.selectedObjects.Remove(uxArgs.uiEvent.target3D);

            }
            else
            {

                // object was not selected, select
                uxArgs.uiEvent.target3D.GetComponent<MeshRenderer>().material = uxArgs.activeInterface.editMat;
                uxArgs.activeInterface.selectedObjects.Add(uxArgs.uiEvent.target3D);

            }

        }

        public static void tapNone(object sender, UIArgs uxArgs)
        {

            uxArgs.uiEvent.callback = uxArgs.activeInterface.tapNoneCallback;
        }


        public static void tapButton2D(object sender, UIArgs uxArgs)
        {


            //  string name = uxArgs.uiEvent.target2D.transform.name;



            //    setTargetBrightness(uxArgs.activeInterface.getButtonNames(), 0.75f, uxArgs.activeInterface);

            //      setTargetBrightness


            //     setBrightness(name, 1f, uxArgs.activeInterface);
            //      setTargetBrightness(name, 0.75f,0.25f, uxArgs.activeInterface);

            //Debug.Log("highlight "+name);

            //      UiButton theButton;
            //      uxArgs.activeInterface.uiButtons.TryGetValue (name, out theButton);
            //

            if (uxArgs.uiEvent.targetButton != null)
            {
                uxArgs.uiEvent.callback = uxArgs.uiEvent.targetButton.callback;

                uxArgs.uiEvent.targetButton.Tap();

            }




            //      uxArgs.uiEvent.callback = theButton.callback; // to be retrieved from button object..
            //
            //      Debug.Log ("tap callback: " + theButton.callback);
        }


        public static void stopControls(object sender, UIArgs uxArgs)
        {
            //      uxArgs.uiEvent.callback = "stopControls";


            setTargetBrightness(uxArgs.activeInterface.getButtonNames(), 0.75f, uxArgs.activeInterface);


        }

        static void setTargetBrightness(string name, float value, Interface activeInterface)
        {
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
                theButton.targetBrightness = value;
        }
        static void setTargetBrightness(string name, float value, float step, Interface activeInterface)
        {
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
            {
                theButton.stepBrightness = step;
                theButton.targetBrightness = value;
            }
        }
        static void setBrightness(string name, float value, Interface activeInterface)
        {
            Button theButton;
            activeInterface.uiButtons.TryGetValue(name, out theButton);

            if (theButton != null)
            {
                theButton.brightness = value;
            }
        }


        static void setTargetBrightness(string[] names, float value, Interface activeInterface)
        {
            foreach (string name in names)
                setTargetBrightness(name, value, activeInterface);
        }



  
       

        //  static public  void orbitCamera (UxInterface state, Vector2 delta)



        static public void rotateCamera(object sender, UIArgs uxArgs)
        {

            if (uxArgs.activeInterface.uiCam3D.control == CAMERACONTROL.ORBIT)
            {

                OrbitCamera(sender, uxArgs);

            }

            if (uxArgs.activeInterface.uiCam3D.control == CAMERACONTROL.TURN)
            {

                turnCamera(sender, uxArgs);

            }

            if (uxArgs.activeInterface.uiCam3D.control == CAMERACONTROL.GYRO)
            {

                gyroCamera(sender, uxArgs);

            }

        }

        static readonly Quaternion baseIdentity = Quaternion.Euler(90, 0, 0);
        //        static readonly Quaternion landscapeRight = Quaternion.Euler(0, 0, 90);
        //       static readonly Quaternion landscapeLeft = Quaternion.Euler(0, 0, -90);
        //     static readonly Quaternion upsideDown = Quaternion.Euler(0, 0, 180);

        static public void gyroCamera(object sender, UIArgs uxArgs)
        {

            GameObject co, ci;
            co = uxArgs.activeInterface.uiCam3D.cameraObject;
            ci = uxArgs.activeInterface.uiCam3D.cameraInterest;

            Vector3 startPosition = co.transform.position;

            Quaternion gyro = GyroToUnity(Input.gyro.attitude);

            //      Debug.Log ("gyro " + gyro.ToString());

            //      ci.transform.rotation = gyro * Quaternion.Euler (0, 0, -90); // landscape left
            ci.transform.rotation = baseIdentity * gyro;

            //      Quaternion landscapeLeft =  Quaternion.Euler(0, 0, -90);

            //      Debug.Log ("gyroToUnity " + ci.transform.rotation.ToString());

            //      ci.transform.rotation = Input.gyro.attitude;

            Vector3 endPosition = co.transform.position;

            ci.transform.position -= (endPosition - startPosition);



            //      Vector3 forward = ci.transform.rotation * Vector3.forward;
            //      Vector3 up = ci.transform.rotation * Vector3.up;
            //      Vector3 right = ci.transform.rotation * Vector3.right;
            //
            //      Debug.Log ("f " + forward.ToString () + "u " + forward.ToString () + "r " + forward.ToString ());

        }



        private static Quaternion GyroToUnity(Quaternion q)
        {

            return new Quaternion(q.x, q.y, -q.z, -q.w);
            //      return new Quaternion(q.y, -q.x, q.z, q.w);

        }



        static public void turnCamera(object sender, UIArgs uxArgs)
        {

            // Create direct references for ease of use.

            GameObject co, ci;
            co = uxArgs.activeInterface.uiCam3D.cameraObject;
            ci = uxArgs.activeInterface.uiCam3D.cameraInterest;


            if (ci != null && co != null)
            {

                Vector3 startPosition = co.transform.position;


                // we need a gameobject to be the interest, and a gameobject that holds the camera.

                float degreesPerPixel = 360f / Screen.height;
                //        float distanceToInterest = -1f * co.transform.localPosition.z;

                // delta.x -> YAW. rotate around (world) y axis.
                Quaternion orientation = co.transform.rotation;
                Quaternion yawRotation = Quaternion.AngleAxis(uxArgs.delta.x * degreesPerPixel, Vector3.up);
                orientation = yawRotation * orientation;

                // delta.y -> PITCH. rotate around (relative) x axis 
                Vector3 cameraDirection = co.transform.rotation * Vector3.forward;
                Vector3 pitchAxis = Vector3.Cross(cameraDirection, Vector3.up);
                Quaternion pitchRotation = Quaternion.AngleAxis(uxArgs.delta.y * degreesPerPixel, pitchAxis);
                orientation = pitchRotation * orientation;

                // neutralise roll, clamp pitch, leave yaw unchanged
                Vector3 euler = orientation.eulerAngles;

                euler.z = 0;

                if (euler.x > 70 && euler.x <= 180)
                {
                    euler.x = 70;
                }
                if (euler.x < 290 && euler.x > 180)
                {
                    euler.x = 290;
                }


                if (false)
                {


                }

                orientation = Quaternion.Euler(euler);

                ci.transform.rotation = orientation;

                Vector3 endPosition = co.transform.position;

                ci.transform.position -= (endPosition - startPosition);

                //          co.transform.position = startPosition;

            }
        }






       


        //  public static void panCamera (UxInterface state, Vector2 delta)
        //  {
        //      panCamera (state, delta, emptyConstraint, false);
        //
        //  }




        public static void none(object sender, UIArgs uxArgs)
        {

        }





        //  public static void zoomCamera (UxInterface state, float delta)







    }

}