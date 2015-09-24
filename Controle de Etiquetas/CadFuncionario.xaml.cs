#region Header

// Criado por eduvm
// Data: 20/09/2015
// Solução: Controle de Etiquetas
// Projeto: Controle de Etiquetas
// Arquivo: CadFuncionario.xaml.cs
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
    ///     Interaction logic for CadFuncionario.xaml
    /// </summary>
    public partial class CadFuncionario : Window {
        #region Construtor

        public CadFuncionario(string operacao, string id) {
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
                var SQL = string.Format("SELECT c_funcionario FROM dados.funcionario WHERE id = '{0}'", CodID);

                // Salvo titulo destino na variavel
                var TituloFuncionario = objDB.ExecuteScalar(SQL);

                // Atualiza campo nome
                tbFuncionario.Text = TituloFuncionario;
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
        private void AlterarFuncionario() {
            // Valida se o campo destino está vazio
            if (string.IsNullOrEmpty(tbFuncionario.Text)) {
                // Informa ao usuário que é necessário preencher o campo destino
                MessageBox.Show("Você não preencheu o campo nome");
            }

            //
            else {
                // Cria objeto de acesso ao banco de dados
                var objAltDestino = new DatabaseHelper();

                // Criamos o dicionario de dados com as chaves e valores
                var dctDados = new Dictionary<string, string>();

                // Incluimos no dicionario o campo de destino
                dctDados.Add("c_funcionario", tbFuncionario.Text);

                // Valida se consegue inserir informação no banco de dados
                if (objAltDestino.Update("dados.funcionario", dctDados, "id =" + CodID)) {
                    // Mostra mensagem de sucesso
                    MessageBox.Show("Funcionário alterado com sucesso");
                }
                else {
                    // Mostra mensagem de erro
                    MessageBox.Show("Ocorreu erro ao tentar alterar o funcionário\nContate o seu administrador");
                }

                // Fecha a janela
                Close();
            }
        }

        /// <summary>
        ///     Trata da inclusão do destino
        /// </summary>
        private void IncluirFuncionario() {
            // Valida se o campo destino está vazio
            if (string.IsNullOrEmpty(tbFuncionario.Text)) {
                // Informa ao usuário que é necessário preencher o campo destino
                MessageBox.Show("Você não preencheu o campo nome");
            }

            // Senão estiver vazio
            else {
                // Cria objeto de acesso ao banco de dados
                var objAddDestino = new DatabaseHelper();

                // Criamos o dicionario de dados com as chaves e valores
                var dctDados = new Dictionary<string, string>();

                // Incluimos no dicionario o campo de destino
                dctDados.Add("c_funcionario", tbFuncionario.Text);

                // Valida se consegue inserir informação no banco de dados
                if (objAddDestino.Insert("dados.funcionario", dctDados)) {
                    // Mostra mensagem de sucesso
                    MessageBox.Show("Funcionário incluído com sucesso");
                }
                else {
                    // Mostra mensagem de erro
                    MessageBox.Show("Ocorreu erro ao tentar incluir o funcionário\nContate o seu administrador");
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
                    IncluirFuncionario();
                    break;

                case "alterar":

                    AlterarFuncionario();
                    break;
            }
        }

        #endregion Botões
    }

}