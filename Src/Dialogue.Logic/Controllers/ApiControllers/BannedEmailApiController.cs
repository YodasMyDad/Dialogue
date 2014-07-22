using System;
using System.Collections.Generic;
using System.Linq;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Models;
using Dialogue.Logic.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Dialogue.Logic.Controllers.ApiControllers
{
    //[ValidateAngularAntiForgeryToken]
    [PluginController("BannedEmail")]
    public class BannedEmailApiController : UmbracoAuthorizedJsonController
    {
        // To debug use the non Json one
        //UmbracoAuthorizedApiController
        //UmbracoAuthorizedJsonController

        private readonly BannedEmailService _bannedEmailService;
        private readonly UnitOfWorkManager _unitOfWorkManager;

        public BannedEmailApiController()
        {
            _bannedEmailService = new BannedEmailService();
            _unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
        }

        //umbraco/backoffice/bannedEmail/bannedemailapi/getall
        
        public IEnumerable<BannedEmail> GetByName(string name)
        {
            using (_unitOfWorkManager.NewUnitOfWork())
            {
                return _bannedEmailService.GetAll().Where(x => x.Email.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        public IEnumerable<BannedEmail> GetAll()
        {
            using (_unitOfWorkManager.NewUnitOfWork())
            {
                return _bannedEmailService.GetAll();
            }
        }

        public bool Delete(int id)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var email = _bannedEmailService.Get(id);
                    _bannedEmailService.Delete(email);
                    unitOfWork.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    AppHelpers.LogError("BannedEmailApi Delete", ex);
                    return false;
                } 
            }
        }

        public BannedEmail Add(BannedEmail email)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    email.DateAdded = DateTime.Now;
                    var newEmail = _bannedEmailService.Add(email);
                    unitOfWork.Commit();
                    return newEmail;
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    AppHelpers.LogError("BannedEmailApi Add", ex);
                    return null;
                }
            }
        }

    }
}