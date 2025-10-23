using System.ComponentModel.DataAnnotations;

namespace inventario_coprotab.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email Requerido")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;


        public bool RememberMe { get; set; }
    }
}
