using System.ComponentModel.DataAnnotations;

namespace AttendanceSystem.Models.Dto
{
    public class RegisterRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }
}
