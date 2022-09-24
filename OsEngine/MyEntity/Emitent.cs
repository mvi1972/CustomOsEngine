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
            Security = security;   
        }
        public string  NameSec 
        { 
            get=> Security.Name;
        }  

       public Security Security;
    }
}
