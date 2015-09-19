using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Controle_de_Etiquetas.Helpers;

namespace Controle_de_Etiquetas {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

#region Construtores
        public MainWindow() {

            // Inicializa os componentes da janela
            InitializeComponent();

            // Chama método que faz o carregamento do cadastro de funcionários
            CarregaFuncionarios();

            // Chama método que faz o carregamento do cadsatro de destinos
            CarregaDestinos();

        }

#endregion Construtores

        #region Métodos

        /// <summary>
        /// Método responsável por carregar os funcionários cadastrados no banco de dados
        /// </summary>
        private void CarregaFuncionarios() {

            // Limpa registros do dataGrid
            dgFuncionarios.ItemsSource = null;
            
            // Cria objeto de acesso ao banco de dados
            DatabaseHelper objCarregaFuncionario  = new DatabaseHelper();

            // Comando SQL
            var SQL = "SELECT id, c_funcionario FROM dados.funcionario WHERE b_deletado = false ORDER BY id";

            // Pega DataTable com resultado do SQL
            DataTable result = objCarregaFuncionario.GetDataTable(SQL);

            // Seta item source do DataGrid
            dgFuncionarios.ItemsSource = result.DefaultView;

        }

        /// <summary>
        /// Método responsável por carregar os funcionários cadastrados no banco de dados
        /// </summary>
        private void CarregaDestinos() {

            // Limpa registros do dataGrid
            dgDestino.ItemsSource = null;

            // Cria objeto de acesso ao banco de dados
            DatabaseHelper objCarregaDestino = new DatabaseHelper();

            // Comando SQL
            var SQL = "SELECT id, c_nome FROM dados.destino WHERE b_deletado = false ORDER BY id";

            // Pega DataTable com resultado do SQL
            DataTable result = objCarregaDestino.GetDataTable(SQL);

            // Seta item source do DataGrid
            dgDestino.ItemsSource = result.DefaultView;

        }


        #endregion Métodos
    }
}
