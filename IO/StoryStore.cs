using System;
using UnityEngine;
namespace StoryEngine.IO
{
    public static class StoryStore
    {

        static string ID = "StoryStore";

        // Copy these into every class for easy debugging. This way we don't have to pass an ID. Stack-based ID doesn't work across platforms.
        static void Log(string message) => StoryEngine.Log.Message(message, ID);
        static void Warning(string message) => StoryEngine.Log.Warning(message, ID);
        static void Error(string message) => StoryEngine.Log.Error(message, ID);
        static void Verbose(string message) => StoryEngine.Log.Message(message, ID, LOGLEVEL.VERBOSE);

        static string StoreName = "StoryStore";
        
        static string StorePath
        {
            get
            {
                return Application.persistentDataPath + "/" + StoreName;
            }
        }

        static string StoreFile
        {
            get
            {
                return StorePath + "/" + "Store.json";
            }
        }

        public static void Commit(string key, string value)
        {
            JSONObject json = GetStore();
            json.SetField(key, value);
            //json.AddField(key, value);
            Transport.TextToFile( json.ToString(), StoreFile);
          //  Log("Store: " + json);
        }

        public static string Retrieve(string key)
        {
           string value = "";
            JSONObject json = GetStore();
            json.GetField(ref  value, key);
      //      Log("Value " + value);
      //      Log("Store: " + json);
            return value;
        }

        static JSONObject GetStore()
        {
        //    Log("path " + StoreFile);
            string storeString = Transport.FileToText(StoreFile);

            if (storeString == "")
            {
                // that's not a store, so create one
                JSONObject json = new JSONObject();
                json.AddField("Store", true);
                StoryEngine.IO.Transport.TextToFile( json.ToString(), StoreFile);
                return json;
            }
            else
            {
                return new JSONObject(storeString);
            }

        }

    }
}
