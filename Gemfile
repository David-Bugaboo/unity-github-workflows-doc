# Gemfile - Gerenciamento de dependências Ruby para Fastlane
#
# Este arquivo define as dependências Ruby necessárias para executar
# as automações do Fastlane neste projeto. O Fastlane é uma ferramenta
# de automação para builds e deploy de aplicativos móveis.
#
# Para instalar as dependências, execute:
#   bundle install
#
# Para atualizar as dependências, execute:
#   bundle update

# Define o repositório de gems Ruby padrão
source "https://rubygems.org"

# Fastlane - Ferramenta principal de automação para iOS e Android
# Usada para automatizar builds, testes, code signing e deploy
gem "fastlane"

# Plugin do Fastlane para integração com GitHub Actions
# Permite configurar deploy keys e outras funcionalidades do GitHub Actions
# Nota: A versão publicada no RubyGems está faltando mudanças necessárias,
# por isso precisamos usar diretamente o repositório Git
gem 'fastlane-plugin-github_action', git: "https://github.com/joshdholtz/fastlane-plugin-github_action"
