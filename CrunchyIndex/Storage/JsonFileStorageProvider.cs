using System;
using System.IO;
using CrunchyIndex.Models;
using Newtonsoft.Json;

namespace CrunchyIndex.Storage
{
    public class JsonFileStorageProvider : IStorageProvider
    {
        private JsonSerializer Serializer;

        public JsonFileStorageProvider()
        {
            this.Serializer = new JsonSerializer();
        }

        public void WriteIndex(Index index)
        {
            Write<Index>(index);
        }

        public Index ReadIndex()
        {
            return Read<Index>();
        }

        public void WriteUserActivity(UserActivity userActivity)
        {
            Write<UserActivity>(userActivity);
        }

        public UserActivity ReadUserActivity()
        {
            return Read<UserActivity>();
        }

        private void Write<T>(T objectToWrite)
        {
            string fileName = String.Format("{0}.json", typeof(T).Name);
            string serializedObject = JsonConvert.SerializeObject(objectToWrite);
            File.WriteAllText(fileName, serializedObject);
        }

        private T Read<T>()
        {
            string fileName = String.Format("{0}.json", typeof(T).Name);
            string serializedObject = File.ReadAllText(fileName);
            return JsonConvert.DeserializeObject<T>(serializedObject);
        }
    }
}
