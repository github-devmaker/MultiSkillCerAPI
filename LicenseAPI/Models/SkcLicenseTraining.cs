using System;
using System.Collections.Generic;

namespace LicenseAPI.Models;

public partial class SkcLicenseTraining
{
    public int TrId { get; set; }

    public string? Empcode { get; set; }

    public string? DictCode { get; set; }

    public DateTime? EffectiveDate { get; set; }

    public DateTime? ExpiredDate { get; set; }

    public DateTime? AlertDate { get; set; }

    public bool TrStatus { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }
}
