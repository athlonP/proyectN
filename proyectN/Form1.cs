using System;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace proyectN
{
    public partial class Form1 : Form
    {
        private static string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.db");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Crear la base de datos y tablas si no existen
            CrearBaseDeDatos();

            // Insertar un producto (ejemplo)
            InsertarProducto("123456789", "Martillo", "Martillo de acero", 50, 200, 350, 1, 1);

            // Cargar productos y mostrarlos en el ListBox
            MostrarProductos();
        }

        public static void CrearBaseDeDatos()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                Console.WriteLine("Base de datos creada en: " + dbPath);

                using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    string query = @"
                        CREATE TABLE IF NOT EXISTS Estantes (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL,
                            capacidad INTEGER NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS Proveedores (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            nombre TEXT NOT NULL,
                            telefono TEXT NOT NULL,
                            direccion TEXT NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS Productos (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            codigo_barra TEXT UNIQUE NOT NULL,
                            nombre TEXT NOT NULL,
                            descripcion TEXT NOT NULL,
                            stock INTEGER NOT NULL,
                            precio_costo REAL NOT NULL,
                            precio_venta REAL NOT NULL,
                            id_estante INTEGER,
                            id_proveedor INTEGER,
                            FOREIGN KEY (id_estante) REFERENCES Estantes(id),
                            FOREIGN KEY (id_proveedor) REFERENCES Proveedores(id)
                        );
                        CREATE TABLE IF NOT EXISTS Pedidos (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            codigo_producto TEXT NOT NULL,
                            cantidad INTEGER NOT NULL,
                            precio_unitario REAL NOT NULL,
                            importe REAL NOT NULL,
                            fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
                            FOREIGN KEY (codigo_producto) REFERENCES Productos(codigo_barra)
                        );
                        CREATE TABLE IF NOT EXISTS CierreCaja (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
                            total_ventas REAL,
                            total_ganancia REAL
                        );
                    ";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Tablas creadas correctamente.");
                }
            }
        }

        public static void InsertarProducto(string codigoBarra, string nombre, string descripcion, int stock, double precioCosto, double precioVenta, int idEstante, int idProveedor)
        {
            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();

                string checkQuery = "SELECT COUNT(*) FROM Productos WHERE codigo_barra = @codigoBarra";
                using (var checkCmd = new SQLiteCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@codigoBarra", codigoBarra);
                    long count = (long)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("El producto con este código de barra ya existe.");
                        return; // Salir si el producto ya existe
                    }
                }

                string query = @"
                    INSERT INTO Productos (codigo_barra, nombre, descripcion, stock, precio_costo, precio_venta, id_estante, id_proveedor)
                    VALUES (@codigoBarra, @nombre, @descripcion, @stock, @precioCosto, @precioVenta, @idEstante, @idProveedor);
                ";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@codigoBarra", codigoBarra);
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@precioCosto", precioCosto);
                    cmd.Parameters.AddWithValue("@precioVenta", precioVenta);
                    cmd.Parameters.AddWithValue("@idEstante", idEstante);
                    cmd.Parameters.AddWithValue("@idProveedor", idProveedor);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Producto insertado correctamente.");
            }
        }

        public void MostrarProductos()
        {
            listBoxProductos.Items.Clear(); // Limpiar el ListBox antes de cargar nuevos productos

            using (var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                string query = "SELECT * FROM Productos";

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string productoInfo = $"ID: {reader["id"]}, Nombre: {reader["nombre"]}, Stock: {reader["stock"]}, Precio de Costo: {reader["precio_costo"]}, Precio de Venta: {reader["precio_venta"]}";
                            listBoxProductos.Items.Add(productoInfo); // Agregar cada producto al ListBox
                        }
                    }
                }
            }
        }
    }
}
