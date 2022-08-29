using System;

namespace helpers.Notifications
{
    public static class Event
    {

        public static event Action<string, dynamic[]> Subscribe;
        public static void Dispatch(string type, params dynamic[] data)
        {
            try
            {
                Subscribe?.Invoke(type, data);
            }
            catch (Exception) {/* Ignored */}
        }
    }
}
