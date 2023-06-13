using System;
using System.Collections.Generic;

namespace LicenseAPI.Models;

public partial class PositMstr
{
    public string PositId { get; set; } = null!;

    public string? PositName { get; set; }

    public string? EnableOt1 { get; set; }

    public string? EnableOt15 { get; set; }

    public string? EnableOt2 { get; set; }

    public string? EnableOt3 { get; set; }

    public string? EnableBgtype { get; set; }

    public decimal? SalAvg { get; set; }

    public int? RangeOrder { get; set; }

    public string? Comtype { get; set; }

    public string? Remark { get; set; }

    public string? AddBy { get; set; }

    public DateTime? AddDt { get; set; }

    public string? UpdBy { get; set; }

    public DateTime? UpdDt { get; set; }
}
