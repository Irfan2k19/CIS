using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class ExceptionLogVM
    {
        public string AccountNumber{ get; set; }
        public string CIF { get; set; }
        public string CardNumber { get; set; }
        public string Result { get; set; }
        public string Filename { get; set; }
    }

    public class ExceptionLogReportVM
    {
        public string AccountNumber { get; set; }
        public string CIF { get; set; }
        public string CardNumber { get; set; }
        public string Result { get; set; }
        public string Filename { get; set; }
        public DateTime? Timestamp { get; set; }
    }

    public static class TempStorageVM
    {
        public static string Key { get; set; }
    }
}
