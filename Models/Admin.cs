using System.ComponentModel.DataAnnotations;

namespace enrollmentSystem.Models
{
    public class Admin
    {
        [Key]
        [Required(ErrorMessage = "Admin ID is required")]
        public string Admin_Id { get; set; }  
        
        [Required(ErrorMessage = "Last name is required")]
        public string Admin_Lname { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        public string Admin_Fname { get; set; }
        
        public string? Admin_Mname { get; set; } 
        
        [Required(ErrorMessage = "Date of birth is required")]
        public DateOnly Admin_DOB { get; set; }
        
        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Admin_Contact { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Admin_Email { get; set; }
        
        [Required(ErrorMessage = "Address is required")]
        public string Admin_Address { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Admin_Password { get; set; }
    }
}