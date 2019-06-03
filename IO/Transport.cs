using System.IO;
using System;
using System.Net.NetworkInformation;
using System.Collections.Generic;

namespace StoryEngine.IO
{

    /*!
  * \brief
  * Provides local storage functionality.
  * 
  * Currently text to file and vice versa. To be expanded.
  */

    public static class Transport
    {

        static string ID = "Transport";

        static string[] emptyList = new string[0];

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        static void Log(string message) => StoryEngine.Log.Message(message, ID);
        static void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        static void Error(string message) => StoryEngine.Log.Error(message, ID);
        static void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        /*!\brief Get a list of files in a directory. */
        public static string[] GetFileList(string _path)
        {

            if (!Directory.Exists(_path))
                return emptyList;

            string[] list = Directory.GetFiles(_path);

            return list;

        }

        /*!\brief Get a list of directories in a directory. */
        public static string[] GetDirectoryList(string _path)
        {

            if (!Directory.Exists(_path))
                return emptyList;

            string[] list = Directory.GetDirectories(_path);

            return list;

        }

        public static string StripExtention(string _name)
        {
            char[] param = new char[] { '.' };
            string[] split = _name.Split(param);

            if (split.Length > 2) Warning("Filename with more than one .");

            return split[0];

        }


        public static string FilenameFromPath(string _full)
        {
            char[] param = new char[] { '/', '\\' };

            string[] split = _full.Split(param);
            if (split.Length > 1)
                return split[split.Length - 1];
            else
                return split[0];

        }

        public static string CamelCase(string _text)
        {
            // remove exotics, cap first letter and cap anything after a space

            string cln = "";

            bool Capitalise = true;

            for (int i = 0; i < _text.Length; i++)
            {
                char c = _text[i];
                if (char.IsWhiteSpace(c)) Capitalise = true;

                if (char.IsLetterOrDigit(c))
                {
                    if (Capitalise)
                    {
                        c=char.ToUpper(c);
                        Capitalise = false;
                    }
                    cln = cln + c;
                }

            }

            Log(cln);

            return cln;


        }

        public static string FilenameOnly(string _full)
        {

            return StripExtention(FilenameFromPath(_full));

        }

        /*!\brief Write a string to disk as a file. */

        public static void TextToFile(string _content, string _path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path));
                Log("Created directory: " + Path.GetDirectoryName(_path));
            }

            try
            {
                File.WriteAllText(_path, _content);
            }
            catch (Exception e)
            {
                Error("File " + _path + " failed writing: " + e.Message);
            }
        }

        /*!\brief Read a file from disk ias a string. */
        public static string FileToText(string _path)
        {

            string result = "";

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

                    Error("File " + _path + " failed loading: " + e.Message);
                    result = "";
                }


            }
            else
            {

                Warning("File not found: " + _path);
                result = "";

            }

            return result;


        }



    }

}
