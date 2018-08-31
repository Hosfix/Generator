using System;

namespace Generator
{
    public partial class Generador
    {
        private void checkEditBaja_CheckedChanged(object sender, EventArgs e)
        {
            lookUpEditColumnaBaja.Enabled = (sender as DevExpress.XtraEditors.CheckEdit).Checked;
        }

        private void gridViewDatos_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
        {
            ComprobarNombreColumna(e.Row as DatosEntidad, e);
        }

        private void buttonGenerar_Click(object sender, EventArgs e)
        {
            if (Comprobaciones())
                GenerarDocumentos();
        }
    }
}
