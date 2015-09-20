using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

using Controle_de_Etiquetas.Helpers;

namespace Controle_de_Etiquetas {

    /// <summary>
    /// Interaction logic for CadDestino.xaml
    /// </summary>
    public partial class CadDestino : Window {
        #region Construtor

        public CadDestino() {
            // Inicializa componentes da janela
            InitializeComponent();
        }

        #endregion Construtor

        #region Métodos

        #endregion Métodos

        #region Botões

        private void btnCancelar_Click(object sender, RoutedEventArgs e) {
            // Fecha a janela
            Close();
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e) {
            // Valida se o campo destino está vazio
            if (string.IsNullOrEmpty(tbDestino.Text)) {
                // Informa ao usuário que é necessário preencher o campo destino
                MessageBox.Show("Você não preencheu o campo destino");
            }

            // Senão estiver vazio
            else {
                // Cria objeto de acesso ao banco de dados
                DatabaseHelper objAddDestino = new DatabaseHelper();

                // Criamos o dicionario de dados com as chaves e valores
                Dictionary<string, string> dctDados = new Dictionary<string, string>();

                // Incluimos no dicionario o campo de destino
                dctDados.Add("c_nome", tbDestino.Text);

                // Valida se consegue inserir informação no banco de dados
                if (objAddDestino.Insert("dados.destino", dctDados)) {
                    // Mostra mensagem de sucesso
                    MessageBox.Show("Destino incluído com sucesso");
                }
                else {
                    // Mostra mensagem de erro
                    MessageBox.Show("Ocorreu erro ao tentar incluir o destino\nContate o seu administrador");
                }

                // Fecha a janela
                Close();
            }
        }

        #endregion Botões
    }

}