// Criado por eduardo
// Data: 23/09/2015
// Solução: Controle de Etiquetas
// Projeto:Controle de Etiquetas
// Arquivo: BarCodeControle.cs
// =========================================
// Última alteração: 23/09/2015

#region Usings

using System.Collections.ObjectModel;

#endregion

namespace Controle_de_Etiquetas.Controles {

    public class BarCodeControle : ObservableCollection<Controle> {

        public string CodigoInicial {
            get;
            set;
        }

        public string CodFinal {
            get;
            set;
        }

        public string CodFuncionario {
            get;
            set;
        }

    }

}