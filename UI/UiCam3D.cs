using UnityEngine;

namespace StoryEngine.UI
{

    /*!
* \brief 
* Describes a 3D camera setup with an interest point.
* 
* Can have a Constraint  
*/

    public class UiCam3D
    {

        public GameObject cameraObject, cameraReference, cameraInterest;
        public Camera camera;
        public Constraint constraint;
        public CAMERACONTROL control;

        public UiCam3D()
        {

        }

        public UiCam3D(GameObject theCameraObject)
        {

            // assumes that reference's parent is interest and that camera is component on reference or child

            if (theCameraObject == null)
                return;

            cameraObject = theCameraObject;

            if (cameraObject.transform.parent != null)
            {

                cameraInterest = theCameraObject.transform.parent.gameObject;

            }

            camera = cameraObject.GetComponentInChildren<Camera>();

            if (camera != null)
            {

                cameraReference = camera.gameObject;

            }

        }

        public void AddContraint(Constraint _constraint)
        {

            constraint = _constraint;

        }



    }
}