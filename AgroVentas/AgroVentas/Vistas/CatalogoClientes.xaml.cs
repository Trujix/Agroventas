using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AgroVentas.TablasSQlite;
using Newtonsoft.Json;
using Acr.UserDialogs;

namespace AgroVentas.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CatalogoClientes : ContentPage
    {
        public CatalogoClientes()
        {
            InitializeComponent();

            LlenarClientesFormLista();
        }

        // ------------------------------------------------------------------------------------------
        // ************************************** [ CLIENTES ] **************************************
        // ---------- VARIABLES GLOBALES PUBLICAS ----------
        private int IdMaxCliente = 0;
        private int IdSeleccCliente = 0;
        private class ClientesAltaPicker
        {
            public int Id { get; set; }
            public string RazonSocial { get; set; }
        }
        private class ClienteAltaWS
        {
            public string ClienteData { get; set; }
            public string CorreosData { get; set; }
            public string CamposData { get; set; }
        }
        private List<ClientesAltaPicker> ClientesAltaLista;
        private List<Campos> CamposAltaClienteLista;
        private List<CorreosClientes> CorreosAltaClienteLista;
        private BDPendientes ClientesDataWS;
        private Clientes DataClienteServidor;
        List<string> ClientesBusq;
        List<string> ClientesValidar;
        // ----------------------------------------


        // FUNCION INICIAL DE LLENADO DE LA LISTA
        private async void LlenarClientesFormLista()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            var clientesQuery = DependencyService.Get<ISQliteParams>().ConsultaClientes("SELECT * FROM Clientes WHERE Estatus = 1 ORDER BY RazonSocial ASC");
            ClientesAltaLista = new List<ClientesAltaPicker>();
            ClientesBusq = new List<string>();
            ClientesValidar = new List<string>();
            foreach (var cliente in clientesQuery)
            {
                ClientesAltaLista.Add(new ClientesAltaPicker
                {
                    Id = cliente.Id,
                    RazonSocial = cliente.RazonSocial
                });
                ClientesBusq.Add(cliente.RazonSocial);
                ClientesValidar.Add(cliente.RazonSocial);
            }
            ClienteFormLista.ItemsSource = ClientesAltaLista;
            ClienteBusq.ItemsSource = ClientesBusq;
            ClienteFormRazSoc.ItemsSource = ClientesValidar;
            UserDialogs.Instance.HideLoading();
        }

        // ----------- FUNCION DE BOTONES DEL FORMULARIO
        // NUEVO CLIENTE
        private void Nuevo_Cliente(object sender, EventArgs e)
        {
            LimpiarCamposClientesForm();
            ClienteFormRazSoc.Focus();
            ClienteFormLista.SelectedItem = string.Empty;
            ClienteBusq.Text = "";
            IdSeleccCliente = 0;
        }

        // GUARDAR CLIENTE
        private async void Guardar_Cliente(object sender, EventArgs e)
        {
            if (ValidarFormularioClientes())
            {
                using (UserDialogs.Instance.Loading("Guardando Cliente..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000));
                    ClientesDataWS = new BDPendientes()
                    {
                        Tabla = "Clientes"
                    };
                    object[] DataCliente = CrearClientesArray();
                    string query = "INSERT INTO Clientes (Id,RazonSocial,RFC,Calle,NumInt,NumExt,Colonia,Localidad,Municipio,Estado,Pais,CP,NombreContacto,Telefono,UsoCFDI,MetodoPago,FormaPago,DiasCredito,LineaCredito,Estatus) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
                    ClientesDataWS.Accion = "Nuevo";
                    if (IdSeleccCliente > 0)
                    {
                        query = "UPDATE Clientes SET RazonSocial=?,RFC=?,Calle=?,NumInt=?,NumExt=?,Colonia=?,Localidad=?,Municipio=?,Estado=?,Pais=?,CP=?,NombreContacto=?,Telefono=?,UsoCFDI=?,MetodoPago=?,FormaPago=?,DiasCredito=?,LineaCredito=? WHERE Id=?";
                        ClientesDataWS.Accion = "Editar";

                        object[] dataEliminar = { IdSeleccCliente };
                        string RestaurarCorreos = DependencyService.Get<ISQliteParams>().QueryMaestra(4, "DELETE FROM CorreosClientes WHERE IdCliente=?", dataEliminar);
                        string RestaurarCampo = DependencyService.Get<ISQliteParams>().QueryMaestra(3, "DELETE FROM Campos WHERE IdCliente=?", dataEliminar);
                    }

                    ClienteAltaWS ClienteAlta = new ClienteAltaWS()
                    {
                        ClienteData = JsonConvert.SerializeObject(DataClienteServidor),
                        CorreosData = JsonConvert.SerializeObject(CorreosAltaClienteLista),
                        CamposData = JsonConvert.SerializeObject(CamposAltaClienteLista)
                    };
                    ClientesDataWS.Data = JsonConvert.SerializeObject(ClienteAlta);
                    try
                    {
                        string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(ClientesDataWS));

                        string GuardarCliente = DependencyService.Get<ISQliteParams>().QueryMaestra(2, query, DataCliente);
                        foreach (var correos in CorreosAltaClienteLista)
                        {
                            object[] retaurarCorreos = { correos.IdCliente, correos.Correo };
                            string RestaurarAccion = DependencyService.Get<ISQliteParams>().QueryMaestra(4, "INSERT INTO CorreosClientes (IdCliente,Correo) VALUES (?,?)", retaurarCorreos);
                        }
                        foreach (var campo in CamposAltaClienteLista)
                        {
                            object[] restaurarCampos = { campo.IdCliente, campo.NombreCampo, campo.Ubicacion };
                            string RestaurarAccion = DependencyService.Get<ISQliteParams>().QueryMaestra(3, "INSERT INTO Campos (IdCliente,NombreCampo,Ubicacion) VALUES (?,?,?)", restaurarCampos);
                        }

                        if (GuardarCliente == "true")
                        {
                            await DisplayAlert("Éxito!", "Cliente Guardado correctamente", "Aceptar");
                            LimpiarCamposClientesForm();
                            LlenarClientesFormLista();
                        }
                        else
                        {
                            await DisplayAlert("Error!", GuardarCliente, "Aceptar");
                        }
                    }
                    catch (Exception err)
                    {
                        await DisplayAlert("Error!", "Ocurrió un problema al guardar Cliente: - " + err, "Aceptar");
                    }
                }
            }
        }

        // EDITAR CLIENTE
        private void Editar_Cliente(object sender, EventArgs e)
        {
            int index = ClienteFormLista.SelectedIndex;
            if (index != -1)
            {
                try
                {
                    IdSeleccCliente = JsonConvert.DeserializeObject<ClientesAltaPicker>(JsonConvert.SerializeObject(ClienteFormLista.ItemsSource[index])).Id;
                    var clienteEditar = DependencyService.Get<ISQliteParams>().ConsultaClientes("SELECT * FROM Clientes WHERE Id = " + IdSeleccCliente.ToString());
                    var camposEditar = DependencyService.Get<ISQliteParams>().ConsultaCampos("SELECT * FROM Campos WHERE IdCliente = " + IdSeleccCliente.ToString());
                    var correosEditar = DependencyService.Get<ISQliteParams>().ConsultaCorreos("SELECT * FROM CorreosClientes WHERE IdCliente = " + IdSeleccCliente.ToString());

                    foreach (var cliente in clienteEditar)
                    {
                        ClienteFormRazSoc.Text = (cliente.RazonSocial == "--") ? "" : cliente.RazonSocial;
                        ClienteFormRFC.Text = (cliente.RFC == "--") ? "" : cliente.RFC;
                        ClienteFormCalle.Text = (cliente.Calle == "--") ? "" : cliente.Calle;
                        ClienteFormNumInt.Text = (cliente.NumInt == "--") ? "" : cliente.NumInt.ToString();
                        ClienteFormNumExt.Text = (cliente.NumExt == "--") ? "" : cliente.NumExt.ToString();
                        ClienteFormColonia.Text = (cliente.Colonia == "--") ? "" : cliente.Colonia;
                        ClienteFormLocalidad.Text = (cliente.Localidad == "--") ? "" : cliente.Localidad;
                        ClienteFormMunicip.Text = (cliente.Municipio == "--") ? "" : cliente.Municipio;
                        ClienteFormEstado.Text = (cliente.Estado == "--") ? "" : cliente.Estado;
                        ClienteFormPais.Text = (cliente.Pais == "--") ? "" : cliente.Pais;
                        ClienteFormCP.Text = (cliente.CP == 0) ? "" : cliente.CP.ToString();
                        ClienteFormNomCont.Text = (cliente.NombreContacto == "--") ? "" : cliente.NombreContacto;
                        ClienteFormTelef.Text = (cliente.Telefono == 0) ? "" : cliente.Telefono.ToString();
                        ClienteFormUsoCFDI.Text = (cliente.UsoCFDI == "--") ? "" : cliente.UsoCFDI;
                        ClienteFormMetPago.Text = (cliente.MetodoPago == "--") ? "" : cliente.MetodoPago;
                        ClienteFormFormPago.Text = (cliente.FormaPago == "--") ? "" : cliente.FormaPago;
                        ClienteFormDiasCred.Text = (cliente.DiasCredito == 0) ? "" : cliente.DiasCredito.ToString();
                        ClienteFormLinCred.Text = (cliente.LineaCredito == 0) ? "" : cliente.LineaCredito.ToString();
                    }

                    if (camposEditar.Count > 0)
                    {
                        CamposClienteAltaGrid.Children.Clear();
                        CamposClienteAltaGrid.RowDefinitions.Clear();
                        CamposClienteAltaGrid.ColumnDefinitions.Clear();
                        ColumnDefinition colDef1 = new ColumnDefinition() { Width = GridLength.Auto };
                        ColumnDefinition colDef2 = new ColumnDefinition() { Width = GridLength.Star };
                        ColumnDefinition colDef3 = new ColumnDefinition() { Width = GridLength.Auto };
                        ColumnDefinition colDef4 = new ColumnDefinition() { Width = GridLength.Star };
                        CamposClienteAltaGrid.ColumnDefinitions.Add(colDef1);
                        CamposClienteAltaGrid.ColumnDefinitions.Add(colDef2);
                        CamposClienteAltaGrid.ColumnDefinitions.Add(colDef3);
                        CamposClienteAltaGrid.ColumnDefinitions.Add(colDef4);
                        int rowIndx = 0;
                        foreach (var campo in camposEditar)
                        {
                            RowDefinition rowDef = new RowDefinition()
                            {
                                Height = GridLength.Auto
                            };
                            CamposClienteAltaGrid.RowDefinitions.Add(rowDef);

                            Label labelCampo = new Label()
                            {
                                Text = "\uf1bb",
                                AutomationId = "l4B3Lc4Mp0",
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 20
                            };
                            Entry entryCampo = new Entry()
                            {
                                Text = campo.NombreCampo,
                                AutomationId = "eN7R1c4Mp0",
                                Placeholder = "Nombre Campo",
                                PlaceholderColor = Color.DarkGray,
                                TextColor = Color.FromRgb(0, 105, 92)
                            };

                            Label labelUbicacion = new Label()
                            {
                                Text = "\uf041",
                                AutomationId = "l4B3LuB1C4c10N",
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 20
                            };
                            Entry entryUbicacion = new Entry()
                            {
                                Text = campo.Ubicacion,
                                AutomationId = "eN7R1uB1C4c10N",
                                Placeholder = "Ubicacion",
                                PlaceholderColor = Color.DarkGray,
                                TextColor = Color.FromRgb(0, 105, 92)
                            };

                            CamposClienteAltaGrid.Children.Add(labelCampo, 0, rowIndx);
                            CamposClienteAltaGrid.Children.Add(entryCampo, 1, rowIndx);
                            CamposClienteAltaGrid.Children.Add(labelUbicacion, 2, rowIndx);
                            CamposClienteAltaGrid.Children.Add(entryUbicacion, 3, rowIndx);
                            rowIndx++;
                        }
                    }

                    if (correosEditar.Count > 0)
                    {
                        CorreosClienteAltaGrid.Children.Clear();
                        CorreosClienteAltaGrid.RowDefinitions.Clear();
                        CorreosClienteAltaGrid.ColumnDefinitions.Clear();
                        ColumnDefinition colDef1 = new ColumnDefinition() { Width = GridLength.Auto };
                        ColumnDefinition colDef2 = new ColumnDefinition() { Width = GridLength.Star };
                        CorreosClienteAltaGrid.ColumnDefinitions.Add(colDef1);
                        CorreosClienteAltaGrid.ColumnDefinitions.Add(colDef2);
                        int rowIndx = 0;
                        foreach (var correos in correosEditar)
                        {
                            RowDefinition rowDef = new RowDefinition()
                            {
                                Height = GridLength.Auto
                            };
                            CorreosClienteAltaGrid.RowDefinitions.Add(rowDef);
                            Label labelEmail = new Label()
                            {
                                Text = "\uf1fa",
                                AutomationId = "l4B3LeMA1l",
                                FontFamily = "fontawesomeicons.ttf#Regular",
                                VerticalTextAlignment = TextAlignment.Center,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 20
                            };
                            Entry entryEmail = new Entry()
                            {
                                Text = correos.Correo,
                                AutomationId = "eN7R1eMA1l",
                                Placeholder = "Correo Electronico",
                                PlaceholderColor = Color.DarkGray,
                                TextColor = Color.FromRgb(0, 105, 92)
                            };

                            CorreosClienteAltaGrid.Children.Add(labelEmail, 0, rowIndx);
                            CorreosClienteAltaGrid.Children.Add(entryEmail, 1, rowIndx);
                            rowIndx++;
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
                DisplayAlert("Error!", "Seleccione un Cliente", "Aceptar");
            }
        }

        // BORRAR CLIENTE
        private async void Borrar_Cliente(object sender, EventArgs e)
        {
            int index = ClienteFormLista.SelectedIndex;
            if (index != -1)
            {
                ClientesAltaPicker clienteInfo = JsonConvert.DeserializeObject<ClientesAltaPicker>(JsonConvert.SerializeObject(ClienteFormLista.ItemsSource[index]));
                var borrar = await DisplayAlert("Atencion!", "¿Desea borrar a " + clienteInfo.RazonSocial + "?", "Si", "Cancelar");
                if (borrar)
                {
                    using (UserDialogs.Instance.Loading("Eliminando Cliente..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        DataClienteServidor = new Clientes()
                        {
                            Id = clienteInfo.Id,
                            Estatus = 0
                        };
                        ClientesDataWS = new BDPendientes()
                        {
                            Tabla = "Clientes",
                            Accion = "Borrar",
                            Data = JsonConvert.SerializeObject(DataClienteServidor)
                        };
                        string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(ClientesDataWS));
                        object[] idBorrar = { 0, clienteInfo.Id };
                        string BorrarCliente = DependencyService.Get<ISQliteParams>().QueryMaestra(2, "UPDATE Clientes SET Estatus=? WHERE Id=?", idBorrar);
                        if (BorrarCliente == "true")
                        {
                            LimpiarCamposClientesForm();
                            LlenarClientesFormLista();
                            IdSeleccCliente = 0;
                        }
                        else
                        {
                            await DisplayAlert("Error!", BorrarCliente, "Aceptar");
                        }
                    }
                }
            }
            else
            {
                await DisplayAlert("Error!", "Seleccione un Cliente", "Aceptar");
            }
        }

        // EVENTO QUE SE EJECUTA AL SELECCIONAR UN CLIENTE DE LA LISTA
        private void ClienteFormLista_SelectedIndexChanged(object sender, EventArgs e)
        {
            IdSeleccCliente = 0;
            LimpiarCamposClientesForm();
        }

        // ------------ FUNCIONES CON CATALOGOS ------------
        // FUNCION QUE LIMPIA LOS CAMPOS DEL FORMULARIO ALTA DE CLIENTE
        private void LimpiarCamposClientesForm()
        {
            ClienteFormRazSoc.Text = "";
            ClienteFormRFC.Text = "";
            ClienteFormCalle.Text = "";
            ClienteFormNumInt.Text = "";
            ClienteFormNumExt.Text = "";
            ClienteFormColonia.Text = "";
            ClienteFormLocalidad.Text = "";
            ClienteFormMunicip.Text = "";
            ClienteFormEstado.Text = "";
            ClienteFormPais.Text = "";
            ClienteFormCP.Text = "";
            ClienteFormNomCont.Text = "";
            ClienteFormTelef.Text = "";
            ClienteFormUsoCFDI.Text = "";
            ClienteFormMetPago.Text = "";
            ClienteFormFormPago.Text = "";
            ClienteFormDiasCred.Text = "";
            ClienteFormLinCred.Text = "";

            CorreosClienteAltaGrid.Children.Clear();
            CorreosClienteAltaGrid.RowDefinitions.Clear();
            CorreosClienteAltaGrid.ColumnDefinitions.Clear();

            CamposClienteAltaGrid.Children.Clear();
            CamposClienteAltaGrid.RowDefinitions.Clear();
            CamposClienteAltaGrid.ColumnDefinitions.Clear();
        }

        // FUNCION QUE EMPAQUETA LOS VALORES DEL FORMULARIO DE CLIENTES EN UN OBJECT ARRAY
        private object[] CrearClientesArray()
        {
            string RazonSocialAlta = ClienteFormRazSoc.Text.ToUpper().Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");
            var ObtenerMaxId = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT MAX(Id) AS Id FROM Clientes");
            if (ObtenerMaxId.Count > 0)
            {
                foreach (var cliente in ObtenerMaxId)
                {
                    IdMaxCliente = cliente.Id + 1;
                }
            }
            else
            {
                IdMaxCliente = 1;
            }

            DataClienteServidor = new Clientes()
            {
                RazonSocial = (string.IsNullOrEmpty(ClienteFormRazSoc.Text)) ? "--" : /*ClienteFormRazSoc.Text.ToUpper()*/RazonSocialAlta,
                RFC = (string.IsNullOrEmpty(ClienteFormRFC.Text)) ? "--" : ClienteFormRFC.Text.ToUpper(),
                Calle = (string.IsNullOrEmpty(ClienteFormCalle.Text)) ? "--" : ClienteFormCalle.Text.ToUpper(),
                NumInt = (string.IsNullOrEmpty(ClienteFormNumInt.Text)) ? "--" : ClienteFormNumInt.Text.ToUpper(),
                NumExt = (string.IsNullOrEmpty(ClienteFormNumExt.Text)) ? "--" : ClienteFormNumExt.Text.ToUpper(),
                Colonia = (string.IsNullOrEmpty(ClienteFormColonia.Text)) ? "--" : ClienteFormColonia.Text.ToUpper(),
                Localidad = (string.IsNullOrEmpty(ClienteFormLocalidad.Text)) ? "--" : ClienteFormLocalidad.Text.ToUpper(),
                Municipio = (string.IsNullOrEmpty(ClienteFormMunicip.Text)) ? "--" : ClienteFormMunicip.Text.ToUpper(),
                Estado = (string.IsNullOrEmpty(ClienteFormEstado.Text)) ? "--" : ClienteFormEstado.Text.ToUpper(),
                Pais = (string.IsNullOrEmpty(ClienteFormPais.Text)) ? "--" : ClienteFormPais.Text.ToUpper(),
                CP = (string.IsNullOrEmpty(ClienteFormCP.Text)) ? 0 : Convert.ToInt32(ClienteFormCP.Text),
                NombreContacto = (string.IsNullOrEmpty(ClienteFormNomCont.Text)) ? "--" : ClienteFormNomCont.Text.ToUpper(),
                Telefono = (string.IsNullOrEmpty(ClienteFormTelef.Text)) ? 0 : Convert.ToDouble(ClienteFormTelef.Text),
                UsoCFDI = (string.IsNullOrEmpty(ClienteFormUsoCFDI.Text)) ? "--" : ClienteFormUsoCFDI.Text.ToUpper(),
                MetodoPago = (string.IsNullOrEmpty(ClienteFormMetPago.Text)) ? "--" : ClienteFormMetPago.Text.ToUpper(),
                FormaPago = (string.IsNullOrEmpty(ClienteFormFormPago.Text)) ? "--" : ClienteFormFormPago.Text.ToUpper(),
                DiasCredito = (string.IsNullOrEmpty(ClienteFormDiasCred.Text)) ? 0 : Convert.ToInt32(ClienteFormDiasCred.Text),
                LineaCredito = (string.IsNullOrEmpty(ClienteFormLinCred.Text)) ? 0 : Convert.ToDouble(ClienteFormLinCred.Text),
                Estatus = 1
            };

            CamposAltaClienteLista = new List<Campos>();
            Campos campoCliente = new Campos()
            {
                IdCliente = (IdSeleccCliente > 0) ? IdSeleccCliente : IdMaxCliente
            };
            foreach (var Elem in CamposClienteAltaGrid.Children)
            {
                if (Elem.AutomationId != "l4B3Lc4Mp0" && Elem.AutomationId != "l4B3LuB1C4c10N")
                {
                    if (Elem.AutomationId == "eN7R1c4Mp0")
                    {
                        Entry campo = (Entry)Elem;
                        campoCliente.NombreCampo = campo.Text.ToUpper();
                    }
                    else if (Elem.AutomationId == "eN7R1uB1C4c10N")
                    {
                        Entry ubicacion = (Entry)Elem;
                        campoCliente.Ubicacion = ubicacion.Text.ToUpper();

                        CamposAltaClienteLista.Add(campoCliente);
                        campoCliente = new Campos()
                        {
                            IdCliente = (IdSeleccCliente > 0) ? IdSeleccCliente : IdMaxCliente
                        };
                    }
                }
            }

            CorreosAltaClienteLista = new List<CorreosClientes>();
            CorreosClientes correoCliente = new CorreosClientes()
            {
                IdCliente = (IdSeleccCliente > 0) ? IdSeleccCliente : IdMaxCliente
            };
            foreach (var Elem in CorreosClienteAltaGrid.Children)
            {
                if (Elem.AutomationId != "l4B3LeMA1l")
                {
                    Entry email = (Entry)Elem;
                    correoCliente.Correo = email.Text;

                    CorreosAltaClienteLista.Add(correoCliente);
                    correoCliente = new CorreosClientes()
                    {
                        IdCliente = (IdSeleccCliente > 0) ? IdSeleccCliente : IdMaxCliente
                    };
                }
            }

            if (IdSeleccCliente > 0)
            {
                DataClienteServidor.Id = IdSeleccCliente;
                object[] clientesLista =
                {
                    (string.IsNullOrEmpty(ClienteFormRazSoc.Text)) ? "--" : /*ClienteFormRazSoc.Text.ToUpper()*/RazonSocialAlta,
                    (string.IsNullOrEmpty(ClienteFormRFC.Text)) ? "--" : ClienteFormRFC.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormCalle.Text)) ? "--" : ClienteFormCalle.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormNumInt.Text)) ? "--" : ClienteFormNumInt.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormNumExt.Text)) ? "--" : ClienteFormNumExt.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormColonia.Text)) ? "--" : ClienteFormColonia.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormLocalidad.Text)) ? "--" : ClienteFormLocalidad.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormMunicip.Text)) ? "--" : ClienteFormMunicip.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormEstado.Text)) ? "--" : ClienteFormEstado.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormPais.Text)) ? "--" : ClienteFormPais.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormCP.Text)) ? 0 : Convert.ToInt32(ClienteFormCP.Text),
                    (string.IsNullOrEmpty(ClienteFormNomCont.Text)) ? "--" : ClienteFormNomCont.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormTelef.Text)) ? 0 : Convert.ToDouble(ClienteFormTelef.Text),
                    (string.IsNullOrEmpty(ClienteFormUsoCFDI.Text)) ? "--" : ClienteFormUsoCFDI.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormMetPago.Text)) ? "--" : ClienteFormMetPago.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormFormPago.Text)) ? "--" : ClienteFormFormPago.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormDiasCred.Text)) ? 0 : Convert.ToInt32(ClienteFormDiasCred.Text),
                    (string.IsNullOrEmpty(ClienteFormLinCred.Text)) ? 0 : Convert.ToDouble(ClienteFormLinCred.Text),
                    IdSeleccCliente
                };
                return clientesLista;
            }
            else
            {
                DataClienteServidor.Id = IdMaxCliente;
                object[] clientesLista =
                {
                    IdMaxCliente,
                    (string.IsNullOrEmpty(ClienteFormRazSoc.Text)) ? "--" : /*ClienteFormRazSoc.Text.ToUpper()*/RazonSocialAlta,
                    (string.IsNullOrEmpty(ClienteFormRFC.Text)) ? "--" : ClienteFormRFC.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormCalle.Text)) ? "--" : ClienteFormCalle.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormNumInt.Text)) ? "--" : ClienteFormNumInt.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormNumExt.Text)) ? "--" : ClienteFormNumExt.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormColonia.Text)) ? "--" : ClienteFormColonia.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormLocalidad.Text)) ? "--" : ClienteFormLocalidad.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormMunicip.Text)) ? "--" : ClienteFormMunicip.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormEstado.Text)) ? "--" : ClienteFormEstado.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormPais.Text)) ? "--" : ClienteFormPais.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormCP.Text)) ? 0 : Convert.ToInt32(ClienteFormCP.Text),
                    (string.IsNullOrEmpty(ClienteFormNomCont.Text)) ? "--" : ClienteFormNomCont.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormTelef.Text)) ? 0 : Convert.ToDouble(ClienteFormTelef.Text),
                    (string.IsNullOrEmpty(ClienteFormUsoCFDI.Text)) ? "--" : ClienteFormUsoCFDI.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormMetPago.Text)) ? "--" : ClienteFormMetPago.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormFormPago.Text)) ? "--" : ClienteFormFormPago.Text.ToUpper(),
                    (string.IsNullOrEmpty(ClienteFormDiasCred.Text)) ? 0 : Convert.ToInt32(ClienteFormDiasCred.Text),
                    (string.IsNullOrEmpty(ClienteFormLinCred.Text)) ? 0 : Convert.ToDouble(ClienteFormLinCred.Text),
                    1
                };
                return clientesLista;
            }
        }

        // FUNCION QUE VALIDA LOS CAMPOS VACIOS DEL FORMULARIO DE CLIENTES
        private bool ValidarFormularioClientes()
        {
            bool correcto = true;
            // VALIDAR EL FORMULARIO DE TEXTOS
            if (string.IsNullOrEmpty(ClienteFormRazSoc.Text))
            {
                ClienteFormRazSoc.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Razón Social");
                correcto = false;
            }
            /*else if (string.IsNullOrEmpty(ClienteFormRFC.Text))
            {
                ClienteFormRFC.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque RFC");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormCalle.Text))
            {
                ClienteFormCalle.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Calle");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormNumExt.Text))
            {
                ClienteFormNumExt.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Num. Externo");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormColonia.Text))
            {
                ClienteFormColonia.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Colonia");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormLocalidad.Text))
            {
                ClienteFormLocalidad.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Localidad");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormMunicip.Text))
            {
                ClienteFormMunicip.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Municipio");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormEstado.Text))
            {
                ClienteFormEstado.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Estado");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormPais.Text))
            {
                ClienteFormPais.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Pais");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormCP.Text))
            {
                ClienteFormCP.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Codigo Postal");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormNomCont.Text))
            {
                ClienteFormNomCont.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Nombre Contacto");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormTelef.Text))
            {
                ClienteFormTelef.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Telefono");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormUsoCFDI.Text))
            {
                ClienteFormUsoCFDI.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Uso CFDI");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormMetPago.Text))
            {
                ClienteFormMetPago.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Método Pago");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormFormPago.Text))
            {
                ClienteFormFormPago.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Forma Pago");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormDiasCred.Text))
            {
                ClienteFormDiasCred.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Días Crédito");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(ClienteFormLinCred.Text))
            {
                ClienteFormLinCred.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Línea Crédito");
                correcto = false;
            }*/
            else
            {
                bool ErrCorreos = false, ErrCampos = false;
                // VALIDAR EL FORMULARIO DE CORREOS
                foreach (var Elem in CorreosClienteAltaGrid.Children)
                {
                    if (Elem.AutomationId != "l4B3LeMA1l")
                    {
                        Entry email = (Entry)Elem;
                        if (string.IsNullOrEmpty(email.Text))
                        {
                            ErrCorreos = true;
                        }
                    }
                }
                if (ErrCorreos)
                {
                    correcto = false;
                    DisplayAlert("Atención!", "No deje valores de CORREO en blanco", "Aceptar");
                }
                else
                {
                    // VALIDAR EL FORMULARIO DE CAMPOS
                    foreach (var Elem in CamposClienteAltaGrid.Children)
                    {
                        if (Elem.AutomationId != "l4B3Lc4Mp0" && Elem.AutomationId != "l4B3LuB1C4c10N")
                        {
                            if (Elem.AutomationId == "eN7R1c4Mp0")
                            {
                                Entry campo = (Entry)Elem;
                                if (string.IsNullOrEmpty(campo.Text))
                                {
                                    ErrCampos = true;
                                }
                            }
                            else if (Elem.AutomationId == "eN7R1uB1C4c10N")
                            {
                                Entry ubicacion = (Entry)Elem;
                                if (string.IsNullOrEmpty(ubicacion.Text))
                                {
                                    ErrCampos = true;
                                }
                            }
                        }
                    }
                    if (ErrCampos)
                    {
                        correcto = false;
                        DisplayAlert("Atención!", "No deje valores de CAMPOS en blanco", "Aceptar");
                    }
                }
            }

            return correcto;
        }

        // -------- [ REPORTE * FICHA CLIENTES ] --------
        // ************************************************
        // BOTON QUE CONTROLA LA GENERACION DEL REPORTE DE CLIENTE
        private async void Generar_FichaCliente(object sender, EventArgs e)
        {
            if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
            {
                int index = ClienteFormLista.SelectedIndex;
                if (index != -1)
                {
                    using (UserDialogs.Instance.Loading("Generando Ficha..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1000));
                        IdSeleccCliente = JsonConvert.DeserializeObject<ClientesAltaPicker>(JsonConvert.SerializeObject(ClienteFormLista.ItemsSource[index])).Id;
                        int idUsuario = 0, idCliente = 0;
                        var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
                        foreach (var usuario in VerifUsuario)
                        {
                            idUsuario = usuario.IdUsuario;
                        }
                        var clienteFicha = DependencyService.Get<ISQliteParams>().ConsultaClientes("SELECT * FROM Clientes WHERE Id = " + IdSeleccCliente.ToString());
                        foreach (var cliente in clienteFicha)
                        {
                            idCliente = cliente.Id;
                        }
                        string correosCliente = "";
                        var correosClienteLista = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM CorreosClientes WHERE IdCliente = " + idCliente.ToString());
                        foreach (var correos in correosClienteLista)
                        {
                            if (correosCliente != "")
                            {
                                correosCliente += ",";
                            }
                            correosCliente += correos.Correo;
                        }

                        try
                        {
                            Device.OpenUri(new Uri(DependencyService.Get<IAgroWS>().GenerarFichaCliente(JsonConvert.SerializeObject(clienteFicha), correosCliente, idUsuario)));
                        }
                        catch (Exception err)
                        {
                            await DisplayAlert("Error!", err.ToString(), "Aceptar");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error!", "Seleccione un Cliente", "Aceptar");
                }
            }
            else
            {
                await DisplayAlert("Atención!", "Requiere Internet para esta accion", "Aceptar");
            }
        }

        // LIMPIA EL GRID DE CAMPOS
        private void Limpiar_CamposCliente(object sender, EventArgs e)
        {
            CamposClienteAltaGrid.Children.Clear();
            CamposClienteAltaGrid.RowDefinitions.Clear();
            CamposClienteAltaGrid.ColumnDefinitions.Clear();
        }

        // LIMPIA EL GRID DE MAILS
        private void Limpiar_MailCliente(object sender, EventArgs e)
        {
            CorreosClienteAltaGrid.Children.Clear();
            CorreosClienteAltaGrid.RowDefinitions.Clear();
            CorreosClienteAltaGrid.ColumnDefinitions.Clear();
        }

        // AGREGAR NUEVOS CAMPOS PARA AGREGAR EMAIL
        private void AgregarNuevo_MailCliente(object sender, EventArgs e)
        {
            RowDefinition rowDef = new RowDefinition()
            {
                Height = GridLength.Auto
            };
            CorreosClienteAltaGrid.RowDefinitions.Add(rowDef);

            int rowIndx = 0;
            foreach (var Elem in CorreosClienteAltaGrid.Children)
            {
                if (Elem.AutomationId == "l4B3LeMA1l")
                {
                    rowIndx++;
                }
            }

            if (rowIndx == 0)
            {
                ColumnDefinition colDef1 = new ColumnDefinition() { Width = GridLength.Auto };
                ColumnDefinition colDef2 = new ColumnDefinition() { Width = GridLength.Star };
                CorreosClienteAltaGrid.ColumnDefinitions.Add(colDef1);
                CorreosClienteAltaGrid.ColumnDefinitions.Add(colDef2);
            }

            Label labelEmail = new Label()
            {
                Text = "\uf1fa",
                AutomationId = "l4B3LeMA1l",
                FontFamily = "fontawesomeicons.ttf#Regular",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            };
            Entry entryEmail = new Entry()
            {
                AutomationId = "eN7R1eMA1l",
                Placeholder = "Correo Electronico",
                PlaceholderColor = Color.DarkGray,
                TextColor = Color.FromRgb(0, 105, 92)
            };

            CorreosClienteAltaGrid.Children.Add(labelEmail, 0, rowIndx);
            CorreosClienteAltaGrid.Children.Add(entryEmail, 1, rowIndx);
        }

        // FUNCION PARA AGREGAR NUEVO CAMPO DINAMICAMENTE A LA ALTA DE CLIENTE
        private void NuevoCampo_Cliente(object sender, EventArgs e)
        {
            RowDefinition rowDef = new RowDefinition()
            {
                Height = GridLength.Auto
            };
            CamposClienteAltaGrid.RowDefinitions.Add(rowDef);

            int rowIndx = 0;
            foreach (var Elem in CamposClienteAltaGrid.Children)
            {
                if (Elem.AutomationId == "l4B3Lc4Mp0")
                {
                    rowIndx++;
                }
            }

            if (rowIndx == 0)
            {
                ColumnDefinition colDef1 = new ColumnDefinition() { Width = GridLength.Auto };
                ColumnDefinition colDef2 = new ColumnDefinition() { Width = GridLength.Star };
                ColumnDefinition colDef3 = new ColumnDefinition() { Width = GridLength.Auto };
                ColumnDefinition colDef4 = new ColumnDefinition() { Width = GridLength.Star };
                CamposClienteAltaGrid.ColumnDefinitions.Add(colDef1);
                CamposClienteAltaGrid.ColumnDefinitions.Add(colDef2);
                CamposClienteAltaGrid.ColumnDefinitions.Add(colDef3);
                CamposClienteAltaGrid.ColumnDefinitions.Add(colDef4);
            }

            Label labelCampo = new Label()
            {
                Text = "\uf1bb",
                AutomationId = "l4B3Lc4Mp0",
                FontFamily = "fontawesomeicons.ttf#Regular",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            };
            Entry entryCampo = new Entry()
            {
                AutomationId = "eN7R1c4Mp0",
                Placeholder = "Nombre Campo",
                PlaceholderColor = Color.DarkGray,
                TextColor = Color.FromRgb(0, 105, 92)
            };

            Label labelUbicacion = new Label()
            {
                Text = "\uf041",
                AutomationId = "l4B3LuB1C4c10N",
                FontFamily = "fontawesomeicons.ttf#Regular",
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                FontSize = 20
            };
            Entry entryUbicacion = new Entry()
            {
                AutomationId = "eN7R1uB1C4c10N",
                Placeholder = "Ubicacion",
                PlaceholderColor = Color.DarkGray,
                TextColor = Color.FromRgb(0, 105, 92)
            };

            CamposClienteAltaGrid.Children.Add(labelCampo, 0, rowIndx);
            CamposClienteAltaGrid.Children.Add(entryCampo, 1, rowIndx);
            CamposClienteAltaGrid.Children.Add(labelUbicacion, 2, rowIndx);
            CamposClienteAltaGrid.Children.Add(entryUbicacion, 3, rowIndx);
        }

        // --------------------- [ NUEVA BUSQUEDA DE ALTA DE CLIENTES (CONSULTA) ] ---------------------
        // FUNCION QUE MANEJA LA ACCION DE ESCRITURA DE BUSQUEDA DE UN CLIENTE
        private void ClienteBusq_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = ClienteBusq.Text.ToUpper();
                var busqResultado = ClientesBusq.Where(i => i.StartsWith(busqCliente)).ToList();
                ClienteBusq.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE CONCRETA LA BUSQUEDA DE CLIENTE
        private void ClienteBusq_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            int index = ClientesBusq.FindIndex(x => x.StartsWith(ClienteBusq.Text.ToUpper()));
            ClienteFormLista.SelectedIndex = index;
        }

        // ------------------------ [ NUEVA VALIDACION ] ------------------------
        // FUNCION QUE MANBEJA LAS SUGERENCIAS AL ESCRIBIR LA RAZON SOCIAL DE UN NUEVO CLIENTE
        private void ClienteFormRazSoc_TextChanged(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.CheckCurrent())
            {
                var busqCliente = ClienteFormRazSoc.Text.ToUpper();
                var busqResultado = ClientesValidar.Where(i => i.StartsWith(busqCliente)).ToList();
                ClienteFormRazSoc.ItemsSource = busqResultado;
            }
        }

        // FUNCION QUE EJECUTA UNA ACCION PARA EVITAR REPETICIONES EN UN NUEVO CLIENTE
        private void ClienteFormRazSoc_SuggestionChosen(object sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            ClienteFormRazSoc.Text = "";
            ClienteFormRazSoc.Unfocus();
            DisplayAlert("Atención!", "Las sugerencias mostradas NO son seleccionables.\nLe recomendamos no repetir la Razón social de los clientes.", "Aceptar");
        }
    }
}