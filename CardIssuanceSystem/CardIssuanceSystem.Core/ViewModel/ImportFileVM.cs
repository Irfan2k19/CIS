using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class ImportFileVM
    {
        public string DirectoryName{ get; set; }
        public string FileFullName { get; set; }
        public string FileName { get; set; }
        public DateTime FileCreationTime { get; set; }
        public DateTime FileModifiedTime { get; set; }
    }
}
