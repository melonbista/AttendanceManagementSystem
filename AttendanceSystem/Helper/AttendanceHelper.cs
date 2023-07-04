using Amazon.Runtime.Internal;
using AttendanceManagementSystem.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Helper
{
    public class AttendanceHelper
    {
        public static bool IsCheckedIn(DbHelper dbHelper, string? userId)
        {
            if (!ObjectId.TryParse(userId, out _))
            {
                return false;
            }

            return dbHelper.RecordExists(CurrentFilter(userId));
        }


        public static FilterDefinition<Attendance> CurrentFilter(string? userId)
        {
            return Builders<Attendance>.Filter.And(
            Builders<Attendance>.Filter.Eq(a => a.UserId, userId) &
            Builders<Attendance>.Filter.Eq(a => a.PunchOutTime, null));
        }
    }
}
