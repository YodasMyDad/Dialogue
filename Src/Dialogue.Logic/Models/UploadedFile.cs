using System;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;

namespace Dialogue.Logic.Models
{
    public partial class UploadedFile
    {
        public UploadedFile()
        {
            Id = AppHelpers.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public int MemberId { get; set; }
        public Member Member { get; set; }
        public virtual Post Post { get; set; }
        public DateTime DateCreated { get; set; }

        public string FriendlyFilename
        {
            get { return Filename.Split('_')[1]; }
        }
        public string FilePath
        {
            get { return string.Format(AppConstants.MemberUploadPath, MemberId, Filename); }
        }
    }
}
