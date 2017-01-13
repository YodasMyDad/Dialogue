namespace Dialogue.Logic.Services
{
    using Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using Application;
    using Data.Context;
    using Models;

    public partial class UploadedFileService : IRequestCachedService
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

        #region General Uploads

        /// <summary>
        /// Upload a file to an Umbraco upload field
        /// </summary>
        /// <param name="file"></param>
        /// <param name="memberMediaFolderId"></param>
        /// <param name="onlyImages"></param>
        /// <returns></returns>
        public UploadFileResult UploadFile(HttpPostedFileBase file, int memberMediaFolderId, bool onlyImages = false)
        {
            var upResult = new UploadFileResult { UploadSuccessful = true };

            var fileName = Path.GetFileName(file.FileName);
            if (fileName != null)
            {
                // check standard Dialogue stuff
                upResult = FileChecks(file, upResult, fileName, onlyImages);
                if (!upResult.UploadSuccessful)
                {
                    return upResult;
                }

                // Create the umbraco media file
 
                var ms = AppHelpers.UmbServices().MediaService;
                var media = ms.CreateMedia(fileName, memberMediaFolderId, "Image");
                media.SetValue("umbracoFile", file);
                ms.Save(media);

                // Get this saved media out the cache
                var typedMedia = AppHelpers.UmbHelper().TypedMedia(media.Id);

                // Set the Urls
                upResult.UploadedFileName = typedMedia.Name;
                upResult.UploadedFileUrl = typedMedia.Url;
                upResult.MediaId = typedMedia.Id;
            }

            return upResult;
        }


        public UploadFileResult UploadFile(HttpPostedFileBase file, string uploadFolderPath, bool onlyImages = false)
        {
            var upResult = new UploadFileResult { UploadSuccessful = true };

            var fileName = Path.GetFileName(file.FileName);
            if (fileName != null)
            {

                upResult = FileChecks(file, upResult, fileName, onlyImages);
                if (!upResult.UploadSuccessful)
                {
                    return upResult;
                }

                // Sort the file name
                var newFileName =
                    $"{AppHelpers.GenerateComb()}_{fileName.Trim(' ').Replace("_", "-").Replace(" ", "-").ToLower()}";
                var path = Path.Combine(uploadFolderPath, newFileName);

                // Save the file to disk
                file.SaveAs(path);

                var hostingRoot = HostingEnvironment.MapPath("~/") ?? "";
                var fileUrl = path.Substring(hostingRoot.Length).Replace('\\', '/').Insert(0, "/");

                upResult.UploadedFileName = newFileName;
                upResult.UploadedFileUrl = fileUrl;
            }

            return upResult;
        }

        private static UploadFileResult FileChecks(HttpPostedFileBase file, UploadFileResult upResult, string fileName, bool onlyImages = false)
        {
            //Before we do anything, check file size
            if (file.ContentLength > Dialogue.Settings().FileUploadMaximumFilesize)
            {
                //File is too big
                upResult.UploadSuccessful = false;
                upResult.ErrorMessage = AppHelpers.Lang("Post.UploadFileTooBig");
                return upResult;
            }

            // now check allowed extensions
            var allowedFileExtensions = Dialogue.Settings().FileUploadAllowedExtensions;

            if (onlyImages)
            {
                allowedFileExtensions = new List<string>
                    {
                        "jpg",
                        "jpeg",
                        "png",
                        "gif"
                    };
            }

            if (allowedFileExtensions.Any())
            {

                // Get the file extension
                var fileExtension = Path.GetExtension(fileName.ToLower());

                // If can't work out extension then just error
                if (string.IsNullOrEmpty(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = AppHelpers.Lang("Errors.GenericMessage");
                    return upResult;
                }

                // Remove the dot then check against the extensions in the web.config settings
                fileExtension = fileExtension.TrimStart('.');
                if (!allowedFileExtensions.Contains(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = AppHelpers.Lang("Post.UploadBannedFileExtension");
                    return upResult;
                }
            }

            return upResult;
        }

        public UploadFileResult UploadFile(Image file, string uploadFolderPath)
        {

            var upResult = new UploadFileResult { UploadSuccessful = true };
            var fileName = string.Concat(AppHelpers.GenerateComb(), ".jpg").ToLower();

            // now check allowed extensions
            var allowedFileExtensions = Dialogue.Settings().FileUploadAllowedExtensions;

            if (allowedFileExtensions.Any())
            {

                // Get the file extension
                var fileExtension = Path.GetExtension(fileName.ToLower());

                // If can't work out extension then just error
                if (string.IsNullOrEmpty(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = AppHelpers.Lang("Errors.GenericMessage");
                    return upResult;
                }

                // Remove the dot then check against the extensions in the web.config settings
                fileExtension = fileExtension.TrimStart('.');
                if (!allowedFileExtensions.Contains(fileExtension))
                {
                    upResult.UploadSuccessful = false;
                    upResult.ErrorMessage = AppHelpers.Lang("Post.UploadBannedFileExtension");
                    return upResult;
                }
            }

            // Sort the file name
            var path = Path.Combine(uploadFolderPath, fileName);

            // Save the file to disk
            file.Save(path, ImageFormat.Jpeg);

            var hostingRoot = HostingEnvironment.MapPath("~/") ?? "";
            var fileUrl = path.Substring(hostingRoot.Length).Replace('\\', '/').Insert(0, "/");

            upResult.UploadedFileName = fileName;
            upResult.UploadedFileUrl = fileUrl;
            return upResult;
        }

        #endregion
    }
}