using CrunchyIndex.Models;

namespace CrunchyIndex.Storage
{
    interface IStorageProvider
    {
        void WriteIndex(Index index);
        Index ReadIndex();

        void WriteUserActivity(UserActivity userActivity);
        UserActivity ReadUserActivity();
    }
}
