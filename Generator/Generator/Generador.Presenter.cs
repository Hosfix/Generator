using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace Generator
{
    public partial class Generador
    {
        private void ComprobarNombreColumna(DatosEntidad linea, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)
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

            if (String.IsNullOrEmpty((textEditNombreMaestro.EditValue as String)))
                XtraMessageBox.Show("Debe rellenar el nombre del maestro", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if ((bool)checkEditEntidad.EditValue && String.IsNullOrEmpty((textEditNombreMaestro.EditValue as String)))
                XtraMessageBox.Show("Debe rellenar el nombre de la entidad si quiere generarla", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if ((bool)checkEditBaja.EditValue && String.IsNullOrEmpty((lookUpEditColumnaBaja.EditValue as String)))
                XtraMessageBox.Show("Debe seleccionar la columna que contendrá el dato de fecha baja", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (_listaDatos.Count <= 0)
                XtraMessageBox.Show("No ha añadido ninguna columna", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
                resultado = true;

            return resultado;
        }

        private void GenerarDocumentos()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    GenerarEntidad(fbd.SelectedPath);
                    GenerarClaseBase(fbd.SelectedPath);
                    GenerarAction(fbd.SelectedPath);
                    //GenerarPresenter(fbd.SelectedPath);
                    //GenerarDesigner(fbd.SelectedPath);
                }
            }


        }

        private void GenerarEntidad(string ruta)
        {
            if (!(bool)checkEditEntidad.EditValue) return;
            string nombreMaestro = textEditNombreMaestro.EditValue.ToString();
            string nombreEntidad = nombreMaestro+"Entidad";
            string path = ruta + "\\" + nombreEntidad + ".cs";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("namespace "+ nombreMaestro);
                sw.WriteLine("{");
                sw.WriteLine(" [Serializable]");
                sw.WriteLine(" [DataContract]");
                sw.WriteLine("  public class "+nombreEntidad);
                sw.WriteLine("  {");
                foreach (var dato in _listaDatos)
                {
                    sw.WriteLine("      [DataMember]");
                    if (dato.Nullable)
                        sw.WriteLine("      public " + dato.TipoDato + "? " + dato.Columna + " { get; set; }");
                    else
                        sw.WriteLine("      public " + dato.TipoDato + " " + dato.Columna + " { get; set; }");
                }
                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarClaseBase(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.EditValue.ToString();
            string nombreEntidad = nombreMaestro + "Entidad";
            string path = ruta + "\\" + nombreMaestro + ".cs";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using System.ComponentModel;");

                sw.WriteLine("namespace " + nombreMaestro);
                sw.WriteLine("{");
                sw.WriteLine("  public partial class " + nombreMaestro + " : DevExpress.XtraEditors.XtraForm");
                sw.WriteLine("  {");

                if ((bool)checkEditEntidad.EditValue)
                    sw.WriteLine("      private List<" + nombreEntidad + "> _lista" + nombreEntidad);

                sw.WriteLine("      public " + nombreMaestro + " ()");
                sw.WriteLine("      {");
                sw.WriteLine("          InitializeComponent();");
                if ((bool)checkEditEntidad.EditValue)
                    sw.WriteLine("          gridControl" + nombreMaestro + ".DataSource = new BindingList<" + nombreEntidad + ">(_lista" + nombreEntidad + ");");
                sw.WriteLine("      }");

                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarAction(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.EditValue.ToString();
            string nombreEntidad = nombreMaestro + "Entidad";
            string path = ruta + "\\" + nombreMaestro + ".Action.cs";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using System.ComponentModel;");

                sw.WriteLine("namespace " + nombreMaestro);
                sw.WriteLine("{");
                sw.WriteLine("  public partial class " + nombreMaestro);
                sw.WriteLine("  {");

                if ((bool)checkEditBaja.EditValue)
                {
                    sw.WriteLine("      private void gridControl" + nombreMaestro + "_EmbeddedNavigator_ButtonClick(object sender, DevExpress.XtraEditors.NavigatorButtonClickEventArgs e)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          if (e.Button.ButtonType == DevExpress.XtraEditors.NavigatorButtonType.Remove)");
                    sw.WriteLine("          {");
                    sw.WriteLine("              foreach (int fila in gridView" + nombreMaestro + ".GetSelectedRows())");
                    sw.WriteLine("                  gridView" + nombreMaestro + ".SetRowCellValue(fila, \"" + lookUpEditColumnaBaja.EditValue.ToString() + "\", DateTime.Now);");
                    sw.WriteLine("          }");
                    sw.WriteLine("          e.Handled = true;");
                    sw.WriteLine("      }");
                }

                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarPresenter(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.EditValue.ToString();
            string nombreEntidad = nombreMaestro + "Entidad";
            string path = ruta + "\\" + nombreMaestro + ".Presenter.cs";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using System.ComponentModel;");

                sw.WriteLine("namespace " + nombreMaestro);
                sw.WriteLine("{");
                sw.WriteLine("  public partial class " + nombreMaestro);
                sw.WriteLine("  {");

                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarDesigner(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.EditValue.ToString();
            string nombreEntidad = nombreMaestro + "Entidad";
            string path = ruta + "\\" + nombreMaestro + ".Designer.cs";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("namespace " + nombreMaestro);
                sw.WriteLine("{");
                sw.WriteLine("  partial class " + nombreMaestro);
                sw.WriteLine("  {");
                sw.WriteLine("      private System.ComponentModel.IContainer components = null;");

                sw.WriteLine("      protected override void Dispose(bool disposing)");
                sw.WriteLine("      {");
                sw.WriteLine("           if (disposing && (components != null))");
                sw.WriteLine("           {");
                sw.WriteLine("              components.Dispose();");
                sw.WriteLine("           }");
                sw.WriteLine("           base.Dispose(disposing);");
                sw.WriteLine("      }");

                sw.WriteLine("      private void InitializeComponent()");
                sw.WriteLine("      {");
                sw.WriteLine("      }");

                sw.WriteLine("      private DevExpress.XtraGrid.GridControl gridControl" + nombreMaestro + ";");
                sw.WriteLine("      private DevExpress.XtraGrid.Views.Grid.GridView gridView"+ nombreMaestro + ";");
                sw.WriteLine("      private DevExpress.XtraEditors.SimpleButton cmdSalir;");
                sw.WriteLine("      private DevExpress.XtraEditors.SimpleButton cmdCancelar;");
                sw.WriteLine("      private DevExpress.XtraEditors.SimpleButton cmdAceptar;");

                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }
    }
}
