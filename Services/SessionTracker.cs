using System.Collections.Concurrent;

namespace Assignment1.Services
{
    public class SessionTracker
    {
        private readonly ConcurrentDictionary<string, string> _sessions = new ConcurrentDictionary<string, string>();

        // Add session to tracker
        public void AddSession(string userId, string sessionId)
        {
            _sessions[userId] = sessionId;
        }

        // Remove session from tracker
        public void RemoveSession(string userId)
        {
            _sessions.TryRemove(userId, out _);
        }

        // Check if session is active for the user
        public bool IsSessionActive(string userId)
        {
            return _sessions.ContainsKey(userId);
        }
    }
}
