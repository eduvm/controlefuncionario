// Criado por eduardo
// Data: 21/09/2015
// Solução: Controle de Etiquetas
// Projeto:Controle de Etiquetas
// Arquivo: CadCliente.xaml.cs
// =========================================
// Última alteração: 23/09/2015

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

using Controle_de_Etiquetas.Helpers;

#endregion

namespace Controle_de_Etiquetas {

    /// <summary>
    ///     Interaction logic for CadCliente.xaml
    /// </summary>
    public partial class CadCliente : Window {
        #region Construtor

        public CadCliente(string operacao, string id) {
            // Inicializa componentes da janela
            InitializeComponent();

            // Define o tipo de operação com o parametro recebido
            TipoOperacao = operacao;

            // Define o id com o parametro recebido
            CodID = id;

            if (TipoOperacao == "alterar") {
                CarregaCliente();
            }
        }

        private void CarregaCliente() {

            try {

                // Atualiza campo ID
                tbCodigo.Text = CodID;

                // Gera novo objeto de conexao ao banco de dados
                var objCliente = new DatabaseHelper("etiquetas");

                // Define SQL Query
                var query = string.Format("SELECT c_nomefantas, i_cdcliente FROM dados.cliente WHERE id = '{0}'", CodID);

                // Executa a query
                var dt = objCliente.GetDataTable(query);

                // Faz for para preencher a lista de pessoas
                foreach (DataRow row in dt.Rows) {
                    tbCpdCli.Text = row["i_cdcliente"].ToString();
                    tbNomeFantasia.Text = row["c_nomefantas"].ToString();

                }

            }

                // Trata excessão
            catch (Exception fail) {
                // Seta mensagem de erro
                var error = "O seguinte erro ocorreu:\n\n";

                // Anexa mensagem de erro na mensagem
                error += fail.Message + "\n\n";

                // Apresenta mensagem na tela
                MessageBox.Show(error);

                // Fecha o formulário
                Close();
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
        private void AlterarCliente() {

            // Verifica validação dos dados
            if (ValidaDados()) {

                // Cria objeto de acesso ao banco de dados
                var objAltCliente = new DatabaseHelper();

                // Criamos o dicionario de dados com as chaves e valores
                var dctDados = new Dictionary<string, string>();

                // Incluimos no dicionario o campo de destino
                dctDados.Add("i_cdcliente", tbCpdCli.Text);
                dctDados.Add("c_nomefantas", tbNomeFantasia.Text);

                // Valida se consegue inserir informação no banco de dados
                if (objAltCliente.Update("dados.cliente", dctDados, "id =" + CodID)) {
                    // Mostra mensagem de sucesso
                    MessageBox.Show("Cliente alterado com sucesso");
                }

                else {
                    // Mostra mensagem de erro
                    MessageBox.Show("Ocorreu erro ao tentar alterar o cliente\nContate o seu administrador");
                }

                // Fecha a janela
                Close();

            }

        }

        /// <summary>
        ///     Trata da inclusão do destino
        /// </summary>
        private void IncluirCliente() {

            // Verifica validação dos campos
            if (ValidaDados()) {

                // Cria objeto de acesso ao banco de dados
                var objAddDestino = new DatabaseHelper();

                // Criamos o dicionario de dados com as chaves e valores
                var dctDados = new Dictionary<string, string>();

                // Incluimos no dicionario o campo de destino
                dctDados.Add("i_cdcliente", tbCpdCli.Text);
                dctDados.Add("c_nomefantas", tbNomeFantasia.Text);

                // Valida se consegue inserir informação no banco de dados
                if (objAddDestino.Insert("dados.cliente", dctDados)) {
                    // Mostra mensagem de sucesso
                    MessageBox.Show("Cliente incluído com sucesso");
                }
                else {
                    // Mostra mensagem de erro
                    MessageBox.Show("Ocorreu erro ao tentar incluir o cliente\nContate o seu administrador");
                }

                // Fecha a janela
                Close();
            }
        }

        /// <summary>
        ///     Método responsável por validar se os campos estão vazios antes de inserir no banco de dados
        /// </summary>
        /// <returns></returns>
        private bool ValidaDados() {

            // Valida o campo CPD
            if (string.IsNullOrEmpty(tbCpdCli.Text)) {

                return false;
            }

            // Valida o campo Nome Fantasia
            if (string.IsNullOrEmpty(tbNomeFantasia.Text)) {

                return false;

            }

            return true;
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
                    IncluirCliente();
                    break;

                case "alterar":

                    AlterarCliente();
                    break;
            }
        }

        #endregion Botões
    }

}