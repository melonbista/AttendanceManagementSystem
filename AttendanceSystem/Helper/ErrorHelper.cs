using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagementSystem.Helper
{
    public static class ErrorHelper
    {
        public static BadRequestObjectResult ErrorResult(string field,string message)
        {
            return new BadRequestObjectResult(Error(field, message));
        }

        public static Dictionary<string, string[]> Error(string field, string message)
        {
            return new Dictionary<string, string[]>()
            {
                {field,new string[] {message}},
            };
        }
    }
}
