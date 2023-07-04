using AttendanceManagementSystem.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AttendanceManagementSystem.Helper
{
    public class OutletVisitHelper
    {
        public static bool IsCheckedIn(DbHelper dbHelper, string? userId)
        {
            if (!ObjectId.TryParse(userId, out _))
            {
                return false;
            }

            return dbHelper.RecordExists(CurrentFilter(userId));
        }


        public static FilterDefinition<OutletVisit> CurrentFilter(string? userId) {
            return Builders<OutletVisit>.Filter.Eq(x => x.UserId, userId)
                & Builders<OutletVisit>.Filter.Exists(x => x.CheckOutTime, false);
        }
    }
}
