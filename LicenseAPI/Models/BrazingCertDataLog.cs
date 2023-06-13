using System;
using System.Collections.Generic;

namespace LicenseAPI.Models;

public partial class BrazingCertDataLog
{
    public int Id { get; set; }

    public string? EmpCode { get; set; }

    public string? StationNo { get; set; }

    public string? BrazingNo { get; set; }

    public int? CountFg { get; set; }

    public int? CountNg { get; set; }

    public DateTime? UpdateDate { get; set; }

    public DateTime? Pddate { get; set; }

    public string? Pdshift { get; set; }

    public DateTime? Expdate { get; set; }

    public string? Line { get; set; }
}
