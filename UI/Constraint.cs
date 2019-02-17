using UnityEngine;


namespace StoryEngine.UI
{

    /*!
  * \brief
  * Describes drag constraints for a Button. 
  * 
  * Includes hardclamp, edge springs and springs for box constraint.
  * Includes hardclamp for circular constraint.
  * Includes hardclamp for 3D camera pitch constraint.
  */

    public class Constraint
    {
        public  bool hardClamp;
        public Vector3 hardClampMin;
        public Vector3 hardClampMax;

        public bool edgeSprings;
        public Vector3 edgeSpringMin;
        public Vector3 edgeSpringMax;

        public bool springs;
        public Vector2[] springPositions;

        public bool radiusClamp;
        public Vector2 anchor;
        public float radiusClampMin;
        public float radiusClampMax;

        public bool pitchClamp;
        public float pitchClampMin;
        public float pitchClampMax;

        static Constraint __empty, __locked;


        public Constraint()
        {
            hardClamp = false;
            edgeSprings = false;
            springs = false;
            radiusClamp = false;
            pitchClamp = false;
        }

        //public void AddPitchClamp(float _min, float _max)
        //{
        //    pitchClampMin = _min;
        //    pitchClampMax = _max;
        //    pitchClamp = true;
        //}

        //public void AddHardClamp(Vector3 _min, Vector3 _max)
        //{

        //    hardClampMin = _min;
        //    hardClampMax = _max;
        //    hardClamp = true;

        //}



        // convenience shortcut for locked

        static public Constraint locked
        {
            get
            {
                if (__locked == null)
                {
                    __locked = new Constraint()
                    {
                        hardClamp = true,
                        hardClampMin = Vector2.zero,
                        hardClampMax = Vector2.zero
                    };
                }

                return __locked;
            }
            set
            {

            }

        }
        
        /*!\brief convenience shortcut for lock in place. */

        public static Constraint LockInPlace(Button _ref)
        {
            Constraint customLocked = new Constraint
            {
                hardClamp = true

            };

            Vector2 pos = _ref.gameObject.GetComponent<RectTransform>().anchoredPosition; // will cause exception if bad input

            customLocked.hardClampMin = pos;
            customLocked.hardClampMax = pos;

            return customLocked;
        }

       
        /*!\brief convenience shortcut for empty constraint. */

        static public Constraint none
        {

            get
            {
                if (__empty == null)
                    __empty = new Constraint();

                return __empty;
            }
            set
            {

            }


        }




    }
}