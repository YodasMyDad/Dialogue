using System.Data.Entity.ModelConfiguration;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Data.Mapping
{
    public class UploadedFileMapping : EntityTypeConfiguration<UploadedFile>
    {
        public UploadedFileMapping()
        {
            ToTable("DialogueUploadedFile");
            HasKey(x => x.Id);
            HasRequired(x => x.Post).WithMany(x => x.Files)
                .Map(x => x.MapKey("DialoguePost_Id"));
            Ignore(x => x.Member);
        }
    }
}
