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
            listaTipoDatos.Add("String");
            listaTipoDatos.Add("Int");
            listaTipoDatos.Add("Decimal");
            listaTipoDatos.Add("Double");
            listaTipoDatos.Add("DateTime");
            listaTipoDatos.Add("Bool");

            repositoryItemLookUpEditTipoDato.DataSource = listaTipoDatos;
        }
    }
}
