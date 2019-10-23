using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class ConfigurationVM
    {
        public int ID { get; set; }
        public string Title       {get;set;}
        public string Description {get;set;}
        public string FED         {get;set;}
        public bool? IsActive    {get;set;}
    }
}
