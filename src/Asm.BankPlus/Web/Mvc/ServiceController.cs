using System;
using System.Collections.Generic;
using System.Text;
using Asm.BankPlus.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Asm.BankPlus.Web.Mvc
{
    public class RepositoryController<T> : ControllerBase  where T : DataRepository
    {
        protected T Repository { get; }

        public RepositoryController(T repository)
        {
            Repository = repository;
        }

    }
}
