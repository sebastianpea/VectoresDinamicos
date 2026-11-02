using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VectoresDinamicos
{
    public partial class FormPrincipal : Form
    {
    private readonly Random generador = new Random(2026);   // semilla fija: resultados repetibles
        private int[] vector = Array.Empty<int>();

        // ---------- Pestaña 1: vector ----------
        private readonly NumericUpDown nudTamano = new() { Minimum = 1, Maximum = 30, Value = 8, Width = 70 };
        private readonly NumericUpDown nudNuevoTamano = new() { Minimum = 1, Maximum = 30, Value = 12, Width = 70 };
        private readonly Button btnGenerar = new() { Text = "Generar vector", AutoSize = true };
        private readonly Button btnResize = new() { Text = "Redimensionar con Array.Resize", AutoSize = true };
        private readonly Button btnManual = new() { Text = "Redimensionar manualmente", AutoSize = true };
        private readonly Label lblInfo = new() { AutoSize = true, Padding = new Padding(4), Text = "Genera un vector para comenzar." };
        private readonly FlowLayoutPanel panelCeldas = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke };
        private readonly TextBox txtBitacora = CrearBitacora();

        // ---------- Pestaña 2: List<int> ----------
        private readonly Button btnList = new() { Text = "Agregar 70 elementos a List<int>", AutoSize = true };
        private readonly DataGridView dgvList = CrearTabla("Elementos (Count)", "Capacidad (Capacity)", "Observación");

        // ---------- Pestaña 3: estrategias ----------
        private readonly Button btnComparar = new() { Text = "Ejecutar comparación (n = 10 000, 20 000 y 40 000)", AutoSize = true };
        private readonly DataGridView dgvEstrategias = CrearTabla("n", "Estrategia", "Tiempo (ms)", "Redimensiones", "Elementos copiados", "Memoria reservada (KB)");

        public FormPrincipal()
        {
            Text = "Tarea 1.2 - Programa 1: vectores redimensionables";
            Size = new Size(1050, 680);
            MinimumSize = new Size(800, 500);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10F);

            var pestanas = new TabControl { Dock = DockStyle.Fill };
            pestanas.TabPages.Add(CrearPestanaVector());
            pestanas.TabPages.Add(CrearPestanaList());
            pestanas.TabPages.Add(CrearPestanaEstrategias());
            Controls.Add(pestanas);

            btnGenerar.Click += BtnGenerar_Click;
            btnResize.Click += (s, e) => Redimensionar(usarArrayResize: true);
            btnManual.Click += (s, e) => Redimensionar(usarArrayResize: false);
            btnList.Click += BtnList_Click;
            btnComparar.Click += BtnComparar_Click;
        }

        // =====================================================================
        //  Construcción de la interfaz
        // =====================================================================

        private TabPage CrearPestanaVector()
        {
            var barra = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            barra.Controls.AddRange(new Control[]
            {
            Etiqueta("Tamaño inicial:"), nudTamano, btnGenerar,
            Etiqueta("     Nuevo tamaño:"), nudNuevoTamano, btnResize, btnManual
            });

            var tabla = TablaVertical(SizeType.AutoSize, SizeType.AutoSize, SizeType.Percent, SizeType.Absolute);
            tabla.RowStyles[3].Height = 170;
            tabla.Controls.Add(barra, 0, 0);
            tabla.Controls.Add(lblInfo, 0, 1);
            tabla.Controls.Add(panelCeldas, 0, 2);
            tabla.Controls.Add(txtBitacora, 0, 3);

            var pagina = new TabPage("1. Generar y redimensionar") { Padding = new Padding(6) };
            pagina.Controls.Add(tabla);
            return pagina;
        }

        private TabPage CrearPestanaList()
        {
            var explicacion = Etiqueta("List<int> guarda sus datos en un arreglo interno. Cuando se llena, crea uno del doble de tamaño y copia los elementos.");
            var tabla = TablaVertical(SizeType.AutoSize, SizeType.AutoSize, SizeType.Percent);
            tabla.Controls.Add(explicacion, 0, 0);
            tabla.Controls.Add(btnList, 0, 1);
            tabla.Controls.Add(dgvList, 0, 2);

            var pagina = new TabPage("2. Crecimiento de List<int>") { Padding = new Padding(6) };
            pagina.Controls.Add(tabla);
            return pagina;
        }

        private TabPage CrearPestanaEstrategias()
        {
            var explicacion = Etiqueta("A (+1): un Array.Resize por cada inserción.   B (x2): se duplica la capacidad solo cuando el arreglo se llena.");
            var nota = Etiqueta("Los tiempos dependen del equipo; las redimensiones, las copias y la memoria reservada son iguales en cualquier computadora.");
            nota.ForeColor = Color.DimGray;

            var tabla = TablaVertical(SizeType.AutoSize, SizeType.AutoSize, SizeType.Percent, SizeType.AutoSize);
            tabla.Controls.Add(explicacion, 0, 0);
            tabla.Controls.Add(btnComparar, 0, 1);
            tabla.Controls.Add(dgvEstrategias, 0, 2);
            tabla.Controls.Add(nota, 0, 3);

            var pagina = new TabPage("3. Comparación de estrategias") { Padding = new Padding(6) };
            pagina.Controls.Add(tabla);
            return pagina;
        }

        // =====================================================================
        //  Eventos
        // =====================================================================

        private void BtnGenerar_Click(object? sender, EventArgs e)
        {
            vector = OperacionesVector.Generar((int)nudTamano.Value, generador);
            DibujarVector(primerNuevo: vector.Length);
            lblInfo.Text = $"Vector generado. Longitud: {vector.Length}";
            Escribir($"Generar: se reservó en tiempo de ejecución un int[{vector.Length}] -> [ {string.Join(", ", vector)} ]");
        }

        private void Redimensionar(bool usarArrayResize)
        {
            if (vector.Length == 0)
            {
                MessageBox.Show("Primero genera un vector.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int nuevoTamano = (int)nudNuevoTamano.Value;
            if (nuevoTamano == vector.Length)
            {
                MessageBox.Show("El nuevo tamaño es igual al actual.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int[] anterior = vector;   // se guarda la referencia para compararla después
            if (usarArrayResize)
                Array.Resize(ref vector, nuevoTamano);
            else
                vector = OperacionesVector.RedimensionarManual(vector, nuevoTamano);

            bool mismoBloque = ReferenceEquals(anterior, vector);
            string metodo = usarArrayResize ? "Array.Resize" : "Método manual";
            DibujarVector(primerNuevo: anterior.Length);

            lblInfo.Text = $"{metodo}: longitud {anterior.Length} -> {vector.Length}.   ¿Es el mismo bloque de memoria? {(mismoBloque ? "Sí" : "No")}";
            Escribir($"{metodo}: {anterior.Length} -> {vector.Length}. ¿Mismo arreglo? {mismoBloque}. Contenido: [ {string.Join(", ", vector)} ]");

            if (vector.Length > anterior.Length)
                Escribir($"   Se agregaron {vector.Length - anterior.Length} posiciones con el valor por defecto 0 (en verde).");
            else
                Escribir($"   Se perdieron {anterior.Length - vector.Length} elementos que ya no cabían.");
        }

        private void BtnList_Click(object? sender, EventArgs e)
        {
            dgvList.Rows.Clear();
            foreach (var (elementos, capacidad) in OperacionesVector.CrecimientoDeList(70))
            {
                string observacion = elementos == 0 ? "Lista recién creada" : "Se redimensionó (nuevo arreglo interno)";
                dgvList.Rows.Add(elementos, capacidad, observacion);
            }
        }

        private async void BtnComparar_Click(object? sender, EventArgs e)
        {
            btnComparar.Enabled = false;
            UseWaitCursor = true;
            dgvEstrategias.Rows.Clear();

            try
            {
                foreach (int n in new[] { 10_000, 20_000, 40_000 })
                {
                    foreach (bool duplicar in new[] { false, true })
                    {
                        // Se mide en un hilo secundario para que la ventana no se congele.
                        ResultadoEstrategia r = await Task.Run(() => OperacionesVector.Medir(n, duplicar));

                        int fila = dgvEstrategias.Rows.Add(r.N.ToString("N0"), r.Estrategia, r.TiempoMs.ToString("F2"),
                            r.Redimensiones.ToString("N0"), r.Copias.ToString("N0"), r.MemoriaKB.ToString("N0"));
                        dgvEstrategias.Rows[fila].DefaultCellStyle.BackColor = duplicar ? Color.Honeydew : Color.MistyRose;
                    }
                }
            }
            finally
            {
                btnComparar.Enabled = true;
                UseWaitCursor = false;
            }
        }

        // =====================================================================
        //  Auxiliares
        // =====================================================================

        /// <summary>Dibuja cada posición del vector como una celda; las posiciones nuevas se pintan de verde.</summary>
        private void DibujarVector(int primerNuevo)
        {
            panelCeldas.SuspendLayout();
            panelCeldas.Controls.Clear();
            for (int i = 0; i < vector.Length; i++)
            {
                panelCeldas.Controls.Add(new Label
                {
                    Text = $"[{i}]{Environment.NewLine}{vector[i]}",
                    Size = new Size(60, 52),
                    Margin = new Padding(3),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = i >= primerNuevo ? Color.LightGreen : Color.White
                });
            }
            panelCeldas.ResumeLayout();
        }

        private void Escribir(string mensaje) => txtBitacora.AppendText(mensaje + Environment.NewLine);

        private static Label Etiqueta(string texto) =>
            new() { Text = texto, AutoSize = true, Margin = new Padding(3, 9, 3, 3) };

        private static TextBox CrearBitacora() => new()
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Consolas", 10F),
            BackColor = Color.White
        };

        private static DataGridView CrearTabla(params string[] columnas)
        {
            var tabla = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = SystemColors.Window
            };
            foreach (string columna in columnas)
            {
                tabla.Columns.Add(columna, columna);
            }
            return tabla;
        }

        /// <summary>Crea un TableLayoutPanel de una columna con filas del tipo indicado.</summary>
        private static TableLayoutPanel TablaVertical(params SizeType[] filas)
        {
            var tabla = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = filas.Length };
            tabla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            foreach (SizeType tipo in filas)
            {
                tabla.RowStyles.Add(tipo == SizeType.Percent ? new RowStyle(SizeType.Percent, 100F) : new RowStyle(tipo));
            }
            return tabla;
        }
    }
}