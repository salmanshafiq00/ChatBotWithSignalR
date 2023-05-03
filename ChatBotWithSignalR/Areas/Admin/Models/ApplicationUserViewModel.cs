using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChatBotWithSignalR.Areas.Admin.Models
{
    public class ApplicationUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        [Remote("IsEmailUsed", "User", "Admin", ErrorMessage = "This mail already used",AdditionalFields = "Id")]
        public string Email { get; set; } = string.Empty;
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [RegularExpression(@"^(?:\+88|88)?(01[0-9]\d{8})$", ErrorMessage = "Please input valid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePhotoUrl { get; set; } = string.Empty;
        public bool IsChatUser { get; set; }

        public string Roles { get; set; } = string.Empty;
        public IList<UserRolesViewModel> UserRoles { get; set; } = new List<UserRolesViewModel>();
    }
}
