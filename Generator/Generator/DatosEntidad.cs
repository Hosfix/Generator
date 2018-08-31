namespace Generator
{
    public class DatosEntidad
    {
        public string Columna { get; set; }

        public string TipoDato { get; set; }

        public bool Nullable { get; set; }

        public bool Visible { get; set; }

        public DatosEntidad()
        {
            TipoDato = "String";
        }
    }
}
