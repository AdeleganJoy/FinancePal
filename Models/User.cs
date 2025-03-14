namespace FinancePal.Models
{
    public class User
    {
        public required int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required decimal Amount { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
