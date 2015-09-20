#region Header

// Criado por eduvm
// Data: 19/09/2015
// Solução: Controle de Etiquetas
// Projeto: Controle de Etiquetas
// Arquivo: CadDestino.xaml.cs
// =========================================
// Última alteração: 20/09/2015

#endregion

#region Usings

using System.Collections.Generic;
using System.Windows;

using Controle_de_Etiquetas.Helpers;

#endregion

namespace Controle_de_Etiquetas {

    /// <summary>
    ///     Interaction logic for CadDestino.xaml
    /// </summary>
    public partial class CadDestino : Window {
        #region Construtor

        public CadDestino(string operacao, string id) {
            // Inicializa componentes da janela
            InitializeComponent();

            // Define o tipo de operação com o parametro recebido
            TipoOperacao = operacao;

            // Define o id com o parametro recebido
            CodID = id;

            if (TipoOperacao == "alterar") {
                // Atualiza campo ID
                tbCodigo.Text = CodID;

                // Cria novo objeto de acesso ao banco de dados
                var objDB = new DatabaseHelper();

                // Define codigo SQL
                var SQL = string.Format("SELECT c_nome FROM dados.destino WHERE id = '{0}'", CodID);

                // Salvo titulo destino na variavel
                var TituloDestino = objDB.ExecuteScalar(SQL);

                tbDestino.Text = TituloDestino;
            }
        }

        #endregion Construtor

        #region Variaveis

        private string TipoOperacao;

        private string CodID;

        #endregion Variáveis

        #region Métodos

        /// <summary>
        ///     Trata da alteração do destino
        /// </summary>
        private void AlterarDestino() {
            // Valida se o campo destino está vazio
            if (string.IsNullOrEmpty(tbDestino.Text)) {
                // Informa ao usuário que é necessário preencher o campo destino
                MessageBox.Show("Você não preencheu o campo destino");
            }

            //
            else {
                // Cria objeto de acesso ao banco de dados
                var objAltDestino = new DatabaseHelper();

                // Criamos o dicionario de dados com as chaves e valores
                var dctDados = new Dictionary<string, string>();

                // Incluimos no dicionario o campo de destino
                dctDados.Add("c_nome", tbDestino.Text);

                // Valida se consegue inserir informação no banco de dados
                if (objAltDestino.Update("dados.destino", dctDados, "id =" + CodID)) {
                    // Mostra mensagem de sucesso
                    MessageBox.Show("Destino alterado com sucesso");
                }
                else {
                    // Mostra mensagem de erro
                    MessageBox.Show("Ocorreu erro ao tentar alterar o destino\nContate o seu administrador");
                }

                // Fecha a janela
                Close();
            }
        }

        /// <summary>
        ///     Trata da inclusão do destino
        /// </summary>
        private void IncluirDestino() {
            // Valida se o campo destino está vazio
            if (string.IsNullOrEmpty(tbDestino.Text)) {
                // Informa ao usuário que é necessário preencher o campo destino
                MessageBox.Show("Você não preencheu o campo destino");
            }

            // Senão estiver vazio
            else {
                // Cria objeto de acesso ao banco de dados
                var objAddDestino = new DatabaseHelper();

                // Criamos o dicionario de dados com as chaves e valores
                var dctDados = new Dictionary<string, string>();

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

        #endregion Métodos

        #region Botões

        private void btnCancelar_Click(object sender, RoutedEventArgs e) {
            // Fecha a janela
            Close();
        }

        private void btnSalvar_Click(object sender, RoutedEventArgs e) {
            // Verifico o tipo de operação
            switch (TipoOperacao) {
                case "incluir":

                    // Chama método de incluir
                    IncluirDestino();
                    break;

                case "alterar":

                    AlterarDestino();
                    break;
            }
        }

        #endregion Botões
    }

}