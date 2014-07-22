namespace Dialogue.Logic.Models
{
    public partial class Report
    {
        public string Reason { get; set; }
        public Member Reporter { get; set; }
        public Member ReportedMember { get; set; }
        public virtual Post ReportedPost { get; set; }
    }
}
