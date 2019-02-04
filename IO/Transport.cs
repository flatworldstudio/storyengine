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

        static string ID = "Transport";

        static string[] emptyList = new string[0];

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
    static    void Log(string message) => StoryEngine.Log.Message(message, ID);
        static void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        static void Error(string message) => StoryEngine.Log.Error(message, ID);
        static void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        public static string[] FileList(string _path){

            if (!   Directory.Exists(_path))
                return emptyList;
            
            string[] list = Directory.GetFiles(_path);

            return list;
         
        }

        public static void TextToFile (string _content, string _path){

            //   Path path = new Path(_path);
            //GetDirectoryName(String)

            if (!Directory.Exists(Path.GetDirectoryName(_path))){
                Directory.CreateDirectory(Path.GetDirectoryName(_path));
                Log("Created directory: " + Path.GetDirectoryName(_path));
            }

            try
            {
            File.WriteAllText(_path, _content);
            }catch (Exception e)
            {
                Error("File "+_path+" failed writing: " + e.Message);
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
                    Log(result);
                    reader.Close();


                }
                catch (Exception e)
                {

                    Error("File "+_path+" failed loading: " + e.Message);
                    result="";
                }


            }
            else
            {

                Warning("File not found: "+_path);
                result="";

            }

            return result;


        }



    }

}