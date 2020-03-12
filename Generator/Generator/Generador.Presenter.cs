using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Linq;

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
            else if (linea.Columna.Any(Char.IsWhiteSpace))
            {
                e.Valid = false;
                e.ErrorText = "El nombre de la propiedad no puede tener espacios";
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

            if (String.IsNullOrEmpty(textEditNombreMaestro.Text))
                XtraMessageBox.Show("Debe rellenar el nombre del maestro", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (checkEditEntidad.Checked && String.IsNullOrEmpty(lookUpEditPais.Text))
                XtraMessageBox.Show("Debe seleccionar el pais del maestro para poder generar el constructor que recoge los datos del IDataReader", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (checkEditBaja.Checked && String.IsNullOrEmpty(lookUpEditColumnaBaja.Text))
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
                    GenerarPresenter(fbd.SelectedPath);
                    GenerarDesigner(fbd.SelectedPath);
                    XtraMessageBox.Show("Maestro generado", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }


        }

        private void GenerarEntidad(string ruta)
        {
            if (!checkEditEntidad.Checked) return;
            string nombreMaestro = textEditNombreMaestro.Text;
            string nombreEntidad = nombreMaestro + "Entidad";
            string path = ruta + "\\" + nombreEntidad + ".cs";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                var pais = lookUpEditPais.Text;
                if (pais == "Colombia")
                    sw.WriteLine("using HM.Comun.Persistencia.Entidad;"); //COL
                else if (pais == "Chile")
                    sw.WriteLine("using HM.MMS.ContratosInterfaces.DAL;"); //CHL
                else
                    sw.WriteLine("using HM.Comun.Persistencia.DAL;"); //Mexico

                sw.WriteLine("using System;");
                sw.WriteLine("using System.Data;");
                sw.WriteLine("using System.Runtime.Serialization;");
                sw.WriteLine("namespace " + nombreMaestro);
                sw.WriteLine("{");
                sw.WriteLine(" [Serializable]");
                sw.WriteLine(" [DataContract]");
                sw.WriteLine("  public class " + nombreEntidad);
                sw.WriteLine("  {");
                foreach (var dato in _listaDatos)
                {
                    sw.WriteLine("      [DataMember]");
                    if (dato.Nullable && dato.TipoDato != "string" && dato.TipoDato != "bool")
                        sw.WriteLine("      public " + dato.TipoDato + "? " + dato.Columna + " { get; set; }");
                    else
                        sw.WriteLine("      public " + dato.TipoDato + " " + dato.Columna + " { get; set; }");
                }
                sw.WriteLine("      [DataMember]");
                sw.WriteLine("      public EstadoFila Estado { get; set; }");
                sw.WriteLine("");
                sw.WriteLine("      public enum EstadoFila");
                sw.WriteLine("      {");
                sw.WriteLine("          Modificada,");
                sw.WriteLine("          Nueva,");
                sw.WriteLine("          Eliminada,");
                sw.WriteLine("          Ninguno");
                sw.WriteLine("      }");
                sw.WriteLine("");
                sw.WriteLine("      public " + nombreEntidad + "()");
                sw.WriteLine("      {");
                sw.WriteLine("          Estado = EstadoFila.Nueva;");
                sw.WriteLine("      }");
                sw.WriteLine("");
                sw.WriteLine("      public " + nombreEntidad + "(IDataReader reader)");
                sw.WriteLine("      {");
                sw.WriteLine("          Estado = EstadoFila.Ninguno;");
                foreach (var dato in _listaDatos)
                {
                    if (dato.Nullable && dato.TipoDato != "string" && dato.TipoDato != "bool")
                        sw.WriteLine("          " + dato.Columna + " = reader.GetValue<" + dato.TipoDato + "?>(\"" + dato.Columna.ToUpper() + "\");");
                    else
                        sw.WriteLine("          " + dato.Columna + " = reader.GetValue<" + dato.TipoDato + ">(\"" + dato.Columna.ToUpper() + "\");");
                }
                sw.WriteLine("      }");
                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarClaseBase(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.Text;
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

                if (checkEditEntidad.Checked)
                    sw.WriteLine("      private List<" + nombreEntidad + "> _lista" + nombreEntidad + " = new List<" + nombreEntidad + ">();");

                sw.WriteLine("      public " + nombreMaestro + " ()");
                sw.WriteLine("      {");
                sw.WriteLine("          InitializeComponent();");
                if (checkEditEntidad.Checked)
                    sw.WriteLine("          ObtenerDatosGrid();");
                sw.WriteLine("      }");

                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarAction(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.Text;
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

                sw.WriteLine("      private void gridView" + nombreMaestro + "_ValidateRow(object sender, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)");
                sw.WriteLine("      {");
                sw.WriteLine("          ValidarColumnas(e.Row as " + nombreEntidad + ", e);");
                sw.WriteLine("      }");

                sw.WriteLine("      private void cmdAceptar_Click(object sender, EventArgs e)");
                sw.WriteLine("      {");
                sw.WriteLine("          Guardar();");
                sw.WriteLine("      }");

                sw.WriteLine("      private void cmdCancelar_Click(object sender, EventArgs e)");
                sw.WriteLine("      {");
                sw.WriteLine("          RefrescarDataSource();");
                sw.WriteLine("      }");

                sw.WriteLine("      private void cmdSalir_Click(object sender, EventArgs e)");
                sw.WriteLine("      {");
                sw.WriteLine("          Close();");
                sw.WriteLine("      }");

                sw.WriteLine("      private void gridView" + nombreMaestro + "_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)");
                sw.WriteLine("      {");
                sw.WriteLine("          if ((" + nombreEntidad + ".EstadoFila)gridView" + nombreMaestro + ".GetRowCellValue(e.RowHandle, \"Estado\") == " + nombreEntidad + ".EstadoFila.Ninguno)");
                sw.WriteLine("          {");
                sw.WriteLine("              gridView" + nombreMaestro + ".SetRowCellValue(e.RowHandle, \"Estado\", " + nombreEntidad + ".EstadoFila.Modificada);");
                sw.WriteLine("          }"); 
                sw.WriteLine("      }");

                if (checkEditBaja.Checked || checkEditAgregar.Checked || checkEditEliminarFila.Checked)
                {
                    sw.WriteLine("      private void gridControl" + nombreMaestro + "_EmbeddedNavigator_ButtonClick(object sender, DevExpress.XtraEditors.NavigatorButtonClickEventArgs e)");
                    sw.WriteLine("      {");
                    if (checkEditBaja.Checked || checkEditEliminarFila.Checked)
                    {
                        sw.WriteLine("          if (e.Button.ButtonType == DevExpress.XtraEditors.NavigatorButtonType.Remove)");
                        sw.WriteLine("          {");
                        if (checkEditBaja.Checked)
                        {
                            sw.WriteLine("              foreach (int fila in gridView" + nombreMaestro + ".GetSelectedRows())");
                            sw.WriteLine("                  gridView" + nombreMaestro + ".SetRowCellValue(fila, \"" + lookUpEditColumnaBaja.Text + "\", DateTime.Now);");
                            sw.WriteLine("                  gridControl" + nombreMaestro + ".RefreshDataSource();");
                            sw.WriteLine("              e.Handled = true;");
                        }
                        else
                        {
                            sw.WriteLine("              foreach (int fila in gridView" + nombreMaestro + ".GetSelectedRows())");
                            sw.WriteLine("                  gridView" + nombreMaestro + ".SetRowCellValue(fila, \"Estado\", " + nombreEntidad + ".EstadoFila.Eliminada);");
                            sw.WriteLine("                  gridControl" + nombreMaestro + ".RefreshDataSource();");
                            sw.WriteLine("              e.Handled = true;");
                        }
                        sw.WriteLine("          }");
                    }
                    if (checkEditAgregar.Checked)
                    {
                        sw.WriteLine("          if (e.Button.ButtonType == DevExpress.XtraEditors.NavigatorButtonType.Append)");
                        sw.WriteLine("          {");
                        sw.WriteLine("              _lista" + nombreEntidad + ".Add(new "+ nombreEntidad + "());");
                        sw.WriteLine("              gridControl" + nombreMaestro + ".RefreshDataSource();");
                        sw.WriteLine("              e.Handled = true;");
                        sw.WriteLine("          }");
                    }
                    sw.WriteLine("      }");
                }

                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarPresenter(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.Text;
            string nombreEntidad = nombreMaestro + "Entidad";
            string path = ruta + "\\" + nombreMaestro + ".Presenter.cs";

            if (File.Exists(path))
                File.Delete(path);

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Collections.Generic;");
                sw.WriteLine("using System.ComponentModel;");
                sw.WriteLine("using System.Windows.Forms;");
                sw.WriteLine("using DevExpress.XtraEditors;");
                sw.WriteLine("using HM.MMS.Agentes;");

                sw.WriteLine("namespace " + nombreMaestro);
                sw.WriteLine("{");
                sw.WriteLine("  public partial class " + nombreMaestro);
                sw.WriteLine("  {");
                var datosValidador = _listaDatos.FindAll(d => d.Validador);
                if (datosValidador.Count > 0)
                {
                    sw.WriteLine("      private void ValidarColumnas(" + nombreEntidad + " linea, DevExpress.XtraGrid.Views.Base.ValidateRowEventArgs e)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          if (linea == null) return;");
                    foreach (var dato in datosValidador)
                    {
                        if (dato.TipoDato == "string")
                        {
                            sw.WriteLine("          if (String.IsNullOrEmpty(linea." + dato.Columna + "))");
                            sw.WriteLine("          {");
                            sw.WriteLine("              e.Valid = false;");
                            sw.WriteLine("              e.ErrorText = \"No ha rellenado el nombre de la columna\";");
                            sw.WriteLine("          }");
                        }
                        else if (dato.TipoDato != "string" && dato.TipoDato != "bool" && dato.Nullable)
                        {
                            sw.WriteLine("          if (linea." + dato.Columna + " == null)");
                            sw.WriteLine("          {");
                            sw.WriteLine("              e.Valid = false;");
                            sw.WriteLine("              e.ErrorText = \"No ha rellenado el nombre de la columna\";");
                            sw.WriteLine("          }");
                        }
                    }
                    sw.WriteLine("      }");
                }

                sw.WriteLine("      private void RefrescarDataSource()");
                sw.WriteLine("      {");

                if (checkEditEntidad.Checked)
                    sw.WriteLine("          _lista" + nombreEntidad + " = new List<" + nombreEntidad + ">();");

                sw.WriteLine("          ObtenerDatosGrid();");

                sw.WriteLine("      }");

                sw.WriteLine("      private void Guardar()");
                sw.WriteLine("      {");
                sw.WriteLine("          Cursor.Current = Cursors.WaitCursor;");

                sw.WriteLine("          try");
                sw.WriteLine("          {");
                sw.WriteLine("              MaestroAgent.Guardar" + nombreMaestro + "(_lista" + nombreEntidad + ");");
                sw.WriteLine("              RefrescarDataSource();");
                sw.WriteLine("              XtraMessageBox.Show(\"Guardado Correctamente.\", \"Informacion\", MessageBoxButtons.OK, MessageBoxIcon.Information);");
                sw.WriteLine("          }");
                sw.WriteLine("          catch (Exception ex)");
                sw.WriteLine("          {");
                sw.WriteLine("              XtraMessageBox.Show(\"Se ha producido un error \" + ex.Message);");
                sw.WriteLine("              throw ex;");
                sw.WriteLine("          }");
                sw.WriteLine("          finally");
                sw.WriteLine("          {");
                sw.WriteLine("              Cursor.Current = Cursors.Default;");
                sw.WriteLine("          }");
                sw.WriteLine("      }");

                sw.WriteLine("      private void ObtenerDatosGrid()");
                sw.WriteLine("          {");
                sw.WriteLine("              Cursor.Current = Cursors.WaitCursor;");

                sw.WriteLine("          try");
                sw.WriteLine("          {");
                sw.WriteLine("              _lista" + nombreEntidad + " = MaestroAgent.Obtener" + nombreMaestro + "();");
                sw.WriteLine("              gridControl" + nombreMaestro + ".DataSource = new BindingList<" + nombreEntidad + ">(_lista" + nombreEntidad + ");");
                sw.WriteLine("              gridControl" + nombreMaestro + ".RefreshDataSource();");
                sw.WriteLine("          }");
                sw.WriteLine("          catch (Exception ex)");
                sw.WriteLine("          {");
                sw.WriteLine("              XtraMessageBox.Show(\"Se ha producido un error al obtener los datos \" + ex.Message);");
                sw.WriteLine("              throw ex;");
                sw.WriteLine("          }");
                sw.WriteLine("          finally");
                sw.WriteLine("          {");
                sw.WriteLine("              Cursor.Current = Cursors.Default;");
                sw.WriteLine("          }");

                sw.WriteLine("      }");
                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }

        private void GenerarDesigner(string ruta)
        {
            string nombreMaestro = textEditNombreMaestro.Text;
            string nombreEntidad = nombreMaestro + "Entidad";
            string path = ruta + "\\" + nombreMaestro + ".Designer.cs";
            var gridControlName = "this.gridControl" + nombreMaestro;
            var gridViewName = "this.gridView" + nombreMaestro;

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
                sw.WriteLine("          this.components = new System.ComponentModel.Container();");
                sw.WriteLine("          this.cmdSalir = new DevExpress.XtraEditors.SimpleButton();");
                sw.WriteLine("          this.cmdCancelar = new DevExpress.XtraEditors.SimpleButton();");
                sw.WriteLine("          this.cmdAceptar = new DevExpress.XtraEditors.SimpleButton();");
                sw.WriteLine("          " + gridControlName + " = new DevExpress.XtraGrid.GridControl();");
                sw.WriteLine("          " + gridViewName + " = new DevExpress.XtraGrid.Views.Grid.GridView();");
                sw.WriteLine("          this.gridColumnEstado = new DevExpress.XtraGrid.Columns.GridColumn();");
                
                foreach (var dato in _listaDatos)
                    sw.WriteLine("          this.gridColumn" + dato.Columna + " = new DevExpress.XtraGrid.Columns.GridColumn();");

                sw.WriteLine("          this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl(); ");
                sw.WriteLine("          this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup(); ");
                sw.WriteLine("          this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem(); ");
                sw.WriteLine("          this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem(); ");
                sw.WriteLine("          this.layoutControlItem3 = new DevExpress.XtraLayout.LayoutControlItem(); ");
                sw.WriteLine("          this.layoutControl2 = new DevExpress.XtraLayout.LayoutControl(); ");
                sw.WriteLine("          this.layoutControlGroup2 = new DevExpress.XtraLayout.LayoutControlGroup(); ");
                sw.WriteLine("          this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem(); ");
                sw.WriteLine("          this.emptySpaceItem1 = new DevExpress.XtraLayout.EmptySpaceItem(); ");

                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(gridControl" + nombreMaestro + ")).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(gridView" + nombreMaestro + ")).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();");
                sw.WriteLine("          this.layoutControl1.SuspendLayout();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).BeginInit();");
                sw.WriteLine("          this.layoutControl2.SuspendLayout();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).BeginInit();");
                sw.WriteLine("          this.SuspendLayout();");

                sw.WriteLine("          // ");
                sw.WriteLine("          // " + gridControlName);
                sw.WriteLine("          // ");
                sw.WriteLine("          " + gridControlName + ".UseEmbeddedNavigator = true;");
                if (!checkEditAgregar.Checked)
                    sw.WriteLine("          " + gridControlName + ".EmbeddedNavigator.Buttons.Append.Enabled = false;");

                if (checkEditBaja.Checked || checkEditAgregar.Checked)
                    sw.WriteLine("          " + gridControlName + ".EmbeddedNavigator.ButtonClick += new DevExpress.XtraEditors.NavigatorButtonClickEventHandler(gridControl" + nombreMaestro + "_EmbeddedNavigator_ButtonClick);");
                sw.WriteLine("          " + gridControlName + ".Location = new System.Drawing.Point(12, 12);");
                sw.WriteLine("          " + gridControlName + ".MainView = gridView" + nombreMaestro + ";");
                sw.WriteLine("          " + gridControlName + ".Name = \"gridControl" + nombreMaestro + "\";");
                sw.WriteLine("          " + gridControlName + ".Size = new System.Drawing.Size(728, 528);");
                sw.WriteLine("          " + gridControlName + ".TabIndex = 50;");
                sw.WriteLine("          " + gridControlName + ".ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {");
                sw.WriteLine("          " + gridViewName + "});");
                sw.WriteLine("          // ");
                sw.WriteLine("          // " + gridViewName);
                sw.WriteLine("          // ");
                sw.WriteLine("          " + gridViewName + ".Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {");

                foreach (var dato in _listaDatos)
                    sw.WriteLine("          this.gridColumn" + dato.Columna + ",");
                sw.WriteLine("          this.gridColumnEstado");
                sw.WriteLine("          });");

                sw.WriteLine("          " + gridViewName + ".GridControl = "+ gridControlName + ";");
                sw.WriteLine("          " + gridViewName + ".Name = \"grvView\";");
                sw.WriteLine("          " + gridViewName + ".RowHeight = 16;");
                sw.WriteLine("          " + gridViewName + ".OptionsView.ShowGroupPanel = false;");
                sw.WriteLine("          " + gridViewName + ".CellValueChanged += new DevExpress.XtraGrid.Views.Base.CellValueChangedEventHandler(" + gridViewName + "_CellValueChanged);");
                sw.WriteLine("          // ");
                sw.WriteLine("          // cmdAceptar");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.cmdAceptar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));");
                sw.WriteLine("          this.cmdAceptar.Location = new System.Drawing.Point(503, 546);");
                sw.WriteLine("          this.cmdAceptar.Name = \"cmdAceptar\";");
                sw.WriteLine("          this.cmdAceptar.Size = new System.Drawing.Size(75, 23);");
                sw.WriteLine("          this.cmdAceptar.StyleController = this.layoutControl1;");
                sw.WriteLine("          this.cmdAceptar.TabIndex = 52;");
                sw.WriteLine("          this.cmdAceptar.Text = \"Aceptar\";");
                sw.WriteLine("          this.cmdAceptar.Click += new System.EventHandler(this.cmdAceptar_Click);");
                sw.WriteLine("          // ");
                sw.WriteLine("          // cmdSalir");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.cmdSalir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));");
                sw.WriteLine("          this.cmdSalir.Location = new System.Drawing.Point(665, 545);");
                sw.WriteLine("          this.cmdSalir.Name = \"cmdSalir\";");
                sw.WriteLine("          this.cmdSalir.Size = new System.Drawing.Size(75, 23);");
                sw.WriteLine("          this.cmdSalir.StyleController = this.layoutControl1;");
                sw.WriteLine("          this.cmdSalir.TabIndex = 53;");
                sw.WriteLine("          this.cmdSalir.Text = \"Salir\";");
                sw.WriteLine("          this.cmdSalir.Click += new System.EventHandler(this.cmdSalir_Click);");
                sw.WriteLine("          // ");
                sw.WriteLine("          // cmdCancelar");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.cmdCancelar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));");
                sw.WriteLine("          this.cmdCancelar.Location = new System.Drawing.Point(584, 545);");
                sw.WriteLine("          this.cmdCancelar.Name = \"cmdCancelar\";");
                sw.WriteLine("          this.cmdCancelar.Size = new System.Drawing.Size(75, 23);");
                sw.WriteLine("          this.cmdCancelar.StyleController = this.layoutControl1;");
                sw.WriteLine("          this.cmdCancelar.TabIndex = 54;");
                sw.WriteLine("          this.cmdCancelar.Text = \"Cancelar\";");
                sw.WriteLine("          this.cmdCancelar.Click += new System.EventHandler(this.cmdCancelar_Click);");

                var count = 0;
                foreach (var dato in _listaDatos)
                {
                    sw.WriteLine("          // ");
                    sw.WriteLine("          // gridColumn" + dato.Columna);
                    sw.WriteLine("          // ");
                    sw.WriteLine("          this.gridColumn" + dato.Columna + ".Caption = \"" + dato.Columna + "\";");
                    sw.WriteLine("          this.gridColumn" + dato.Columna + ".FieldName = \"" + dato.Columna + "\";");
                    sw.WriteLine("          this.gridColumn" + dato.Columna + ".Name = \"gridColumn" + dato.Columna + "\";");
                    sw.WriteLine("          this.gridColumn" + dato.Columna + ".Visible = " + dato.Visible.ToString().ToLower() + ";");
                    if (dato.Visible)
                        sw.WriteLine("          this.gridColumn" + dato.Columna + ".VisibleIndex = " + count + ";");
                    sw.WriteLine("          this.gridColumn" + dato.Columna + ".OptionsColumn.AllowEdit = " + dato.Editable.ToString().ToLower() + ";");
                    count++;
                }

                sw.WriteLine("          // ");
                sw.WriteLine("          // gridColumnEstado");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.gridColumnEstado.Caption = \"Estado\";");
                sw.WriteLine("          this.gridColumnEstado.FieldName = \"Estado\";");
                sw.WriteLine("          this.gridColumnEstado.Name = \"gridColumnEstado\";");
                sw.WriteLine("          this.gridColumnEstado.Visible = false;");

                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControl1");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControl1.Controls.Add(this.cmdSalir);");
                sw.WriteLine("          this.layoutControl1.Controls.Add(this.cmdCancelar);");
                sw.WriteLine("          this.layoutControl1.Controls.Add(this.cmdAceptar);");
                sw.WriteLine("          this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Bottom;");
                sw.WriteLine("          this.layoutControl1.Location = new System.Drawing.Point(0, 540);");
                sw.WriteLine("          this.layoutControl1.Name = \"layoutControl1\"; ");
                sw.WriteLine("          this.layoutControl1.Root = this.layoutControlGroup1;");
                sw.WriteLine("          this.layoutControl1.Size = new System.Drawing.Size(752, 46);");
                sw.WriteLine("          this.layoutControl1.TabIndex = 55;");
                sw.WriteLine("          this.layoutControl1.Text = \"layoutControl1\";");
                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControlGroup1");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;");
                sw.WriteLine("          this.layoutControlGroup1.GroupBordersVisible = false;");
                sw.WriteLine("          this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {");
                sw.WriteLine("          this.layoutControlItem1,");
                sw.WriteLine("          this.layoutControlItem2,");
                sw.WriteLine("          this.layoutControlItem3,");
                sw.WriteLine("          this.emptySpaceItem1});");
                sw.WriteLine("          this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);");
                sw.WriteLine("          this.layoutControlGroup1.Name = \"layoutControlGroup1\";");
                sw.WriteLine("          this.layoutControlGroup1.Size = new System.Drawing.Size(752, 46);");
                sw.WriteLine("          this.layoutControlGroup1.TextVisible = false;");
                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControlItem1");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControlItem1.Control = this.cmdAceptar;");
                sw.WriteLine("          this.layoutControlItem1.Location = new System.Drawing.Point(495, 0);");
                sw.WriteLine("          this.layoutControlItem1.Name = \"layoutControlItem1\";");
                sw.WriteLine("          this.layoutControlItem1.Size = new System.Drawing.Size(95, 26);");
                sw.WriteLine("          this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);");
                sw.WriteLine("          this.layoutControlItem1.TextVisible = false;");
                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControlItem2");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControlItem2.Control = this.cmdCancelar;");
                sw.WriteLine("          this.layoutControlItem2.Location = new System.Drawing.Point(590, 0);");
                sw.WriteLine("          this.layoutControlItem2.Name = \"layoutControlItem2\"; ");
                sw.WriteLine("          this.layoutControlItem2.Size = new System.Drawing.Size(74, 26);");
                sw.WriteLine("          this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);");
                sw.WriteLine("          this.layoutControlItem2.TextVisible = false;");
                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControlItem3");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControlItem3.Control = this.cmdSalir;");
                sw.WriteLine("          this.layoutControlItem3.Location = new System.Drawing.Point(664, 0);");
                sw.WriteLine("          this.layoutControlItem3.Name = \"layoutControlItem3\";");
                sw.WriteLine("          this.layoutControlItem3.Size = new System.Drawing.Size(68, 26);");
                sw.WriteLine("          this.layoutControlItem3.TextSize = new System.Drawing.Size(0, 0);");
                sw.WriteLine("          this.layoutControlItem3.TextVisible = false;");
                sw.WriteLine("          // ");
                sw.WriteLine("          // emptySpaceItem1");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.emptySpaceItem1.AllowHotTrack = false;");
                sw.WriteLine("          this.emptySpaceItem1.Location = new System.Drawing.Point(0, 0);");
                sw.WriteLine("          this.emptySpaceItem1.Name = \"emptySpaceItem1\"; ");
                sw.WriteLine("          this.emptySpaceItem1.Size = new System.Drawing.Size(495, 26);");
                sw.WriteLine("          this.emptySpaceItem1.TextSize = new System.Drawing.Size(0, 0);");
                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControl2");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControl2.Controls.Add(this.gridControl" + nombreMaestro + ");");
                sw.WriteLine("          this.layoutControl2.Dock = System.Windows.Forms.DockStyle.Fill;");
                sw.WriteLine("          this.layoutControl2.Location = new System.Drawing.Point(0, 0);");
                sw.WriteLine("          this.layoutControl2.Name = \"layoutControl2\"; ");
                sw.WriteLine("          this.layoutControl2.Root = this.layoutControlGroup2;");
                sw.WriteLine("          this.layoutControl2.Size = new System.Drawing.Size(752, 540);");
                sw.WriteLine("          this.layoutControl2.TabIndex = 56;");
                sw.WriteLine("          this.layoutControl2.Text = \"layoutControl2\"; ");
                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControlGroup2");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControlGroup2.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;");
                sw.WriteLine("          this.layoutControlGroup2.GroupBordersVisible = false;");
                sw.WriteLine("          this.layoutControlGroup2.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {");
                sw.WriteLine("          this.layoutControlItem4});");
                sw.WriteLine("          this.layoutControlGroup2.Location = new System.Drawing.Point(0, 0);");
                sw.WriteLine("          this.layoutControlGroup2.Name = \"layoutControlGroup2\"; ");
                sw.WriteLine("          this.layoutControlGroup2.Size = new System.Drawing.Size(752, 540);");
                sw.WriteLine("          this.layoutControlGroup2.TextVisible = false;");
                sw.WriteLine("          // ");
                sw.WriteLine("          // layoutControlItem4");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.layoutControlItem4.Control = this.gridControl" + nombreMaestro + ";");
                sw.WriteLine("          this.layoutControlItem4.Location = new System.Drawing.Point(0, 0);");
                sw.WriteLine("          this.layoutControlItem4.Name = \"layoutControlItem4\";");
                sw.WriteLine("          this.layoutControlItem4.Size = new System.Drawing.Size(732, 520);");
                sw.WriteLine("          this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);");
                sw.WriteLine("          this.layoutControlItem4.TextVisible = false;");



                sw.WriteLine("          // ");
                sw.WriteLine("          // MaestroEspecialidades");
                sw.WriteLine("          // ");
                sw.WriteLine("          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);");
                sw.WriteLine("          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;");
                sw.WriteLine("          this.ClientSize = new System.Drawing.Size(752, 586);");
                sw.WriteLine("          this.Controls.Add(this.layoutControl2);");
                sw.WriteLine("          this.Controls.Add(this.layoutControl1);");
                sw.WriteLine("          this.Name = \"Maestro " + nombreMaestro + "\";");
                sw.WriteLine("          this.Text = \"" + nombreMaestro + "\";");


                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)("+ gridControlName + ")).EndInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)("+ gridViewName + ")).EndInit();");

                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();");
                sw.WriteLine("          this.layoutControl1.ResumeLayout(false);");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem3)).EndInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControl2)).EndInit();");
                sw.WriteLine("          this.layoutControl2.ResumeLayout(false);");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup2)).EndInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();");
                sw.WriteLine("          ((System.ComponentModel.ISupportInitialize)(this.emptySpaceItem1)).EndInit();");

                sw.WriteLine("          this.ResumeLayout(false);");

                sw.WriteLine("      }");

                sw.WriteLine("      private DevExpress.XtraGrid.GridControl gridControl" + nombreMaestro + ";");
                sw.WriteLine("      private DevExpress.XtraGrid.Views.Grid.GridView gridView" + nombreMaestro + ";");
                sw.WriteLine("      private DevExpress.XtraEditors.SimpleButton cmdSalir;");
                sw.WriteLine("      private DevExpress.XtraEditors.SimpleButton cmdCancelar;");
                sw.WriteLine("      private DevExpress.XtraEditors.SimpleButton cmdAceptar;");

                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControl layoutControl1;");
                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;");
                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;");
                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;");
                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControlItem layoutControlItem3;");
                sw.WriteLine("      private DevExpress.XtraLayout.EmptySpaceItem emptySpaceItem1;");
                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControl layoutControl2;");
                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup2;");
                sw.WriteLine("      private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;");

                foreach (var dato in _listaDatos)
                    sw.WriteLine("      private DevExpress.XtraGrid.Columns.GridColumn gridColumn" + dato.Columna + ";");
                sw.WriteLine("      private DevExpress.XtraGrid.Columns.GridColumn gridColumnEstado;");

                sw.WriteLine("  }");
                sw.WriteLine("}");
            }
        }
    }
}