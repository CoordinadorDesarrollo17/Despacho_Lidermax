namespace Sln_Lidermax.Dtos
{
    public class SubirImagenesDto
    {
        public int DocEntryHojaRuta {  get; set; }  
        public int DocEntryTicket { get; set; }
        public int DocNumTicket { get; set; } 
        public IFormFile Img1 { get; set; }
        public IFormFile Img2 { get; set; }
    }
}
