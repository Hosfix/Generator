using DevExpress.XtraEditors;
using System;
using System.Windows.Forms;

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
            Cursor = System.Windows.Forms.Cursors.WaitCursor;

            if (Comprobaciones())
                GenerarDocumentos();

            Cursor = System.Windows.Forms.Cursors.Default;
        }

        private void lookUpEditColumnaBaja_EditValueChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty((lookUpEditColumnaBaja.EditValue as String)))
            {
                var columna = lookUpEditColumnaBaja.EditValue as String;
                var fila = _listaDatos.Find(d => d.Columna == columna);
                if (!fila.Nullable || fila.TipoDato != "DateTime")
                {
                    if (XtraMessageBox.Show("El dato seleccionado como Columna de baja no es nullable o no es de tipo fecha. ¿Desea que se corrija automáticamente? En caso contrario se cancelará la selección.", "Alerta", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        fila.Nullable = true;
                        fila.TipoDato = "DateTime";
                        gridControlDatos.RefreshDataSource();
                    }
                    else
                    {
                        lookUpEditColumnaBaja.EditValue = null;
                    }
                }
            }
        }
    }
}
