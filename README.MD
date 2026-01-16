# Documentação - Unity GitHub Workflows

Este repositório contém workflows do GitHub Actions para automação de builds e deploy de projetos Unity para múltiplas plataformas (iOS, Android, Windows, WebGL).

## Preparando Segredos e Ambiente

Para que os workflows funcionem corretamente, é necessário configurar os seguintes segredos no GitHub. Acesse **Settings > Secrets and variables > Actions** no seu repositório e adicione os seguintes segredos:

### 🔐 Credenciais Unity

Credenciais necessárias para ativação da licença do Unity durante os builds.

| Variável         | Descrição                                 | Obrigatório | Onde Usar                   |
| ---------------- | ----------------------------------------- | ----------- | --------------------------- |
| `UNITY_LICENSE`  | Licença do Unity (gerada automaticamente) | ✅ Sim      | Todos os workflows de build |
| `UNITY_EMAIL`    | Email da conta Unity                      | ✅ Sim      | Todos os workflows de build |
| `UNITY_PASSWORD` | Senha da conta Unity                      | ✅ Sim      | Todos os workflows de build |

**Como obter:**

- Acesse [Unity ID](https://id.unity.com/)
- Use suas credenciais de login
- A licença será gerada automaticamente pelo workflow na primeira execução

---

### 🍎 Credenciais Apple Developer / App Store Connect

Credenciais necessárias para builds e publicação de aplicativos iOS.

#### Credenciais de Conta Apple

| Variável                | Descrição                                                        | Obrigatório | Onde Usar  |
| ----------------------- | ---------------------------------------------------------------- | ----------- | ---------- |
| `APPLE_DEVELOPER_EMAIL` | Email da conta Apple Developer (para Apple Developer Portal)     | ✅ Sim      | iOS builds |
| `APPLE_CONNECT_EMAIL`   | Email da conta App Store Connect (pode ser o mesmo do Developer) | ✅ Sim      | iOS builds |
| `APPLE_TEAM_ID`         | ID da equipe de desenvolvimento da Apple (Team ID)               | ✅ Sim      | iOS builds |

**Como obter:**

- Acesse [Apple Developer Portal](https://developer.apple.com/account/)
- O Team ID pode ser encontrado no canto superior direito do portal
- Use o email associado à sua conta Apple Developer

#### App Store Connect API

| Variável             | Descrição                                                     | Obrigatório | Onde Usar               |
| -------------------- | ------------------------------------------------------------- | ----------- | ----------------------- |
| `APPSTORE_KEY_ID`    | ID da chave da API do App Store Connect                       | ✅ Sim      | iOS builds e publicação |
| `APPSTORE_ISSUER_ID` | ID do issuer da API do App Store Connect                      | ✅ Sim      | iOS builds e publicação |
| `APPSTORE_P8`        | Conteúdo completo da chave privada `.p8` do App Store Connect | ✅ Sim      | iOS builds e publicação |

**Como obter:**

1. Acesse [App Store Connect](https://appstoreconnect.apple.com/)
2. Vá em **Users and Access > Keys**
3. Clique em **Generate API Key** ou use uma existente
4. Baixe o arquivo `.p8` (você só pode baixar uma vez!)
5. Copie o conteúdo completo do arquivo `.p8` para o segredo `APPSTORE_P8`
6. Anote o **Key ID** e o **Issuer ID** exibidos na interface

---

### 📱 Configurações iOS

| Variável             | Descrição                                                      | Obrigatório   | Onde Usar  |
| -------------------- | -------------------------------------------------------------- | ------------- | ---------- |
| `IOS_BUNDLE_ID`      | Bundle identifier do aplicativo iOS (ex: `com.empresa.meuapp`) | ✅ Sim        | iOS builds |
| `IOS_PROJECT_NAME`   | Nome do projeto iOS                                            | ✅ Sim        | iOS builds |
| `IOS_VERSION_NUMBER` | Número da versão do aplicativo (ex: `1.0.0`)                   | ❌ Opcional   | iOS builds |
| `IOS_BUILD_PATH`     | Caminho para o diretório de build do Unity                     | ⚠️ Automático | iOS builds |

**Nota:** `IOS_BUILD_PATH` é gerado automaticamente pelo workflow como `{workspace}/build/iOS`. Não é necessário configurá-lo manualmente.

---

### 🔑 GitHub e Fastlane Match

Credenciais para acesso ao repositório de certificados (match) e configuração do GitHub Actions.

| Variável            | Descrição                                                         | Obrigatório   | Onde Usar                               |
| ------------------- | ----------------------------------------------------------------- | ------------- | --------------------------------------- |
| `GH_PAT`            | Personal Access Token do GitHub (com permissões de repositório)   | ✅ Sim        | Setup inicial e geração de certificados |
| `GITHUB_REPOSITORY` | Nome do repositório no formato `org/repo`                         | ⚠️ Automático | Setup inicial                           |
| `MATCH_REPOSITORY`  | Repositório Git para armazenar certificados (formato: `org/repo`) | ✅ Sim        | iOS builds                              |

| `MATCH_PASSWORD` | Senha para descriptografar os certificados no repositório match | ✅ Sim | iOS builds |

**Como obter:**

**GH_PAT:**

1. Acesse [GitHub Settings > Developer settings > Personal access tokens](https://github.com/settings/tokens)
2. Gere um novo token com as permissões: `repo`, `admin:repo_hook`, `workflow`
3. Copie o token e adicione como segredo

**MATCH_REPOSITORY:**

- Crie um repositório privado no GitHub para armazenar os certificados
- Use o formato: `organizacao/nome-do-repositorio`
- Veja o capítulo **"Configurando o Match Repository"** para instruções detalhadas

**MATCH_DEPLOY_KEY:**

- ⚠️ **A chave de deploy é gerada automaticamente pelas pipelines**
- Não é necessário configurar manualmente
- A chave será criada automaticamente quando você executar o workflow `ios_setup.yml`
- Veja o capítulo **"Configurando o Match Repository"** para mais detalhes

**MATCH_PASSWORD:**

- Escolha uma senha forte para criptografar os certificados
- Esta senha será usada sempre que os certificados forem acessados
- Veja o capítulo **"Configurando o Match Repository"** para instruções detalhadas

**Nota:** `GITHUB_REPOSITORY` é automaticamente fornecido pelo GitHub Actions e não precisa ser configurado manualmente.

---

### ☁️ AWS S3 (Upload de Builds)

Credenciais para fazer upload das builds compiladas para o Amazon S3.

| Variável                | Descrição                                          | Obrigatório | Onde Usar                                  |
| ----------------------- | -------------------------------------------------- | ----------- | ------------------------------------------ |
| `AWS_ACCESS_KEY_ID`     | Access Key ID da conta AWS                         | ✅ Sim      | Upload de builds (Android, Windows, WebGL) |
| `AWS_SECRET_ACCESS_KEY` | Secret Access Key da conta AWS                     | ✅ Sim      | Upload de builds (Android, Windows, WebGL) |
| `AWS_REGION`            | Região do bucket S3 (ex: `us-east-1`)              | ✅ Sim      | Upload de builds                           |
| `AWS_S3_BUCKET`         | Nome do bucket S3 onde as builds serão armazenadas | ✅ Sim      | Upload de builds                           |

**Como obter:**

1. Acesse o [AWS Console](https://console.aws.amazon.com/)
2. Crie um usuário IAM com permissões de escrita no S3
3. Gere Access Keys para o usuário
4. Crie um bucket S3 ou use um existente
5. Configure as credenciais e o nome do bucket nos segredos

---

### 🚀 Vercel (Deploy WebGL)

Credenciais para deploy automático de builds WebGL no Vercel.

| Variável            | Descrição                       | Obrigatório | Onde Usar    |
| ------------------- | ------------------------------- | ----------- | ------------ |
| `VERCEL_TOKEN`      | Token de autenticação do Vercel | ✅ Sim      | Deploy WebGL |
| `VERCEL_ORG_ID`     | ID da organização no Vercel     | ✅ Sim      | Deploy WebGL |
| `VERCEL_PROJECT_ID` | ID do projeto no Vercel         | ✅ Sim      | Deploy WebGL |

**Como obter:**

1. Acesse [Vercel Dashboard](https://vercel.com/dashboard)
2. Vá em **Settings > Tokens** e crie um novo token
3. Para obter o `ORG_ID` e `PROJECT_ID`:
   - Execute `vercel link` no projeto local
   - Ou encontre no arquivo `.vercel/project.json` após fazer link
   - Ou consulte a URL do projeto no dashboard do Vercel

---

### 📋 Resumo por Workflow

#### iOS Build (`build_for_ios.yml`)

- ✅ Unity: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`
- ✅ Apple: `APPLE_DEVELOPER_EMAIL`, `APPLE_CONNECT_EMAIL`, `APPLE_TEAM_ID`
- ✅ App Store Connect API: `APPSTORE_KEY_ID`, `APPSTORE_ISSUER_ID`, `APPSTORE_P8`
- ✅ iOS: `IOS_BUNDLE_ID`, `IOS_PROJECT_NAME`
- ✅ Match: `MATCH_REPOSITORY`, `MATCH_DEPLOY_KEY`, `MATCH_PASSWORD`

#### Android Build (`build_for_android.yml`)

- ✅ Unity: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`
- ✅ AWS S3: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`, `AWS_S3_BUCKET`

#### Windows Build (`build_for_windows.yml`)

- ✅ Unity: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`
- ✅ AWS S3: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`, `AWS_S3_BUCKET`

#### WebGL Build (`build_for_webgl.yml`)

- ✅ Unity: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`
- ✅ Vercel: `VERCEL_TOKEN`, `VERCEL_ORG_ID`, `VERCEL_PROJECT_ID`
- ✅ AWS S3: `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION`, `AWS_S3_BUCKET`

#### iOS Setup (`ios_setup.yml`)

- ✅ App Store Connect API: `APPSTORE_KEY_ID`, `APPSTORE_ISSUER_ID`, `APPSTORE_P8`
- ✅ GitHub: `GH_PAT`
- ✅ Match: `MATCH_REPOSITORY`

#### Generate Certificates (`generate_certs.yml`)

- ✅ App Store Connect API: `APPSTORE_KEY_ID`, `APPSTORE_ISSUER_ID`, `APPSTORE_P8`
- ✅ iOS: `IOS_BUNDLE_ID`
- ✅ GitHub: `GH_PAT`
- ✅ Match: `MATCH_REPOSITORY`, `MATCH_DEPLOY_KEY`, `MATCH_PASSWORD`

---

### ⚠️ Variáveis Geradas Automaticamente

As seguintes variáveis são fornecidas automaticamente pelo GitHub Actions e **não precisam** ser configuradas:

- `GITHUB_REPOSITORY`: Nome do repositório no formato `org/repo`
- `IOS_BUILD_PATH`: Caminho gerado automaticamente como `{workspace}/build/iOS`

---

### 🔒 Boas Práticas de Segurança

1. **Nunca commite segredos no código**: Use sempre os Secrets do GitHub
2. **Rotacione credenciais regularmente**: Especialmente tokens e chaves de API
3. **Use permissões mínimas**: Configure apenas as permissões necessárias para cada credencial
4. **Proteja o repositório do match**: Mantenha-o privado e com acesso restrito
5. **Backup do arquivo `.p8`**: Faça backup seguro do arquivo App Store Connect API, pois ele só pode ser baixado uma vez

---

### 📝 Checklist de Configuração

Use este checklist para garantir que todos os segredos estão configurados:

#### Para todos os workflows:

- [ ] `UNITY_LICENSE`
- [ ] `UNITY_EMAIL`
- [ ] `UNITY_PASSWORD`

#### Para workflows iOS:

- [ ] `APPLE_DEVELOPER_EMAIL`
- [ ] `APPLE_CONNECT_EMAIL`
- [ ] `APPLE_TEAM_ID`
- [ ] `APPSTORE_KEY_ID`
- [ ] `APPSTORE_ISSUER_ID`
- [ ] `APPSTORE_P8`
- [ ] `IOS_BUNDLE_ID`
- [ ] `IOS_PROJECT_NAME`
- [ ] `MATCH_REPOSITORY`
- [ ] `MATCH_DEPLOY_KEY`
- [ ] `MATCH_PASSWORD`
- [ ] `GH_PAT`

#### Para upload S3 (Android, Windows, WebGL):

- [ ] `AWS_ACCESS_KEY_ID`
- [ ] `AWS_SECRET_ACCESS_KEY`
- [ ] `AWS_REGION`
- [ ] `AWS_S3_BUCKET`

#### Para deploy Vercel (WebGL):

- [ ] `VERCEL_TOKEN`
- [ ] `VERCEL_ORG_ID`
- [ ] `VERCEL_PROJECT_ID`

## Configurando o Match Repository

O Fastlane Match é uma ferramenta que gerencia certificados e perfis de provisionamento da Apple de forma centralizada e segura. Este capítulo explica como configurar o repositório privado que armazenará os certificados do seu projeto iOS.

### O que é o Match Repository?

O Match Repository é um repositório Git privado que armazena de forma criptografada:

- Certificados de desenvolvimento e distribuição da Apple
- Perfis de provisionamento (Provisioning Profiles)
- Chaves privadas

Todos os arquivos são criptografados antes de serem commitados no repositório, garantindo segurança mesmo que o repositório seja acessado por terceiros.

### Passo 1: Criar o Repositório Privado

1. Acesse o GitHub e vá até sua organização ou conta pessoal
2. Clique em **New repository** (ou **+ > New repository**)
3. Configure o repositório:
   - **Nome**: Escolha um nome descritivo (ex: `ios-certificates`, `app-certificates-match`)
   - **Visibilidade**: Selecione **Private** (obrigatório para segurança)
   - **Não inicialize** com README, .gitignore ou licença (deixe vazio)
4. Clique em **Create repository**

**Importante:** Mantenha este repositório sempre privado. Nunca torne-o público, pois ele contém informações sensíveis.

### Passo 2: Obter Token Pessoal do GitHub

Você precisará de um Personal Access Token (PAT) do GitHub com acesso total a todos os repositórios para que o Fastlane possa acessar o repositório de certificados.

1. Acesse [GitHub Settings > Developer settings > Personal access tokens](https://github.com/settings/tokens)
2. Clique em **Generate new token** > **Generate new token (classic)**
3. Configure o token:
   - **Note**: Dê um nome descritivo (ex: "Fastlane Match Access")
   - **Expiration**: Escolha uma data de expiração apropriada
   - **Scopes**: Marque **repo** (isso concede acesso total a todos os repositórios)
     - Isso inclui automaticamente: `repo:status`, `repo_deployment`, `public_repo`, `repo:invite`, `security_events`
4. Clique em **Generate token**
5. **Copie o token imediatamente** - você não poderá vê-lo novamente!

**Adicionar como segredo:**

- Vá em **Settings > Secrets and variables > Actions** no repositório principal
- Clique em **New repository secret**
- Nome: `GH_PAT`
- Valor: Cole o token copiado
- Clique em **Add secret**

### Passo 3: Configurar a Senha de Criptografia

Escolha uma senha forte que será usada para criptografar e descriptografar os certificados no repositório Match.

**Requisitos da senha:**

- Mínimo de 12 caracteres (recomendado: 16+ caracteres)
- Combine letras maiúsculas, minúsculas, números e símbolos
- Não use senhas comuns ou relacionadas ao projeto

**Exemplo de senha forte:**

```
Kx9#mP2$vL8@nQ4!wR7
```

**Adicionar como segredo:**

- Vá em **Settings > Secrets and variables > Actions** no repositório principal
- Clique em **New repository secret**
- Nome: `MATCH_PASSWORD`
- Valor: Cole a senha escolhida
- Clique em **Add secret**

**Importante:**

- Guarde esta senha em um local seguro (gerenciador de senhas)
- Você precisará desta senha sempre que os certificados forem acessados
- Se perder a senha, não será possível descriptografar os certificados existentes

### Passo 4: Configurar o Nome do Repositório

O nome do repositório Match deve ser configurado no formato `<org>/<repo>`, onde:

- `<org>` é o nome da organização ou usuário do GitHub
- `<repo>` é o nome do repositório criado no Passo 1

**Exemplos:**

- Se o repositório está em `https://github.com/minha-org/certificados-ios`, use: `minha-org/certificados-ios`
- Se está em `https://github.com/meuusuario/app-certificates`, use: `meuusuario/app-certificates`

**Adicionar como segredo:**

- Vá em **Settings > Secrets and variables > Actions** no repositório principal
- Clique em **New repository secret**
- Nome: `MATCH_REPOSITORY`
- Valor: Cole o nome no formato `organizacao/nome-do-repositorio`
- Clique em **Add secret**

### Passo 5: Chave de Deploy (Automática)

A chave de deploy SSH será **gerada automaticamente pelas pipelines** do GitHub Actions quando você executar o workflow `ios_setup.yml` pela primeira vez.

**O que acontece:**

1. O workflow `ios_setup.yml` usa o token `GH_PAT` para criar uma Deploy Key no repositório Match
2. A chave SSH é gerada automaticamente
3. A chave pública é adicionada ao repositório Match como Deploy Key com permissão de escrita
4. A chave privada é armazenada automaticamente e usada nos builds subsequentes

**Você não precisa:**

- Gerar manualmente chaves SSH
- Adicionar a chave pública ao repositório
- Configurar o segredo `MATCH_DEPLOY_KEY` manualmente

### Resumo da Configuração

Após completar os passos acima, você deve ter configurado os seguintes segredos:

| Segredo            | Valor                                     | Status                         |
| ------------------ | ----------------------------------------- | ------------------------------ |
| `GH_PAT`           | Token pessoal do GitHub com acesso `repo` | ✅ Configurado                 |
| `MATCH_REPOSITORY` | Nome do repositório no formato `org/repo` | ✅ Configurado                 |
| `MATCH_PASSWORD`   | Senha forte para criptografia             | ✅ Configurado                 |
| `MATCH_DEPLOY_KEY` | Chave SSH (gerada automaticamente)        | ⚠️ Será gerada automaticamente |

### Próximos Passos

Após configurar o Match Repository:

1. **Execute o workflow de setup inicial** (`ios_setup.yml`) para gerar a chave de deploy automaticamente
2. **Execute o workflow de geração de certificados** (`generate_certs.yml`) para criar e armazenar os certificados no repositório Match
3. Os certificados estarão disponíveis para uso nos builds iOS

### Boas Práticas

- ✅ Mantenha o repositório Match sempre privado
- ✅ Use uma senha forte e guarde-a em local seguro
- ✅ Rotacione o token `GH_PAT` periodicamente
- ✅ Limite o acesso ao repositório Match apenas a membros confiáveis da equipe
- ✅ Faça backup da senha `MATCH_PASSWORD` em um gerenciador de senhas
- ❌ Nunca torne o repositório Match público
- ❌ Não compartilhe a senha `MATCH_PASSWORD` por email ou mensagens não criptografadas

### Tudo Pronto!

Após completar todas as configurações acima, **tudo estará pronto** para executar os workflows de build. Você pode começar a usar os workflows do GitHub Actions imediatamente.

#### Acompanhando os Jobs

1. **Acesse a aba Actions** no seu repositório GitHub
2. **Selecione o workflow** que deseja executar (ex: "Build for iOS", "Build for Android", etc.)
3. **Clique em "Run workflow"** para executar manualmente
4. **Acompanhe o progresso** em tempo real:
   - Veja os logs de cada etapa
   - Identifique erros rapidamente
   - Monitore o tempo de execução

#### Onde Encontrar as Builds

Após a conclusão bem-sucedida de um workflow, as builds estarão disponíveis em dois locais:

**1. Bucket S3 Configurado**

- As builds são automaticamente enviadas para o bucket S3 configurado
- Localização: `s3://{AWS_S3_BUCKET}/builds/{data}-{plataforma}build/`
- Exemplo: `s3://meu-bucket/builds/2024-01-15-androidbuild/`
- Acesse através do [AWS Console](https://console.aws.amazon.com/s3/) ou use a AWS CLI

**2. Artefatos do GitHub Actions**

- Cada workflow gera artefatos que podem ser baixados diretamente do GitHub
- **Como acessar:**
  1. Vá até a aba **Actions** no repositório
  2. Clique na execução do workflow concluída
  3. Role até a seção **Artifacts** no resumo do workflow
  4. Clique no artefato para fazer o download
- Os artefatos ficam disponíveis por **90 dias** após a execução do workflow

**Nota:** Para workflows iOS, o artefato é gerado após a compilação do Unity e antes do upload para a App Store/TestFlight.

## Configurando o Ambiente Ruby

O Fastlane é uma ferramenta Ruby e requer que o ambiente Ruby esteja configurado corretamente. Este capítulo explica como configurar o ambiente Ruby localmente para trabalhar com os workflows iOS.

### Pré-requisitos

Antes de começar, certifique-se de que o arquivo `Gemfile` está presente na pasta raiz do projeto. Este arquivo contém as dependências Ruby necessárias para o Fastlane.

### Passo 1: Instalar Ruby

Primeiro, você precisa instalar o Ruby no seu sistema. A versão recomendada é **Ruby 3.2** ou superior.

**No macOS (usando Homebrew):**

```bash
brew install ruby
```

**No Linux (Ubuntu/Debian):**

```bash
sudo apt-get update
sudo apt-get install ruby-full
```

**No Windows:**

- Baixe e instale o [RubyInstaller](https://rubyinstaller.org/)

**Verificar instalação:**

```bash
ruby --version
```

### Passo 2: Instalar Bundler

O Bundler é o gerenciador de dependências do Ruby. Instale a versão específica `2.7.2`:

```bash
gem install bundler:2.7.2
```

**Verificar instalação:**

```bash
bundler --version
```

**Nota:** Se você tiver múltiplas versões do Bundler instaladas, pode ser necessário usar o comando completo:

```bash
bundler_2.7.2 --version
```

### Passo 3: Instalar Dependências

Navegue até a pasta raiz do projeto (onde está o `Gemfile`) e instale as dependências:

```bash
cd /caminho/para/o/projeto
bundler_2.7.2 install
```

Este comando irá:

- Ler o arquivo `Gemfile`
- Instalar todas as gems especificadas (incluindo o Fastlane e seus plugins)
- Gerar o arquivo `Gemfile.lock` com as versões exatas das dependências instaladas

**Importante:** O arquivo `Gemfile.lock` deve ser commitado no repositório para garantir que todos os ambientes usem as mesmas versões das dependências.

### Verificação

Após a instalação, você pode verificar se o Fastlane está instalado corretamente:

```bash
bundler_2.7.2 exec fastlane --version
```

### Ambiente Pronto

Após completar estes passos, o ambiente Ruby está configurado e pronto para uso. Você pode agora:

- Executar comandos do Fastlane localmente
- Testar as lanes antes de executá-las no GitHub Actions
- Desenvolver e depurar configurações do Fastlane

**Exemplo de uso:**

```bash
# Executar uma lane específica
bundler_2.7.2 exec fastlane ios sync_certificates

# Ver todas as lanes disponíveis
bundler_2.7.2 exec fastlane lanes
```

### Troubleshooting

**Erro: "bundler: command not found"**

- Certifique-se de que o Bundler foi instalado corretamente
- Tente usar o caminho completo: `bundler_2.7.2` em vez de `bundler`

**Erro: "Permission denied"**

- No Linux/macOS, pode ser necessário usar `sudo` para instalar gems globalmente
- Alternativamente, configure um gerenciador de versões Ruby como `rbenv` ou `rvm` para evitar problemas de permissão

**Erro ao instalar dependências**

- Verifique se você está na pasta raiz do projeto (onde está o `Gemfile`)
- Certifique-se de que tem conexão com a internet
- Tente limpar o cache: `bundler_2.7.2 clean`
