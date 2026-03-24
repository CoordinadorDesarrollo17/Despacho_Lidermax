namespace Sln_Lidermax.Dtos
{
    public class TicketsDto
    {
        public int? DocEntryTicket { get; set; }
        public int? DocEntryHojaRuta { get; set; }
        public int? DocNumTicket { get; set; }
        public string? CardCode { get; set; }
        public string? CardName { get; set; }    
        public string? Direccion1 { get; set; }
        public string? Direccion2 { get; set; }  
        public string? Agencia { get;set; }
        public string? ModoEnvio {  get; set; } 
        public DateTime? FechaRecojo { get; set; }   
        public DateTime? FechaDespacho { get; set; }
        public int? Cajas { get; set; }
        public double? Peso { get; set; }    
        public string? Contacto { get; set; }
        public string? Telefono { get; set; }

        public string? Estado { get; set; }

        public string? DistritoTransporte { get; set; } 

        public string? GuiaRemision { get; set; } 

        public string? GuiaTransportista { get; set; }

        public DateTime? FechaDevolucion { get; set; }
        public DateTime? FechaEntrega { get; set; }
    }
}
