using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardIssuanceSystem.Core.ViewModel
{
    public class EligibilityVM
    {
    public string IdentificationType { get; set; }
    public string IdentificationNumber { get;set;}
    public string Resident            {get;set;}
    public string Nationality         {get;set;}
    public string SectorCode          {get;set;}
    public string AccountStatus       {get;set;}
    public string FatherName          {get;set;}
    public string MotherName          {get;set;}
    public string DOB                 {get;set;}
    public string PostingRestriction  {get;set;}
    public string OpInstruction { get; set; }
    public string AccountNo{ get; set; }
    }
}
