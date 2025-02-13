using System.Collections.Concurrent;

public class SessionTracker
{
    private readonly ConcurrentDictionary<string, (string SessionId, DateTime LoginTime)> _sessions = new ConcurrentDictionary<string, (string, DateTime)>();

    // Add session to tracker
    public void AddSession(string userId, string sessionId)
    {
        _sessions[userId] = (sessionId, DateTime.Now);
    }

    // Remove session from tracker
    public void RemoveSession(string userId)
    {
        _sessions.TryRemove(userId, out _);
    }

    // Check if session is active for the user
    public bool IsSessionActive(string userId)
    {
        if (_sessions.ContainsKey(userId))
        {
            var (sessionId, loginTime) = _sessions[userId];

            // Check if session is expired (10 seconds timeout)
            if (DateTime.Now - loginTime > TimeSpan.FromSeconds(10))
            {
                // Session expired, remove it
                RemoveSession(userId);
                return false;
            }

            return true; // Session is still active
        }

        return false; // No session found
    }
}
