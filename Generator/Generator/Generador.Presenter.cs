using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Generator
{
    public partial class Generador
    {
        private void ComprobarNombreColumna(DatosEntidad linea)
        {
            if (linea == null) return;
            if (String.IsNullOrEmpty(linea.Columna))
            {
                e.Valid = false;
                e.ErrorText = "No ha rellenado el nombre de la columna";
            }
            else if (_listaDatos.FindAll(d => d.Columna.ToUpper() == linea.Columna.ToUpper()).Count > 1)
            {
                e.Valid = false;
                e.ErrorText = "El nombre de la columna está repetido";
            }
        }

        private bool Comprobaciones()
        {
            bool resultado = false;
            var datosGrid = (gridViewDatos.DataSource as List<DatosEntidad>);

            if (String.IsNullOrEmpty((textEditNombreMaestro.EditValue as String)))
                XtraMessageBox.Show("Debe rellenar el nombre del maestro", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if ((bool)checkEditEntidad.EditValue && String.IsNullOrEmpty((textEditNombreMaestro.EditValue as String)))
                XtraMessageBox.Show("Debe rellenar el nombre de la entidad si quiere generarla", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if ((bool)checkEditBaja.EditValue && String.IsNullOrEmpty((lookUpEditColumnaBaja.EditValue as String)))
                XtraMessageBox.Show("Debe seleccionar la columna que contendrá el dato de fecha baja", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (datosGrid == null || datosGrid.Count <= 0)
                XtraMessageBox.Show("No ha añadido ninguna columna", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                resultado = true;

            return resultado;
        }

        private void GenerarDocumentos()
        {
            throw new NotImplementedException();
        }
    }
}
