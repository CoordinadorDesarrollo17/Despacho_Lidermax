namespace Sln_Lidermax.Dtos
{
    public class FiltrosTicketsDto
    {
        public Boolean? esHojaRuta {  get; set; }  
        public int? DocNumHojaRuta { get; set; }    
        public int? DocEntryHojaRuta { get; set; }
        public string? Buscar { get; set; }
        public PaginacionDto Paginacion { get; set; } = new();
    }
}
