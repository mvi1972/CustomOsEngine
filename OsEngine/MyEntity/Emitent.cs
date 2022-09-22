using OsEngine.Entity;
using OsEngine.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsEngine.MyEntity
{
    /// <summary>
    ///...Бумаги
    /// </summary>
    public class Emitent : BaseVM
    {
        public Emitent (Security security)
        {
            _security = security;   
        }
        public string  NameSec 
        { 
            get=> _security.Name;
        }  

        private Security _security;
    }
}
