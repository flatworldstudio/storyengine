
namespace StoryEngine.UI
{


    public enum TOUCH
    {
        BEGAN,
        TOUCHING,
        ENDED,
        NONE
    }

    public enum ACTION
    {
        // describes the action outcome. note that this is independent of UITOUCH which describes actual touch state.

        SINGLEDRAG,
        DOUBLEDRAG,
        TAP,
        VOID,
        DELETE
    }

    public enum DIRECTION
    {
        // Direction of drag. Ortho buttons snap to either horizontal or vertical.

        FREE,
        HORIZONTAL,
        VERTICAL
    }

    public enum CAMERACONTROL
    {
        ORBIT,
        GYRO,
        TURN,
        PAN,
        VOID
    }


}