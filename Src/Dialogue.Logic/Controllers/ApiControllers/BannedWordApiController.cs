//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Dialogue.Logic.Application;
//using Dialogue.Logic.Data.Context;
//using Dialogue.Logic.Data.UnitOfWork;
//using Dialogue.Logic.Models;
//using Dialogue.Logic.Services;
//using Umbraco.Web.Editors;
//using Umbraco.Web.Mvc;

//namespace Dialogue.Logic.Controllers.ApiControllers
//{
//    [PluginController("BannedWord")]
//    public class BannedWordApiController : UmbracoAuthorizedJsonController
//    {
//        private readonly UnitOfWorkManager _unitOfWorkManager;

//        public BannedWordApiController()
//        {
//            _unitOfWorkManager = new UnitOfWorkManager(ContextPerRequest.Db);
//        }

//        public IEnumerable<BannedWord> GetByName(string name)
//        {
//            using (_unitOfWorkManager.NewUnitOfWork())
//            {
//                return ServiceFactory.BannedWordService.GetAll().Where(x => x.Word.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
//            }
//        }

//        public IEnumerable<BannedWord> GetAll()
//        {
//            using (_unitOfWorkManager.NewUnitOfWork())
//            {
//                return ServiceFactory.BannedWordService.GetAll();
//            }
//        }

//        public bool Delete(int id)
//        {
//            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
//            {
//                try
//                {
//                    var email = ServiceFactory.BannedWordService.Get(id);
//                    ServiceFactory.BannedWordService.Delete(email);
//                    unitOfWork.Commit();
//                    return true;
//                }
//                catch (Exception ex)
//                {
//                    unitOfWork.Rollback();
//                    AppHelpers.LogError("BannedEmailApi Delete", ex);
//                    return false;
//                }
//            }
//        }

//        public BannedWord Add(BannedWord email)
//        {
//            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
//            {
//                try
//                {
//                    email.DateAdded = DateTime.Now;
//                    var newEmail = ServiceFactory.BannedWordService.Add(email);
//                    unitOfWork.Commit();
//                    return newEmail;
//                }
//                catch (Exception ex)
//                {
//                    unitOfWork.Rollback();
//                    AppHelpers.LogError("BannedEmailApi Add", ex);
//                    return null;
//                }
//            }
//        }
//    }
//}