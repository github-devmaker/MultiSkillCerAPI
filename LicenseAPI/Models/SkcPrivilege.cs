using System;
using System.Collections.Generic;

namespace LicenseAPI.Models;

public partial class SkcPrivilege
{
    public string PriRole { get; set; } = null!;

    public string PriProgram { get; set; } = null!;

    public string? PriSearch { get; set; }

    public string? PriAdd { get; set; }

    public string? PriModify { get; set; }

    public string? PriDelete { get; set; }

    public string? PriCreateBy { get; set; }

    public DateTime? PriCreateDate { get; set; }
}
