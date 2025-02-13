using Assignment1.Model;

namespace Assignment1.Services
{
    public class AuditLogger
    {
        private readonly AuthDbContext _context;

        public AuditLogger(AuthDbContext context)
        {
            _context = context;
        }

        // Change Log method to accept userId and action parameters
        public void Log(string userId, string action)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User ID cannot be null or empty");
            }

            var auditLog = new AuditLog
            {
                UserId = userId,  // Set userId properly
                Action = action,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            _context.SaveChanges();
        }
    }
}
