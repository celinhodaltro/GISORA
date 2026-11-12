# 0007 - Autenticacao e autorizacao

## Status

Aceito

## Contexto

A aplicacao hoje opera local-first, sem backend, com dados no `localStorage` do dispositivo. Nao existe fronteira de confianca: qualquer pessoa com acesso ao navegador le e altera os dados. O produto e de administracao municipal e manipula dado pessoal (CPF/CNPJ), historico cadastral e informacao fiscal.

Duas saidas ruins precisam ser evitadas:

1. Login ficticio no cliente, que aparenta seguranca e nao protege nada.
2. Adiar o tema para depois da API, deixando permissoes espalhadas por telas.

## Decisao

- `Identity` e um bounded context com `User`, `Role`, `Permission` e `Policy`.
- A autorizacao e um servico de aplicacao (`IAuthorizationService`), chamado pelos handlers. Componente nao autoriza nada: apenas oculta acao.
- Senha, quando existir, e hash com sal (PBKDF2) via port `IPasswordHasher`. Nunca texto claro.
- Nenhum token, senha ou segredo vai para `localStorage`. Sessao vive em memoria.
- Enquanto nao houver backend, a aplicacao declara **modo demonstracao**: os dados locais nao sao protegidos e o produto nao deve ser usado com dado pessoal real.
- A fronteira de seguranca definitiva e o servidor. Quando a API existir, autorizacao e auditoria passam a ser aplicadas la, e o cliente apenas reflete permissoes.

## Alternativas consideradas

- Autenticacao local com senha "de verdade" no navegador: da falsa sensacao de seguranca, porque o atacante tem o proprio banco local. Rejeitada como garantia; mantida apenas como ensaio de UX, sem dado real.
- Sem `Identity` ate a API: rejeitada; permissoes e auditoria precisam nascer no dominio, senao viram `if` espalhado em componente.
- Login social/OIDC agora: sem servidor para trocar codigo, nao ha ganho e ha dependencia externa prematura.

## Consequencias

- Toda tela sensivel precisa de guarda de rota e o handler correspondente precisa recusar chamada sem permissao (teste obrigatorio).
- A UI deve exibir aviso de modo demonstracao enquanto nao houver backend.
- Auditoria registra usuario, acao, entidade e instante; no cliente, e registro local e verificavel apenas como trilha de uso.
- O modelo de identidade e descartavel: trocar a implementacao de `Identity.Infrastructure` por API/OIDC nao altera dominio nem casos de uso.
- Dado pessoal real fica proibido ate a API entrar em operacao.

## Criterios para revisao desta decisao

- Existir API ASP.NET Core com autenticacao centralizada.
- Definicao do provedor de identidade do municipio (OIDC, LDAP ou base propria).
- Requisito legal de retencao de auditoria definido.
