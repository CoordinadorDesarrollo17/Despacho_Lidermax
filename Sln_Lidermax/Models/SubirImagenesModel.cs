namespace Sln_Lidermax.Models
{
    public class SubirImagenesModel
    {
        public int Linea { get; set; }
        public int DocEntryHojaRuta {  get; set; }  
        public int DocEntryTicket { get; set; }
        public int DocNumTicket { get; set; } 
        public IFormFile Img1 { get; set; }
        public IFormFile Img2 { get; set; }
    }
}
