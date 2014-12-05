using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Constants;
using Dialogue.Logic.Models;
using Dialogue.Logic.Models.ViewModels;
using Dialogue.Logic.Services;
using Umbraco.Core.Models;

namespace Dialogue.Logic.Controllers
{
    public partial class DialogueUploadSurfaceController : BaseSurfaceController
    {
        private readonly IMemberGroup _membersGroup;

        public DialogueUploadSurfaceController()
        {
            _membersGroup = (CurrentMember == null ? ServiceFactory.MemberService.GetGroupByName(AppConstants.GuestRoleName) : CurrentMember.Groups.FirstOrDefault());
        }

        [HttpPost]
        [Authorize]
        public ActionResult UploadPostFiles(AttachFileToPostViewModel attachFileToPostViewModel)
        {

            if (attachFileToPostViewModel != null && attachFileToPostViewModel.Files != null)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var message = new GenericMessageViewModel();

                    // First this to do is get the post
                    var post = ServiceFactory.PostService.Get(attachFileToPostViewModel.UploadPostId);

                    // Check we get a valid post back and have some file
                    if (post != null && attachFileToPostViewModel.Files != null)
                    {
                        Topic topic = null;
                        try
                        {
                            // Now get the topic
                            topic = post.Topic;

                            // Now get the category
                            var category = ServiceFactory.CategoryService.Get(topic.CategoryId);


                            // Get the permissions for this category, and check they are allowed to update and 
                            // not trying to be a sneaky mofo
                            var permissions = ServiceFactory.PermissionService.GetPermissions(category, _membersGroup);
                            if (permissions[AppConstants.PermissionAttachFiles].IsTicked == false && CurrentMember.DisableFileUploads != true)
                            {
                                return ErrorToHomePage(Lang("Errors.NoPermission"));
                            }

                            // woot! User has permission and all seems ok
                            // Before we save anything, check the user already has an upload folder and if not create one
                            var uploadFolderPath = Server.MapPath(string.Concat(AppConstants.UploadFolderPath, CurrentMember.Id));
                            if (!Directory.Exists(uploadFolderPath))
                            {
                                Directory.CreateDirectory(uploadFolderPath);
                            }

                            // Loop through each file and get the file info and save to the users folder and Db
                            foreach (var file in attachFileToPostViewModel.Files)
                            {
                                if (file != null)
                                {
                                    // If successful then upload the file
                                    var uploadResult = AppHelpers.UploadFile(file, uploadFolderPath);
                                    if (!uploadResult.UploadSuccessful)
                                    {
                                        message.Message = uploadResult.ErrorMessage;
                                        message.MessageType = GenericMessages.Danger;
                                        ShowMessage(message);
                                        return Redirect(topic.Url);
                                    }

                                    // Add the filename to the database
                                    var uploadedFile = new UploadedFile
                                    {
                                        Filename = uploadResult.UploadedFileName,
                                        Post = post,
                                        MemberId = CurrentMember.Id
                                    };
                                    ServiceFactory.UploadedFileService.Add(uploadedFile);

                                }
                            }

                            //Commit
                            unitOfWork.Commit();

                            // Redirect to the topic with a success message
                            message.Message = Lang("Post.FilesUploaded");
                            message.MessageType = GenericMessages.Success;
                            ShowMessage(message);

                            return Redirect(topic.Url);
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LogError(ex);
                            message.Message = Lang("Errors.GenericMessage");
                            message.MessageType = GenericMessages.Danger;
                            ShowMessage(message);
                            return topic != null ? Redirect(topic.Url) : ErrorToHomePage(Lang("Errors.GenericMessage"));
                        }

                    }
                }

            }

            // Else return with error to home page
            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }

        [Authorize]
        public ActionResult DeleteUploadedFile(Guid id)
        {
            if (id != Guid.Empty)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    Topic topic = null;
                    var message = new GenericMessageViewModel();
                    try
                    {
                        // Get the file and associated objects we'll need
                        var uploadedFile = ServiceFactory.UploadedFileService.Get(id);
                        var post = uploadedFile.Post;
                        topic = post.Topic;

                        if (_membersGroup.Name == AppConstants.AdminRoleName || uploadedFile.MemberId == CurrentMember.Id)
                        {
                            // Ok to delete file
                            // Remove it from the post
                            post.Files.Remove(uploadedFile);

                            // store the file path as we'll need it to delete on the file system
                            var filePath = uploadedFile.FilePath;

                            // Now delete it
                            ServiceFactory.UploadedFileService.Delete(uploadedFile);


                            // And finally delete from the file system
                            System.IO.File.Delete(Server.MapPath(filePath));
                        }
                        else
                        {

                            message.Message = Lang("Errors.NoPermission");
                            message.MessageType = GenericMessages.Danger;
                            ShowMessage(message);
              
                            Redirect(topic.Url);
                        }

                        //Commit
                        unitOfWork.Commit();

                        message.Message = Lang("Post.FileSuccessfullyDeleted");
                        message.MessageType = GenericMessages.Success;
                        ShowMessage(message);

                        return Redirect(topic.Url);
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LogError(ex);

                        message.Message = Lang("Errors.GenericMessage");
                        message.MessageType = GenericMessages.Danger;
                        ShowMessage(message);

                        return topic != null ? Redirect(topic.Url) : ErrorToHomePage(Lang("Errors.GenericMessage"));
                    }
                }
            }
            return ErrorToHomePage(Lang("Errors.GenericMessage"));
        }

    }
}