using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AgroVentas.Vistas;
using Acr.UserDialogs;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace AgroVentas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MenuMaster : ContentPage
    {
        public ListView ListView;

        public MenuMaster()
        {
            InitializeComponent();

            BindingContext = new MenuMasterViewModel();
            ListView = MenuItemsListView;

            var VerifUsuario = DependencyService.Get<ISQliteParams>().ConsultaUsuario("SELECT * FROM Usuario WHERE IdString = 'u5U4r10+2236'");
            foreach (var usuario in VerifUsuario)
            {
                AgenteNombreMenu.Text = usuario.Nombre;
            }
            UserDialogs.Instance.HideLoading();
        }

        class MenuMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<MenuMenuItem> MenuItems { get; set; }

            public MenuMasterViewModel()
            {
                MenuItems = new ObservableCollection<MenuMenuItem>(new[]
                {
                    new MenuMenuItem { Id = 0, Title = "Orden Pedido", Icon = "\uf291", TargetType = typeof(OrdenPedidos) },
                    new MenuMenuItem { Id = 1, Title = "Cotizacion", Icon = "\uf0d6", TargetType = typeof(Cotizacion) },
                    //new MenuMenuItem { Id = 2, Title = "Devoluciones", Icon = "\uf05e" },
                    new MenuMenuItem { Id = 3, Title = "Entregas", Icon = "\uf0d1", TargetType = typeof(Entregas) },
                    new MenuMenuItem { Id = 4, Title = "Catalogo Clientes", Icon = "\uf007", TargetType = typeof(CatalogoClientes) },
                    new MenuMenuItem { Id = 5, Title = "Catalogo Productos", Icon = "\uf02a", TargetType = typeof(CatalogoProductos) },
                    new MenuMenuItem { Id = 6, Title = "Catalogo Repartidores", Icon = "\uf2bb", TargetType = typeof(CatalogoRepartidores) },
                    new MenuMenuItem { Id = 7, Title = "Configuracion", Icon = "\uf013", TargetType = typeof(Configuracion) },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}