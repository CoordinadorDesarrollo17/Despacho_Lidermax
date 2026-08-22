namespace Sln_Lidermax.Models
{
    public class DireccionProvinciaSuelta_E
    {
        public string CardCode { get; set; }
        public string CardName { get; set; }          // Razón Social
        public string Departamento { get; set; }       // CRD1.County
        public string Provincia { get; set; }           // CRD1.City
        public string Distrito { get; set; }            // CRD1.Block
        public string SlpCode { get; set; }             // Vendedor
        public string Vendedor { get; set; }             // Nombre del Vendedor
        public string DireccionEnvio { get; set; }      // ODLN.Address2
        public string NombreTransportista { get; set; } // ODLN.U_SYP_MDNT
        public string NroBultos { get; set; }           // ODLN.U_BPP_NUMBUL
        public string CodigoAlmacen { get; set; }       // ODLN.U_CFR_WHS_NET

        public string Direccion1 { get; set; }
    }
}
