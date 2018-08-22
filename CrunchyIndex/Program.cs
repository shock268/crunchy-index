using System;
using CrunchyIndex.Data;
using CrunchyIndex.Models;
using CrunchyIndex.Storage;
using WebAutomation;

namespace CrunchyIndex
{
    class Program
    {
        static void Main(string[] args)
        {
            int newSeriesCount = 0;
            int updatedSeriesCount = 0;
            var jsonStorageProvider = new JsonFileStorageProvider();
            using (var sqlStorageProvider = new SqlStorageProvider())
            {
                UserActivity userActivity;
                Index index;

                if (Config.Storage.ReadFromStorage)
                {
                    userActivity = jsonStorageProvider.ReadUserActivity();
                    index = jsonStorageProvider.ReadIndex();
                    // index = new Index();
                }
                else
                {
                    userActivity = new UserActivity();
                    index = new Index();
                }

                // high-level tasks
                using (var webAccessor = new WebAccessor())
                {
                    var taskRunner = new TaskRunner(webAccessor);
                    taskRunner.LogIn();
                    // newSeriesCount = taskRunner.AddSeriesToIndex(index);
                    // updatedSeriesCount = taskRunner.UpdatesSeriesToIndex(index);
                    // taskRunner.AddGenresToIndex(index);
                    taskRunner.UpdateUserActivity(userActivity);
                    taskRunner.AddUserActivityToIndex(userActivity, index);
                }

                if (Config.Storage.WriteToStorage)
                {
                    jsonStorageProvider.WriteIndex(index);
                    jsonStorageProvider.WriteUserActivity(userActivity);
                    sqlStorageProvider.WriteIndex(index);
                }
            }

            Console.WriteLine("Added {0} new series, updated {1}", newSeriesCount, updatedSeriesCount);
            Console.WriteLine("Press [Spacebar] to exit.");
            while (Console.ReadKey().Key != ConsoleKey.Spacebar) { };
        }
    }
}
