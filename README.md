# README #

Programa para controle de saída de funcionário.

### Utilização ###

* Controlar a saída e retorno de funcionários
* Versão: 0.0.1

### Funcionamento ###

* Recebimento dos códigos
Os códigos de barras podem ser digitados diretamente no campo
Podem ser recebidos por Sockets (Pacotes TCP)
Podem ser lidos com um leitor de código de barras

* Funcionamento
Programa recebe o primeiro código de controle.
Ex.: Código de saída do funcionário

Recebe o código do funcionário

Recebe o código do cliente/local ao qual irá visitar.
- Podem ser encadeados vários locais aos quais o funcionário irá escaneando vários códigos em sequência

Recebe o código de controle para finalizar a operação

O sistema então irá gravar dos dados em uma banco de dados e atualizará o grid com estas informações