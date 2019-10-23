using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class CustomVM
    {
    public long ID                {get;set;}
    public string CardNo            {get;set;}
    public string CardStatusActive  {get;set;}
    public string CardTypeID        {get;set;}
    public string AccountNo         {get;set;}

    public string CardExpiry        {get;set;}
    public string CIF { get; set; }
    }
}
