// Criado por eduardo
// Data: 23/09/2015
// Solução: Controle de Etiquetas
// Projeto:Controle de Etiquetas
// Arquivo: Controle.cs
// =========================================
// Última alteração: 23/09/2015

namespace Controle_de_Etiquetas.Controles {

    /// <summary>
    ///     CLasse responsável por armazenar todas as informações do cadastro de controle
    /// </summary>
    public class Controle {

        /// <summary>
        ///     Propriedade com o ID do controle
        /// </summary>
        public string Id {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com o nome do funcionário
        /// </summary>
        public string NomeFuncionario {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com o ID do funcionário
        /// </summary>
        public string IdFuncionario {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com o Nome do Cliente
        /// </summary>
        public string NomeCliente {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com o ID do cliente
        /// </summary>
        public string IdCliente {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com a Data da Saída
        /// </summary>
        public string DataSaida {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com a Hora da Saída
        /// </summary>
        public string HoraSaida {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com a Data da Chegada
        /// </summary>
        public string DataChegada {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade com a Hora da Chegada
        /// </summary>
        public string HoraChegada {
            get;
            set;
        }

        /// <summary>
        ///     Propriedade de controle se o registro é uma saída ou chegada
        /// </summary>
        public bool FlagFechado {
            get;
            set;
        }

    }

}