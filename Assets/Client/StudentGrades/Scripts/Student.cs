namespace Assets.Client.StudentGrades.Scripts
{
    public enum Classification
    {
        Unassigned = 0,
        Approved = 1,
        Failed = 2
    }

    public sealed class Student
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string Code { get; }
        public string Email { get; }
        public decimal FinalGrade { get; }

        public string FullName => $"{FirstName} {LastName}";

        public Student(
            string firstName,
            string lastName,
            string code,
            string email,
            decimal finalGrade)
        {
            FirstName = firstName;
            LastName = lastName;
            Code = code;
            Email = email;
            FinalGrade = finalGrade;
        }
    }
}