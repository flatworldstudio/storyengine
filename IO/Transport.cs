using System.IO;
using System;
using System.Net.NetworkInformation;


namespace StoryEngine.IO
{

    /*!
  * \brief
  * Provides local storage functionality.
  * 
  * Currently text to file and vice versa. To be expanded.
  */

    public static class Transport {

        static string me = "Transport";

        static string[] emptyList = new string[0];

        public static string[] FileList(string _path){

            if (!   Directory.Exists(_path))
                return emptyList;
            
            string[] list = Directory.GetFiles(_path);

            return list;
         
        }

        public static void TextToFile (string _content, string _path){

            try{
            File.WriteAllText(_path, _content);
            }catch (Exception e)
            {
                Log.Error("File failed writing: " + e.Message, me);
            }
        }

        public static string FileToText (string _path){

            string result="";

            if (File.Exists(_path))
            {

                try
                {

                    StreamReader reader = new StreamReader(_path);

                     result = reader.ReadToEnd();
                    Log.Message(result,me);
                    reader.Close();


                }
                catch (Exception e)
                {

                    Log.Error("File failed loading: " + e.Message,me);
                    result="";
                }


            }
            else
            {

                Log.Error("File missing");
                result="";

            }

            return result;


        }



    }

}