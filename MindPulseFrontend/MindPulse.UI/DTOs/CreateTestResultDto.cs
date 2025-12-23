namespace MindPulse.UI.DTOs
{
    public class CreateTestResultDto
    {
        public int UserId { get; set; }
        public string TestType { get; set; } // "Dikkat" veya "Hafıza"
        public int Score { get; set; }
        public double ReactionTimeMs { get; set; }
    }
}