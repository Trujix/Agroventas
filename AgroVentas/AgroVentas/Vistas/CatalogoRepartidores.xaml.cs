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
    public partial class CatalogoRepartidores : ContentPage
    {
        public CatalogoRepartidores()
        {
            InitializeComponent();

            LlenarRepartidoresFormLista();
        }

        // ------------------------------------------------------------------------------------------
        // ---------------- VARIABLES GLOBALES PUBLICAS ----------------------
        public int IdRepartidorSelecc = 0;
        private class RepartidorAltaPicker
        {
            public int Id { get; set; }
            public string NombreRepartidor { get; set; }
        }
        private List<RepartidorAltaPicker> RepartidorAltaLista;
        private BDPendientes RepartidoresDataWS;
        private Repartidores DataRepartidorServidor;
        // ----------------------------------------

        // FUNCION INICIAL QUE HACE UN LLENADO DE LISTA DE REPARTIDORES
        private async void LlenarRepartidoresFormLista()
        {
            var repartidoresQuery = DependencyService.Get<ISQliteParams>().ConsultaRepartidores("SELECT * FROM Repartidores WHERE Estatus = 1");
            RepartidorAltaLista = new List<RepartidorAltaPicker>();
            foreach (var repartidor in repartidoresQuery)
            {
                RepartidorAltaLista.Add(new RepartidorAltaPicker
                {
                    Id = repartidor.Id,
                    NombreRepartidor = repartidor.NombreRepartidor
                });
            }
            RepartidorFormLista.ItemsSource = RepartidorAltaLista;
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            UserDialogs.Instance.HideLoading();
        }


        // ----------- FUNCION DE BOTONES DEL FORMULARIO
        // NUUEVO REPARTIDOR
        private void Nuevo_Repartidor(object sender, EventArgs e)
        {
            LimpiarCamposRepartidoresForm();
            RepartidorNombre.Focus();
            RepartidorFormLista.SelectedItem = string.Empty;
            IdRepartidorSelecc = 0;
        }

        // GUARDAR REPARTIDOR
        private async void Guardar_Repartidor(object sender, EventArgs e)
        {
            if (ValidarFormularioRepartidores())
            {
                using (UserDialogs.Instance.Loading("Guardando Repartidor..."))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    object[] DataRepartidor = CrearRepartidoresArray();
                    RepartidoresDataWS = new BDPendientes()
                    {
                        Tabla = "Repartidores",
                        Accion = "Nuevo",
                        Data = JsonConvert.SerializeObject(DataRepartidorServidor)
                    };
                    string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(RepartidoresDataWS));
                    string GuardarRepartidor = DependencyService.Get<ISQliteParams>().QueryMaestra(5, "INSERT INTO Repartidores (Id,NombreRepartidor,Correo,Estatus) VALUES (?,?,?,?)", DataRepartidor);
                    if (GuardarRepartidor == "true")
                    {
                        await DisplayAlert("Éxito!", "Repartidor Guardado correctamente", "Aceptar");
                        LimpiarCamposRepartidoresForm();
                        LlenarRepartidoresFormLista();
                    }
                    else
                    {
                        await DisplayAlert("Error!", GuardarRepartidor, "Aceptar");
                    }
                }
            }
        }
        // BORRAR REPARTIDOR
        private async void Borrar_Repartidor(object sender, EventArgs e)
        {
            int index = RepartidorFormLista.SelectedIndex;
            if (index != -1)
            {
                RepartidorAltaPicker repartidorInfo = JsonConvert.DeserializeObject<RepartidorAltaPicker>(JsonConvert.SerializeObject(RepartidorFormLista.ItemsSource[index]));
                var borrar = await DisplayAlert("Atencion!", "¿Desea borrar a " + repartidorInfo.NombreRepartidor + "?", "Si", "Cancelar");
                if (borrar)
                {
                    using (UserDialogs.Instance.Loading("Eliminando Repartidor..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        DataRepartidorServidor = new Repartidores()
                        {
                            Id = repartidorInfo.Id,
                            Estatus = 0
                        };
                        RepartidoresDataWS = new BDPendientes()
                        {
                            Tabla = "Repartidores",
                            Accion = "Borrar",
                            Data = JsonConvert.SerializeObject(DataRepartidorServidor)
                        };
                        string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(RepartidoresDataWS));
                        object[] idBorrar = { 0, repartidorInfo.Id };
                        string BorrarCliente = DependencyService.Get<ISQliteParams>().QueryMaestra(2, "UPDATE Repartidores SET Estatus=? WHERE Id=?", idBorrar);
                        if (BorrarCliente == "true")
                        {
                            LimpiarCamposRepartidoresForm();
                            LlenarRepartidoresFormLista();
                            IdRepartidorSelecc = 0;
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
                await DisplayAlert("Error!", "Seleccione un Repartidor", "Aceptar");
            }
        }

        // ------------ FUNCIONES CON CATALOGOS ------------
        // FUNCION QUE LIMPIA LOS CAMPOS DEL FORMULARIO ALTA DE CLIENTE
        private void LimpiarCamposRepartidoresForm()
        {
            RepartidorNombre.Text = "";
            RepartidorCorreo.Text = "";
        }

        // FUNCION QUE EMPAQUETA LOS VALORES DEL FORMULARIO DE CLIENTES EN UN OBJECT ARRAY
        private object[] CrearRepartidoresArray()
        {
            int IdMaxRepartidor = 0;
            var ObtenerMaxId = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT MAX(Id) AS Id FROM Repartidores");
            if (ObtenerMaxId.Count > 0)
            {
                foreach (var cliente in ObtenerMaxId)
                {
                    IdMaxRepartidor = cliente.Id + 1;
                }
            }
            else
            {
                IdMaxRepartidor = 1;
            }

            DataRepartidorServidor = new Repartidores()
            {
                NombreRepartidor = (string.IsNullOrEmpty(RepartidorNombre.Text.Trim())) ? "--" : RepartidorNombre.Text.Trim().ToUpper(),
                Correo = (string.IsNullOrEmpty(RepartidorCorreo.Text.Trim())) ? "--" : RepartidorCorreo.Text.Trim().ToUpper(),
                Estatus = 1
            };

            DataRepartidorServidor.Id = IdMaxRepartidor;
            object[] repartidorLista =
            {
                IdMaxRepartidor,
                (string.IsNullOrEmpty(RepartidorNombre.Text.Trim())) ? "--" : RepartidorNombre.Text.Trim().ToUpper(),
                (string.IsNullOrEmpty(RepartidorCorreo.Text.Trim())) ? "--" : RepartidorCorreo.Text.Trim().ToUpper(),
                1
            };
            return repartidorLista;
        }

        // FUNCION QUE VALIDA LOS CAMPOS VACIOS DEL FORMULARIO DE REPARTIDORES
        private bool ValidarFormularioRepartidores()
        {
            bool correcto = true;
            // VALIDAR EL FORMULARIO DE TEXTOS
            if (string.IsNullOrEmpty(RepartidorNombre.Text))
            {
                RepartidorNombre.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Nombre Repartidor");
                correcto = false;
            }
            else if (string.IsNullOrEmpty(RepartidorCorreo.Text))
            {
                RepartidorCorreo.Focus();
                DependencyService.Get<IAlertas>().MsgCorto("Coloque Correo");
                correcto = false;
            }

            return correcto;
        }

        // BOTON QUE REENVIA EL CORREO AL REPARTIDOR (POR SI AUN NO LE HA LLEGADO)
        private async void Reenviar_CorreoRepartidor(object sender, EventArgs e)
        {
            if (DependencyService.Get<IAgroWS>().VerificarInternetConeccion())
            {
                int index = RepartidorFormLista.SelectedIndex;
                if (index != -1)
                {
                    using (UserDialogs.Instance.Loading("Generando Correo..."))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500));
                        IdRepartidorSelecc = JsonConvert.DeserializeObject<RepartidorAltaPicker>(JsonConvert.SerializeObject(RepartidorFormLista.ItemsSource[index])).Id;
                        DataRepartidorServidor = new Repartidores()
                        {
                            Id = IdRepartidorSelecc
                        };
                        RepartidoresDataWS = new BDPendientes()
                        {
                            Tabla = "Repartidores",
                            Accion = "Correo",
                            Data = JsonConvert.SerializeObject(DataRepartidorServidor)
                        };
                        string WS = DependencyService.Get<IAgroWS>().EnviarDataWS(JsonConvert.SerializeObject(RepartidoresDataWS));
                        if (WS == "true")
                        {
                            await DisplayAlert("Éxito!", "Correo Enviado a Repartidor", "Aceptar");
                        }
                        else
                        {
                            await DisplayAlert("Error!", "Ocurrió un problema al enviar correo al Repartidor", "Aceptar");
                        }

                    }
                }
                else
                {
                    await DisplayAlert("Error!", "Seleccione un Repartidor", "Aceptar");
                }
            }
            else
            {
                await DisplayAlert("Atención!", "Requiere Internet para esta accion", "Aceptar");
            }
        }
    }
}