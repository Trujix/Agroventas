using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using AgroVentas.TablasSQlite;
using Acr.UserDialogs;

namespace AgroVentas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuRepartidor : TabbedPage
    {
        // ------------------ [ ********* ] ------------------
        // VARIABLES GLOBALES
        private class RepartidorLogin
        {
            public string Usuario { get; set; }
            public string Pass { get; set; }
            public string SecurityID { get; set; }
        }
        private class ParamsRepartidorLogin
        {
            public string Respuesta { get; set; }
            public string UsuarioInfo { get; set; }
            public string TipoUsuario { get; set; }
        }
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
        // ------------------------------------------------------
        public MenuRepartidor()
        {
            InitializeComponent();

            LlenarRepartidorListaAjustes(false);
        }


        // ******************************************** [ MENU REPARTOS ] ********************************************
        // BOTON QUE ACTIVA LA FUNCION QUE OBTIENE LA LISTA DE ORDENES DE PEDIDOS
        private void Obtener_ListaRepartos(object sender, EventArgs e)
        {
            LlenarRepartidorListaAjustes(true);
        }

        // FUNCION AL INICIO QUE LLENA LOS DATOS GENERALES DEL REPARTIDOR
        private async void LlenarRepartidorListaAjustes(bool boton)
        {
            using (UserDialogs.Instance.Loading("Verif. Solicitudes..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (!boton)
                {
                    string NomRepartidor = "", Email = "";
                    var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                    foreach (var usuario in VerifUsuario)
                    {
                        NomRepartidor = usuario.Nombre;
                        Email = usuario.Correo;
                    }

                    NombreRepartidor.Text = NomRepartidor;
                    EmailRepartidor.Text = Email;
                }
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    string Usuario = "";
                    var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                    foreach (var usuario in VerifUsuario)
                    {
                        Usuario = usuario.UsuarioNombre;
                    }
                    RepartosListaForm.Children.Clear();
                    List<OrdenesPedidosLista> ListaPedidos = JsonConvert.DeserializeObject<List<OrdenesPedidosLista>>(DependencyService.Get<IAgroWS>().ObtenerOrdenesPedido());
                    if (ListaPedidos.Count > 0)
                    {
                        foreach (var Pedido in ListaPedidos)
                        {
                            Grid GridPedido1 = new Grid()
                            {
                                AutomationId = "6R1d1_" + Pedido.Id.ToString()
                            };
                            GridPedido1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                            GridPedido1.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                            GridPedido1.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                            Label label = new Label()
                            {
                                AutomationId = "l4B3L1_&" + Pedido.Id.ToString(),
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
                                        VerticalOptions = LayoutOptions.Center
                                    };
                                    GridControles.Children.Add(btnEntregado);
                                    Grid.SetRow(btnEntregado, 0);
                                    Grid.SetColumn(btnEntregado, 0);
                                }
                                else
                                {
                                    BKGFrame = Color.FromRgb(255, 224, 178);
                                    if (Pedido.Repartidor == Usuario)
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
                                        Button btnEntrega = new Button()
                                        {
                                            AutomationId = "b7N3n7R364_" + Pedido.Id.ToString(),
                                            Text = "Marcar Entregado",
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
                                                EntregarOrdenPedido(Pedido.Id, new View[] { GridControles, StackPrincipal, FramePedido });
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
                                Button btnTomarPedido = new Button()
                                {
                                    AutomationId = "b7N70m4Rp3D1d0_" + Pedido.Id.ToString(),
                                    Text = "Tomar Pedido",
                                    FontSize = 8,
                                    CornerRadius = 20,
                                    TextColor = Color.White,
                                    BackgroundColor = Color.FromRgb(23, 162, 184),
                                    FontFamily = "fontawesomeicons.ttf#Regular",
                                    WidthRequest = 30,
                                    HeightRequest = 30,
                                    VerticalOptions = LayoutOptions.Center,
                                    Command = new Command(() =>
                                    {
                                        AsignarOrdenPedidoRepartidor(Pedido.Id, Usuario, new View[] { GridControles, StackPrincipal, FramePedido });
                                    })
                                };

                                GridControles.Children.Add(btnSolicitud);
                                GridControles.Children.Add(btnTomarPedido);
                                Grid.SetRow(btnSolicitud, 0);
                                Grid.SetColumn(btnSolicitud, 0);
                                Grid.SetRow(btnTomarPedido, 0);
                                Grid.SetColumn(btnTomarPedido, 1);
                            }

                            StackPrincipal.Children.Add(GridPedido1);
                            StackPrincipal.Children.Add(GridPedido2);
                            StackPrincipal.Children.Add(GridControles);

                            FramePedido.Content = StackPrincipal;
                            FramePedido.BackgroundColor = BKGFrame;

                            Grid GridPedidoPrinc = new Grid()
                            {
                                AutomationId = "6R1dPr1nc_" + Pedido.Id.ToString(),
                            };
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
                            Text = "\uf0d1\nSin Pedidos",
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
                    if (boton)
                    {
                        await DisplayAlert("Atención!", "Necesita conección a Internet para esta acción", "Aceptar");
                    }
                }
            }
        }

        // ------------------- [ BOTONES DE LISTA DE PEDIDOS ] -------------------
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

                    int IdUsuario = DependencyService.Get<ISQliteParams>().ObtenerIDUsuarioReparto();
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

        // FUNCION QUE ASIGNA UNA ORDEN PEDIDO A UN REPARTIDOR
        private async void AsignarOrdenPedidoRepartidor(int idordenpedido, string repartidor, View[] viewslistaop)
        {
            var asignar = await DisplayAlert("Atencion!", "¿Desea tomar esta asignación?", "Si", "Cancelar");
            if (asignar)
            {
                using (UserDialogs.Instance.Loading("Asignando Pedido..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                    {
                        int IdUsuario = DependencyService.Get<ISQliteParams>().ObtenerIDUsuarioReparto();
                        string WS = DependencyService.Get<IAgroWS>().AsignarOPRepartidor(IdUsuario, idordenpedido, repartidor);
                        Grid GridView = (Grid)viewslistaop[0];
                        StackLayout StackView = (StackLayout)viewslistaop[1];
                        Frame FrameView = (Frame)viewslistaop[2];
                        if (WS == "true")
                        {
                            GridView.Children.Clear();
                            GridView.RowDefinitions.Clear();
                            GridView.ColumnDefinitions.Clear();

                            GridView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                            GridView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

                            Button btnSolicitud = new Button()
                            {
                                AutomationId = "b7N50L1c17Ud_" + idordenpedido.ToString(),
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
                                    VerSolicitudRepartidor(idordenpedido);
                                })
                            };
                            Button btnEntrega = new Button()
                            {
                                AutomationId = "b7N3n7R364_" + idordenpedido.ToString(),
                                Text = "Marcar Entregado",
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
                                    EntregarOrdenPedido(idordenpedido, viewslistaop);
                                })
                            };

                            GridView.Children.Add(btnSolicitud);
                            GridView.Children.Add(btnEntrega);
                            Grid.SetRow(btnSolicitud, 0);
                            Grid.SetColumn(btnSolicitud, 0);
                            Grid.SetRow(btnEntrega, 0);
                            Grid.SetColumn(btnEntrega, 1);

                            FrameView.BackgroundColor = Color.FromRgb(255, 224, 178);
                        }
                        else if (WS == "invalido")
                        {
                            StackView.Children.Remove(viewslistaop[0]);
                            FrameView.BackgroundColor = Color.FromRgb(255, 224, 178);
                            await DisplayAlert("Atención!", "Al parecer esta Orden ya fue asignada a otro Repartidor", "Aceptar");
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

        // FUNCION QUE CAMBIA EL ESTATUS DE UNA ORDEN DE PEDIDO A ENTREGADO
        private async void EntregarOrdenPedido(int idordenpedido, View[] viewslistaop)
        {
            var entregado = await DisplayAlert("Atencion!", "¿Desea marcar como Entregada esta Orden de Pedido?", "Si", "Cancelar");
            if (entregado)
            {
                using (UserDialogs.Instance.Loading("Entregando..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                    {
                        int IdUsuario = DependencyService.Get<ISQliteParams>().ObtenerIDUsuarioReparto();
                        string WS = DependencyService.Get<IAgroWS>().CambiarEstatusOP(IdUsuario, idordenpedido, 1);
                        Grid GridView = (Grid)viewslistaop[0];
                        Frame FrameView = (Frame)viewslistaop[2];
                        if (WS == "true")
                        {
                            GridView.Children.Clear();
                            GridView.RowDefinitions.Clear();
                            GridView.ColumnDefinitions.Clear();

                            GridView.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                            Button btnEntregado = new Button()
                            {
                                AutomationId = "b7N3n7R364_" + idordenpedido.ToString(),
                                Text = "\uf058 Pedido Entregado",
                                FontSize = 8,
                                CornerRadius = 20,
                                TextColor = Color.White,
                                BackgroundColor = Color.FromRgb(40, 167, 69),
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                WidthRequest = 30,
                                HeightRequest = 30,
                                VerticalOptions = LayoutOptions.Center
                            };
                            GridView.Children.Add(btnEntregado);
                            Grid.SetRow(btnEntregado, 0);
                            Grid.SetColumn(btnEntregado, 0);

                            FrameView.BackgroundColor = Color.FromRgb(200, 230, 201);
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Ocurríó un problema al entregar Orden de Pedido", "Aceptar");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Atención!", "Requiere conección a Internet para esta acción", "Aceptar");
                    }
                }
            }
        }


        // ******************************************** [ MENU AJUSTES ] ********************************************
        // -----------------------------------------------------------------------

        // BOTON QUE GUARDA EL CAMBIO DE CONTRASEÑA DEL REPARTIDOR
        private async void Cambiar_PasswordRep(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(RepAntiguaPass.Text))
            {
                RepAntiguaPass.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Antigua Contraseña");
            }
            else if (string.IsNullOrEmpty(RepNuevaPass.Text))
            {
                RepNuevaPass.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Nueva Contraseña");
            }
            else if (string.IsNullOrEmpty(RepConfirmarPass.Text))
            {
                RepConfirmarPass.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Confirmar Contraseña");
            }
            else if (RepNuevaPass.Text != RepConfirmarPass.Text)
            {
                RepNuevaPass.Focus();
                await DisplayAlert("Atención!", "Las contraseñas no coinciden", "Aceptar");
            }
            else
            {
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    using (UserDialogs.Instance.Loading("Guardando Cambios..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        string user = "", pass = "", securityid = "";
                        foreach (var usuario in VerifUsuario)
                        {
                            user = usuario.UsuarioNombre;
                            pass = RepAntiguaPass.Text;
                            securityid = usuario.SecurityID;
                        }
                        RepartidorLogin LoginVerif = new RepartidorLogin()
                        {
                            Usuario = user,
                            Pass = pass,
                            SecurityID = securityid
                        };
                        ParamsRepartidorLogin loginMsg = JsonConvert.DeserializeObject<ParamsRepartidorLogin>(DependencyService.Get<IAgroWS>().Login(JsonConvert.SerializeObject(LoginVerif)));
                        if (loginMsg.Respuesta == "true")
                        {
                            Usuario UsuarioData = new Usuario()
                            {
                                UsuarioNombre = user,
                                SecurityID = RepNuevaPass.Text
                            };
                            BDPendientes CambioPass = new BDPendientes()
                            {
                                Tabla = "Repartidores",
                                Accion = "Password",
                                Data = JsonConvert.SerializeObject(UsuarioData)
                            };
                            string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(CambioPass));
                            if (WS == "true")
                            {
                                await DisplayAlert("Éxito!", "La Contraseña ha sido modificada", "Aceptar");
                                RepAntiguaPass.Text = "";
                                RepNuevaPass.Text = "";
                                RepConfirmarPass.Text = "";
                            }
                            else
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al actualizar contraseña", "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Contraseña incorrecta", "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esa acción", "Aceptar");
                }
            }
        }

        // BOTON QUE GUARDA LA INFO DEL REPARTIDOR (NOMBRE Y CORREO)
        private async void Guardar_InfoRepartidor(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(NombreRepartidor.Text))
            {
                NombreRepartidor.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Nombre Repartidor");
            }
            else if (string.IsNullOrEmpty(EmailRepartidor.Text))
            {
                EmailRepartidor.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Correo Electrónico");
            }
            else
            {
                using (UserDialogs.Instance.Loading("Guardando Cambios..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    var VerifRepartidor = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                    string user = "";
                    foreach (var repartidor in VerifRepartidor)
                    {
                        user = repartidor.UsuarioNombre;
                    }
                    Usuario UsuarioData = new Usuario()
                    {
                        UsuarioNombre = user,
                        Nombre = NombreRepartidor.Text,
                        Correo = EmailRepartidor.Text
                    };
                    BDPendientes CambiosRepartidor = new BDPendientes()
                    {
                        Accion = "Modificar",
                        Tabla = "Repartidores",
                        Data = JsonConvert.SerializeObject(UsuarioData)
                    };
                    string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(CambiosRepartidor));
                    object[] cambioData = { NombreRepartidor.Text, EmailRepartidor.Text, "u5U4r10+2236" };
                    string RestaurarAccion = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "UPDATE Usuario SET Nombre=?,Correo=? WHERE IdString=?", cambioData);
                    if (RestaurarAccion == "true")
                    {
                        await DisplayAlert("Éxito!", "Cambios Guardados correctamente", "Aceptar");
                    }
                    else
                    {
                        await DisplayAlert("Error!", "Ocurrió un problema al modificar la información del Repartidor", "Aceptar");
                    }
                }
            }
        }

        // BOTON QUE ABRE EL MANUAL DE USUARIO
        private async void Abrir_ManualUsuario(object sender, EventArgs e)
        {
            using (UserDialogs.Instance.Loading("Abriendo Manual..."))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    try
                    {
                        string Manual = "";
                        var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        foreach (var Usuario in VerifUsuario)
                        {
                            Manual = Usuario.TipoUsuario;
                        }
                        var UrlManuales = DependencyService.Get<IAgroWS>().ObtenerManualUrl();
                        Device.OpenUri(new Uri(UrlManuales + Manual + ".pdf"));
                    }
                    catch (Exception err)
                    {
                        await DisplayAlert("Error!", err.ToString(), "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Necesita conección a Internet para esta acción", "Aceptar");
                }
            }
        }

        // BOTON QUE PERMITE CERRAR LA SESION DEL REPARTIDOR
        private async void Cerrar_SesionRep(object sender, EventArgs e)
        {
            var borrar = await DisplayAlert("Atencion!", "¿Desea cerrar sesión?", "Si", "Cancelar");
            if (borrar)
            {
                object[] CerrarSesionData = { false, "u5U4r10+2236" };
                string CerrarSesion = DependencyService.Get<ISQliteParams>().QueryMaestra(1, "UPDATE Usuario SET Logeado=? WHERE IdString=?", CerrarSesionData);
                await Navigation.PushModalAsync(new MainPage());
            }
        }

        // BOTON QUE DESACTIVA EL DISPOSITVO DE LA SESION DEL REPARTIDOR
        private async void Desactivar_DispositivoRep(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(RepPassDesactivar.Text))
            {
                RepPassDesactivar.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Contraseña");
            }
            else
            {
                if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
                {
                    using (UserDialogs.Instance.Loading("Desactivando dispositivo..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        var VerifRepartidor = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        string user = "", pass = "", securityid = "";
                        foreach (var repartidor in VerifRepartidor)
                        {
                            user = repartidor.UsuarioNombre;
                            pass = RepPassDesactivar.Text;
                            securityid = repartidor.SecurityID;
                        }
                        RepartidorLogin LoginVerif = new RepartidorLogin()
                        {
                            Usuario = user,
                            Pass = pass,
                            SecurityID = securityid
                        };
                        ParamsRepartidorLogin loginMsg = JsonConvert.DeserializeObject<ParamsRepartidorLogin>(DependencyService.Get<IAgroWS>().Login(JsonConvert.SerializeObject(LoginVerif)));
                        if (loginMsg.Respuesta == "true")
                        {
                            Usuario RepartidorData = new Usuario()
                            {
                                UsuarioNombre = user
                            };
                            BDPendientes DesactivarDisp = new BDPendientes()
                            {
                                Accion = "Desactivar",
                                Tabla = "Repartidores",
                                Data = JsonConvert.SerializeObject(RepartidorData)
                            };
                            string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(DesactivarDisp));
                            if (WS == "true")
                            {
                                string limpiarAPP = DependencyService.Get<ISQliteParams>().DesactivarSQliteWS();
                                if (limpiarAPP == "true")
                                {
                                    await Navigation.PushModalAsync(new MainPage());
                                }
                                else
                                {
                                    await DisplayAlert("Error!", "Ocurrió un problema al desactivar dispositivo", "Aceptar");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Error!", "Ocurrió un problema al verificar usuario", "Aceptar");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Contraseña incorrecta", "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Atención!", "Requiere conección a Internet para esa acción", "Aceptar");
                }
            }
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