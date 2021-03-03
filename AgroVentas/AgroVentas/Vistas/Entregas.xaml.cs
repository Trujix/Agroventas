using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Acr.UserDialogs;
using Newtonsoft.Json;

namespace AgroVentas.Vistas
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Entregas : ContentPage
	{
        // ------------------ [ ********* ] ------------------
        // VARIABLES GLOBALES
        private class OrdenesPedidosLista
        {
            public int Id { get; set; }
            public int IdUsuario { get; set; }
            public int NumeroOrden { get; set; }
            public int IdTipoDocumento { get; set; }
            public string TipoDocumento { get; set; }
            public int IdTipoCliente { get; set; }
            public string TipoCliente { get; set; }
            public int IdCliente { get; set; }
            public string Cliente { get; set; }
            public string Campo { get; set; }
            public string Ubicacion { get; set; }
            public double Total { get; set; }
            public DateTime FechaEntrega { get; set; }
            public int Estatus { get; set; }
            public string Repartidor { get; set; }
            public string ListaProductos { get; set; }
        }
        private class PedidoData
        {
            public int Id { get; set; }
            public string Cliente { get; set; }
            public string Campo { get; set; }
            public string Ubicacion { get; set; }
        }
        private class RepartidorARPicker
        {
            public int Id { get; set; }
            public string NombreRepartidor { get; set; }
        }
        private List<RepartidorARPicker> RepartidorARLista;
        // ------------------------------------------------------

        public Entregas ()
		{
			InitializeComponent ();

            LlenarListaPedidosAgente();
        }

        // BOTON QUE MUESTRA LA LISTA DE PEDIDOS PARA MOSTRAR AL AGENTE
        private void Obtener_PedidosAgente(object sender, EventArgs e)
        {
            LlenarListaPedidosAgente();
        }

        // FUNCION QUE LLENA LA LISTA DE PEDIDOS PARA EL AGENTE
        private async void LlenarListaPedidosAgente()
        {
            using (UserDialogs.Instance.Loading("Verif. Solicitudes..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    RepartosListaForm.Children.Clear();
                    List<OrdenesPedidosLista> ListaPedidos = JsonConvert.DeserializeObject<List<OrdenesPedidosLista>>(DependencyService.Get<IAgroWS>().ObtenerOrdenesPedido());
                    if (ListaPedidos.Count > 0)
                    {
                        foreach (var Pedido in ListaPedidos)
                        {
                            Grid GridPedidoPrinc = new Grid()
                            {
                                AutomationId = "6R1dPr1nc_" + Pedido.Id.ToString(),
                            };
                            Grid GridPedido1 = new Grid()
                            {
                                AutomationId = "6R1d1_" + Pedido.Id.ToString()
                            };
                            GridPedido1.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            GridPedido1.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            GridPedido1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                            GridPedido1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                            Label label = new Label()
                            {
                                AutomationId = "l4B3L_&" + Pedido.Id.ToString(),
                                Text = "Orden de Pedido: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };

                            Label labelOP = new Label()
                            {
                                AutomationId = "l4B3L0rD3nP3d1D0_&" + Pedido.Id.ToString(),
                                Text = GenerarFolioEntregas(Pedido.Id),
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };

                            Label label1 = new Label()
                            {
                                AutomationId = "l4B3L1_&" + Pedido.Id.ToString(),
                                Text = "Cliente: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };
                            Label labelCliente = new Label()
                            {
                                AutomationId = "l4B3LcL13n73_&" + Pedido.Id.ToString(),
                                Text = Pedido.Cliente,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };

                            GridPedido1.Children.Add(label);
                            GridPedido1.Children.Add(labelOP);
                            GridPedido1.Children.Add(label1);
                            GridPedido1.Children.Add(labelCliente);

                            Grid.SetRow(label, 0);
                            Grid.SetColumn(label, 0);
                            Grid.SetRow(labelOP, 0);
                            Grid.SetColumn(labelOP, 1);
                            Grid.SetRow(label1, 1);
                            Grid.SetColumn(label1, 0);
                            Grid.SetRow(labelCliente, 1);
                            Grid.SetColumn(labelCliente, 1);

                            Grid GridPedido2 = new Grid()
                            {
                                AutomationId = "6R1d2_" + Pedido.Id.ToString()
                            };
                            GridPedido2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                            GridPedido2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                            GridPedido2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                            GridPedido2.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                            GridPedido2.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                            Label label2 = new Label()
                            {
                                AutomationId = "l4B3L2_&" + Pedido.Id.ToString(),
                                Text = "Campo: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };
                            Label labelCampo = new Label()
                            {
                                AutomationId = "l4B3Lc4Mp0_&" + Pedido.Id.ToString(),
                                Text = Pedido.Campo,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };
                            Label label3 = new Label()
                            {
                                AutomationId = "l4B3L3_&" + Pedido.Id.ToString(),
                                Text = "Ubicación: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };
                            Label labelUbicacion = new Label()
                            {
                                AutomationId = "l4B3LuB1c4C10n_&" + Pedido.Id.ToString(),
                                Text = Pedido.Ubicacion,
                                VerticalTextAlignment = TextAlignment.Center,
                                FontSize = 10
                            };

                            GridPedido2.Children.Add(label2);
                            GridPedido2.Children.Add(labelCampo);
                            GridPedido2.Children.Add(label3);
                            GridPedido2.Children.Add(labelUbicacion);

                            Grid.SetRow(label2, 0);
                            Grid.SetColumn(label2, 0);
                            Grid.SetRow(labelCampo, 0);
                            Grid.SetColumn(labelCampo, 1);
                            Grid.SetRow(label3, 0);
                            Grid.SetColumn(label3, 2);
                            Grid.SetRow(labelUbicacion, 0);
                            Grid.SetColumn(labelUbicacion, 3);

                            Grid GridControles = new Grid()
                            {
                                AutomationId = "6R1dC0n7R0L35_" + Pedido.Id.ToString(),
                            };
                            GridControles.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            Color BKGFrame = Color.FromRgb(225, 245, 254);

                            Frame FramePedido = new Frame()
                            {
                                AutomationId = "fR4m3_" + Pedido.Id.ToString(),
                                BorderColor = Color.Black,
                                CornerRadius = 20,
                                Margin = new Thickness(5, 5),
                            };
                            StackLayout StackPrincipal = new StackLayout()
                            {
                                AutomationId = "5T4cK_" + Pedido.Id.ToString()
                            };
                            if (Pedido.Repartidor != "NA")
                            {
                                if (Pedido.Estatus > 0)
                                {
                                    BKGFrame = Color.FromRgb(200, 230, 201);
                                    GridControles.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                    Button btnEntregado = new Button()
                                    {
                                        AutomationId = "b7N3n7R364_" + Pedido.Id.ToString(),
                                        Text = "\uf058 Pedido Entregado",
                                        FontSize = 8,
                                        CornerRadius = 20,
                                        TextColor = Color.White,
                                        BackgroundColor = Color.FromRgb(40, 167, 69),
                                        FontFamily = "fontawesomeicons.ttf#Regular",
                                        WidthRequest = 30,
                                        HeightRequest = 30,
                                        VerticalOptions = LayoutOptions.Center,
                                        Command = new Command(() =>
                                        {
                                            MarcarOPVisto(Pedido.Id, new View[] { GridPedidoPrinc, FramePedido });
                                        })
                                    };
                                    GridControles.Children.Add(btnEntregado);
                                    Grid.SetRow(btnEntregado, 0);
                                    Grid.SetColumn(btnEntregado, 0);
                                }
                                else
                                {
                                    BKGFrame = Color.FromRgb(255, 224, 178);
                                    GridControles.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                    GridControles.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                                    Button btnSolicitud = new Button()
                                    {
                                        AutomationId = "b7N50L1c17Ud_" + Pedido.Id.ToString(),
                                        Text = "Ver solicitud",
                                        FontSize = 8,
                                        CornerRadius = 20,
                                        TextColor = Color.White,
                                        BackgroundColor = Color.FromRgb(52, 58, 64),
                                        FontFamily = "fontawesomeicons.ttf#Regular",
                                        WidthRequest = 30,
                                        HeightRequest = 30,
                                        VerticalOptions = LayoutOptions.Center,
                                        Command = new Command(() =>
                                        {
                                            VerSolicitudRepartidor(Pedido.Id);
                                        })
                                    };
                                    Button btnEntrega = new Button()
                                    {
                                        AutomationId = "b7N3n7R364_" + Pedido.Id.ToString(),
                                        Text = "Reasignar Repartidor",
                                        FontSize = 8,
                                        CornerRadius = 20,
                                        TextColor = Color.Black,
                                        BackgroundColor = Color.FromRgb(255, 193, 7),
                                        FontFamily = "fontawesomeicons.ttf#Regular",
                                        WidthRequest = 30,
                                        HeightRequest = 30,
                                        VerticalOptions = LayoutOptions.Center,
                                        Command = new Command(() =>
                                        {
                                            Dictionary<string, object> PedidoData = new Dictionary<string, object>()
                                            {
                                                { "Id", Pedido.Id },
                                                { "Cliente", Pedido.Cliente },
                                                { "Campo", Pedido.Campo },
                                                { "Ubicacion", Pedido.Ubicacion }
                                            };
                                            ReasignarRepartidor(JsonConvert.SerializeObject(PedidoData));
                                        })
                                    };

                                    GridControles.Children.Add(btnSolicitud);
                                    GridControles.Children.Add(btnEntrega);
                                    Grid.SetRow(btnSolicitud, 0);
                                    Grid.SetColumn(btnSolicitud, 0);
                                    Grid.SetRow(btnEntrega, 0);
                                    Grid.SetColumn(btnEntrega, 1);
                                }
                            }
                            else
                            {
                                GridControles.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                                GridControles.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                                Button btnSolicitud = new Button()
                                {
                                    AutomationId = "b7N50L1c17Ud_" + Pedido.Id.ToString(),
                                    Text = "Ver solicitud",
                                    FontSize = 8,
                                    CornerRadius = 20,
                                    TextColor = Color.White,
                                    BackgroundColor = Color.FromRgb(52, 58, 64),
                                    FontFamily = "fontawesomeicons.ttf#Regular",
                                    WidthRequest = 30,
                                    HeightRequest = 30,
                                    VerticalOptions = LayoutOptions.Center,
                                    Command = new Command(() =>
                                    {
                                        VerSolicitudRepartidor(Pedido.Id);
                                    })
                                };

                                GridControles.Children.Add(btnSolicitud);
                                Grid.SetRow(btnSolicitud, 0);
                                Grid.SetColumn(btnSolicitud, 0);
                            }

                            StackPrincipal.Children.Add(GridPedido1);
                            StackPrincipal.Children.Add(GridPedido2);
                            StackPrincipal.Children.Add(GridControles);

                            FramePedido.Content = StackPrincipal;
                            FramePedido.BackgroundColor = BKGFrame;

                            GridPedidoPrinc.Children.Add(FramePedido);

                            RepartosListaForm.Children.Add(GridPedidoPrinc);
                        }
                    }
                    else
                    {
                        Grid gridNoPedidos = new Grid();
                        gridNoPedidos.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        gridNoPedidos.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                        Label labelNoRepartos = new Label()
                        {
                            Text = "\uf0d1\nSin Entregas",
                            FontFamily = "fontawesomeicons.ttf#Regular",
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontAttributes = FontAttributes.Bold,
                            FontSize = 50,
                            TextColor = Color.LightGray
                        };
                        await DisplayAlert("Atención!", "No tiene pedidos disponibles", "Aceptar");
                        gridNoPedidos.Children.Add(labelNoRepartos);
                        Grid.SetRow(labelNoRepartos, 0);
                        Grid.SetColumn(labelNoRepartos, 0);
                        RepartosListaForm.Children.Add(gridNoPedidos);
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Necesita conección a Internet para esta acción", "Aceptar");
                }
            }
        }

        // BOTON QUE EJECUTA EL VER LA SOLICITUD MAS VIGENTE DE UNA 
        private async void VerSolicitudRepartidor(int idordenpedido)
        {
            using (UserDialogs.Instance.Loading("Cargando documento..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    string folioTxt = "";
                    if (idordenpedido < 10)
                    {
                        folioTxt = "00";
                    }
                    else if (idordenpedido < 100)
                    {
                        folioTxt = "0";
                    }
                    folioTxt = folioTxt + idordenpedido.ToString();

                    int IdUsuario = 0;
                    var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                    foreach (var Usuario in VerifUsuario)
                    {
                        IdUsuario = Usuario.IdUsuario;
                    }
                    string WS = DependencyService.Get<IAgroWS>().ObtenerOrdenPedidoDoc(IdUsuario, "OP", folioTxt);
                    if (WS != "error")
                    {
                        Device.OpenUri(new Uri(WS));
                    }
                    else
                    {
                        await DisplayAlert("Error!", "Ocurríó un problema al asignar Orden de Pedido", "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                }
            }
        }

        // BOTON QUE EJECUTA EL CAMBIO DE ESTATUS A REVISADO POR PARTE DEL AGENTE LA ORDEN DE PEDIDO
        private async void MarcarOPVisto(int idordenpedido, View[] Views)
        {
            var entregado = await DisplayAlert("Atencion!", "¿Desea marcar como Verificada esta Orden de Pedido?", "Si", "Cancelar");
            if (entregado)
            {
                using (UserDialogs.Instance.Loading("Espere porfavor..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                    {
                        int IdUsuario = 0;
                        var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        foreach (var Usuario in VerifUsuario)
                        {
                            IdUsuario = Usuario.IdUsuario;
                        }
                        string WS = DependencyService.Get<IAgroWS>().CambiarEstatusOP(IdUsuario, idordenpedido, 2);
                        if (WS == "true")
                        {
                            Grid gridView = (Grid)Views[0];
                            gridView.Children.Remove(Views[1]);
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Ocurríó un problema al verificar Orden de Pedido", "Aceptar");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                    }
                }
            }
        }

        // FUNCION QUE REASIGNA UN REPARTIDOR A UNA ORDEN DE PEDIDO (SOLO POR AGENTE)
        private async void ReasignarRepartidor(string PedidoData)
        {
            using (UserDialogs.Instance.Loading("Verif. Entrega..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    try
                    {
                        PedidoData Pedido = JsonConvert.DeserializeObject<PedidoData>(PedidoData);
                        int IdRepartidor = DependencyService.Get<IAgroWS>().ObtenerIdRepartidorOP(Pedido.Id);
                        if (IdRepartidor > 0)
                        {
                            var repartidoresQuery = DependencyService.Get<ISQliteParams>().ConsultaRepartidores("SELECT * FROM Repartidores WHERE Estatus = 1 AND Id <> " + IdRepartidor.ToString());
                            RepartidorARLista = new List<RepartidorARPicker>();
                            foreach (var repartidor in repartidoresQuery)
                            {
                                RepartidorARLista.Add(new RepartidorARPicker
                                {
                                    Id = repartidor.Id,
                                    NombreRepartidor = repartidor.NombreRepartidor
                                });
                            }

                            ReparidoresARStack.Children.Clear();

                            Grid PedidoInfoARGrid = new Grid()
                            {
                                AutomationId = "p3D1d056r1D"
                            };
                            PedidoInfoARGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            PedidoInfoARGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            PedidoInfoARGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            PedidoInfoARGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            PedidoInfoARGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                            PedidoInfoARGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                            PedidoInfoARGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                            Label label1 = new Label()
                            {
                                Text = "Orden Pedido: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };
                            Label labelOP = new Label()
                            {
                                Text = GenerarFolioEntregas(Pedido.Id),
                                FontAttributes = FontAttributes.None,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };

                            Label label2 = new Label()
                            {
                                Text = "Cliente: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };
                            Label labelCliente = new Label()
                            {
                                Text = Pedido.Cliente,
                                FontAttributes = FontAttributes.None,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };

                            Label label3 = new Label()
                            {
                                Text = "Campo: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };
                            Label labelCampo = new Label()
                            {
                                Text = Pedido.Campo,
                                FontAttributes = FontAttributes.None,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };

                            Label label4 = new Label()
                            {
                                Text = "Ubicacion: ",
                                FontAttributes = FontAttributes.Bold,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };
                            Label labelUbicacion = new Label()
                            {
                                Text = Pedido.Ubicacion,
                                FontAttributes = FontAttributes.None,
                                VerticalTextAlignment = TextAlignment.Center,
                                TextColor = Color.FromRgb(0, 105, 92),
                                FontSize = 15
                            };

                            Picker pickerRepartidor = new Picker()
                            {
                                Title = "- Seleccione Repartidor -",
                                ItemDisplayBinding = new Binding("NombreRepartidor"),
                                SelectedItem = new Binding("Id"),
                                ItemsSource = RepartidorARLista
                            };

                            PedidoInfoARGrid.Children.Add(label1);
                            PedidoInfoARGrid.Children.Add(labelOP);
                            PedidoInfoARGrid.Children.Add(label2);
                            PedidoInfoARGrid.Children.Add(labelCliente);
                            PedidoInfoARGrid.Children.Add(label3);
                            PedidoInfoARGrid.Children.Add(labelCampo);
                            PedidoInfoARGrid.Children.Add(label4);
                            PedidoInfoARGrid.Children.Add(labelUbicacion);
                            PedidoInfoARGrid.Children.Add(pickerRepartidor);
                            Grid.SetRow(label1, 0);
                            Grid.SetColumn(label1, 0);
                            Grid.SetRow(labelOP, 0);
                            Grid.SetColumn(labelOP, 1);
                            Grid.SetRow(label2, 1);
                            Grid.SetColumn(label2, 0);
                            Grid.SetRow(labelCliente, 1);
                            Grid.SetColumn(labelCliente, 1);
                            Grid.SetRow(label3, 2);
                            Grid.SetColumn(label3, 0);
                            Grid.SetRow(labelCampo, 2);
                            Grid.SetColumn(labelCampo, 1);
                            Grid.SetRow(label4, 3);
                            Grid.SetColumn(label4, 0);
                            Grid.SetRow(labelUbicacion, 3);
                            Grid.SetColumn(labelUbicacion, 1);
                            Grid.SetRow(pickerRepartidor, 4);
                            Grid.SetColumnSpan(pickerRepartidor, 2);

                            Grid BotonesARGrid = new Grid()
                            {
                                AutomationId = "b070N356r1D"
                            };
                            BotonesARGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                            BotonesARGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                            BotonesARGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                            BotonesARGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                            Button btnEntregaAR = new Button()
                            {
                                Text = "\uf007 Reasignar Repartidor",
                                FontSize = 10,
                                CornerRadius = 20,
                                TextColor = Color.White,
                                BackgroundColor = Color.FromRgb(40, 167, 69),
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                WidthRequest = 38,
                                HeightRequest = 38,
                                VerticalOptions = LayoutOptions.Center,
                                Command = new Command(() =>
                                {
                                    ReasignarRepartidor(Pedido.Id, pickerRepartidor);
                                })
                            };

                            Button btnCancelarAR = new Button()
                            {
                                Text = "\uf00d Cancelar",
                                FontSize = 10,
                                CornerRadius = 20,
                                TextColor = Color.White,
                                BackgroundColor = Color.FromRgb(220, 53, 69),
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                WidthRequest = 38,
                                HeightRequest = 38,
                                VerticalOptions = LayoutOptions.Center,
                                Command = new Command(() =>
                                {
                                    CancelarReasignarRepartidor();
                                })
                            };

                            BotonesARGrid.Children.Add(btnEntregaAR);
                            BotonesARGrid.Children.Add(btnCancelarAR);
                            Grid.SetRow(btnEntregaAR, 0);
                            Grid.SetColumn(btnEntregaAR, 0);
                            Grid.SetRow(btnCancelarAR, 0);
                            Grid.SetColumn(btnCancelarAR, 1);

                            ReparidoresARStack.Children.Add(PedidoInfoARGrid);
                            ReparidoresARStack.Children.Add(BotonesARGrid);

                            await EntregasScrollView.ScrollToAsync(0, EntregasScrollView.Height, true);
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Ocurrió un problema al verificar Repartidor de Orden de Pedido", "Aceptar");
                        }
                    }
                    catch (Exception err)
                    {
                        await DisplayAlert("Error!", "Ocurrió un problema al verificar Repartidor de Orden de Pedido: - " + err.ToString(), "Aceptar");
                    } 
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                }
            }
        }

        // BOTON QUE EJECUTA LA REASIGNACION DE UN REPARTIDOR A UNA ORDEN DE PEDIDO (SOLO AGENTE)
        private async void ReasignarRepartidor(int idordenpedido, View repartidorpicker)
        {
            Picker pickerRep = (Picker)repartidorpicker;
            int index = pickerRep.SelectedIndex;
            if(index != -1)
            {
                var asignar = await DisplayAlert("Atencion!", "¿Desea asignar esta Orden al Repartidor seleccionado?", "Si", "Cancelar");
                if (asignar)
                {
                    using (UserDialogs.Instance.Loading("Asignando Pedido..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                        {
                            int IdUsuario = 0;
                            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                            foreach (var Usuario in VerifUsuario)
                            {
                                IdUsuario = Usuario.IdUsuario;
                            }
                            int IdRepartidor = JsonConvert.DeserializeObject<RepartidorARPicker>(JsonConvert.SerializeObject(pickerRep.ItemsSource[index])).Id;
                            string WS = DependencyService.Get<IAgroWS>().AsignarOPRepartidor(IdUsuario, idordenpedido, IdRepartidor.ToString());
                            if (WS == "true")
                            {
                                ReparidoresARStack.Children.Clear();
                                await EntregasScrollView.ScrollToAsync(0, EntregasScrollView.Height, true);
                            }
                            else
                            {
                                await DisplayAlert("Error!", "Ocurríó un problema al asignar Orden de Pedido", "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("Atención!", "Seleccione un Repartidor", "Aceptar");
            }
        }

        // BOTON QUE EJECUTA EL CANCELAR LA ASIGNACION DE UN NUEVO REPARTIDOR A ORDEN PEDIDO
        private async void CancelarReasignarRepartidor()
        {
            ReparidoresARStack.Children.Clear();
            await EntregasScrollView.ScrollToAsync(0, EntregasScrollView.Height, true);
        }

        // FUNCION QUE  GENERA UN ID DE TEXTO EN UN FOLIO (PARA ID DE ORDEN DE PEDIDO)
        private string GenerarFolioEntregas(int IdOrdenPedido)
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
    }
}