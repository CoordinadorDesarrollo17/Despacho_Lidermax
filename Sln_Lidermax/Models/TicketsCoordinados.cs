namespace Sln_Lidermax.Models
{
    public class TicketsCoordinados
    {
        public int? Linea { get; set; } 
        public int? DocEntryTicket { get; set; }
        public int? DocNumTicket { get; set; }
        public int? DocEntryHojaRuta { get; set; }
        public int? DocNumHojaRuta { get; set; }    
        public DateTime? FechaDocumento { get; set; }
        public string? Socio { get; set; }   
        public string? Placa { get; set; }
        public string? Conductor { get; set; }
        public string? DetallePedido { get; set; }
        public string? Estado { get; set; }
    }
}
