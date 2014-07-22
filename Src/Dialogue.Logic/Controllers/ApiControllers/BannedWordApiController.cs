using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dialogue.Logic.Application;
using Dialogue.Logic.Data.Context;
using Dialogue.Logic.Data.UnitOfWork;
using Dialogue.Logic.Models;
using Dialogue.Logic.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Dialogue.Logic.Controllers.ApiControllers
{
    [PluginController("BannedWord")]
    public class BannedWordApiController : UmbracoAuthorizedJsonController
    {
        private readonly BannedWordService _bannedWordService;
        private readonly UnitOfWorkManager _unitOfWorkManager;

        public BannedWordApiController()
        {
            _bannedWordService = new BannedWordService();
            _unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
        }

        public IEnumerable<BannedWord> GetByName(string name)
        {
            using (_unitOfWorkManager.NewUnitOfWork())
            {
                return _bannedWordService.GetAll().Where(x => x.Word.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        public IEnumerable<BannedWord> GetAll()
        {
            using (_unitOfWorkManager.NewUnitOfWork())
            {
                return _bannedWordService.GetAll();
            }
        }

        public bool Delete(int id)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var email = _bannedWordService.Get(id);
                    _bannedWordService.Delete(email);
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

        public BannedWord Add(BannedWord email)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    email.DateAdded = DateTime.Now;
                    var newEmail = _bannedWordService.Add(email);
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