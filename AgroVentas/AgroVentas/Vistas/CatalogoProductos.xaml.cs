using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;
using AgroVentas.TablasSQlite;
using Newtonsoft.Json;

namespace AgroVentas.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CatalogoProductos : ContentPage
    {
        public CatalogoProductos()
        {
            InitializeComponent();

            LlenarListadoProdsPresent();
        }

        // -------------------------------------------------------------------------------------------
        // ************************************** [ PRODUCTOS ] **************************************
        // ---------- VARIABLES GLOBALES PUBLICAS ----------
        private int IdMaxProducto = 0;
        private int IdSeleccProducto = 0;
        private class ProductosAltaPicker
        {
            public int Id { get; set; }
            public string NombreProducto { get; set; }
        }
        private class ProductoAltaWS
        {
            public string ProductoData { get; set; }
            public string PresentacionesData { get; set; }
        }
        private class TipoMonedaPicker
        {
            public int Id { get; set; }
            public string Moneda { get; set; }
        }
        private List<ProductosAltaPicker> ProductosAltaLista;
        private List<Presentaciones> PresentAltaProductoLista;
        private List<TipoMonedaPicker> TipoMonedaLista;
        private BDPendientes ProductosDataWS;
        private Productos DataProductoServidor;
        List<string> ProductosBusq;
        List<string> ProductosValidar;
        // ----------------------------------------


        // FUNCION DE ARRANQUE INICIAL QUE LLENA LOS COMBOS DE PRODUCTOS Y PRESENTACIONES
        public async void LlenarListadoProdsPresent()
        {
            var productosQuery = DependencyService.Get<ISQliteParams>().ConsultaProductos("SELECT * FROM Productos WHERE Estatus = 1 ORDER BY NombreProducto ASC");
            ProductosAltaLista = new List<ProductosAltaPicker>();
            ProductosBusq = new List<string>();
            ProductosValidar = new List<string>();
            foreach (var producto in productosQuery)
            {
                ProductosAltaLista.Add(new ProductosAltaPicker
                {
                    Id = producto.Id,
                    NombreProducto = producto.NombreProducto
                });
                ProductosBusq.Add(producto.NombreProducto);
                ProductosValidar.Add(producto.NombreProducto);
            }
            ProductoFormLista.ItemsSource = ProductosAltaLista;
            ProductoBusq.ItemsSource = ProductosBusq;
            ProductoNombre.ItemsSource = ProductosValidar;

            TipoMonedaLista = new List<TipoMonedaPicker>()
            {
                new TipoMonedaPicker(){ Id = 1, Moneda = "PESO MEX" },
                new TipoMonedaPicker(){ Id = 2, Moneda = "DOLARES" }
            };
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            UserDialogs.Instance.HideLoading();
        }


        // ----------- FUNCION DE BOTONES DEL FORMULARIO
        // NUEVO PRODUCTO
        private void Nuevo_Producto(object sender, EventArgs e)
        {
            LimpiarProductosForm();
            ProductoNombre.Focus();
            ProductoFormLista.SelectedItem = string.Empty;
            ProductoBusq.Text = "";
            IdSeleccProducto = 0;
        }

        // GUARDAR PRODUCTO
        private async void Guardar_Producto(object sender, EventArgs e)
        {
            if (ValidarFormularioProductos())
            {
                using (UserDialogs.Instance.Loading("Guardando Producto..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    ProductosDataWS = new BDPendientes()
                    {
                        Tabla = "Productos"
                    };
                    object[] DataProducto = CrearProductosArray();
                    string query = "INSERT INTO Productos (Id,NombreProducto,Estatus) VALUES (?,?,?)";
                    ProductosDataWS.Accion = "Nuevo";
                    if (IdSeleccProducto > 0)
                    {
                        query = "UPDATE Productos SET NombreProducto=? WHERE Id=?";
                        ProductosDataWS.Accion = "Editar";

                        object[] dataEliminar = { IdSeleccProducto };
                        string RestaurarPresents = DependencyService.Get<ISQliteParams>().QueryMaestra(7, "DELETE FROM Presentaciones WHERE IdProducto=?", dataEliminar);
                    }

                    ProductoAltaWS ProductoAlta = new ProductoAltaWS()
                    {
                        ProductoData = JsonConvert.SerializeObject(DataProductoServidor),
                        PresentacionesData = JsonConvert.SerializeObject(PresentAltaProductoLista)
                    };
                    ProductosDataWS.Data = JsonConvert.SerializeObject(ProductoAlta);
                    string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(ProductosDataWS));

                    string GuardarProducto = DependencyService.Get<ISQliteParams>().QueryMaestra(6, query, DataProducto);
                    foreach (var present in PresentAltaProductoLista)
                    {
                        object[] retaurarPresents = { present.IdProducto, present.NombrePresentacion, present.Precio, present.IVA, present.IEPS, present.Moneda };
                        string RestaurarAccion = DependencyService.Get<ISQliteParams>().QueryMaestra(7, "INSERT INTO Presentaciones (IdProducto,NombrePresentacion,Precio,IVA,IEPS,Moneda) VALUES (?,?,?,?,?,?)", retaurarPresents);
                    }

                    if (GuardarProducto == "true")
                    {
                        await DisplayAlert("Éxito!", "Producto Guardado correctamente", "Aceptar");
                        LimpiarProductosForm();
                        LlenarListadoProdsPresent();
                    }
                    else
                    {
                        await DisplayAlert("Error!", GuardarProducto, "Aceptar");
                    }
                }
            }
        }

        // EDITAR PRODUCTO
        private void Editar_Producto(object sender, EventArgs e)
        {
            int index = ProductoFormLista.SelectedIndex;
            if (index != -1)
            {
                try
                {
                    IdSeleccProducto = JsonConvert.DeserializeObject<ProductosAltaPicker>(JsonConvert.SerializeObject(ProductoFormLista.ItemsSource[index])).Id;
                    var productoEditar = DependencyService.Get<ISQliteParams>().ConsultaProductos("SELECT * FROM Productos WHERE Id = " + IdSeleccProducto.ToString());
                    var presentEditar = DependencyService.Get<ISQliteParams>().ConsultaPresentaciones("SELECT * FROM Presentaciones WHERE IdProducto = " + IdSeleccProducto.ToString());

                    foreach (var producto in productoEditar)
                    {
                        ProductoNombre.Text = (producto.NombreProducto == "--") ? "" : producto.NombreProducto;
                    }
                    
                    if (presentEditar.Count > 0)
                    {
                        PresentProductosAltaGrid.Children.Clear();
                        PresentProductosAltaGrid.RowDefinitions.Clear();
                        PresentProductosAltaGrid.ColumnDefinitions.Clear();
                        /*ColumnDefinition colDef1 = new ColumnDefinition() { Width = GridLength.Auto };
                        ColumnDefinition colDef2 = new ColumnDefinition() { Width = GridLength.Star };
                        ColumnDefinition colDef3 = new ColumnDefinition() { Width = GridLength.Auto };
                        ColumnDefinition colDef4 = new ColumnDefinition() { Width = GridLength.Star };*/
                        PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        int rowIndx = 0;
                        foreach (var present in presentEditar)
                        {
                            /*RowDefinition rowDef = new RowDefinition()
                            {
                                Height = GridLength.Auto
                            };*/
                            PresentProductosAltaGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            PresentProductosAltaGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            PresentProductosAltaGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                            Label labelCampo = new Label()
                            {
                                Text = "\uf24e",
                                AutomationId = "l4B3LpR353n7",
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 20
                            };
                            Entry entryCampo = new Entry()
                            {
                                Text = present.NombrePresentacion,
                                AutomationId = "eN7R1pR353n7",
                                Placeholder = "Presentacion",
                                PlaceholderColor = Color.DarkGray,
                                TextColor = Color.FromRgb(0, 105, 92)
                            };

                            Label labelUbicacion = new Label()
                            {
                                Text = "\uf155",
                                AutomationId = "l4B3LpR35c10",
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 20
                            };
                            Entry entryUbicacion = new Entry()
                            {
                                Text = present.Precio.ToString("F"),
                                AutomationId = "eN7R1pR35c10",
                                Placeholder = "Precio",
                                Keyboard = Keyboard.Numeric,
                                PlaceholderColor = Color.DarkGray,
                                TextColor = Color.FromRgb(0, 105, 92)
                            };

                            Label labelIVA = new Label()
                            {
                                Text = "IVA %",
                                AutomationId = "l4B3LIVA",
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 20
                            };
                            Entry entryIVA = new Entry()
                            {
                                AutomationId = "eN7R1IVA",
                                Placeholder = "IVA",
                                PlaceholderColor = Color.DarkGray,
                                TextColor = Color.FromRgb(0, 105, 92),
                                Keyboard = Keyboard.Numeric,
                                Text = present.IVA.ToString()
                            };

                            Label labelIEPS = new Label()
                            {
                                Text = "IEPS %",
                                AutomationId = "l4B3LIEPS",
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 20
                            };

                            Entry entryIEPS = new Entry()
                            {
                                AutomationId = "eN7R1IEPS",
                                Placeholder = "IEPS",
                                PlaceholderColor = Color.DarkGray,
                                TextColor = Color.FromRgb(0, 105, 92),
                                Keyboard = Keyboard.Numeric,
                                Text = present.IEPS.ToString()
                            };

                            Picker pickerMoneda = new Picker()
                            {
                                AutomationId = "p1Ck3Rm0N3d4",
                                Title = "- Seleccione Moneda -",
                                ItemDisplayBinding = new Binding("Moneda"),
                                SelectedItem = new Binding("Id"),
                                ItemsSource = TipoMonedaLista
                            };
                            pickerMoneda.SelectedItem = ((List<TipoMonedaPicker>)pickerMoneda.ItemsSource).FirstOrDefault(c => c.Id == present.Moneda);

                            PresentProductosAltaGrid.Children.Add(labelCampo);
                            PresentProductosAltaGrid.Children.Add(entryCampo);
                            PresentProductosAltaGrid.Children.Add(labelUbicacion);
                            PresentProductosAltaGrid.Children.Add(entryUbicacion);
                            PresentProductosAltaGrid.Children.Add(labelIVA);
                            PresentProductosAltaGrid.Children.Add(entryIVA);
                            PresentProductosAltaGrid.Children.Add(labelIEPS);
                            PresentProductosAltaGrid.Children.Add(entryIEPS);
                            PresentProductosAltaGrid.Children.Add(pickerMoneda);

                            Grid.SetRow(labelCampo, rowIndx);
                            Grid.SetColumn(labelCampo, 0);
                            Grid.SetRow(entryCampo, rowIndx);
                            Grid.SetColumn(entryCampo, 1);
                            Grid.SetRow(labelUbicacion, rowIndx);
                            Grid.SetColumn(labelUbicacion, 2);
                            Grid.SetRow(entryUbicacion, rowIndx);
                            Grid.SetColumn(entryUbicacion, 3);
                            rowIndx++;
                            Grid.SetRow(labelIVA, rowIndx);
                            Grid.SetColumn(labelIVA, 0);
                            Grid.SetRow(entryIVA, rowIndx);
                            Grid.SetColumn(entryIVA, 1);
                            Grid.SetRow(labelIEPS, rowIndx);
                            Grid.SetColumn(labelIEPS, 2);
                            Grid.SetRow(entryIEPS, rowIndx);
                            Grid.SetColumn(entryIEPS, 3);
                            rowIndx++;
                            Grid.SetRow(pickerMoneda, rowIndx);
                            Grid.SetColumnSpan(pickerMoneda, 4);
                            rowIndx++;
                            /*PresentProductosAltaGrid.Children.Add(labelCampo, 0, rowIndx);
                            PresentProductosAltaGrid.Children.Add(entryCampo, 1, rowIndx);
                            PresentProductosAltaGrid.Children.Add(labelUbicacion, 2, rowIndx);
                            PresentProductosAltaGrid.Children.Add(entryUbicacion, 3, rowIndx);
                            rowIndx++;*/
                        }
                    }
                }
                catch (Exception err)
                {
                    DisplayAlert("Error!", "Ocurrió un problema: " + err.ToString(), "Aceptar");
                }
            }
            else
            {
                DisplayAlert("Error!", "Seleccione un Producto", "Aceptar");
            }
        }

        // BORRAR PRODUCTO
        private async void Borrar_Producto(object sender, EventArgs e)
        {
            int index = ProductoFormLista.SelectedIndex;
            if (index != -1)
            {
                ProductosAltaPicker productoInfo = JsonConvert.DeserializeObject<ProductosAltaPicker>(JsonConvert.SerializeObject(ProductoFormLista.ItemsSource[index]));
                var borrar = await DisplayAlert("Atencion!", "¿Desea borrar a " + productoInfo.NombreProducto + "?", "Si", "Cancelar");
                if (borrar)
                {
                    using (UserDialogs.Instance.Loading("Eliminando Producto..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        DataProductoServidor = new Productos()
                        {
                            Id = productoInfo.Id,
                            Estatus = 0
                        };
                        ProductosDataWS = new BDPendientes()
                        {
                            Tabla = "Productos",
                            Accion = "Borrar",
                            Data = JsonConvert.SerializeObject(DataProductoServidor)
                        };
                        string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(ProductosDataWS));
                        object[] idBorrar = { 0, productoInfo.Id };
                        string BorrarProducto = DependencyService.Get<ISQliteParams>().QueryMaestra(2, "UPDATE Productos SET Estatus=? WHERE Id=?", idBorrar);
                        if (BorrarProducto == "true")
                        {
                            LimpiarProductosForm();
                            LlenarListadoProdsPresent();
                            IdSeleccProducto = 0;
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Ocurrió un problema: " + BorrarProducto, "Aceptar");
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("Error!", "Seleccione un Producto", "Aceptar");
            }
        }


        // ------------ FUNCIONES CON CATALOGOS ------------
        // FUNCION QUE LIMPIA EL FORMULARIO DE ALTA DE PRODUCTOS
        private void LimpiarProductosForm()
        {
            ProductoNombre.Text = "";

            PresentProductosAltaGrid.Children.Clear();
            PresentProductosAltaGrid.RowDefinitions.Clear();
            PresentProductosAltaGrid.ColumnDefinitions.Clear();
        }

        // FUNCION QUE VALIDA EL FORMULARIO DE PRODUCTOS
        private bool ValidarFormularioProductos()
        {
            bool correcto = true;
            // VALIDAR EL FORMULARIO DE TEXTOS
            if (string.IsNullOrEmpty(ProductoNombre.Text))
            {
                ProductoNombre.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Nombre Producto");
                correcto = false;
            }
            else
            {
                bool ErrPresent = false;
                // VALIDAR EL FORMULARIO DE CAMPOS
                foreach (var Elem in PresentProductosAltaGrid.Children)
                {
                    if ((Elem.AutomationId != "l4B3LpR353n7") && (Elem.AutomationId != "l4B3LpR35c10") && (Elem.AutomationId != "l4B3LIVA") && (Elem.AutomationId != "l4B3LIEPS"))
                    {
                        if (Elem.AutomationId == "eN7R1pR353n7")
                        {
                            Entry presentacion = (Entry)Elem;
                            if (string.IsNullOrEmpty(presentacion.Text))
                            {
                                ErrPresent = true;
                            }
                        }
                        else if (Elem.AutomationId == "eN7R1pR35c10")
                        {
                            Entry precio = (Entry)Elem;
                            if (string.IsNullOrEmpty(precio.Text))
                            {
                                ErrPresent = true;
                            }
                        }
                        else if (Elem.AutomationId == "eN7R1IVA")
                        {
                            Entry iva = (Entry)Elem;
                            if (string.IsNullOrEmpty(iva.Text))
                            {
                                ErrPresent = true;
                            }
                        }
                        else if (Elem.AutomationId == "eN7R1IEPS")
                        {
                            Entry ieps = (Entry)Elem;
                            if (string.IsNullOrEmpty(ieps.Text))
                            {
                                ErrPresent = true;
                            }
                        }
                        else if (Elem.AutomationId == "p1Ck3Rm0N3d4")
                        {
                            Picker moneda = (Picker)Elem;
                            if (moneda.SelectedIndex == -1)
                            {
                                ErrPresent = true;
                            }
                        }
                    }
                }
                if (ErrPresent)
                {
                    correcto = false;
                    DisplayAlert("Atención!", "No deje valores de PRESENTACIONES en blanco", "Aceptar");
                }
            }

            return correcto;
        }


        // -------------- [ PRESENTACIONES ] --------------
        // BOTON QUE LIMPIA LOS ELEMENTOS GENERADOS PARA AGREGAR UNA NUEVA PRESENTACION DEL PRODUCTO
        private void Limpiar_PresentProducto(object sender, EventArgs e)
        {
            PresentProductosAltaGrid.Children.Clear();
            PresentProductosAltaGrid.RowDefinitions.Clear();
            PresentProductosAltaGrid.ColumnDefinitions.Clear();
        }

        // FUNCION QUE EMPAQUETA LOS VALORES DEL FORMULARIO DE PERODUCTOS EN UN OBJECT ARRAY
        private object[] CrearProductosArray()
        {
            string ProductoNombreAlta = ProductoNombre.Text.ToUpper().Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");
            var ObtenerMaxId = DependencyService.Get<ISQliteParams>().ConsultaProductos("SELECT MAX(Id) AS Id FROM Productos");
            if (ObtenerMaxId.Count > 0)
            {
                foreach (var producto in ObtenerMaxId)
                {
                    IdMaxProducto = producto.Id + 1;
                }
            }
            else
            {
                IdMaxProducto = 1;
            }

            DataProductoServidor = new Productos()
            {
                NombreProducto = (string.IsNullOrEmpty(ProductoNombre.Text)) ? "--" : ProductoNombreAlta,
                Estatus = 1
            };

            PresentAltaProductoLista = new List<Presentaciones>();
            Presentaciones presentProducto = new Presentaciones()
            {
                IdProducto = (IdSeleccProducto > 0) ? IdSeleccProducto : IdMaxProducto
            };
            foreach (var Elem in PresentProductosAltaGrid.Children)
            {
                if ((Elem.AutomationId != "l4B3LpR353n7") && (Elem.AutomationId != "l4B3LpR35c10") && (Elem.AutomationId != "l4B3LIVA") && (Elem.AutomationId != "l4B3LIEPS"))
                {
                    if (Elem.AutomationId == "eN7R1pR353n7")
                    {
                        Entry presentacion = (Entry)Elem;
                        presentProducto.NombrePresentacion = presentacion.Text.ToUpper();
                    }
                    else if (Elem.AutomationId == "eN7R1pR35c10")
                    {
                        Entry precio = (Entry)Elem;
                        presentProducto.Precio = Convert.ToDouble(precio.Text);
                    }
                    else if (Elem.AutomationId == "eN7R1IVA")
                    {
                        Entry iva = (Entry)Elem;
                        presentProducto.IVA = Convert.ToInt32(iva.Text);
                    }
                    else if (Elem.AutomationId == "eN7R1IEPS")
                    {
                        Entry ieps = (Entry)Elem;
                        presentProducto.IEPS = Convert.ToInt32(ieps.Text);
                    }
                    else if (Elem.AutomationId == "p1Ck3Rm0N3d4")
                    {
                        Picker moneda = (Picker)Elem;
                        int index = moneda.SelectedIndex;
                        TipoMonedaPicker monedasInfo = JsonConvert.DeserializeObject<TipoMonedaPicker>(JsonConvert.SerializeObject(moneda.ItemsSource[index]));
                        presentProducto.Moneda = monedasInfo.Id;

                        PresentAltaProductoLista.Add(presentProducto);
                        presentProducto = new Presentaciones()
                        {
                            IdProducto = (IdSeleccProducto > 0) ? IdSeleccProducto : IdMaxProducto
                        };
                    }
                }
            }

            if (IdSeleccProducto > 0)
            {
                DataProductoServidor.Id = IdSeleccProducto;
                object[] productoLista =
                {
                    (string.IsNullOrEmpty(ProductoNombre.Text)) ? "--" : ProductoNombreAlta,
                    IdSeleccProducto
                };
                return productoLista;
            }
            else
            {
                DataProductoServidor.Id = IdMaxProducto;
                object[] productoLista =
                {
                    IdMaxProducto,
                    (string.IsNullOrEmpty(ProductoNombre.Text)) ? "--" : ProductoNombreAlta,
                    1
                };
                return productoLista;
            }
        }

        // BOTON QUE AGREGA UNA NUEVA PRESENTACIÓN PARA UN NUEVO  PRODUCTO
        private void Nuevo_PresentProducto(object sender, EventArgs e)
        {
            PresentProductosAltaGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            PresentProductosAltaGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            PresentProductosAltaGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            int rowIndx = 0;
            foreach (var Elem in PresentProductosAltaGrid.Children)
            {
                if ((Elem.AutomationId == "l4B3LpR353n7") || (Elem.AutomationId == "l4B3LIVA") || (Elem.AutomationId == "p1Ck3Rm0N3d4"))
                {
                    rowIndx++;
                }
            }

            if (rowIndx == 0)
            {
                PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                PresentProductosAltaGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            }

            Label labelPresent = new Label()
            {
                Text = "\uf24e",
                AutomationId = "l4B3LpR353n7",
                FontFamily = "fontawesomeicons.ttf#Regular",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            };
            Entry entryPresent = new Entry()
            {
                AutomationId = "eN7R1pR353n7",
                Placeholder = "Presentacion",
                PlaceholderColor = Color.DarkGray,
                TextColor = Color.FromRgb(0, 105, 92)
            };

            Label labelPrecio = new Label()
            {
                Text = "\uf155",
                AutomationId = "l4B3LpR35c10",
                FontFamily = "fontawesomeicons.ttf#Regular",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            };
            Entry entryPrecio = new Entry()
            {
                AutomationId = "eN7R1pR35c10",
                Placeholder = "Precio",
                Keyboard = Keyboard.Numeric,
                PlaceholderColor = Color.DarkGray,
                TextColor = Color.FromRgb(0, 105, 92)
            };

            Label labelIVA = new Label()
            {
                Text = "IVA %",
                AutomationId = "l4B3LIVA",
                FontFamily = "fontawesomeicons.ttf#Regular",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            };
            Entry entryIVA = new Entry()
            {
                AutomationId = "eN7R1IVA",
                Placeholder = "IVA",
                PlaceholderColor = Color.DarkGray,
                TextColor = Color.FromRgb(0, 105, 92),
                Keyboard = Keyboard.Numeric,
                Text = "0"
            };

            Label labelIEPS = new Label()
            {
                Text = "IEPS %",
                AutomationId = "l4B3LIEPS",
                FontFamily = "fontawesomeicons.ttf#Regular",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            };
            Entry entryIEPS = new Entry()
            {
                AutomationId = "eN7R1IEPS",
                Placeholder = "IEPS",
                PlaceholderColor = Color.DarkGray,
                TextColor = Color.FromRgb(0, 105, 92),
                Keyboard = Keyboard.Numeric,
                Text = "0"
            };

            Picker pickerMoneda = new Picker()
            {
                AutomationId = "p1Ck3Rm0N3d4",
                Title = "- Seleccione Moneda -",
                ItemDisplayBinding = new Binding("Moneda"),
                SelectedItem = new Binding("Id"),
                ItemsSource = TipoMonedaLista
            };

            PresentProductosAltaGrid.Children.Add(labelPresent);
            PresentProductosAltaGrid.Children.Add(entryPresent);
            PresentProductosAltaGrid.Children.Add(labelPrecio);
            PresentProductosAltaGrid.Children.Add(entryPrecio);
            PresentProductosAltaGrid.Children.Add(labelIVA);
            PresentProductosAltaGrid.Children.Add(entryIVA);
            PresentProductosAltaGrid.Children.Add(labelIEPS);
            PresentProductosAltaGrid.Children.Add(entryIEPS);
            PresentProductosAltaGrid.Children.Add(pickerMoneda);

            Grid.SetRow(labelPresent, rowIndx);
            Grid.SetColumn(labelPresent, 0);
            Grid.SetRow(entryPresent, rowIndx);
            Grid.SetColumn(entryPresent, 1);
            Grid.SetRow(labelPrecio, rowIndx);
            Grid.SetColumn(labelPrecio, 2);
            Grid.SetRow(entryPrecio, rowIndx);
            Grid.SetColumn(entryPrecio, 3);
            Grid.SetRow(labelIVA, rowIndx + 1);
            Grid.SetColumn(labelIVA, 0);
            Grid.SetRow(entryIVA, rowIndx + 1);
            Grid.SetColumn(entryIVA, 1);
            Grid.SetRow(labelIEPS, rowIndx + 1);
            Grid.SetColumn(labelIEPS, 2);
            Grid.SetRow(entryIEPS, rowIndx + 1);
            Grid.SetColumn(entryIEPS, 3);
            Grid.SetRow(pickerMoneda, rowIndx + 2);
            Grid.SetColumnSpan(pickerMoneda, 4);
        }

        // --------------------- [ NUEVA BUSQUEDA DE ALTA DE PRODUCTOS (CONSULTA) ] ---------------------
        // FUNCION QUE MANEJA LA CONSULTA DEL PRODUCTO AL ESCRIBIR EL NOMBRE
        private void ProductoBusq_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = ProductoBusq.Text.ToUpper();
                var busqResultado = ProductosBusq.Where(i => i.StartsWith(busqCliente)).ToList();
                ProductoBusq.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE MANEJA LA CONSULTA DEL PRODUCTO
        private void ProductoBusq_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = ProductosBusq.FindIndex(x => x.StartsWith(ProductoBusq.Text.ToUpper()));
            ProductoFormLista.SelectedIndex = index;
        }


        // --------------------- [ VALIDADOR ESPECIAL PARA LA ALTA DE PRODUCTO ] ---------------------
        // FUNCION QUE MANEJA EL MOSTRAR SUGERENCIAS AL ESCRIBIR EL NOMBRE DEL PRODUCTO
        private void ProductoNombre_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = ProductoNombre.Text.ToUpper();
                var busqResultado = ProductosValidar.Where(i => i.StartsWith(busqCliente)).ToList();
                ProductoNombre.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE MUESTRA UNA ACCION ESPECIAL AL ELEGIR UNA SUGERENCIA DE NOMBRE DE PRODUCTO
        private void ProductoNombre_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            ProductoNombre.Text = "";
            ProductoNombre.Unfocus();
            DisplayAlert("Atención!", "Las sugerencias mostradas NO son seleccionables.\nLe recomendamos no repetir los nombres de los productos.", "Aceptar");
        }
    }
}