namespace StoryEngine.Network
{

    /*!
  * \brief
  * Holds update data for changes in StoryTask object, currently based on dataobject
  */

    public class StoryTaskUpdate : StoryDataUpdate
    {


        //  public  StoryDataUpdate dataUpdate;

        public StoryTaskUpdate() : base()
        {

            //  dataUpdate = _dataUpdate;

        }

        public StoryTaskUpdate(StoryDataUpdate dataUpdate) : base()
        {
            pointID = dataUpdate.pointID;

            updatedIntNames = dataUpdate.updatedIntNames;
            updatedIntValues = dataUpdate.updatedIntValues;

            updatedFloatNames = dataUpdate.updatedFloatNames;
            updatedFloatValues = dataUpdate.updatedFloatValues;

            updatedQuaternionNames = dataUpdate.updatedQuaternionNames;
            updatedQuaternionValues = dataUpdate.updatedQuaternionValues;

            updatedVector3Names = dataUpdate.updatedVector3Names;
            updatedVector3Values = dataUpdate.updatedVector3Values;

            updatedStringNames = dataUpdate.updatedStringNames;
            updatedStringValues = dataUpdate.updatedStringValues;
            
            updatedUshortNames = dataUpdate.updatedUshortNames;
            updatedUshortValues = dataUpdate.updatedUshortValues;

            updatedByteNames = dataUpdate.updatedByteNames;
            updatedByteValues = dataUpdate.updatedByteValues;

            updatedVector3ArrayNames = dataUpdate.updatedVector3ArrayNames;
            updatedVector3ArrayValues = dataUpdate.updatedVector3ArrayValues;

            updatedBoolArrayNames = dataUpdate.updatedBoolArrayNames;
            updatedBoolArrayValues = dataUpdate.updatedBoolArrayValues;

            updatedStringArrayNames = dataUpdate.updatedStringArrayNames;
            updatedStringArrayValues = dataUpdate.updatedStringArrayValues;
            
            debug = dataUpdate.debug;

        }



    }


}
