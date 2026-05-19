namespace Sln_Lidermax.Models
{
    public class FiltrosTicketsModel
    {
        public int? DocEntryTicket {  get; set; }   
        public Boolean? esHojaRuta {  get; set; }  
        public int? DocNumHojaRuta { get; set; }    
        public int? DocEntryHojaRuta { get; set; }
        public string? Buscar { get; set; }
        public string? Estado { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime? FechaRecojo { get; set; }
        public string? NombreCompleto { get; set; }
        public PaginacionModel Paginacion { get; set; } = new();
    }
}
