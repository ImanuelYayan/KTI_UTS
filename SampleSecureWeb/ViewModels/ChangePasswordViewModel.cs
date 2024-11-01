using System;
using System.ComponentModel.DataAnnotations;

namespace SampleSecuredWeb.ViewModel;

    public class ChangePasswordViewModel
    {
        [Required] 
        [DataType(DataType.Password)]
        [Display(Name = "Old Password")]
        public string? CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmNewPassword { get; set; }

    } 
