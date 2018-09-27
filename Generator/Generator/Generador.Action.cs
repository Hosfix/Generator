using DevExpress.XtraEditors;
using System;
using System.Windows.Forms;

namespace Generator
{
    public partial class Generador
    {
        private void checkEditBaja_CheckedChanged(object sender, EventArgs e)
        {
            lookUpEditColumnaBaja.Enabled = checkEditBaja.Checked;
            if (checkEditBaja.Checked && checkEditEliminarFila.Checked)
            {
                XtraMessageBox.Show("Dar de baja registros es incompatible con eliminarlos", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                checkEditBaja.EditValue = !checkEditBaja.Checked;
            }
        }

        private void checkEditEliminarFila_CheckedChanged(object sender, System.EventArgs e)
        {
            if (checkEditBaja.Checked && checkEditEliminarFila.Checked)
            {
                XtraMessageBox.Show("Eliminar la fila es incompatible con dar de baja los registros", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                checkEditEliminarFila.EditValue = !checkEditEliminarFila.Checked;
            }
        }

        private void checkEditEntidad_CheckedChanged(object sender, System.EventArgs e)
        {
            lookUpEditPais.Enabled = checkEditEntidad.Checked;
        }

        private void gridViewDatos_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
        {
            ComprobarNombreColumna(e.Row as DatosEntidad, e);
        }

        private void buttonGenerar_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = System.Windows.Forms.Cursors.WaitCursor;

                if (Comprobaciones())
                    GenerarDocumentos();

                Cursor = System.Windows.Forms.Cursors.Default;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Error en la generación del maestro: \n " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = System.Windows.Forms.Cursors.Default;
            }
        }

        private void lookUpEditColumnaBaja_EditValueChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(lookUpEditColumnaBaja.Text))
            {
                var columna = lookUpEditColumnaBaja.Text;
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
