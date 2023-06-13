using System;
using System.Collections.Generic;

namespace LicenseAPI.Models;

public partial class SkcDictMstr
{
    public int DictId { get; set; }

    public string? DictType { get; set; }

    public string? Code { get; set; }

    public string? DictDesc { get; set; }

    public string? RefCode { get; set; }

    public string? RefItem { get; set; }

    public string? Note { get; set; }

    public DateTime CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool? DictStatus { get; set; }
}
