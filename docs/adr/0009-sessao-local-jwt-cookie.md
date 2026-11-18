# 0009 - Sessao local com JWT em cookie

## Status

Aceito

## Contexto

A POC nao tem backend e a issue #104 exige perfis de acesso com troca de experiencia sem prometer seguranca. O ADR 0007 proibe token, senha ou segredo em `localStorage` e exige aviso de modo demonstracao.

Sem servidor, a unica forma de manter a sessao entre recargas e um artefato no navegador. `localStorage` e legivel por qualquer script da origem e fica exposto por muito tempo; cookie de sessao expira e pode ser revogado limpando o cookie.

## Decisao

- `Identity` e um bounded context com `User`, `UserRole` e `AccessRight` (`RoleRights` mapeia perfil para direitos).
- Usuarios ficam em `localStorage` no namespace `gisora:identity`. A senha e gravada apenas como hash PBKDF2 com sal por usuario. O hash nunca e comparado em texto claro.
- A sessao e um JWT HS256 (`sub`, `name`, `email`, `role`, `iss`, `aud`, `iat`, `exp`) gravado no cookie `gisora_session` com `Path=/` e `SameSite=Lax`, validade de 8 horas.
- O cookie e lido e validado na inicializacao pelo `GisoraAuthenticationStateProvider`. Token invalido, expirado ou de usuario inexistente/inativo e descartado e o cookie e limpo.
- Autorizacao e aplicada por `[Authorize]` e `[Authorize(Roles = ...)]` nas rotas, com `AuthorizeRouteView`. A UI oculta acao sem direito, mas a decisao de acesso e da rota, nao do botao.
- A base local e semeada com tres contas demonstrativas (administrador, analista, consulta publica) na primeira execucao.

## Alternativas consideradas

- Guardar o JWT em `localStorage`: rejeitado no ADR 0007.
- Sessao apenas em memoria: rejeitado; o usuario perderia a sessao a cada recarga e a issue pede usuario logado persistente na navegacao.
- Login ficticio sem token: rejeitado; nao permitiria validar expiracao nem revogacao.
- Assinatura no servidor: impossivel sem API; a chave HS256 fica no bundle e e publica.

## Consequencias

- O JWT e assinado no cliente: qualquer pessoa pode forjar um token valido. A barreira e de UX e de organizacao de permissoes, nao de seguranca. O aviso de modo demonstracao permanece no login e no painel.
- A chave de assinatura e as iteracoes do PBKDF2 sao constantes do codigo. Sao descartaveis junto com o adaptador quando a API existir.
- Perfis e direitos precisam ser aplicados tambem no servidor quando o backend entrar em operacao (issue #107). Nenhuma regra de negocio pode depender do perfil do cliente.
- Trocar a implementacao por OIDC exige apenas substituir `IAccessTokenCodec` e `ISessionStore`; dominio e casos de uso nao mudam.
- Dado pessoal real continua proibido enquanto nao houver autenticacao server-side.
