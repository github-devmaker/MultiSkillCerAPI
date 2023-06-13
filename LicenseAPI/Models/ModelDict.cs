namespace LicenseAPI.Models
{
    public class ModelDict
    {
        public string? fac { get; set; }
        public string? line { get; set; }
        public string? station { get; set; }
        public string? license { get; set; }
        public string? type { get; set; }
        public string? state { get; set; }
        public string? code { get; set; } 

        public string? desc { get; set; }
        
        public string? refCode { get; set; }

        public string? refItem {  get; set; }

        public int? dictId { get; set; }
        public string? dictDesc { get; set; }

        public string? dictType { get; set; }
        public string? empCode { get; set; }
    }
}
