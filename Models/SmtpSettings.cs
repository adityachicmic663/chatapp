using System.ComponentModel.DataAnnotations;

namespace backendChatApplication.Models
{
    public class SmtpSettings
    {
        [Required(ErrorMessage = "SMTP Host is required")]
        public string SmtpHost { get; set; }

        [Required(ErrorMessage = "SMTP Port is required")]
        public int SmtpPort { get; set; }

        [Required(ErrorMessage = "SMTP Username is required")]
        public string SmtpUsername { get; set; }

        [Required(ErrorMessage = "SMTP Password is required")]
        public string SmtpPassword { get; set; }

        [Required(ErrorMessage = "Sender email is required")]
        [RegularExpression("^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+\\.[a-zA-z]{2,3}$", ErrorMessage = "invalid email")]
        public string SenderEmail { get; set; }

        public bool EnableSsl { get; set; } = true;
    }
}
