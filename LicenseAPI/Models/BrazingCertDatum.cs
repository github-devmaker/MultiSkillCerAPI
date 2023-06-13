using System;
using System.Collections.Generic;

namespace LicenseAPI.Models;

public partial class BrazingCertDatum
{
    public string Empcode { get; set; } = null!;

    public string? BrazingNo { get; set; }

    public string? SerialLastUpdate { get; set; }

    public DateTime? LastInsertDate { get; set; }

    public int? CountFg { get; set; }

    public int? CountNg { get; set; }

    public string? SerialNgupdate { get; set; }

    public string? LeakPointNg { get; set; }

    public DateTime? NgupdateDate { get; set; }

    public DateTime? Pddate { get; set; }

    public string? Pdshift { get; set; }

    public DateTime? Expdate { get; set; }

    public string? Line { get; set; }
}
