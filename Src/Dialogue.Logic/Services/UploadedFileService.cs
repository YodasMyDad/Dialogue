using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Models;

namespace Dialogue.Logic.Services
{
    public partial class UploadedFileService
    {
        public UploadedFile Add(UploadedFile uploadedFile)
        {
            uploadedFile.DateCreated = DateTime.UtcNow;
            return ContextPerRequest.Db.UploadedFile.Add(uploadedFile);
        }

        public void Delete(UploadedFile uploadedFile)
        {
            ContextPerRequest.Db.UploadedFile.Remove(uploadedFile);
        }

        public IList<UploadedFile> GetAll()
        {
            return ContextPerRequest.Db.UploadedFile.ToList();
        }

        public IList<UploadedFile> GetAllByPost(Guid postId)
        {
            return ContextPerRequest.Db.UploadedFile.Where(x => x.Post.Id == postId).ToList();
        }

        public IList<UploadedFile> GetAllByUser(int membershipUserId)
        {
            return ContextPerRequest.Db.UploadedFile.Where(x => x.MemberId == membershipUserId).ToList();
        }

        public UploadedFile Get(Guid id)
        {
            return ContextPerRequest.Db.UploadedFile.FirstOrDefault(x => x.Id == id);
        }
    }
}