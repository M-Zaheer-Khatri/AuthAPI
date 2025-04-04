namespace AuthAPI.Models
{
    public class RegisterModel
    {
        public string? Name { get; set; }
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
