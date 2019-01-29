using UnityEngine;


namespace StoryEngine.UI
{

    public class Constraint
    {
        public Vector3 hardClampMin, hardClampMax;
        public bool hardClamp;

        public Vector3 edgeSpringMin, edgeSpringMax;
        public bool edgeSprings;

        public Vector2[] springPositions;
        public bool springs;

        public Vector2 anchor;
        public float radiusClampMin, radiusClampMax;
        public bool radiusClamp;

        public bool pitchClamp;
        public float pitchClampMin, pitchClampMax;
        static Constraint __empty;


        public Constraint()
        {
            hardClamp = false;
            edgeSprings = false;
            springs = false;
            radiusClamp = false;
            pitchClamp = false;
        }

        public void AddPitchClamp(float _min, float _max)
        {

            pitchClampMin = _min;
            pitchClampMax = _max;
            pitchClamp = true;

        }

        public void AddHardClamp(Vector3 _min, Vector3 _max)
        {

            hardClampMin = _min;
            hardClampMax = _max;
            hardClamp = true;

        }

        static public Constraint empty
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