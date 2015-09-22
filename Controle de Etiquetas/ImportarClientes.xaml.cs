// Criado por eduardo
// Data: 22/09/2015
// Solução: Controle de Etiquetas
// Projeto:Controle de Etiquetas
// Arquivo: ImportarClientes.xaml.cs
// =========================================
// Última alteração: 22/09/2015

#region Usings

using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows;

using Controle_de_Etiquetas.Helpers;

#endregion

namespace Controle_de_Etiquetas {

    /// <summary>
    ///     Interaction logic for ImportarClientes.xaml
    /// </summary>
    public partial class ImportarClientes : Window {

        public ImportarClientes() {
            InitializeComponent();

            // Instacia Thread passando a ThreadStart
            var ImportThread = new Thread(ImportClientes);

            // Seto thread como background
            ImportThread.IsBackground = true;

            // Inicia a Thread
            ImportThread.Start();
        }

        /// <summary>
        ///     Método que faz a importação dos clientes da base Mastere salva na base etiquetas
        /// </summary>
        private void ImportClientes() {

            // Gera novo objetode dados selecionando a base Master
            var objMaster = new DatabaseHelper("master");

            // Gera novo query SQL
            var SQL = "SELECT i_cdcliente, c_nomefantas FROM dados.cliente WHERE c_nomefantas <> '' AND i_cdcidade = '1' ORDER BY i_cdcliente";

            // Salva resultado do SQL em um Data Table
            var dResult = objMaster.GetDataTable(SQL);

            // Seta valormaximo na progress barr
            Dispatcher.Invoke(() => {
                pbStatus.Maximum = dResult.Rows.Count;

            });

            // Cria laço para percorrer todos os registros do DataTable
            foreach (DataRow row in dResult.Rows) {



                // Atualiza progressbar
                Dispatcher.Invoke(() => {

                    tbInfo.Text = "Atualizando clientes " + pbStatus.Value + " de " + dResult.Rows.Count;
                    pbStatus.Value++;

                });

                // Crio novo objeto de acesso ao banco e dados
                var objDBTest = new DatabaseHelper();

                // Defino query SQL
                var SQLTest = string.Format("SELECT id FROM dados.cliente WHERE i_cdcliente = '{0}' ", row["i_cdcliente"]);

                // Salva resultado do SQL em string
                var sResult = objDBTest.ExecuteScalar(SQLTest);

                // Verifica se o resultado for vazio ou nulo (não não existe o registro na base)
                if (string.IsNullOrEmpty(sResult)) {

                    // Gera novo objeto de acesso ao banco de dados
                    var objDB = new DatabaseHelper("etiquetas");

                    // Cria novo Dicionario de dados com par de chave/valor para inserir no banco de dados
                    var dctDados = new Dictionary<string, string>();

                    // Adiciona campos no dicionário
                    dctDados.Add("i_cdcliente", row["i_cdcliente"].ToString());
                    dctDados.Add("c_nomefantas", row["c_nomefantas"].ToString());

                    // Verifica se não consegue adicionar os dados na base
                    if (!objDB.Insert("dados.cliente", dctDados)) {

                        // Apresenta mensagem de erro
                        MessageBox.Show("Ocorreu um erro ao tentar fazer a atualização do banco deddados\nVocê estaráutilizando uma base desatualizada, que pode não conter todos os clientes\nPor favor, informe ao Administrador");

                        // Sai do loop
                        break;
                    }

                }

            }


            // Finaliza a janela de importação
            Dispatcher.Invoke(() =>
            {

                Close();

            });

        }

    }

}