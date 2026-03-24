namespace Sln_Lidermax.Dtos
{
    public class FiltrosTicketsDto
    {
        public int? DocEntryTicket {  get; set; }   
        public Boolean? esHojaRuta {  get; set; }  
        public int? DocNumHojaRuta { get; set; }    
        public int? DocEntryHojaRuta { get; set; }
        public string? Buscar { get; set; }

        public string? Estado { get; set; }
        public DateTime? FechaDespacho { get; set; }
        public PaginacionDto Paginacion { get; set; } = new();
    }
}
