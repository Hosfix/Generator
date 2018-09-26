using System.Collections.Generic;
using System.ComponentModel;

namespace Generator
{
    public partial class Generador : DevExpress.XtraEditors.XtraForm
    {
        private List<DatosEntidad> _listaDatos = new List<DatosEntidad>();

        public Generador()
        {
            InitializeComponent();

            gridControlDatos.DataSource = new BindingList<DatosEntidad>(_listaDatos);

            lookUpEditColumnaBaja.Properties.DisplayMember = "Columna";
            lookUpEditColumnaBaja.Properties.ValueMember = "Columna";
            lookUpEditColumnaBaja.Properties.DataSource = _listaDatos;

            List<string> listaTipoDatos = new List<string>();
            listaTipoDatos.Add("string");
            listaTipoDatos.Add("int");
            listaTipoDatos.Add("decimal");
            listaTipoDatos.Add("double");
            listaTipoDatos.Add("DateTime");
            listaTipoDatos.Add("bool");
            
            repositoryItemLookUpEditTipoDato.DataSource = listaTipoDatos;
        }
    }
}
