using System;
using System.Collections.Generic;

namespace LicenseAPI.Models;

public partial class SkcUserInRole
{
    public string PriRole { get; set; } = null!;

    public string PriEmpcode { get; set; } = null!;

    public DateTime? CreateDate { get; set; }
}
