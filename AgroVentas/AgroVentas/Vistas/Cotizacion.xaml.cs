using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;
using Newtonsoft.Json;
using AgroVentas.TablasSQlite;

namespace AgroVentas.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Cotizacion : TabbedPage
    {
        // ---------- VARIABLES GLOBALES PUBLICAS ----------
        private class ClientesCotizPicker
        {
            public int Id { get; set; }
            public string RazonSocial { get; set; }
        }
        private class ProductosCotizPicker
        {
            public int Id { get; set; }
            public string NombreProducto { get; set; }
        }
        private class PresentCotizPicker
        {
            public int Id { get; set; }
            public string NombrePresentacion { get; set; }
            public int IdProducto { get; set; }
            public double Precio { get; set; }
            public int IVA { get; set; }
            public int IEPS { get; set; }
            public int Moneda { get; set; }
        }
        private class CotizTipoCliente
        {
            public int Id { get; set; }
            public string TipoCliente { get; set; }
            public double DollarValor { get; set; }
        }
        private class CotizProductosClase
        {
            public string Token { get; set; }
            public int IdOrdenPedido { get; set; }
            public int IdProducto { get; set; }
            public string Descripcion { get; set; }
            public string Presentacion { get; set; }
            public float Cantidad { get; set; }
            public double PrecioUnitario { get; set; }
            public double PrecioUnitarioDesc { get; set; }
            public int DescuentoPorc { get; set; }
            public double Descuento { get; set; }
            public int IVA { get; set; }
            public double IVAMonto { get; set; }
            public int IEPS { get; set; }
            public double IEPSMonto { get; set; }
            public double Importe { get; set; }
        }
        private class CotizacionPDF
        {
            public string Cotizacion { get; set; }
            public string CotizacionProductos { get; set; }
        }
        private class ReporteEstructura
        {
            public string Url { get; set; }
            public string Data { get; set; }
        }
        private class ReporteParamsConsulta
        {
            public int IdCliente { get; set; }
            public string FechaInicio { get; set; }
            public string FechaFin { get; set; }
        }
        private class ReportesWS
        {
            public DateTime Fecha { get; set; }
            public string Folio { get; set; }
            public string Documento { get; set; }
            public string TipoCliente { get; set; }
            public string Total { get; set; }
        }
        private List<ClientesCotizPicker> ClientesCotizLista;
        private List<ProductosCotizPicker> ProductosCotizLista;
        private List<PresentCotizPicker> PresentCotizLista;
        private List<CotizTipoCliente> CotizTipoClienteLista;
        PresentCotizPicker PresentCotizValores;
        private List<CotizProductosClase> CotizProductosLista;
        List<string> NombresClientesCotiz = new List<string>();
        List<string> NombresProductosCotiz = new List<string>();
        List<string> NombresClientesCotizBusq = new List<string>();

        int IdClienteCotiz = 0;
        int IdProductoCotiz = 0;
        string NomPresentCotiz = "";
        bool CargadoProductos = false;
        double ImportePrductoCotiz = 0;
        View LabelPrecioPublico;
        bool CotizElemsVisibles = false;
        string UrlServerCotiz = "";
        // ----------------------------------------

        public Cotizacion()
        {
            InitializeComponent();
            LlenarValoresCotizacionLista();
        }

        // :::::::::::::::::::::::::::::::::::::::: [ MENU GENERAR ] ::::::::::::::::::::::::::::::::::::::::

        // FUNCION INICIAL DE LLENADO DE LA LISTA
        private async void LlenarValoresCotizacionLista()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            var clientesQuery = DependencyService.Get<ISQliteParams>().ConsultaClientes("SELECT * FROM Clientes WHERE Estatus = 1 ORDER BY RazonSocial ASC");
            ClientesCotizLista = new List<ClientesCotizPicker>();
            foreach (var cliente in clientesQuery)
            {
                ClientesCotizLista.Add(new ClientesCotizPicker
                {
                    Id = cliente.Id,
                    RazonSocial = cliente.RazonSocial
                });
                NombresClientesCotiz.Add(cliente.RazonSocial);
                NombresClientesCotizBusq.Add(cliente.RazonSocial);
            }

            CotizClienteFormLista.ItemsSource = ClientesCotizLista;
            CotizBuscClienteFormLista.ItemsSource = ClientesCotizLista;

            CotizClienteBusq.ItemsSource = NombresClientesCotiz;
            CotizBusqClienteLista.ItemsSource = NombresClientesCotizBusq;

            double DollarCredito = 0, DollarContado = 0;
            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var usuario in VerifUsuario)
            {
                DollarContado = usuario.DollarContado;
                DollarCredito = usuario.DollarCredito;
            }
            CotizTipoClienteLista = new List<CotizTipoCliente>()
            {
                new CotizTipoCliente { Id = 1, TipoCliente = "CONTADO", DollarValor = DollarContado },
                new CotizTipoCliente { Id = 2, TipoCliente = "CREDITO", DollarValor = DollarCredito }
            };

            CotizTipoClienteFormLista.ItemsSource = CotizTipoClienteLista;
            CotizTipoClienteFormLista.SelectedIndex = 0;

            UserDialogs.Instance.HideLoading();

            if ((DollarCredito == 0) || (DollarContado == 0))
            {
                await DisplayAlert("Atención!", "Vaya al menú Configuración, pestaña OTROS para configurar los valores de CRÉDITO y CONTADO para el Dolar", "Aceptar");
            }
        }

        // ---------- FORMULARIO CLIENTE, PRODUCTO Y PRESENTACION ---------------
        // BOTON QUE ACCIONA LA FUNCION AL ELEGIR UN CLIENTE
        private async void CotizElegir_Cliente(object sender, EventArgs e)
        {
            int index = CotizClienteFormLista.SelectedIndex;
            if (index != -1)
            {
                using (UserDialogs.Instance.Loading("Espere..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    ClientesCotizPicker clienteInfo = JsonConvert.DeserializeObject<ClientesCotizPicker>(JsonConvert.SerializeObject(CotizClienteFormLista.ItemsSource[index]));
                    IdClienteCotiz = clienteInfo.Id;

                    if (!CargadoProductos)
                    {
                        var productosQuery = DependencyService.Get<ISQliteParams>().ConsultaProductos("SELECT * FROM Productos WHERE Estatus = 1 ORDER BY NombreProducto ASC");
                        ProductosCotizLista = new List<ProductosCotizPicker>();
                        NombresProductosCotiz = new List<string>();
                        foreach (var producto in productosQuery)
                        {
                            ProductosCotizLista.Add(new ProductosCotizPicker
                            {
                                Id = producto.Id,
                                NombreProducto = producto.NombreProducto
                            });
                            NombresProductosCotiz.Add(producto.NombreProducto);
                        }

                        CotizProductoFormLista.ItemsSource = ProductosCotizLista;
                        CotizProductosBusq.ItemsSource = NombresProductosCotiz;
                        CargadoProductos = true;
                    }

                    CotizProductoFormLista.SelectedItem = string.Empty;
                    CotizProductosBusq.Text = "";
                    CotizPresentFormLista.SelectedItem = string.Empty;
                    PresentCotizLista = new List<PresentCotizPicker>();
                    CotizProductosLista = new List<CotizProductosClase>();
                    IdProductoCotiz = 0;
                    NomPresentCotiz = "";
                    CotizPresentFormLista.ItemsSource = PresentCotizLista;

                    CotizListaProductos.Children.Clear();
                    CotizListaProductos.RowDefinitions.Clear();
                    CotizListaProductos.ColumnDefinitions.Clear();
                    CotizPreciosClientes.Children.Clear();

                    CotizLineaListaProductos.IsVisible = false;
                    CotizTotalesListaProductos.IsVisible = false;
                    CotizElemsVisibles = false;
                    LimpiarCamposCotizacion();

                    LlenarTablaCotizacionesCliente();
                }
            }
        }

        // BOTON QUE ACCIONA LA FUNCION AL ELEGIR UN PRODUCTO
        private async void CotizElegir_Producto(object sender, EventArgs e)
        {
            int index = CotizProductoFormLista.SelectedIndex;
            if (index != -1)
            {
                using (UserDialogs.Instance.Loading("Espere..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    ProductosCotizPicker productoInfo = JsonConvert.DeserializeObject<ProductosCotizPicker>(JsonConvert.SerializeObject(CotizProductoFormLista.ItemsSource[index]));
                    IdProductoCotiz = productoInfo.Id;

                    var presentProductosQuery = DependencyService.Get<ISQliteParams>().ConsultaPresentaciones("SELECT * FROM Presentaciones WHERE IdProducto = " + productoInfo.Id.ToString());
                    PresentCotizLista = new List<PresentCotizPicker>();
                    int c = 1;
                    foreach (var presents in presentProductosQuery)
                    {
                        PresentCotizLista.Add(new PresentCotizPicker
                        {
                            Id = c,
                            IdProducto = presents.IdProducto,
                            NombrePresentacion = presents.NombrePresentacion,
                            Precio = presents.Precio,
                            IVA = presents.IVA,
                            IEPS = presents.IEPS,
                            Moneda = presents.Moneda
                        });
                        c++;
                    }
                    CotizPresentFormLista.ItemsSource = PresentCotizLista;

                    CotizPreciosClientes.Children.Clear();
                    NomPresentCotiz = "";

                    if (!CotizElemsVisibles)
                    {
                        CotizLineaListaProductos.IsVisible = true;
                        CotizTotalesListaProductos.IsVisible = true;
                        CotizElemsVisibles = true;
                    }
                }
            }
        }

        // BOTON QUE ACCIONA LA FUNCION AL ELEGIR UNA PRESENTACION
        private async void CotizElegir_Present(object sender, EventArgs e)
        {
            CotizPreciosClientes.Children.Clear();
            int index = CotizPresentFormLista.SelectedIndex;
            if (index != -1)
            {
                try
                {
                    using (UserDialogs.Instance.Loading("Espere..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        PresentCotizValores = JsonConvert.DeserializeObject<PresentCotizPicker>(JsonConvert.SerializeObject(CotizPresentFormLista.ItemsSource[index]));
                    
                        Grid GridCotizacion = new Grid()
                        {
                            AutomationId = "6r1Dc071z4C10n",
                            Margin = new Thickness(0, 10, 0, 10)
                        };
                        GridCotizacion.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        GridCotizacion.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        GridCotizacion.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        GridCotizacion.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        GridCotizacion.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        GridCotizacion.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                        int indexTC = CotizTipoClienteFormLista.SelectedIndex;
                        CotizTipoCliente infoTC = JsonConvert.DeserializeObject<CotizTipoCliente>(JsonConvert.SerializeObject(CotizTipoClienteFormLista.ItemsSource[indexTC]));
                        double Importe = (PresentCotizValores.Moneda == 2) ? (PresentCotizValores.Precio * infoTC.DollarValor) : PresentCotizValores.Precio;

                        ImportePrductoCotiz = Importe;

                        Label label = new Label()
                        {
                            AutomationId = "l4B3L",
                            Text = "Precio público: ",
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            TextColor = Color.FromRgb(0, 105, 92)
                        };
                        Label labelPrecioPublico = new Label()
                        {
                            AutomationId = "l4B3LpR3c10Pu8L1c0",
                            Text = "$ " + Importe.ToString("F"),
                            FontAttributes = FontAttributes.None,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            TextColor = Color.FromRgb(0, 105, 92)
                        };
                        LabelPrecioPublico = labelPrecioPublico;

                        Label label2 = new Label()
                        {
                            AutomationId = "l4B3L2",
                            Text = "IEPS: ",
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            TextColor = Color.FromRgb(0, 105, 92)
                        };
                        Label labelIEPS = new Label()
                        {
                            AutomationId = "l4B3LIEPS",
                            Text = PresentCotizValores.IEPS.ToString() + "%",
                            FontAttributes = FontAttributes.None,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            TextColor = Color.FromRgb(0, 105, 92)
                        };

                        Label label3 = new Label()
                        {
                            AutomationId = "l4B3L3",
                            Text = "IVA: ",
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            TextColor = Color.FromRgb(0, 105, 92)
                        };
                        Label labelIVA = new Label()
                        {
                            AutomationId = "l4B3LIVA",
                            Text = PresentCotizValores.IVA.ToString() + "%",
                            FontAttributes = FontAttributes.None,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 18,
                            TextColor = Color.FromRgb(0, 105, 92)
                        };

                        GridCotizacion.Children.Add(label);
                        GridCotizacion.Children.Add(labelPrecioPublico);
                        GridCotizacion.Children.Add(label2);
                        GridCotizacion.Children.Add(labelIEPS);
                        GridCotizacion.Children.Add(label3);
                        GridCotizacion.Children.Add(labelIVA);

                        Grid.SetRow(label, 0);
                        Grid.SetColumn(label, 0);
                        Grid.SetRow(labelPrecioPublico, 0);
                        Grid.SetColumn(labelPrecioPublico, 1);
                        Grid.SetRow(label2, 1);
                        Grid.SetColumn(label2, 0);
                        Grid.SetRow(labelIEPS, 1);
                        Grid.SetColumn(labelIEPS, 1);
                        Grid.SetRow(label3, 1);
                        Grid.SetColumn(label3, 2);
                        Grid.SetRow(labelIVA, 1);
                        Grid.SetColumn(labelIVA, 3);

                        Grid GridInputs = new Grid()
                        {
                            AutomationId = "6r1D1NpU75",
                            Margin = new Thickness(0, 10, 0, 10)
                        };
                        GridInputs.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        GridInputs.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        GridInputs.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        GridInputs.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                        Entry EntryDescuento = new Entry()
                        {
                            AutomationId = "3n7RyD35cU3N70",
                            Placeholder = "Descuento %",
                            TextColor = Color.FromRgb(0, 105, 92),
                            Margin = new Thickness(0, 10, 0, 0),
                            Keyboard = Keyboard.Numeric
                        };
                        Entry EntryCantidad = new Entry()
                        {
                            AutomationId = "3n7RyC4n7",
                            Placeholder = "Cantidad",
                            TextColor = Color.FromRgb(0, 105, 92),
                            Margin = new Thickness(0, 10, 0, 0),
                            Keyboard = Keyboard.Numeric
                        };

                        Button btnCalcularDesc = new Button()
                        {
                            AutomationId = "b7N3nC4LcuL4rD35c",
                            Text = "\uf155 AGREGAR A COTIZACIÓN",
                            FontSize = 12,
                            CornerRadius = 20,
                            TextColor = Color.White,
                            BackgroundColor = Color.FromRgb(23, 162, 184),
                            FontFamily = "fontawesomeicons.ttf#Regular",
                            WidthRequest = 38,
                            HeightRequest = 38,
                            VerticalOptions = LayoutOptions.Center,
                            Command = new Command(() =>
                            {
                                CotizacionPrecio(EntryDescuento, EntryCantidad);
                            })
                        };

                        GridInputs.Children.Add(EntryDescuento);
                        GridInputs.Children.Add(EntryCantidad);
                        GridInputs.Children.Add(btnCalcularDesc);

                        Grid.SetRow(EntryDescuento, 0);
                        Grid.SetColumn(EntryDescuento, 0);
                        Grid.SetRow(EntryCantidad, 0);
                        Grid.SetColumn(EntryCantidad, 1);
                        Grid.SetRow(btnCalcularDesc, 1);
                        Grid.SetColumnSpan(btnCalcularDesc, 2);

                        CotizPreciosClientes.Children.Add(GridCotizacion);
                        CotizPreciosClientes.Children.Add(GridInputs);
                    }
                }
                catch (Exception err)
                {
                    await DisplayAlert("Error!", err.ToString(), "Aceptar");
                }
            }
        }

        // BOTON QUE ACTUALIZA 
        private void CotizPrecio_TipoCliente(object sender, EventArgs e)
        {
            if (NomPresentCotiz != "")
            {
                int indexTC = CotizTipoClienteFormLista.SelectedIndex;
                CotizTipoCliente infoTC = JsonConvert.DeserializeObject<CotizTipoCliente>(JsonConvert.SerializeObject(CotizTipoClienteFormLista.ItemsSource[indexTC]));
                double Importe = (PresentCotizValores.Moneda == 2) ? (PresentCotizValores.Precio * infoTC.DollarValor) : PresentCotizValores.Precio;

                ImportePrductoCotiz = Importe;

                Label LabelPrecio = (Label)LabelPrecioPublico;
                LabelPrecio.Text = "$ " + Importe.ToString("F");
            }
        }

        // FUNCION QUE GENERA UNA COTIZACION APLICANDO UN DESCUENTO AL PRECIO PARA LA COTIZACION
        private async void CotizacionPrecio(View EntryDesc, View EntryCant)
        {
            using (UserDialogs.Instance.Loading("Espere..."))
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    Entry EntryDescuento = (Entry)EntryDesc;
                    Entry EntryCantidad = (Entry)EntryCant;
                    int CantidadProducto = (string.IsNullOrEmpty(EntryCantidad.Text)) ? 0 : Convert.ToInt32(EntryCantidad.Text);
                    if(CantidadProducto == 0)
                    {
                        CantidadProducto = 1;
                    }

                    int rowIndx = 0;
                    string nuevoProd = "";
                    foreach (var Elem in CotizListaProductos.Children)
                    {
                        string[] elemArr = Elem.AutomationId.Split('_');
                        if (elemArr[0] == "l4B3Ln0mPr0Duc70")
                        {
                            rowIndx++;
                            nuevoProd = elemArr[1];
                        }
                    }

                    if (rowIndx == 0)
                    {
                        CotizListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        CotizListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        CotizListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        CotizListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    }
                    int indexProducto = CotizProductoFormLista.SelectedIndex;
                    int indexPresent = CotizPresentFormLista.SelectedIndex;
                    ProductosCotizPicker productoInfo = JsonConvert.DeserializeObject<ProductosCotizPicker>(JsonConvert.SerializeObject(CotizProductoFormLista.ItemsSource[indexProducto]));
                    PresentCotizPicker presentInfo = JsonConvert.DeserializeObject<PresentCotizPicker>(JsonConvert.SerializeObject(CotizPresentFormLista.ItemsSource[indexPresent]));
                    string idProdPresent = productoInfo.Id.ToString() + "ø" + presentInfo.NombrePresentacion.Replace(' ', '¢').Replace('_', 'æ');

                    if (idProdPresent != nuevoProd)
                    {
                        string Token = CrearTokenCotiz();
                        CotizListaProductos.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        double Importe = ImportePrductoCotiz;
                        int DescuentoPorc = (string.IsNullOrEmpty(EntryDescuento.Text)) ? 0 : Convert.ToInt32(EntryDescuento.Text);
                        string descTxt = "";
                        if ((DescuentoPorc > 0) && (DescuentoPorc < 10))
                        {
                            descTxt = "0";
                        }
                        double DescuentoMonto = (DescuentoPorc > 0) ? ((Importe * Convert.ToDouble("0." + descTxt + DescuentoPorc.ToString())) * CantidadProducto) : 0;
                        double DescuentoMontoUnit = (DescuentoPorc > 0) ? (Importe * Convert.ToDouble("0." + descTxt + DescuentoPorc.ToString())) : 0;
                        double valorUniDesc = (DescuentoMonto > 0) ? Importe - (Importe * Convert.ToDouble("0." + descTxt + DescuentoPorc.ToString())) : Importe;
                        double subTotal = CantidadProducto * Importe;
                        int ieps = (PresentCotizValores.IEPS > 0) ? PresentCotizValores.IEPS : 0;
                        string iepsTxt = "";
                        if ((PresentCotizValores.IEPS) > 0 && (PresentCotizValores.IEPS < 10))
                        {
                            iepsTxt = "0";
                        }
                        double iepsMonto = (ieps > 0) ? (subTotal - DescuentoMonto) * Convert.ToDouble("0." + iepsTxt + ieps.ToString()) : 0;
                        double iepsMontoUnit = (ieps > 0) ? (Importe - DescuentoMontoUnit) * Convert.ToDouble("0." + iepsTxt + ieps.ToString()) : 0;
                        int iva = (PresentCotizValores.IVA > 0) ? PresentCotizValores.IVA : 0;
                        double ivaMonto = (iva > 0) ? ((subTotal - DescuentoMonto) + iepsMonto) * Convert.ToDouble("0." + iva.ToString()) : 0;
                        double ivaMontoUnit = (iva > 0) ? ((Importe - DescuentoMontoUnit) + iepsMontoUnit) * Convert.ToDouble("0." + iva.ToString()) : 0;

                        Label labelNomProd = new Label()
                        {
                            AutomationId = "l4B3Ln0mPr0Duc70_" + idProdPresent,
                            Text = productoInfo.NombreProducto + " " + presentInfo.NombrePresentacion,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        };
                        Label labelCantidad = new Label()
                        {
                            AutomationId = "l4B3Lc4N7Pr0Duc70_&",
                            Text = CantidadProducto.ToString(),
                            FontAttributes = FontAttributes.Bold,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        };
                        Label labelPrecio = new Label()
                        {
                            AutomationId = "l4B3LpR3c10Pr0Duc70_&",
                            Text = "$ " + subTotal.ToString("F"),
                            FontAttributes = FontAttributes.Bold,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        };
                        Button btnBorrarProd = new Button()
                        {
                            AutomationId = "b7NPr0Duc70_" + Token,
                            Text = "x",
                            FontSize = 8,
                            CornerRadius = 20,
                            TextColor = Color.White,
                            BackgroundColor = Color.FromRgb(220, 53, 69),
                            FontFamily = "fontawesomeicons.ttf#Regular",
                            WidthRequest = 30,
                            HeightRequest = 30,
                            VerticalOptions = LayoutOptions.Center
                        };
                        btnBorrarProd.Command = new Command(() => {
                            QuitarProductoListaCotiz(new View[] { labelNomProd, labelCantidad, labelPrecio, btnBorrarProd }, new string[] { labelNomProd.Text, labelCantidad.Text, labelPrecio.Text });
                        });

                        CotizListaProductos.Children.Add(labelNomProd, 0, rowIndx);
                        CotizListaProductos.Children.Add(labelCantidad, 1, rowIndx);
                        CotizListaProductos.Children.Add(labelPrecio, 2, rowIndx);
                        CotizListaProductos.Children.Add(btnBorrarProd, 3, rowIndx);

                        CotizProductosLista.Add(new CotizProductosClase()
                        {
                            Token = Token,
                            IdProducto = productoInfo.Id,
                            Descripcion = productoInfo.NombreProducto + " " + presentInfo.NombrePresentacion,
                            Presentacion = presentInfo.NombrePresentacion,
                            Cantidad = CantidadProducto,
                            PrecioUnitario = Importe,
                            PrecioUnitarioDesc = valorUniDesc,
                            DescuentoPorc = DescuentoPorc,
                            Descuento = DescuentoMonto,
                            IEPS = ieps,
                            IEPSMonto = iepsMonto,
                            IVA = iva,
                            IVAMonto = ivaMonto,
                            Importe = subTotal
                        });

                        CalcularTotalesCotiz();

                        CotizProductosBusq.Text = "";
                        CotizProductoFormLista.SelectedItem = string.Empty;
                        CotizPresentFormLista.SelectedItem = string.Empty;
                        PresentCotizLista = new List<PresentCotizPicker>();
                        CotizPresentFormLista.ItemsSource = PresentCotizLista;
                        CotizPreciosClientes.Children.Clear();
                    }
                    else
                    {
                        await DisplayAlert("Atención!", "El producto elegido ya está en la lista", "Aceptar");
                    }
                }
                catch (Exception err)
                {
                    await DisplayAlert("Error!", "Ocurrió un error al añadir producto: " + err.ToString(), "Aceptar");
                }
            }
        }

        // FUNCION QUE QUITA LOS ELEMENTOS DE LA LISTA DE PRODUCTOS
        private async void QuitarProductoListaCotiz(View[] ElemsBorrar, string[] ElemsData)
        {
            var borrar = await DisplayAlert("Atencion!", "¿Desea borrar a " + ElemsData[0] + "?", "Si", "Cancelar");
            if (borrar)
            {
                CotizListaProductos.Children.Remove(ElemsBorrar[0]);
                CotizListaProductos.Children.Remove(ElemsBorrar[1]);
                CotizListaProductos.Children.Remove(ElemsBorrar[2]);
                CotizListaProductos.Children.Remove(ElemsBorrar[3]);

                int cont = 0;
                foreach (var Elem in CotizListaProductos.Children)
                {
                    cont++;
                }
                if (cont == 0)
                {
                    CotizListaProductos.Children.Clear();
                    CotizListaProductos.RowDefinitions.Clear();
                    CotizListaProductos.ColumnDefinitions.Clear();
                    CotizProductosLista = new List<CotizProductosClase>();
                }
                CalcularTotalesCotiz();
            }
        }

        // FUNCION QUE LIMPIA LOS CAMPOS
        private void LimpiarCamposCotizacion()
        {
            CotizCantidadProductos.Text = "0";
            CotizTotalPrecio.Text = "$ 0.00";
            CotizSubTotal.Text = "$ 0.00";
            CotizIEPS.Text = "$ 0.00";
            CotizImpuestos.Text = "$ 0.00";
            CotizDescuento.Text = "$ 0.00";
        }

        // FUNCION QUE CALCULA LOS TOTALES
        private async void CalcularTotalesCotiz()
        {
            try
            {
                List<string> Tokens = new List<string>();
                foreach (var Elem in CotizListaProductos.Children)
                {
                    string[] elemArr = Elem.AutomationId.Split('_');
                    if (elemArr[0] == "b7NPr0Duc70")
                    {
                        Tokens.Add(elemArr[1]);
                    }
                }
                string[] TokensArr = Tokens.ToArray();
                float cantidad = 0;
                double subtotal = 0, descuento = 0, ieps = 0, iva = 0;
                foreach (var CotizProducto in CotizProductosLista)
                {
                    if (TokensArr.Contains(CotizProducto.Token))
                    {
                        cantidad += CotizProducto.Cantidad;
                        subtotal += CotizProducto.Importe;
                        descuento += CotizProducto.Descuento;
                        ieps += CotizProducto.IEPSMonto;
                        iva += CotizProducto.IVAMonto;
                    }
                }

                CotizCantidadProductos.Text = cantidad.ToString();
                CotizSubTotal.Text = "$ " + Math.Round(subtotal, 2).ToString("F");
                CotizIEPS.Text = "$ " + Math.Round(ieps, 2).ToString("F");
                CotizImpuestos.Text = "$ " + Math.Round(iva, 2).ToString("F");
                CotizDescuento.Text = "$ " + descuento.ToString("F");
                CotizTotalPrecio.Text = "$ " + ((Math.Round(subtotal, 2) + Math.Round(ieps, 2) + Math.Round(iva, 2)) - descuento).ToString("F");
            }
            catch (Exception err)
            {
                await DisplayAlert("Error!", "Ocurrió un problema al agregar producto a la lista: - " + err.ToString(), "Aceptar");
            }
        }

        // FUNCION QUE CREA UNA CADENA DE CARACTERES LARGA PARA EL USO DE REALIZAR TOKENS
        private static string CrearTokenCotiz()
        {
            string token = ""; Int64 cadFecha = 0; string cadFechaTXT = ""; Random num = new Random();
            var f = new DateTime(1970, 1, 1);
            TimeSpan t = (DateTime.Now.ToUniversalTime() - f);
            cadFecha = (Int64)(t.TotalMilliseconds + 0.5);
            cadFechaTXT = cadFecha.ToString();
            foreach (char c in cadFechaTXT)
            {
                int l = num.Next(0, 26);
                char let = (char)('a' + l);
                token += c + "" + let;
            }
            return token;
        }

        // BOTON QUE GENERA LA COTIZACION EN FORMTO PDF
        private async void Generar_Cotizacion(object sender, EventArgs e)
        {
            if(Convert.ToDouble(CotizTotalPrecio.Text.Replace("$ ", "")) > 0)
            {
                using (UserDialogs.Instance.Loading("Generando Cotizacion..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));

                    int indexCliente = CotizClienteFormLista.SelectedIndex;
                    int indexTC = CotizTipoClienteFormLista.SelectedIndex;
                    CotizTipoCliente infoTC = JsonConvert.DeserializeObject<CotizTipoCliente>(JsonConvert.SerializeObject(CotizTipoClienteFormLista.ItemsSource[indexTC]));
                    ClientesCotizPicker clienteInfo = JsonConvert.DeserializeObject<ClientesCotizPicker>(JsonConvert.SerializeObject(CotizClienteFormLista.ItemsSource[indexCliente]));
                    List<Cotizaciones> altaCotizacion = new List<Cotizaciones>();
                    List<CotizacionProductos> altaCotizProductos = new List<CotizacionProductos>();

                    int IdMaxCotiz = 0;
                    var ObtenerMaxId = DependencyService.Get<ISQliteParams>().ConsultaCotizacion("SELECT MAX(Id) AS Id FROM Cotizaciones");
                    if (ObtenerMaxId.Count > 0)
                    {
                        foreach (var cotiz in ObtenerMaxId)
                        {
                            IdMaxCotiz = cotiz.Id + 1;
                        }
                    }
                    else
                    {
                        IdMaxCotiz = 1;
                    }

                    Cotizaciones altaCotizacionSingle = new Cotizaciones()
                    {
                        Id = IdMaxCotiz,
                        TipoCliente = infoTC.TipoCliente,
                        IdCliente = clienteInfo.Id,
                        Cliente = clienteInfo.RazonSocial,
                        CantidadArticulos = CotizCantidadProductos.Text.Replace("$ ", ""),
                        Subtotal = CotizSubTotal.Text.Replace("$ ", ""),
                        IEPS = CotizIEPS.Text.Replace("$ ", ""),
                        IVA = CotizImpuestos.Text.Replace("$ ", ""),
                        Descuentps = CotizDescuento.Text.Replace("$ ", ""),
                        Total = CotizTotalPrecio.Text.Replace("$ ", ""),
                        FechaCreada = DateTime.Now
                    };
                    altaCotizacion.Add(altaCotizacionSingle);

                    List<string> Tokens = new List<string>();
                    foreach (var Elem in CotizListaProductos.Children)
                    {
                        string[] elemArr = Elem.AutomationId.Split('_');
                        if (elemArr[0] == "b7NPr0Duc70")
                        {
                            Tokens.Add(elemArr[1]);
                        }
                    }
                    string[] TokensArr = Tokens.ToArray();
                    foreach (var CotizProducto in CotizProductosLista)
                    {
                        if (TokensArr.Contains(CotizProducto.Token))
                        {
                            altaCotizProductos.Add(new CotizacionProductos()
                            {
                                IdCotizacion = IdMaxCotiz,
                                IdProducto = CotizProducto.IdProducto,
                                Descripcion = CotizProducto.Descripcion,
                                Presentacion = CotizProducto.Presentacion,
                                Cantidad = CotizProducto.Cantidad,
                                PrecioUnitario = CotizProducto.PrecioUnitario,
                                PrecioUnitarioDesc = CotizProducto.PrecioUnitarioDesc,
                                DescuentoPorc = CotizProducto.DescuentoPorc,
                                Descuento = CotizProducto.Descuento,
                                IVA = CotizProducto.IVA,
                                IVAMonto = CotizProducto.IVAMonto,
                                IEPS = CotizProducto.IEPS,
                                IEPSMonto = CotizProducto.IEPSMonto,
                                Importe = CotizProducto.Importe
                            });
                        }
                    }

                    if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                    {
                        CotizacionPDF cotizPDF = new CotizacionPDF()
                        {
                            Cotizacion = JsonConvert.SerializeObject(altaCotizacionSingle),
                            CotizacionProductos = JsonConvert.SerializeObject(altaCotizProductos)
                        };
                        try
                        {
                            Device.OpenUri(new Uri(DependencyService.Get<IAgroWS>().GenerarCotizacion(JsonConvert.SerializeObject(cotizPDF))));
                            LimpiarValoresCotizEsp();
                        }
                        catch
                        {
                            var guardarCotiz = await DisplayAlert("Atencion!", "Ocurrió un problema al generar Cotización ¿Desea guardarla para generarla más tarde?", "Si", "Cancelar");
                            if (guardarCotiz)
                            {
                                try
                                {
                                    string AltaCotiz = DependencyService.Get<ISQliteParams>().GuardarCotizacion(1, altaCotizacion, altaCotizProductos);
                                    if (AltaCotiz == "true")
                                    {
                                        LimpiarValoresCotizEsp();
                                    }
                                    else
                                    {
                                        await DisplayAlert("Error!", "Ocurrió un problema al almacenar Cotización: - " + AltaCotiz, "Aceptar");
                                    }
                                }
                                catch (Exception err)
                                {
                                    await DisplayAlert("Error!", err.ToString(), "Aceptar");
                                }
                            }
                        }
                    }
                    else
                    {
                        var guardarCotiz = await DisplayAlert("Atencion!", "No tiene conección a Internet ¿Desea guardar la cotización para generarla más tarde?", "Si", "Cancelar");
                        if (guardarCotiz)
                        {
                            try
                            {
                                string AltaCotiz = DependencyService.Get<ISQliteParams>().GuardarCotizacion(1, altaCotizacion, altaCotizProductos);
                                if (AltaCotiz == "true")
                                {
                                    LimpiarValoresCotizEsp();
                                }
                                else
                                {
                                    await DisplayAlert("Error!", "Ocurrió un problema al almacenar Cotización: - " + AltaCotiz, "Aceptar");
                                }
                            }
                            catch (Exception err)
                            {
                                await DisplayAlert("Error!", err.ToString(), "Aceptar");
                            }
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("Atención!", "No tiene Productos en la Cotización", "Aceptar");
            }
        }

        // FUNCION QUE LIMPIA LOS VALORES COMPLETOS (USADO AL TERMINAR DE GENERAR LA COTIZACION)
        private void LimpiarValoresCotizEsp()
        {
            CotizProductoFormLista.SelectedItem = string.Empty;
            CotizProductosBusq.Text = "";
            CotizPresentFormLista.SelectedItem = string.Empty;
            PresentCotizLista = new List<PresentCotizPicker>();
            CotizProductosLista = new List<CotizProductosClase>();
            IdProductoCotiz = 0;
            NomPresentCotiz = "";
            CotizPresentFormLista.ItemsSource = PresentCotizLista;

            CotizListaProductos.Children.Clear();
            CotizListaProductos.RowDefinitions.Clear();
            CotizListaProductos.ColumnDefinitions.Clear();
            CotizPreciosClientes.Children.Clear();

            CotizLineaListaProductos.IsVisible = false;
            CotizTotalesListaProductos.IsVisible = false;
            CotizElemsVisibles = false;
            LimpiarCamposCotizacion();

            LlenarTablaCotizacionesCliente();
        }
        // :::::::::::::::::::::::::::::::::::::::: [ MENU HISTORIAL ] ::::::::::::::::::::::::::::::::::::::::

        // FUNCION QUE GENERA LA LISTA DE COTIZACIONES VIGENTES SIN GUARDARSE
        private async void LlenarTablaCotizacionesCliente()
        {
            using (UserDialogs.Instance.Loading("Espere..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));

                CotizTablaCliente.Children.Clear();
                var ObtenerCotiz = DependencyService.Get<ISQliteParams>().ConsultaCotizacion("SELECT * FROM Cotizaciones WHERE IdCliente = " + IdClienteCotiz.ToString());
                if(ObtenerCotiz.Count > 0)
                {
                    Grid GridTabla = new Grid()
                    {
                        AutomationId = "6r1Dt4BL4",
                        Margin = new Thickness(0, 5, 0, 10)
                    };
                    GridTabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    Label labelT1 = new Label()
                    {
                        AutomationId = "l4B3Lt1",
                        Text = "FECHA",
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 13
                    };
                    Frame FrameT1 = new Frame()
                    {
                        BackgroundColor = Color.FromRgb(214, 219, 223),
                        BorderColor = Color.Black,
                        Padding = new Thickness(2, 0, 2, 0),
                        Margin = new Thickness(0, 0, 0, 0),
                        Content = labelT1
                    };

                    Label labelT2 = new Label()
                    {
                        AutomationId = "l4B3Lt2",
                        Text = "TIPO CLIENTE",
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = 13
                    };
                    Frame FrameT2 = new Frame()
                    {
                        BackgroundColor = Color.FromRgb(214, 219, 223),
                        BorderColor = Color.Black,
                        Padding = new Thickness(2, 0, 2, 0),
                        Margin = new Thickness(0, 0, 0, 0),
                        Content = labelT2
                    };

                    Label labelT3 = new Label()
                    {
                        AutomationId = "l4B3Lt3",
                        Text = "IMPRIMIR",
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 13
                    };
                    Frame FrameT3 = new Frame()
                    {
                        BackgroundColor = Color.FromRgb(214, 219, 223),
                        BorderColor = Color.Black,
                        Padding = new Thickness(2, 0, 2, 0),
                        Margin = new Thickness(0, 0, 0, 0),
                        Content = labelT3
                    };

                    GridTabla.Children.Add(FrameT1);
                    GridTabla.Children.Add(FrameT2);
                    GridTabla.Children.Add(FrameT3);

                    Grid.SetRow(FrameT1, 0);
                    Grid.SetColumn(FrameT1, 0);
                    Grid.SetRow(FrameT2, 0);
                    Grid.SetColumn(FrameT2, 1);
                    Grid.SetRow(FrameT3, 0);
                    Grid.SetColumn(FrameT3, 2);

                    int row = 1;
                    foreach (var Cotizacion in ObtenerCotiz)
                    {
                        GridTabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        Label labelP1 = new Label()
                        {
                            AutomationId = "l4B3Lp1",
                            Text = Cotizacion.FechaCreada.ToString("dd/MM/yyyy"),
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 12
                        };
                        Frame FrameP1 = new Frame()
                        {
                            BorderColor = Color.Black,
                            Padding = new Thickness(2, 0, 2, 0),
                            Margin = new Thickness(0, 0, 0, 0),
                            Content = labelP1
                        };

                        Label labelP2 = new Label()
                        {
                            AutomationId = "l4B3Lp2",
                            Text = Cotizacion.TipoCliente,
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 12
                        };
                        Frame FrameP2 = new Frame()
                        {
                            BorderColor = Color.Black,
                            Padding = new Thickness(2, 0, 2, 0),
                            Margin = new Thickness(0, 0, 0, 0),
                            Content = labelP2
                        };

                        Button btnImpCotiz = new Button()
                        {
                            AutomationId = "b7N1mPc071z",
                            Text = "*",
                            FontSize = 8,
                            CornerRadius = 20,
                            TextColor = Color.White,
                            BackgroundColor = Color.FromRgb(52, 58, 64),
                            FontFamily = "fontawesomeicons.ttf#Regular",
                            WidthRequest = 30,
                            HeightRequest = 30,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center
                        };
                        btnImpCotiz.Command = new Command(() => {
                            ReimprimirCotizacion(Cotizacion.Id);
                        });

                        GridTabla.Children.Add(FrameP1);
                        GridTabla.Children.Add(FrameP2);
                        GridTabla.Children.Add(btnImpCotiz);

                        Grid.SetRow(FrameP1, row);
                        Grid.SetColumn(FrameP1, 0);
                        Grid.SetRow(FrameP2, row);
                        Grid.SetColumn(FrameP2, 1);
                        Grid.SetRow(btnImpCotiz, row);
                        Grid.SetColumn(btnImpCotiz, 2);

                        row++;
                    }

                    CotizTablaCliente.Children.Add(GridTabla);
                }
                else
                {
                    Grid GridNoCotiz = new Grid()
                    {
                        AutomationId = "6r1Dn0C071z",
                        Margin = new Thickness(0, 10, 0, 10)
                    };

                    GridNoCotiz.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    GridNoCotiz.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                    Label labelNoHist = new Label()
                    {
                        AutomationId = "l4B3Ln0C071z",
                        Text = "  NO TIENE COTIZACIONES PENDIENTES  ",
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 17,
                        BackgroundColor = Color.FromRgb(220, 53, 69),
                        TextColor = Color.White
                    };

                    GridNoCotiz.Children.Add(labelNoHist);

                    Grid.SetRow(labelNoHist, 0);
                    Grid.SetColumn(labelNoHist, 0);

                    CotizTablaCliente.Children.Add(GridNoCotiz);
                }
            }
        }

        // BOTON COMMAND QUE CONTROLA LA IMPRESION DE LA COTIZACIÓN ALMACENADA EN EL DISPOSITIVO (POR ERROR EN INTERNET)
        private async void ReimprimirCotizacion(int idcotizacion)
        {
            using (UserDialogs.Instance.Loading("Imprimiendo Cotizacion..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    try
                    {
                        var CotizConsulta = DependencyService.Get<ISQliteParams>().ConsultaCotizacion("SELECT * FROM Cotizaciones WHERE Id = " + idcotizacion);
                        var CotizProductosConsulta = DependencyService.Get<ISQliteParams>().ConsultaCotizacionProductos("SELECT * FROM CotizacionProductos WHERE IdCotizacion = " + idcotizacion);
                        Cotizaciones CotizData = new Cotizaciones();
                        List<CotizacionProductos> CotizProdData = new List<CotizacionProductos>();
                        foreach (var Cotiz in CotizConsulta)
                        {
                            CotizData = new Cotizaciones()
                            {
                                TipoCliente = Cotiz.TipoCliente,
                                IdCliente = Cotiz.IdCliente,
                                Cliente = Cotiz.Cliente,
                                CantidadArticulos = Cotiz.CantidadArticulos,
                                Subtotal = Cotiz.Subtotal,
                                Descuentps = Cotiz.Descuentps,
                                IEPS = Cotiz.IEPS,
                                IVA = Cotiz.IVA,
                                Total = Cotiz.Total
                            };
                        }
                        foreach (var CotizProd in CotizProductosConsulta)
                        {
                            CotizProdData.Add(new CotizacionProductos()
                            {
                                IdProducto = CotizProd.IdProducto,
                                Descripcion = CotizProd.Descripcion,
                                Presentacion = CotizProd.Presentacion,
                                Cantidad = CotizProd.Cantidad,
                                PrecioUnitario = CotizProd.PrecioUnitario,
                                PrecioUnitarioDesc = CotizProd.PrecioUnitarioDesc,
                                DescuentoPorc = CotizProd.DescuentoPorc,
                                Descuento = CotizProd.Descuento,
                                IVA = CotizProd.IVA,
                                IVAMonto = CotizProd.IVAMonto,
                                IEPS = CotizProd.IEPS,
                                IEPSMonto = CotizProd.IEPSMonto,
                                Importe = CotizProd.Importe
                            });
                        }
                        CotizacionPDF cotizPDF = new CotizacionPDF()
                        {
                            Cotizacion = JsonConvert.SerializeObject(CotizData),
                            CotizacionProductos = JsonConvert.SerializeObject(CotizProdData)
                        };
                        try
                        {
                            Device.OpenUri(new Uri(DependencyService.Get<IAgroWS>().GenerarCotizacion(JsonConvert.SerializeObject(cotizPDF))));
                            object[] borrarCotiz = { idcotizacion };
                            string borrarCotiz1 = DependencyService.Get<ISQliteParams>().QueryMaestra(11, "DELETE FROM Cotizaciones WHERE Id = ?", borrarCotiz);
                            string borrarCotiz2 = DependencyService.Get<ISQliteParams>().QueryMaestra(12, "DELETE FROM CotizacionProductos WHERE IdCotizacion = ?", borrarCotiz);
                            LlenarTablaCotizacionesCliente();
                        }
                        catch
                        {
                            await DisplayAlert("Error!", "Ocurrió un problema al imprimir la Cotización", "Aceptar");
                        }
                    }
                    catch (Exception err)
                    {
                        await DisplayAlert("Error!", err.ToString(), "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Necesita conección a Internet para realizar esta acción", "Aceptar");
                }
            }
        }

        // ------------------------ [ NUEVA BUSQUEDA ] ------------------------

        // FUNCION QUE CONTROLA  LA ESCRITURA QUE EJECUTA LA BUSQUEDA DEL CLIENTE EN COTIZACION
        private void CotizClienteBusq_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = CotizClienteBusq.Text.ToUpper();
                var busqResultado = NombresClientesCotiz.Where(i => i.StartsWith(busqCliente)).ToList();
                CotizClienteBusq.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE CONTROLA LA BUSQUEDA ESCRITA DEL CLIENTE EN COTIZACION
        private void CotizClienteBusq_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = NombresClientesCotiz.FindIndex(x => x.StartsWith(CotizClienteBusq.Text.ToUpper()));
            CotizClienteFormLista.SelectedIndex = index;
        }
        // FUNCION QUE CONTROLA LA BUSQUEDA DEL PRODUCTO EN COTIZACION
        private void CotizProductosBusq_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqProducto = CotizProductosBusq.Text.ToUpper();
                var busqResultado = NombresProductosCotiz.Where(i => i.StartsWith(busqProducto)).ToList();
                CotizProductosBusq.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE CONTROLA AL ELEGIR UN ELEMENTO DE LA BUSQUEDA DE PRODUCTO PARA LA COTIZACION
        private void CotizProductosBusq_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = NombresProductosCotiz.FindIndex(x => x.StartsWith(CotizProductosBusq.Text.ToUpper()));
            CotizProductoFormLista.SelectedIndex = index;
        }

        // :::::::::::::::::::::::::::::::::::::::: [ MENU BUSCAR ] ::::::::::::::::::::::::::::::::::::::::

        // BOTON QUE TRAE LA LISTA DE COTIZACIONES ONLINE (DEL SERVIDOR)
        private async void Cotizaciones_Online(object sender, EventArgs e)
        {
            if(CotizBusqFechaIniFormLista.Date <= CotizBusqFechaFinFormLista.Date)
            {
                int indexCliente = CotizBuscClienteFormLista.SelectedIndex;
                if(indexCliente != -1)
                {
                    using (UserDialogs.Instance.Loading("Consultando Cotizaciones..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                        {
                            try
                            {
                                CotizOnlineClientes.Children.Clear();
                                ClientesCotizPicker clienteInfo = JsonConvert.DeserializeObject<ClientesCotizPicker>(JsonConvert.SerializeObject(CotizBuscClienteFormLista.ItemsSource[indexCliente]));
                                ReporteParamsConsulta ReporteConsulta = new ReporteParamsConsulta()
                                {
                                    IdCliente = clienteInfo.Id,
                                    FechaInicio = CotizBusqFechaIniFormLista.Date/*.AddDays(-1)*/.ToString("yyyy/MM/dd"),
                                    FechaFin = CotizBusqFechaFinFormLista.Date.AddDays(1).ToString("yyyy/MM/dd")
                                };
                                ReporteEstructura ReporteInfo = JsonConvert.DeserializeObject<ReporteEstructura>(DependencyService.Get<IAgroWS>().ReportesOnline(JsonConvert.SerializeObject(ReporteConsulta), 2));
                                UrlServerCotiz = ReporteInfo.Url;
                                List<ReportesWS> ReporteData = JsonConvert.DeserializeObject<List<ReportesWS>>(ReporteInfo.Data);
                                if (ReporteData.Count > 0)
                                {
                                    Grid GridTabla = new Grid()
                                    {
                                        AutomationId = "6r1Dt4BL4",
                                        Margin = new Thickness(0, 5, 0, 10)
                                    };
                                    GridTabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                                    Label labelT1 = new Label()
                                    {
                                        AutomationId = "l4B3Lt1",
                                        Text = "FECHA",
                                        FontAttributes = FontAttributes.Bold,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        FontSize = 13
                                    };
                                    Frame FrameT1 = new Frame()
                                    {
                                        BackgroundColor = Color.FromRgb(214, 219, 223),
                                        BorderColor = Color.Black,
                                        Padding = new Thickness(2, 0, 2, 0),
                                        Margin = new Thickness(0, 0, 0, 0),
                                        Content = labelT1
                                    };

                                    Label labelT2 = new Label()
                                    {
                                        AutomationId = "l4B3Lt2",
                                        Text = "FOLIO",
                                        FontAttributes = FontAttributes.Bold,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        FontSize = 13
                                    };
                                    Frame FrameT2 = new Frame()
                                    {
                                        BackgroundColor = Color.FromRgb(214, 219, 223),
                                        BorderColor = Color.Black,
                                        Padding = new Thickness(2, 0, 2, 0),
                                        Margin = new Thickness(0, 0, 0, 0),
                                        Content = labelT2
                                    };

                                    Label labelT3 = new Label()
                                    {
                                        AutomationId = "l4B3Lt3",
                                        Text = "IMPRIMIR",
                                        FontAttributes = FontAttributes.Bold,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        FontSize = 13
                                    };
                                    Frame FrameT3 = new Frame()
                                    {
                                        BackgroundColor = Color.FromRgb(214, 219, 223),
                                        BorderColor = Color.Black,
                                        Padding = new Thickness(2, 0, 2, 0),
                                        Margin = new Thickness(0, 0, 0, 0),
                                        Content = labelT3
                                    };

                                    GridTabla.Children.Add(FrameT1);
                                    GridTabla.Children.Add(FrameT2);
                                    GridTabla.Children.Add(FrameT3);

                                    Grid.SetRow(FrameT1, 0);
                                    Grid.SetColumn(FrameT1, 0);
                                    Grid.SetRow(FrameT2, 0);
                                    Grid.SetColumn(FrameT2, 1);
                                    Grid.SetRow(FrameT3, 0);
                                    Grid.SetColumn(FrameT3, 2);

                                    int row = 1;
                                    foreach (var Reporte in ReporteData)
                                    {
                                        GridTabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                                        Label labelP1 = new Label()
                                        {
                                            AutomationId = "l4B3Lp1",
                                            Text = Reporte.Fecha.ToString("dd/MM/yyyy hh:mm tt"),
                                            FontAttributes = FontAttributes.Bold,
                                            VerticalTextAlignment = TextAlignment.Center,
                                            FontSize = 12
                                        };
                                        Frame FrameP1 = new Frame()
                                        {
                                            BorderColor = Color.Black,
                                            Padding = new Thickness(2, 0, 2, 0),
                                            Margin = new Thickness(0, 0, 0, 0),
                                            Content = labelP1
                                        };

                                        Label labelP2 = new Label()
                                        {
                                            AutomationId = "l4B3Lp2",
                                            Text = Reporte.Folio,
                                            FontAttributes = FontAttributes.Bold,
                                            VerticalTextAlignment = TextAlignment.Center,
                                            FontSize = 12
                                        };
                                        Frame FrameP2 = new Frame()
                                        {
                                            BorderColor = Color.Black,
                                            Padding = new Thickness(2, 0, 2, 0),
                                            Margin = new Thickness(0, 0, 0, 0),
                                            Content = labelP2
                                        };

                                        Button btnImpCotiz = new Button()
                                        {
                                            AutomationId = "b7N1mPc071z",
                                            Text = "*",
                                            FontSize = 8,
                                            CornerRadius = 20,
                                            TextColor = Color.White,
                                            BackgroundColor = Color.FromRgb(52, 58, 64),
                                            FontFamily = "fontawesomeicons.ttf#Regular",
                                            WidthRequest = 30,
                                            HeightRequest = 30,
                                            VerticalOptions = LayoutOptions.Center,
                                            HorizontalOptions = LayoutOptions.Center
                                        };
                                        btnImpCotiz.Command = new Command(() => {
                                            ImprimirCotizacionOnline(Reporte.Documento);
                                        });

                                        GridTabla.Children.Add(FrameP1);
                                        GridTabla.Children.Add(FrameP2);
                                        GridTabla.Children.Add(btnImpCotiz);

                                        Grid.SetRow(FrameP1, row);
                                        Grid.SetColumn(FrameP1, 0);
                                        Grid.SetRow(FrameP2, row);
                                        Grid.SetColumn(FrameP2, 1);
                                        Grid.SetRow(btnImpCotiz, row);
                                        Grid.SetColumn(btnImpCotiz, 2);

                                        row++;
                                    }

                                    CotizOnlineClientes.Children.Add(GridTabla);
                                }
                                else
                                {
                                    Grid GridNoCotiz = new Grid()
                                    {
                                        AutomationId = "6r1Dn0C071z",
                                        Margin = new Thickness(0, 10, 0, 10)
                                    };

                                    GridNoCotiz.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                                    GridNoCotiz.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                                    Label labelNoHist = new Label()
                                    {
                                        AutomationId = "l4B3Ln0C071z",
                                        Text = "  NO SE ENCONTRARON COTIZACIONES  ",
                                        FontAttributes = FontAttributes.Bold,
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        FontSize = 17,
                                        BackgroundColor = Color.FromRgb(220, 53, 69),
                                        TextColor = Color.White
                                    };

                                    GridNoCotiz.Children.Add(labelNoHist);

                                    Grid.SetRow(labelNoHist, 0);
                                    Grid.SetColumn(labelNoHist, 0);

                                    CotizOnlineClientes.Children.Add(GridNoCotiz);
                                }
                            }
                            catch (Exception err)
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al consultar las Cotizaciones: - " + err.ToString(), "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Seleeccione un cliente", "Aceptar");
                }
            }
            else
            {
                await DisplayAlert("Atención!", "La fechas seleccionadas son incorrectas", "Aceptar");
            }
        }

        // FUNCION QUE IMPRIME EL REPORTE ONLINE
        private async void ImprimirCotizacionOnline(string pdfreportecotizacion)
        {
            using (UserDialogs.Instance.Loading("Imprimiendo Cotización..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    Device.OpenUri(new Uri(UrlServerCotiz + pdfreportecotizacion + ".pdf"));
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                }
            }
        }

        // :::::::::::::::::::::::::::: [ NUEVA BUSQUEDA ] ::::::::::::::::::::::::::::
        // FUNCION  QUE CONTROLA LA BUSQUEDA ESCRITA DEL CLIENTE EN LA BUSQUEDA ONLINE
        private void CotizBusqClienteLista_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = CotizBusqClienteLista.Text.ToUpper();
                var busqResultado = NombresClientesCotizBusq.Where(i => i.StartsWith(busqCliente)).ToList();
                CotizBusqClienteLista.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE DETONA LA BUSQUEDA DEL CLIENTE EN LA BUSQUEDA ONLINE
        private void CotizBusqClienteLista_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = NombresClientesCotizBusq.FindIndex(x => x.StartsWith(CotizBusqClienteLista.Text.ToUpper()));
            CotizBuscClienteFormLista.SelectedIndex = index;
        }
    }
}