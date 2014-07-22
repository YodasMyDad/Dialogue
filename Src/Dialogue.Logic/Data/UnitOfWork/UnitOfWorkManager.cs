using System;
using System.Data.Entity;
using System.Web;
using Dialogue.Logic.Data.Context;

namespace Dialogue.Logic.Data.UnitOfWork
{
    // http://blogs.msdn.com/b/adonet/archive/2011/01/27/using-dbcontext-in-ef-feature-ctp5-part-1-introduction-and-model.aspx
    // http://blogs.msdn.com/b/adonet/archive/2011/03/15/ef-4-1-code-first-walkthrough.aspx

    public class UnitOfWorkManager : IDisposable
    {
        private bool _isDisposed;
        private readonly DatabaseContext _context;

        public UnitOfWorkManager(DatabaseContext context)
        {
            // http://stackoverflow.com/questions/3552000/entity-framework-code-only-error-the-model-backing-the-context-has-changed-sinc
            
            Database.SetInitializer<DatabaseContext>(null);

            _context = context as DatabaseContext;
        }

        /// <summary>
        /// Provides an instance of a unit of work. This wrapping in the manager
        /// class helps keep concerns separated
        /// </summary>
        /// <returns></returns>
        public UnitOfWork NewUnitOfWork()
        {
            return new UnitOfWork(_context);
        }

        /// <summary>
        /// Make sure there are no open sessions.
        /// In the web app this will be called when the injected UnitOfWork manager
        /// is disposed at the end of a request.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _context.Dispose();
                _isDisposed = true;
            }
        }


    }
}