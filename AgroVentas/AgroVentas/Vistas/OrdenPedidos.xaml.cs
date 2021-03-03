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
    public partial class OrdenPedidos : TabbedPage
    {
        public OrdenPedidos()
        {
            InitializeComponent();

            LLenarClientesOrdenPedido();
        }

        // *******************************************************************************
        // ----------------------------- [ ORDENES DE PAGO ] -----------------------------
        // ---------- VARIABLES GLOBALES PUBLICAS ----------
        // --------- CLASE PARA REPORTE PDF
        // ESTRUCTURA GENERAL
        private class OrdenesPedidoPDF
        {
            public string NumOrdenPedido { get; set; }
            public string NombreAgente { get; set; }
            public string TipoDocumento { get; set; }
            public string TipoCliente { get; set; }
            public string RazonSocial { get; set; }
            public string Campo { get; set; }
            public string Ubicacion { get; set; }
            public string FechaEntrega { get; set; }
            public string ListaProductos { get; set; }
            public string CantidadArticulos { get; set; }
            public string Subtotal { get; set; }
            public string Descuentps { get; set; }
            public string IEPS { get; set; }
            public string IVA { get; set; }
            public string Total { get; set; }
        }
        // TABLA DE LISTADO PRODUCTOS 
        private class OrdenesPedidoTablaPDF
        {
            public int IdProducto { get; set; }
            public string Presentacion { get; set; }
            public string Producto { get; set; }
            public string Cantidad { get; set; }
            public string Precio { get; set; }
            public string DescuentoPorc { get; set; }
            public string DescuentoMonto { get; set; }
            public string PrecioConDescuento { get; set; }
            public string IEPS { get; set; }
            public string Impuestos { get; set; }
            public string Subtotal { get; set; }
        }
        // ---------- VARIABLES GENERALES
        private class OPClientesPicker
        {
            public int Id { get; set; }
            public string RazonSocial { get; set; }
        }
        private class OPTipoDocumento
        {
            public int Id { get; set; }
            public string NombreDocumento { get; set; }
        }
        private class OPTipoCliente
        {
            public int Id { get; set; }
            public string TipoCliente { get; set; }
            public double DollarValor { get; set; }
        }
        private class OPCamposCliente
        {
            public int Id { get; set; }
            public int IdCliente { get; set; }
            public string CampoCliente { get; set; }
            public string Ubicacion { get; set; }
        }
        private class OPProductosPicker
        {
            public int Id { get; set; }
            public string NombreProducto { get; set; }
        }
        private class OPPresentProductoPicker
        {
            public int Id { get; set; }
            public string NombrePresentacion { get; set; }
            public int IdProducto { get; set; }
            public double Precio { get; set; }
            public int IVA { get; set; }
            public int IEPS { get; set; }
            public int Moneda { get; set; }
        }
        private class OrdenesPedidoAltaWS
        {
            public string OrdenesPedido { get; set; }
            public string OrdenesPedidoProductos { get; set; }
        }
        private class OrdenesPedidoProductosClase
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
        private List<OPClientesPicker> OPClientesLista;
        private List<OPTipoDocumento> OPTipoDoccumentoLista;
        private List<OPTipoCliente> OPTipoClienteLista;
        private List<OPCamposCliente> OPCamposClienteLista;
        private List<OPProductosPicker> OPProductosLista;
        private List<OPPresentProductoPicker> OPPresentProductoLista;
        private BDPendientes OrdenesPedidoDataWS;
        private OPPresentProductoPicker PresentGlobalOP;
        private List<OrdenesPedidoProductosClase> OrdenesPedidoProductosLista;
        List<string> NombresProductosBusq;
        List<string> NombresClientesBusq = new List<string>();
        List<string> NombresClientesOnlineBusq = new List<string>();

        bool NuevaOrdenPedido = true;
        int IdOrdenPedidoEditar = 0;

        bool msgOPClienteSelect = true;
        bool msgOPProductoSelect = true;

        double PrecioPublicoOP = 0;
        int IdClienteOP = 0;
        string UrlServerOP = "";
        // --------------------------------------------------

        // :::::::::::::::::::::::::::::::::::::::: [ MENU GENERAR ] ::::::::::::::::::::::::::::::::::::::::
        // FUNCION INICIAL QUE LLENA LOS LISTADOS DE LOS  COMBOS PARA REALIZAR UNA ORDEN DE PEDIDO
        private async void LLenarClientesOrdenPedido()
        {
            double DollarCredito = 0, DollarContado = 0;
            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var usuario in VerifUsuario)
            {
                DollarContado = usuario.DollarContado;
                DollarCredito = usuario.DollarCredito;
            }
            var clientesQuery = DependencyService.Get<ISQliteParams>().ConsultaClientes("SELECT * FROM Clientes WHERE Estatus = 1 ORDER BY RazonSocial ASC");
            OPClientesLista = new List<OPClientesPicker>();
            foreach (var cliente in clientesQuery)
            {
                OPClientesLista.Add(new OPClientesPicker
                {
                    Id = cliente.Id,
                    RazonSocial = cliente.RazonSocial
                });
                NombresClientesBusq.Add(cliente.RazonSocial);
                NombresClientesOnlineBusq.Add(cliente.RazonSocial);
            }

            OPClientesBusq.ItemsSource = NombresClientesBusq;
            OPBusqClientesLista.ItemsSource = NombresClientesBusq;

            var productosQuery = DependencyService.Get<ISQliteParams>().ConsultaProductos("SELECT * FROM Productos WHERE Estatus = 1 ORDER BY NombreProducto ASC");
            OPProductosLista = new List<OPProductosPicker>();
            NombresProductosBusq = new List<string>();
            foreach (var producto in productosQuery)
            {
                OPProductosLista.Add(new OPProductosPicker
                {
                    Id = producto.Id,
                    NombreProducto = producto.NombreProducto
                });
                NombresProductosBusq.Add(producto.NombreProducto);
            }

            OPTipoDoccumentoLista = new List<OPTipoDocumento>()
            {
                new OPTipoDocumento { Id = 1, NombreDocumento = "FACTURA" },
                new OPTipoDocumento { Id = 2, NombreDocumento = "REMISION" }
            };

            OPTipoClienteLista = new List<OPTipoCliente>()
            {
                new OPTipoCliente { Id = 1, TipoCliente = "CONTADO", DollarValor = DollarContado },
                new OPTipoCliente { Id = 2, TipoCliente = "CREDITO", DollarValor = DollarCredito }
            };

            OPClienteFormLista.ItemsSource = OPClientesLista;
            OPBuscClienteFormLista.ItemsSource = OPClientesLista;

            OPTipoDocumentoFormLista.ItemsSource = OPTipoDoccumentoLista;
            OPTipoClienteFormLista.ItemsSource = OPTipoClienteLista;
            OPProductosFormLista.ItemsSource = OPProductosLista;
            OPFechaEntrega.MinimumDate = DateTime.Now;
            OPProductosBusq.ItemsSource = NombresProductosBusq;

            OrdenesPedidoProductosLista = new List<OrdenesPedidoProductosClase>();

            LlenarListaOPNuevas();
            LlenarListaOPPendientes(); 

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            UserDialogs.Instance.HideLoading();

            if ((DollarCredito == 0) || (DollarContado == 0))
            {
                await DisplayAlert("Atención!", "Vaya al menú Configuración, pestaña OTROS para configurar los valores de CRÉDITO y CONTADO para el Dolar", "Aceptar");
            }
        }

        // BOTON CREAR NUEVA ORDEN DE PEDIDO
        private async void Nueva_OrdenPedido(object sender, EventArgs e)
        {
            if (NuevaOrdenPedido)
            {
                LimpiarOrdenPedidoFormulario(true);
            }
            else
            {
                var nuevaOP = await DisplayAlert("Atencion!", "Una Orden de Pedido se está editando, ¿Desea cancelar los cambios y empezar una nueva?", "Si", "Cancelar");
                if (nuevaOP)
                {
                    LimpiarOrdenPedidoFormulario(true);
                }
            }
        }

        // BOTON SELECT QUE EJECUTA LA ACCION AL ELEGIR UN CLIENTE - MUESTRA SUS CAMPOS CON SUS HUBICACIONES
        private async void OPElegir_Cliente(object sender, EventArgs e)
        {
            if (OPNumeroOrden.Text != "---")
            {
                int index = OPClienteFormLista.SelectedIndex;
                if (index != -1)
                {
                    using (UserDialogs.Instance.Loading("Espere..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        OPClientesPicker clienteInfo = JsonConvert.DeserializeObject<OPClientesPicker>(JsonConvert.SerializeObject(OPClienteFormLista.ItemsSource[index]));
                        var camposClientesQuery = DependencyService.Get<ISQliteParams>().ConsultaCampos("SELECT * FROM Campos WHERE IdCliente = " + clienteInfo.Id.ToString());
                        OPCamposClienteLista = new List<OPCamposCliente>();
                        int c = 1;
                        foreach (var campos in camposClientesQuery)
                        {
                            OPCamposClienteLista.Add(new OPCamposCliente
                            {
                                Id = c,
                                IdCliente = campos.IdCliente,
                                CampoCliente = campos.NombreCampo,
                                Ubicacion = campos.Ubicacion
                            });
                            c++;
                        }
                        IdClienteOP = clienteInfo.Id;
                        OPClienteCampoFormLista.ItemsSource = OPCamposClienteLista;
                    }
                }
            }
            else
            {
                if (msgOPClienteSelect)
                {
                    await DisplayAlert("Atención!", "No ha iniciado una Nueva Orden de Pedido", "Aceptar");
                    msgOPClienteSelect = false;
                    OPClienteFormLista.SelectedItem = string.Empty;
                    OPClientesBusq.Text = "";
                }
                else
                {
                    msgOPClienteSelect = true;
                }
            }
        }

        // BOTON SELECT QUE EJECUTA LA ACCION AL ELEGIR EL CAMPO DEL CLIENTE MUESTRA LA UBICACION DEL CAMPO ELEGIDO
        private void OPElegir_CampoCliente(object sender, EventArgs e)
        {
            int index = OPClienteCampoFormLista.SelectedIndex;
            if (index != -1)
            {
                OPCamposCliente camposInfo = JsonConvert.DeserializeObject<OPCamposCliente>(JsonConvert.SerializeObject(OPClienteCampoFormLista.ItemsSource[index]));
                OPClienteUbicacion.Text = camposInfo.Ubicacion;
            }
        }

        // BOTON SELECT QUE EJECUTA LA ACCION AL ELEGIR EL PRODUCTO
        private async void OPProducto_Elegir(object sender, EventArgs e)
        {
            int indexCliente = OPClienteFormLista.SelectedIndex;
            if (indexCliente != -1)
            {
                int index = OPProductosFormLista.SelectedIndex;
                if (index != -1)
                {
                    using (UserDialogs.Instance.Loading("Espere..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        OPProductosPicker productoInfo = JsonConvert.DeserializeObject<OPProductosPicker>(JsonConvert.SerializeObject(OPProductosFormLista.ItemsSource[index]));
                        var presentProductosQuery = DependencyService.Get<ISQliteParams>().ConsultaPresentaciones("SELECT * FROM Presentaciones WHERE IdProducto = " + productoInfo.Id.ToString());
                        OPPresentProductoLista = new List<OPPresentProductoPicker>();
                        int c = 1;
                        foreach (var presents in presentProductosQuery)
                        {
                            OPPresentProductoLista.Add(new OPPresentProductoPicker
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
                        OPPresentProductoFormLista.ItemsSource = OPPresentProductoLista;
                        OPCantProductoFormLista.Text = "";
                        OPPrecioProductoFormLista.Text = "";
                    }
                }
            }
            else
            {
                if (msgOPProductoSelect)
                {
                    await DisplayAlert("Atención!", "Eliga un cliente", "Aceptar");
                    msgOPProductoSelect = false;
                    OPProductosFormLista.SelectedItem = string.Empty;
                    OPProductosBusq.Text = "";
                }
                else
                {
                    msgOPProductoSelect = true;
                }
            }
        }

        // BOTON SELECT QUE EJECUTA EL SELECCIONAR UN PRECIO DEL CLIENTE AL SELECCIONAR UN PRODUCTO Y UNA PRESENTACION
        private async void OPElegir_Presentacion(object sender, EventArgs e)
        {
            int index = OPPresentProductoFormLista.SelectedIndex;
            if (index != -1)
            {
                using (UserDialogs.Instance.Loading("Espere..."))
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        int indexCliente = OPClienteFormLista.SelectedIndex;
                        OPClientesPicker clienteInfo = JsonConvert.DeserializeObject<OPClientesPicker>(JsonConvert.SerializeObject(OPClienteFormLista.ItemsSource[indexCliente]));
                        int indexProducto = OPProductosFormLista.SelectedIndex;
                        OPProductosPicker productoInfo = JsonConvert.DeserializeObject<OPProductosPicker>(JsonConvert.SerializeObject(OPProductosFormLista.ItemsSource[indexProducto]));
                        OPPresentProductoPicker presentProductoInfo = JsonConvert.DeserializeObject<OPPresentProductoPicker>(JsonConvert.SerializeObject(OPPresentProductoFormLista.ItemsSource[index]));

                        OPCantProductoFormLista.Text = "";

                        int indexTC = OPTipoClienteFormLista.SelectedIndex;
                        OPTipoCliente infoTC = JsonConvert.DeserializeObject<OPTipoCliente>(JsonConvert.SerializeObject(OPTipoClienteFormLista.ItemsSource[indexTC]));
                        double Importe = (presentProductoInfo.Moneda == 2) ? (presentProductoInfo.Precio * infoTC.DollarValor) : presentProductoInfo.Precio;
                        OPPrecioProductoFormLista.Text = Importe.ToString("F");
                        PrecioPublicoOP = Importe;

                        PresentGlobalOP = presentProductoInfo;
                        OPCantProductoFormLista.Focus();
                    }
                    catch (Exception err)
                    {
                        await DisplayAlert("Error!", err.ToString(), "Aceptar");
                    }
                }
            }
        }

        // BOTON QUE SE ENCARGA DE AGREGAR EL PRODUCTO CONFIGURADO A  LA LISTA DE PRODUCTOS PARA LA ORDEN DE PEDIDO
        private async void OPAgregar_Producto(object sender, EventArgs e)
        {
            int indexProducto = OPProductosFormLista.SelectedIndex;
            int indexPresent = OPPresentProductoFormLista.SelectedIndex;
            if (indexProducto != -1)
            {
                if (indexPresent != -1)
                {
                    if (string.IsNullOrEmpty(OPCantProductoFormLista.Text))
                    {
                        OPCantProductoFormLista.Focus();
                        DependencyService.Get<IAlertas>().MsgCorto("Coloque Cantidad");
                    }
                    else if (string.IsNullOrEmpty(OPPrecioProductoFormLista.Text))
                    {
                        OPPrecioProductoFormLista.Focus();
                        DependencyService.Get<IAlertas>().MsgCorto("Coloque Precio");
                    }
                    else if (Convert.ToDouble(OPCantProductoFormLista.Text) == 0)
                    {
                        OPCantProductoFormLista.Focus();
                        DependencyService.Get<IAlertas>().MsgCorto("Cantidad Incorrecto");
                    }
                    else if (Convert.ToDouble(OPPrecioProductoFormLista.Text) == 0)
                    {
                        OPPrecioProductoFormLista.Focus();
                        DependencyService.Get<IAlertas>().MsgCorto("Precio Incorrecto");
                    }
                    else
                    {
                        try
                        {
                            int rowIndx = 0;
                            string nuevoProd = "";
                            foreach (var Elem in OPListaProductos.Children)
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
                                OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                            }
                            OPProductosPicker productoInfo = JsonConvert.DeserializeObject<OPProductosPicker>(JsonConvert.SerializeObject(OPProductosFormLista.ItemsSource[indexProducto]));
                            OPPresentProductoPicker presentInfo = JsonConvert.DeserializeObject<OPPresentProductoPicker>(JsonConvert.SerializeObject(OPPresentProductoFormLista.ItemsSource[indexPresent]));
                            string idProdPresent = productoInfo.Id.ToString() + "ø" + presentInfo.NombrePresentacion.Replace(' ', '¢').Replace('_', 'æ');

                            if (idProdPresent != nuevoProd)
                            {
                                string Token = CrearTokenOP();
                                OPListaProductos.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                                double Importe = Convert.ToDouble(OPPrecioProductoFormLista.Text);
                                int DescuentoPorc = (string.IsNullOrEmpty(OPDescuentoFormLista.Text)) ? 0 : Convert.ToInt32(OPDescuentoFormLista.Text);
                                string descTxt = "";
                                if ((DescuentoPorc > 0) && (DescuentoPorc < 10))
                                {
                                    descTxt = "0";
                                }
                                double DescuentoMonto = (DescuentoPorc > 0) ? ((Importe * Convert.ToDouble("0." + descTxt + DescuentoPorc.ToString())) * float.Parse(OPCantProductoFormLista.Text)) : 0;
                                double DescuentoMontoUnit = (DescuentoPorc > 0) ? (Importe * Convert.ToDouble("0." + descTxt + DescuentoPorc.ToString())) : 0;
                                double valorUniDesc = (DescuentoMonto > 0) ? Importe - (Importe * Convert.ToDouble("0." + descTxt + DescuentoPorc.ToString())) : Importe;
                                double subTotal = float.Parse(OPCantProductoFormLista.Text) * Importe;
                                int ieps = (PresentGlobalOP.IEPS > 0) ? PresentGlobalOP.IEPS : 0;
                                string iepsTxt = "";
                                if ((PresentGlobalOP.IEPS) > 0 && (PresentGlobalOP.IEPS < 10))
                                {
                                    iepsTxt = "0";
                                }
                                double iepsMonto = (ieps > 0) ? (/*Importe*/subTotal - DescuentoMonto) * Convert.ToDouble("0." + iepsTxt + ieps.ToString()) : 0;
                                double iepsMontoUnit = (ieps > 0) ? (Importe - DescuentoMontoUnit) * Convert.ToDouble("0." + iepsTxt + ieps.ToString()) : 0;
                                int iva = (PresentGlobalOP.IVA > 0) ? PresentGlobalOP.IVA : 0;
                                double ivaMonto = (iva > 0) ? ((/*Importe*/subTotal - DescuentoMonto) + iepsMonto) * Convert.ToDouble("0." + iva.ToString()) : 0;
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
                                    Text = OPCantProductoFormLista.Text,
                                    FontAttributes = FontAttributes.Bold,
                                    HorizontalTextAlignment = TextAlignment.Center,
                                    VerticalTextAlignment = TextAlignment.Center
                                };
                                Label labelPrecio = new Label()
                                {
                                    AutomationId = "l4B3LpR3c10Pr0Duc70_" + OPPrecioProductoFormLista.Text,
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
                                    QuitarProductoListaOP(new View[] { labelNomProd, labelCantidad, labelPrecio, btnBorrarProd }, new string[] { labelNomProd.Text, labelCantidad.Text, labelPrecio.Text });
                                });

                                OPListaProductos.Children.Add(labelNomProd, 0, rowIndx);
                                OPListaProductos.Children.Add(labelCantidad, 1, rowIndx);
                                OPListaProductos.Children.Add(labelPrecio, 2, rowIndx);
                                OPListaProductos.Children.Add(btnBorrarProd, 3, rowIndx);

                                OrdenesPedidoProductosLista.Add(new OrdenesPedidoProductosClase()
                                {
                                    Token = Token,
                                    IdProducto = productoInfo.Id,
                                    Descripcion = productoInfo.NombreProducto + " " + presentInfo.NombrePresentacion,
                                    Presentacion = presentInfo.NombrePresentacion,
                                    Cantidad = float.Parse(OPCantProductoFormLista.Text),
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

                                CalcularTotalesOP();

                                msgOPProductoSelect = false;
                                OPProductosFormLista.SelectedItem = string.Empty;
                                OPPresentProductoFormLista.SelectedItem = string.Empty;
                                OPPresentProductoLista = new List<OPPresentProductoPicker>();
                                OPPresentProductoFormLista.ItemsSource = OPPresentProductoLista;
                                OPCantProductoFormLista.Text = "";
                                OPPrecioProductoFormLista.Text = "";
                                OPDescuentoFormLista.Text = "";
                                OPProductosBusq.Text = "";
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
                else
                {
                    await DisplayAlert("Error!", "Eliga una Presentación", "Aceptar");
                }
            }
            else
            {
                await DisplayAlert("Error!", "Eliga un Producto", "Aceptar");
            }
        }

        // --------------------- [ ::::::: ** ::::::::: ] ---------------------
        // GUARDAR ORDEN DE PEDIDO
        private async void Guardar_OrdenPedido(object sender, EventArgs e)
        {
            var ImprimirOP = await DisplayAlert("Atencion!", "¿Desea guardar la Orden de Pedido?", "Si", "Cancelar");
            if (ImprimirOP)
            {
                AltaOrdenPedido();
            }
        }

        // ******************* [ ALTA ORDENES PEDIDOS ] ***************************
        // FUNCION QUE ALMACENA LA ORDEN DE PEDIDO
        private async void AltaOrdenPedido()
        {
            int contProds = 0;
            foreach (var Elem in OPListaProductos.Children)
            {
                contProds++;
            }
            int indexTD = OPTipoDocumentoFormLista.SelectedIndex;
            int indexTC = OPTipoClienteFormLista.SelectedIndex;
            int indexRZ = OPClienteFormLista.SelectedIndex;

            using (UserDialogs.Instance.Loading("Generando Orden Pedido..."))
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    OPTipoDocumento infoTD = JsonConvert.DeserializeObject<OPTipoDocumento>(JsonConvert.SerializeObject(OPTipoDocumentoFormLista.ItemsSource[indexTD]));
                    OPTipoCliente infoTC = JsonConvert.DeserializeObject<OPTipoCliente>(JsonConvert.SerializeObject(OPTipoClienteFormLista.ItemsSource[indexTC]));
                    OPClientesPicker infoRZ = JsonConvert.DeserializeObject<OPClientesPicker>(JsonConvert.SerializeObject(OPClienteFormLista.ItemsSource[indexRZ]));
                    int indexCA = OPClienteCampoFormLista.SelectedIndex;
                    string Campo = "S/C", Ubicacion = "---";
                    if (indexCA != -1)
                    {
                        Campo = JsonConvert.DeserializeObject<OPCamposCliente>(JsonConvert.SerializeObject(OPClienteCampoFormLista.ItemsSource[indexCA])).CampoCliente;
                        Ubicacion = OPClienteUbicacion.Text;
                    }

                    string nombreAgente = "";
                    var Usuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                    foreach (var usuario in Usuario)
                    {
                        nombreAgente = usuario.Nombre;
                    }

                    int IdMaxOrdenPedido = 0;
                    var ObtenerMaxId = DependencyService.Get<ISQliteParams>().ConsultaOrdenesPedido("SELECT MAX(Id) AS Id FROM OrdenesPedido");
                    if (ObtenerMaxId.Count > 0)
                    {
                        foreach (var cliente in ObtenerMaxId)
                        {
                            IdMaxOrdenPedido = cliente.Id + 1;
                        }
                    }
                    else
                    {
                        IdMaxOrdenPedido = 1;
                    }

                    List<OrdenesPedidoTablaPDF> ListaPedidos = new List<OrdenesPedidoTablaPDF>();
                    List<OrdenesPedidoProductos> ListaPedidosAlta = new List<OrdenesPedidoProductos>();
                    List<string> Tokens = new List<string>();
                    foreach (var Elem in OPListaProductos.Children)
                    {
                        string[] elemArr = Elem.AutomationId.Split('_');
                        if (elemArr[0] == "l4B3Ln0mPr0Duc70")
                        {
                            Label labelProducto = (Label)Elem;
                            string[] ProductoInfo = elemArr[1].Split('ø');
                        }
                        else if (elemArr[0] == "b7NPr0Duc70")
                        {
                            Tokens.Add(elemArr[1]);
                        }
                    }

                    string[] TokensArr = Tokens.ToArray();
                    foreach (var OPProducto in OrdenesPedidoProductosLista)
                    {
                        if (TokensArr.Contains(OPProducto.Token))
                        {
                            ListaPedidosAlta.Add(new OrdenesPedidoProductos()
                            {
                                IdOrdenPedido = Convert.ToInt32(OPNumeroOrden.Text),
                                IdProducto = OPProducto.IdProducto,
                                Descripcion = OPProducto.Descripcion,
                                Presentacion = OPProducto.Presentacion,
                                Cantidad = OPProducto.Cantidad,
                                PrecioUnitario = OPProducto.PrecioUnitario,
                                PrecioUnitarioDesc = OPProducto.PrecioUnitarioDesc,
                                DescuentoPorc = OPProducto.DescuentoPorc,
                                Descuento = OPProducto.Descuento,
                                IVA = OPProducto.IVA,
                                IVAMonto = OPProducto.IVAMonto,
                                IEPS = OPProducto.IEPS,
                                IEPSMonto = OPProducto.IEPSMonto,
                                Importe = OPProducto.Importe
                            });
                        }
                    }

                    OrdenesPedido PedidoInfoAlta = new OrdenesPedido()
                    {
                        Id = (NuevaOrdenPedido) ? IdMaxOrdenPedido : IdOrdenPedidoEditar,
                        NumeroOrden = Convert.ToInt32(OPNumeroOrden.Text),
                        IdTipoDocumento = infoTD.Id,
                        TipoDocumento = infoTD.NombreDocumento,
                        IdTipoCliente = infoTC.Id,
                        TipoCliente = infoTC.TipoCliente,
                        IdCliente = infoRZ.Id,
                        Cliente = infoRZ.RazonSocial,
                        Campo = Campo,
                        Ubicacion = Ubicacion,
                        Total = Convert.ToDouble(OPTotalPrecio.Text.Replace("$ ", "")),
                        Estatus = 0,
                        FechaEntrega = OPFechaEntrega.Date
                    };

                    List<OrdenesPedido> OPLista = new List<OrdenesPedido>() {
                        PedidoInfoAlta
                    };

                    string AltaOP = DependencyService.Get<ISQliteParams>().GuardarOrdenesPedido((NuevaOrdenPedido) ? 1 : 2, OPLista, ListaPedidosAlta);
                    if (AltaOP == "true")
                    {
                        LlenarListaOPNuevas();
                        await DisplayAlert("Éxito!", "La Orden de Pedido se ha almacenado correctamente", "Aceptar");
                        LimpiarOrdenPedidoFormulario(false);
                    }
                    else
                    {
                        await DisplayAlert("Error!", "Ocurrió un problema al guardar Orden Pedido - " + AltaOP, "Aceptar");
                    }
                }
                catch (Exception err1)
                {
                    await DisplayAlert("Error!", "Ocurrió un problema al generar Orden Pedido ERR1: " + err1.ToString(), "Aceptar");
                }
            }
        }

        // -------------------------- [ ACCIONES DE FORMULARIOS ] --------------------------
        // FUNCION QUE LIMPIA EL FORMULARIO DE LA ORDEN DE PEDIDO
        private void LimpiarOrdenPedidoFormulario(bool nuevo)
        {
            if (nuevo)
            {
                OPNumeroOrden.Text = DependencyService.Get<ISQliteParams>().ObtenerFolioOrdenPedido();
            }
            else
            {
                OPNumeroOrden.Text = "---";
            }

            NuevaOrdenPedido = true;

            msgOPClienteSelect = false;
            msgOPProductoSelect = false;

            OPTipoDocumentoFormLista.SelectedItem = string.Empty;
            OPTipoClienteFormLista.SelectedItem = string.Empty;
            OPClienteFormLista.SelectedItem = string.Empty;
            OPProductosFormLista.SelectedItem = string.Empty;
            OPClienteUbicacion.Text = "---";
            OPCamposClienteLista = new List<OPCamposCliente>();
            OPPresentProductoLista = new List<OPPresentProductoPicker>();
            OPClienteCampoFormLista.ItemsSource = OPCamposClienteLista;
            OPPresentProductoFormLista.ItemsSource = OPPresentProductoLista;
            OPCantProductoFormLista.Text = "";
            OPPrecioProductoFormLista.Text = "";
            OPProductosBusq.Text = "";

            OPCantidadProductos.Text = "0";
            OPTotalPrecio.Text = "$ 0.00";
            OPSubTotal.Text = "$ 0.00";
            OPIEPS.Text = "$ 0.00";
            OPImpuestos.Text = "$ 0.00";
            OPDescuento.Text = "$ 0.00";
            OPFechaEntrega.Date = DateTime.Now;

            OPListaProductos.Children.Clear();
            OPListaProductos.RowDefinitions.Clear();
            OPListaProductos.ColumnDefinitions.Clear();

            OrdenesPedidoProductosLista = new List<OrdenesPedidoProductosClase>();
        }

        // FUNCION QUE CALCULA LOS TOTALES
        private async void CalcularTotalesOP()
        {
            try
            {
                List<string> Tokens = new List<string>();
                foreach (var Elem in OPListaProductos.Children)
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
                foreach (var OPProducto in OrdenesPedidoProductosLista)
                {
                    if (TokensArr.Contains(OPProducto.Token))
                    {
                        cantidad += OPProducto.Cantidad;
                        subtotal += OPProducto.Importe;
                        descuento += OPProducto.Descuento;
                        ieps += OPProducto.IEPSMonto;
                        iva += OPProducto.IVAMonto;
                    }
                }

                OPCantidadProductos.Text = cantidad.ToString();
                OPSubTotal.Text = "$ " + Math.Round(subtotal, 2).ToString("F");
                OPIEPS.Text = "$ " + Math.Round(ieps, 2).ToString("F");
                OPImpuestos.Text = "$ " + Math.Round(iva, 2).ToString("F");
                OPDescuento.Text = "$ " + descuento.ToString("F");
                OPTotalPrecio.Text = "$ " + ((Math.Round(subtotal, 2) + Math.Round(ieps, 2) + Math.Round(iva, 2)) - descuento).ToString("F");
            }
            catch (Exception err)
            {
                await DisplayAlert("Error!", "Ocurrió un problema al agregar producto a la lista: - " + err.ToString(), "Aceptar");
            }
        }

        // FUNCION QUE QUITA LOS ELEMENTOS DE LA LISTA DE PRODUCTOS
        private async void QuitarProductoListaOP(View[] ElemsBorrar, string[] ElemsData)
        {
            var borrar = await DisplayAlert("Atencion!", "¿Desea borrar a " + ElemsData[0] + "?", "Si", "Cancelar");
            if (borrar)
            {
                OPListaProductos.Children.Remove(ElemsBorrar[0]);
                OPListaProductos.Children.Remove(ElemsBorrar[1]);
                OPListaProductos.Children.Remove(ElemsBorrar[2]);
                OPListaProductos.Children.Remove(ElemsBorrar[3]);

                int cont = 0;
                foreach (var Elem in OPListaProductos.Children)
                {
                    cont++;
                }
                if (cont == 0)
                {
                    OPListaProductos.Children.Clear();
                    OPListaProductos.RowDefinitions.Clear();
                    OPListaProductos.ColumnDefinitions.Clear();
                    OrdenesPedidoProductosLista = new List<OrdenesPedidoProductosClase>();
                }
                CalcularTotalesOP();
            }
        }

        // FUNCION QUE CREA UNA CADENA DE CARACTERES LARGA PARA EL USO DE REALIZAR TOKENS
        private static string CrearTokenOP()
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

        // --------------- [ NUEVA BUSQUEDA DE CLIENTES ] ----------------------
        // FUNCION QUE EJECUTA LA ACCION DE BUSQUEDA AL ESCRIBIR EL NOMBRE DEL CLIENTE
        private void OPClientesBusq_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = OPClientesBusq.Text.ToUpper();
                var busqResultado = NombresClientesBusq.Where(i => i.StartsWith(busqCliente)).ToList();
                OPClientesBusq.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE EJECUTA LA ELECCION DEL CLIENTE
        private void OPClientesBusq_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = NombresClientesBusq.FindIndex(x => x.StartsWith(OPClientesBusq.Text.ToUpper()));
            OPClienteFormLista.SelectedIndex = index;
        }

        // --------------- [ NUEVA BUSQUEDA DE PRODUCTOS ] ----------------------
        // ACCION QUE CONTROLA LA BUSQUEDA DE PRODUCTOS TIPO SEARCH
        private void OPProductosBusq_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqProducto = OPProductosBusq.Text.ToUpper();
                var busqResultado = NombresProductosBusq.Where(i => i.StartsWith(busqProducto)).ToList();
                OPProductosBusq.ItemsSource = busqResultado;
            }
        }

        // ACCION QUE CONTROLA AL ELEGIR UN PRODUCTO
        private void OPProductosBusq_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = NombresProductosBusq.FindIndex(x => x.StartsWith(OPProductosBusq.Text.ToUpper()));
            OPProductosFormLista.SelectedIndex = index;
        }


        // :::::::::::::::::::::::::::::::::::::::: [ MENU CONSULTAR ] ::::::::::::::::::::::::::::::::::::::::
        // FUNCION QUE CONSULTA LAS ORDENES PEDIDO ONLINE (DEL SERVIDOR)
        private async void OrdenesPedido_Online(object sender, EventArgs e)
        {
            if (OPBusqFechaIniFormLista.Date <= OPBusqFechaFinFormLista.Date)
            {
                int indexCliente = OPBuscClienteFormLista.SelectedIndex;
                if (indexCliente != -1)
                {
                    using (UserDialogs.Instance.Loading("Consultando Ordenes Pedido..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                        {
                            try
                            {
                                OPOnlineClientes.Children.Clear();
                                OPClientesPicker clienteInfo = JsonConvert.DeserializeObject<OPClientesPicker>(JsonConvert.SerializeObject(OPBuscClienteFormLista.ItemsSource[indexCliente]));
                                ReporteParamsConsulta ReporteConsulta = new ReporteParamsConsulta()
                                {
                                    IdCliente = clienteInfo.Id,
                                    FechaInicio = OPBusqFechaIniFormLista.Date/*.AddDays(-1)*/.ToString("yyyy/MM/dd"),
                                    FechaFin = OPBusqFechaFinFormLista.Date.AddDays(1).ToString("yyyy/MM/dd")
                                };
                                ReporteEstructura ReporteInfo = JsonConvert.DeserializeObject<ReporteEstructura>(DependencyService.Get<IAgroWS>().ReportesOnline(JsonConvert.SerializeObject(ReporteConsulta), 1));
                                UrlServerOP = ReporteInfo.Url;
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
                                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                                    Label labelT1 = new Label()
                                    {
                                        AutomationId = "l4B3Lt1",
                                        Text = "FECHA",
                                        FontAttributes = FontAttributes.Bold,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        FontSize = 12
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
                                        FontSize = 12
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
                                        Text = "TOTAL",
                                        FontAttributes = FontAttributes.Bold,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        HorizontalTextAlignment = TextAlignment.Center,
                                        FontSize = 12
                                    };
                                    Frame FrameT3 = new Frame()
                                    {
                                        BackgroundColor = Color.FromRgb(214, 219, 223),
                                        BorderColor = Color.Black,
                                        Padding = new Thickness(2, 0, 2, 0),
                                        Margin = new Thickness(0, 0, 0, 0),
                                        Content = labelT3
                                    };

                                    Label labelT4 = new Label()
                                    {
                                        AutomationId = "l4B3Lt3",
                                        Text = "IMPRIMIR",
                                        FontAttributes = FontAttributes.Bold,
                                        VerticalTextAlignment = TextAlignment.Center,
                                        FontSize = 12
                                    };
                                    Frame FrameT4 = new Frame()
                                    {
                                        BackgroundColor = Color.FromRgb(214, 219, 223),
                                        BorderColor = Color.Black,
                                        Padding = new Thickness(2, 0, 2, 0),
                                        Margin = new Thickness(0, 0, 0, 0),
                                        Content = labelT4
                                    };

                                    GridTabla.Children.Add(FrameT1);
                                    GridTabla.Children.Add(FrameT2);
                                    GridTabla.Children.Add(FrameT3);
                                    GridTabla.Children.Add(FrameT4);

                                    Grid.SetRow(FrameT1, 0);
                                    Grid.SetColumn(FrameT1, 0);
                                    Grid.SetRow(FrameT2, 0);
                                    Grid.SetColumn(FrameT2, 1);
                                    Grid.SetRow(FrameT3, 0);
                                    Grid.SetColumn(FrameT3, 2);
                                    Grid.SetRow(FrameT4, 0);
                                    Grid.SetColumn(FrameT4, 3);

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
                                            FontSize = 11
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
                                            FontSize = 11
                                        };
                                        Frame FrameP2 = new Frame()
                                        {
                                            BorderColor = Color.Black,
                                            Padding = new Thickness(2, 0, 2, 0),
                                            Margin = new Thickness(0, 0, 0, 0),
                                            Content = labelP2
                                        };

                                        Label labelP3 = new Label()
                                        {
                                            AutomationId = "l4B3Lp3",
                                            Text = "$" + Reporte.Total,
                                            FontAttributes = FontAttributes.Bold,
                                            VerticalTextAlignment = TextAlignment.Center,
                                            FontSize = 11
                                        };
                                        Frame FrameP3 = new Frame()
                                        {
                                            BorderColor = Color.Black,
                                            Padding = new Thickness(2, 0, 2, 0),
                                            Margin = new Thickness(0, 0, 0, 0),
                                            Content = labelP3
                                        };

                                        Button btnImpCotiz = new Button()
                                        {
                                            AutomationId = "b7N1mP0P",
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
                                            ImprimirOrdenPeidoOnline(Reporte.Documento);
                                        });

                                        GridTabla.Children.Add(FrameP1);
                                        GridTabla.Children.Add(FrameP2);
                                        GridTabla.Children.Add(FrameP3);
                                        GridTabla.Children.Add(btnImpCotiz);

                                        Grid.SetRow(FrameP1, row);
                                        Grid.SetColumn(FrameP1, 0);
                                        Grid.SetRow(FrameP2, row);
                                        Grid.SetColumn(FrameP2, 1);
                                        Grid.SetRow(FrameP3, row);
                                        Grid.SetColumn(FrameP3, 2);
                                        Grid.SetRow(btnImpCotiz, row);
                                        Grid.SetColumn(btnImpCotiz, 3);

                                        row++;
                                    }

                                    OPOnlineClientes.Children.Add(GridTabla);
                                }
                                else
                                {
                                    Grid GridNoCotiz = new Grid()
                                    {
                                        AutomationId = "6r1Dn00P",
                                        Margin = new Thickness(0, 10, 0, 10)
                                    };

                                    GridNoCotiz.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                                    GridNoCotiz.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                                    Label labelNoHist = new Label()
                                    {
                                        AutomationId = "l4B3Ln00P",
                                        Text = "  NO SE ENCONTRARON ORDENES PEDIDO  ",
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

                                    OPOnlineClientes.Children.Add(GridNoCotiz);
                                }
                            }
                            catch (Exception err)
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al consultar las Ordenes Pedido: - " + err.ToString(), "Aceptar");
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
        private async void ImprimirOrdenPeidoOnline(string pdfreporteop)
        {
            using (UserDialogs.Instance.Loading("Imprimiendo Orden Pedido..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    Device.OpenUri(new Uri(UrlServerOP + pdfreporteop + ".pdf"));
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                }
            }
        }

        // FUNCION QUE LLENA EL LISTADO DE ORDENES PEDIDO NUEVAS
        private async void LlenarListaOPNuevas()
        {
            using (UserDialogs.Instance.Loading("Espere..."))
            {
                OPNuevasLista.Children.Clear();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                var NuevasData = DependencyService.Get<ISQliteParams>().ConsultaOrdenesPedido("SELECT * FROM OrdenesPedido WHERE Estatus = 0");
                if (NuevasData.Count > 0)
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
                    GridTabla.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                    Label labelT1 = new Label()
                    {
                        AutomationId = "l4B3Lt1",
                        Text = "FOLIO",
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 12
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
                        Text = "CLIENTE",
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        FontSize = 12
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
                        Text = "EDITAR",
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = 9
                    };
                    Frame FrameT3 = new Frame()
                    {
                        BackgroundColor = Color.FromRgb(214, 219, 223),
                        BorderColor = Color.Black,
                        Padding = new Thickness(2, 0, 2, 0),
                        Margin = new Thickness(0, 0, 0, 0),
                        Content = labelT3
                    };

                    Label labelT4 = new Label()
                    {
                        AutomationId = "l4B3Lt4",
                        Text = "IMPRIMIR",
                        FontAttributes = FontAttributes.Bold,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = 9
                    };
                    Frame FrameT4 = new Frame()
                    {
                        BackgroundColor = Color.FromRgb(214, 219, 223),
                        BorderColor = Color.Black,
                        Padding = new Thickness(2, 0, 2, 0),
                        Margin = new Thickness(0, 0, 0, 0),
                        Content = labelT4
                    };

                    GridTabla.Children.Add(FrameT1);
                    GridTabla.Children.Add(FrameT2);
                    GridTabla.Children.Add(FrameT3);
                    GridTabla.Children.Add(FrameT4);

                    Grid.SetRow(FrameT1, 0);
                    Grid.SetColumn(FrameT1, 0);
                    Grid.SetRow(FrameT2, 0);
                    Grid.SetColumn(FrameT2, 1);
                    Grid.SetRow(FrameT3, 0);
                    Grid.SetColumn(FrameT3, 2);
                    Grid.SetRow(FrameT4, 0);
                    Grid.SetColumn(FrameT4, 3);

                    int row = 1;
                    foreach (var Nuevas in NuevasData)
                    {
                        GridTabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        
                        Label labelP1 = new Label()
                        {
                            AutomationId = "l4B3Lp1",
                            Text = GenerarFolioOP(Nuevas.Id),
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 11
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
                            Text = Nuevas.Cliente,
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 11
                        };
                        Frame FrameP2 = new Frame()
                        {
                            BorderColor = Color.Black,
                            Padding = new Thickness(2, 0, 2, 0),
                            Margin = new Thickness(0, 0, 0, 0),
                            Content = labelP2
                        };

                        Button btnEditarCotiz = new Button()
                        {
                            AutomationId = "b7N3d170P",
                            Text = "/",
                            FontSize = 8,
                            CornerRadius = 20,
                            TextColor = Color.White,
                            BackgroundColor = Color.FromRgb(255, 193, 7),
                            FontFamily = "fontawesomeicons.ttf#Regular",
                            WidthRequest = 30,
                            HeightRequest = 30,
                            VerticalOptions = LayoutOptions.Center,
                            HorizontalOptions = LayoutOptions.Center
                        };
                        btnEditarCotiz.Command = new Command(() => {
                            EditarOrdenPedido(Nuevas.Id);
                        });

                        Button btnImpCotiz = new Button()
                        {
                            AutomationId = "b7N1mP0P",
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
                            ImprimirOrdenPedido(Nuevas.Id);
                        });

                        GridTabla.Children.Add(FrameP1);
                        GridTabla.Children.Add(FrameP2);
                        GridTabla.Children.Add(btnEditarCotiz);
                        GridTabla.Children.Add(btnImpCotiz);

                        Grid.SetRow(FrameP1, row);
                        Grid.SetColumn(FrameP1, 0);
                        Grid.SetRow(FrameP2, row);
                        Grid.SetColumn(FrameP2, 1);
                        Grid.SetRow(btnEditarCotiz, row);
                        Grid.SetColumn(btnEditarCotiz, 2);
                        Grid.SetRow(btnImpCotiz, row);
                        Grid.SetColumn(btnImpCotiz, 3);

                        row++;
                    }

                    OPNuevasLista.Children.Add(GridTabla);
                }
                else
                {
                    Grid GridNoCotiz = new Grid()
                    {
                        AutomationId = "6r1Dn00P",
                        Margin = new Thickness(0, 10, 0, 10)
                    };

                    GridNoCotiz.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    GridNoCotiz.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                    Label labelNoHist = new Label()
                    {
                        AutomationId = "l4B3Ln00P",
                        Text = "  NO TIENE ORDENES NUEVAS  ",
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

                    OPNuevasLista.Children.Add(GridNoCotiz);
                }
            }
        }

        // FUNCION QUE EJECUTA LA ACCION DE EDITAR UNA ORDEN DE PEDIDO
        private async void EditarOrdenPedido(int IdOP)
        {
            using (UserDialogs.Instance.Loading("Abriendo Orden Pedido..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                IdOrdenPedidoEditar = IdOP;
                LimpiarOrdenPedidoFormulario(false);
                try
                {
                    var OPData = DependencyService.Get<ISQliteParams>().ConsultaOrdenesPedido("SELECT * FROM OrdenesPedido WHERE Id = " + IdOP.ToString());
                    foreach (var OP in OPData)
                    {
                        OPNumeroOrden.Text = GenerarFolioOP(OP.Id);
                        OPTipoDocumentoFormLista.SelectedItem = ((List<OPTipoDocumento>)OPTipoDocumentoFormLista.ItemsSource).FirstOrDefault(c => c.Id == OP.IdTipoDocumento);
                        OPTipoClienteFormLista.SelectedItem = ((List<OPTipoCliente>)OPTipoClienteFormLista.ItemsSource).FirstOrDefault(c => c.Id == OP.IdTipoCliente);
                        OPClienteFormLista.SelectedItem = ((List<OPClientesPicker>)OPClienteFormLista.ItemsSource).FirstOrDefault(c => c.Id == OP.IdCliente);
                        await Task.Delay(TimeSpan.FromMilliseconds(3000));
                        OPClienteCampoFormLista.SelectedItem = ((List<OPCamposCliente>)OPClienteCampoFormLista.ItemsSource).FirstOrDefault(c => c.CampoCliente == OP.Campo);
                        OPFechaEntrega.Date = OP.FechaEntrega;
                    }

                    var OPPrecios = DependencyService.Get<ISQliteParams>().ConsultaClientesPrecios("SELECT * FROM ClientesPrecios WHERE IdOrdenPedido = " + IdOP.ToString());

                    int rowIndx = 0;
                    OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                    OPListaProductos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    var OPProductosData = DependencyService.Get<ISQliteParams>().ConsultaOrdenesPedidoProductos("SELECT * FROM OrdenesPedidoProductos WHERE IdOrdenPedido = " + IdOP.ToString());
                    foreach (var OPProducto in OPProductosData)
                    {
                        string Token = CrearTokenOP();
                        OPListaProductos.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        Label labelNomProd = new Label()
                        {
                            AutomationId = "l4B3Ln0mPr0Duc70_&",
                            Text = OPProducto.Descripcion,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        };
                        Label labelCantidad = new Label()
                        {
                            AutomationId = "l4B3Lc4N7Pr0Duc70_&",
                            Text = OPProducto.Cantidad.ToString(),
                            FontAttributes = FontAttributes.Bold,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        };
                        Label labelPrecio = new Label()
                        {
                            AutomationId = "l4B3LpR3c10Pr0Duc70_" + OPPrecioProductoFormLista.Text,
                            Text = "$ " + OPProducto.Importe.ToString("F"),
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
                            QuitarProductoListaOP(new View[] { labelNomProd, labelCantidad, labelPrecio, btnBorrarProd }, new string[] { labelNomProd.Text, labelCantidad.Text, labelPrecio.Text });
                        });

                        OPListaProductos.Children.Add(labelNomProd, 0, rowIndx);
                        OPListaProductos.Children.Add(labelCantidad, 1, rowIndx);
                        OPListaProductos.Children.Add(labelPrecio, 2, rowIndx);
                        OPListaProductos.Children.Add(btnBorrarProd, 3, rowIndx);

                        OrdenesPedidoProductosLista.Add(new OrdenesPedidoProductosClase()
                        {
                            Token = Token,
                            IdProducto = OPProducto.IdProducto,
                            Descripcion = OPProducto.Descripcion,
                            Presentacion = OPProducto.Presentacion,
                            Cantidad = OPProducto.Cantidad,
                            PrecioUnitario = OPProducto.PrecioUnitario,
                            PrecioUnitarioDesc = OPProducto.PrecioUnitarioDesc,
                            DescuentoPorc = OPProducto.DescuentoPorc,
                            Descuento = OPProducto.Descuento,
                            IEPS = OPProducto.IEPS,
                            IEPSMonto = OPProducto.IEPSMonto,
                            IVA = OPProducto.IVA,
                            IVAMonto = OPProducto.IVAMonto,
                            Importe = OPProducto.Importe
                        });

                        rowIndx++;
                    }
                    CalcularTotalesOP();
                    CurrentPage = OPTabNueva;
                    NuevaOrdenPedido = false;
                }
                catch (Exception err)
                {
                    await DisplayAlert("Error!", "Ocurrió un error al abrir Orden Pedido: - " + err.ToString(), "Aceptar");
                }
            }
        }

        // FUNCION QUE IMPRIME Y ENVIA AL SERVIDOR UNA ORDEN DE PEDIDO
        private async void ImprimirOrdenPedido(int IdOP)
        {
            var ImprimirOP = await DisplayAlert("Atencion!", "Una vez enviada la Orden de Pedido al Servidor, no podrá editarla, ¿Desea continuar?", "Si", "Cancelar");
            if (ImprimirOP)
            {
                using (UserDialogs.Instance.Loading("Generando Orden Pedido..."))
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        string nombreAgente = "";
                        var Usuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        foreach (var usuario in Usuario)
                        {
                            nombreAgente = usuario.Nombre;
                        }

                        List<OrdenesPedidoTablaPDF> ListaPedidos = new List<OrdenesPedidoTablaPDF>();
                        List<OrdenesPedidoProductos> ListaPedidosAlta = new List<OrdenesPedidoProductos>();

                        var OPData = DependencyService.Get<ISQliteParams>().ConsultaOrdenesPedido("SELECT * FROM OrdenesPedido WHERE Id = " + IdOP.ToString());
                        var OPProductosData = DependencyService.Get<ISQliteParams>().ConsultaOrdenesPedidoProductos("SELECT * FROM OrdenesPedidoProductos WHERE IdOrdenPedido = " + IdOP.ToString());

                        float CantidadArticulos = 0;
                        double SubTotal = 0;
                        double DescuentoTotal = 0;
                        double IEPSTotal = 0;
                        double IVATotal = 0;
                        foreach (var OPProducto in OPProductosData)
                        {
                            ListaPedidos.Add(new OrdenesPedidoTablaPDF()
                            {
                                IdProducto = OPProducto.IdProducto,
                                Presentacion = OPProducto.Presentacion,
                                Producto = OPProducto.Descripcion,
                                Cantidad = OPProducto.Cantidad.ToString(),
                                Precio = OPProducto.PrecioUnitario.ToString("F"),
                                DescuentoPorc = OPProducto.DescuentoPorc.ToString() + "%",
                                DescuentoMonto = OPProducto.Descuento.ToString("F"),
                                PrecioConDescuento = OPProducto.PrecioUnitarioDesc.ToString("F"),
                                IEPS = OPProducto.IEPS.ToString() + "%",
                                Impuestos = OPProducto.IVAMonto.ToString("F"),
                                Subtotal = OPProducto.Importe.ToString("F")
                            });

                            ListaPedidosAlta.Add(new OrdenesPedidoProductos()
                            {
                                IdOrdenPedido = OPProducto.IdOrdenPedido,
                                IdProducto = OPProducto.IdProducto,
                                Descripcion = OPProducto.Descripcion,
                                Presentacion = OPProducto.Presentacion,
                                Cantidad = OPProducto.Cantidad,
                                PrecioUnitario = OPProducto.PrecioUnitario,
                                PrecioUnitarioDesc = OPProducto.PrecioUnitarioDesc,
                                DescuentoPorc = OPProducto.DescuentoPorc,
                                Descuento = OPProducto.Descuento,
                                IVA = OPProducto.IVA,
                                IVAMonto = OPProducto.IVAMonto,
                                IEPS = OPProducto.IEPS,
                                IEPSMonto = OPProducto.IEPSMonto,
                                Importe = OPProducto.Importe
                            });
                            CantidadArticulos += OPProducto.Cantidad;
                            SubTotal += OPProducto.Importe;
                            DescuentoTotal += OPProducto.Descuento;
                            IEPSTotal += OPProducto.IEPSMonto;
                            IVATotal += OPProducto.IVAMonto;
                        }

                        OrdenesPedidoPDF PedidoInfo = new OrdenesPedidoPDF();
                        OrdenesPedido PedidoInfoAlta = new OrdenesPedido();
                        foreach (var OP in OPData)
                        {
                            PedidoInfo = new OrdenesPedidoPDF()
                            {
                                NumOrdenPedido = GenerarFolioOP(OP.Id),
                                NombreAgente = nombreAgente,
                                TipoDocumento = OP.TipoDocumento,
                                TipoCliente = OP.TipoCliente,
                                RazonSocial = OP.Cliente,
                                Campo = OP.Campo,
                                Ubicacion = OP.Ubicacion,
                                FechaEntrega = OP.FechaEntrega.ToString("dd/MM/yyyy"),
                                ListaProductos = JsonConvert.SerializeObject(ListaPedidos),
                                CantidadArticulos = CantidadArticulos.ToString(),
                                Subtotal = Math.Round(SubTotal, 2).ToString("F"),
                                Descuentps = DescuentoTotal.ToString("F"),
                                IEPS = Math.Round(IEPSTotal, 2).ToString("F"),
                                IVA = Math.Round(IVATotal, 2).ToString("F"),
                                Total = ((Math.Round(SubTotal, 2) + Math.Round(IEPSTotal, 2) + Math.Round(IVATotal, 2)) - DescuentoTotal).ToString("F")
                            };
                            PedidoInfoAlta = new OrdenesPedido()
                            {
                                Id = OP.Id,
                                NumeroOrden = OP.NumeroOrden,
                                IdTipoDocumento = OP.IdTipoDocumento,
                                TipoDocumento = OP.TipoDocumento,
                                IdTipoCliente = OP.IdTipoCliente,
                                TipoCliente = OP.TipoCliente,
                                IdCliente = OP.IdCliente,
                                Cliente = OP.Cliente,
                                Campo = OP.Campo,
                                Ubicacion = OP.Ubicacion,
                                Total = OP.Total,
                                Estatus = 1,
                                FechaEntrega = OP.FechaEntrega
                            };
                        }

                        OrdenesPedidoAltaWS OPWS = new OrdenesPedidoAltaWS()
                        {
                            OrdenesPedido = JsonConvert.SerializeObject(PedidoInfoAlta),
                            OrdenesPedidoProductos = JsonConvert.SerializeObject(ListaPedidosAlta),
                            //ClientePrecios = JsonConvert.SerializeObject(ClientePrecioAlta)
                        };
                        OrdenesPedidoDataWS = new BDPendientes()
                        {
                            Tabla = "OrdenesPedido",
                            Accion = "Nuevo",
                            Data = JsonConvert.SerializeObject(OPWS)
                        };

                        List<OrdenesPedido> OPLista = new List<OrdenesPedido>() {
                            PedidoInfoAlta
                        };

                        if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                        {
                            try
                            {
                                string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(OrdenesPedidoDataWS));
                                if (WS == "true")
                                {
                                    string borrarCotiz1 = DependencyService.Get<ISQliteParams>().QueryMaestra(8, "UPDATE OrdenesPedido SET Estatus = ? WHERE Id = ?", new object[] { 1, IdOP });
                                    LlenarListaOPNuevas();
                                    LimpiarOrdenPedidoFormulario(false);
                                    try
                                    {
                                        if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                                        {
                                            Device.OpenUri(new Uri(DependencyService.Get<IAgroWS>().GenerarOrdenPedido(JsonConvert.SerializeObject(PedidoInfo))));
                                        }
                                        else
                                        {
                                            List<OrdenesPedidoPendientes> OPPendienteLista = new List<OrdenesPedidoPendientes>() {
                                                new OrdenesPedidoPendientes()
                                                {
                                                    IdOrdenPedido = GenerarFolioOP(IdOP),
                                                    FechaCreado = DateTime.Now,
                                                    PedidoCad = JsonConvert.SerializeObject(PedidoInfo)
                                                }
                                            };
                                            string AltaPendiente = DependencyService.Get<ISQliteParams>().GuardarOrdenesPedidoPendientes(1, OPPendienteLista);
                                            LlenarListaOPPendientes();
                                            await DisplayAlert("Éxito!", "La Orden de Pedido se ha almacenado correctamente; pero no fue posible generar la Solicitud - La solicitud de Impresión fué enviada a pendientes", "Aceptar");
                                        }
                                    }
                                    catch (Exception err2)
                                    {
                                        await DisplayAlert("Error!", "Ocurrió un problema al generar Orden Pedido ERR2: " + err2.ToString(), "Aceptar");
                                    }
                                }
                            }
                            catch (Exception err1)
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al generar Orden Pedido ERR2: " + err1.ToString(), "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Atención!", "Necesita conección a Internet para esta acción", "Aceptar");
                        }
                    }
                    catch (Exception err)
                    {
                        await DisplayAlert("Error!", "Ocurrió un problema al generar Orden Pedido ERR1: " + err.ToString(), "Aceptar");
                    }
                }
            }
        }


        // ****************************** [ ORDENES PENDIENTES ] ******************************
        // ------------------------------------------------------------------------------
        // FUNCION QUE LLENA EL LISTADO DE ORDENES PEDIDO PENDIENTES
        private async void LlenarListaOPPendientes()
        {
            using (UserDialogs.Instance.Loading("Espere..."))
            {
                OPPendientesLista.Children.Clear();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                var PendientesData = DependencyService.Get<ISQliteParams>().ConsultaOrdenPedidoPendiente("SELECT * FROM OrdenesPedidoPendientes");
                if (PendientesData.Count > 0)
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
                        FontSize = 12
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
                        FontSize = 12
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
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = 12
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
                    foreach (var Pendientes in PendientesData)
                    {
                        GridTabla.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                        Label labelP1 = new Label()
                        {
                            AutomationId = "l4B3Lp1",
                            Text = Pendientes.FechaCreado.ToString("dd/MM/yyyy hh:mm tt"),
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 11
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
                            Text = Pendientes.IdOrdenPedido,
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center,
                            FontSize = 11
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
                            AutomationId = "b7N1mP0P",
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
                            ReimprimirOrdenPedidoPendiente(Pendientes.Id, Pendientes.PedidoCad);
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

                    OPPendientesLista.Children.Add(GridTabla);
                }
                else
                {
                    Grid GridNoCotiz = new Grid()
                    {
                        AutomationId = "6r1Dn00P",
                        Margin = new Thickness(0, 10, 0, 10)
                    };

                    GridNoCotiz.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    GridNoCotiz.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                    Label labelNoHist = new Label()
                    {
                        AutomationId = "l4B3Ln00P",
                        Text = "  NO SE ENCONTRARON ORDENES PEDIDO  ",
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

                    OPPendientesLista.Children.Add(GridNoCotiz);
                }
            }
        }

        // FUNCION QUE REIMPRIME LA ORDEN DE PEDIDO PENDIENTE
        private async void ReimprimirOrdenPedidoPendiente(int idoppendiente, string oppdata)
        {
            using (UserDialogs.Instance.Loading("Generando Orden Pedido..."))
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                    {
                        Device.OpenUri(new Uri(DependencyService.Get<IAgroWS>().GenerarOrdenPedido(oppdata)));
                        string borrarCotiz1 = DependencyService.Get<ISQliteParams>().QueryMaestra(13, "DELETE FROM OrdenesPedidoPendientes WHERE Id = ?", new object[] { idoppendiente });
                        LlenarListaOPPendientes();
                    }
                    else
                    {
                        await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                    }
                }
                catch (Exception err)
                {
                    await DisplayAlert("Error!", err.ToString(), "Aceptar");
                }
            }                    
        }

        // FUNCION QUE GENERA UN ID DE TEXTO EN UN FOLIO (PARA ID DE ORDEN DE PEDIDO)
        private string GenerarFolioOP(int IdOrdenPedido)
        {
            string folioTxt = "";
            if (IdOrdenPedido < 10)
            {
                folioTxt = "00";
            }
            else if (IdOrdenPedido < 100)
            {
                folioTxt = "0";
            }
            folioTxt = folioTxt + IdOrdenPedido.ToString();
            return folioTxt;
        }

        // ------------------- [ NUEVA BUSQUEDA - ONLINE ] -------------------
        // FUNCION QUE MANEJA LA CONSULTA AL ESCRIBIR EL NOMBRE DEL CLIENTE
        private void OPBusqClientesLista_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = OPBusqClientesLista.Text.ToUpper();
                var busqResultado = NombresClientesOnlineBusq.Where(i => i.StartsWith(busqCliente)).ToList();
                OPBusqClientesLista.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE MANEJA LA CONSULTA DEL CLIENTE
        private void OPBusqClientesLista_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = NombresClientesOnlineBusq.FindIndex(x => x.StartsWith(OPBusqClientesLista.Text.ToUpper()));
            OPBuscClienteFormLista.SelectedIndex = index;
        }
    }
}