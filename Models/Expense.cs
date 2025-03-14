namespace FinancePal.Models{
    public class Expense{
        public required int Id {get; set;}
        public required string Category{get; set;} 
        public required decimal Amount{get; set;}
        public required DateTime Date {get; set;}
        public required int UserId{get; set;}
    }
}