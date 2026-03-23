namespace Sln_Lidermax.Dtos
{
    public class RecogerTicketsDto
    {
        public List<TicketSeleccionadoDto> Tickets { get; set; }
    }

    public class TicketSeleccionadoDto
    {
        public int DocEntryTicket { get; set; }
        public int DocEntryHojaRuta { get; set; }
    }

   
}
