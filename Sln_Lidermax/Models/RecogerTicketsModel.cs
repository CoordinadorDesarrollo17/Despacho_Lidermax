namespace Sln_Lidermax.Models
{
    public class RecogerTicketsModel
    {
        public List<TicketSeleccionadoDto> Tickets { get; set; }
    }

    public class TicketSeleccionadoDto
    {
        public int DocEntryTicket { get; set; }
        public int DocEntryHojaRuta { get; set; }
        public int DocNumTicket { get; set; }   
        public int Linea { get ; set; } 

        public DateTime Fecha { get; set; } 
        public string? Observacion { get; set; }

        public bool Excluido { get; set; } = false;
        public double? MontoFlete { get; set; }

        public int IdRol { get; set; }
        public string EntregaPedido { get; set; }
    }

   
}
