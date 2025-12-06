<img width="1032" height="270" alt="rfc" src="https://github.com/user-attachments/assets/1551d655-0abc-4f59-bd22-d475787fdb3d" />

# NutriTrack-Backend-TCC
> Backend do Sistema de Acompanhamento Nutricional com IA – Desenvolvido por Gustavo Costa

<br>

## 🔎 Visão Geral
Este repositório contém o backend do **NutriTrack**, desenvolvido como Trabalho de Conclusão de Curso (TCC) para o curso de Engenharia de Software.

O sistema inova ao utilizar **Inteligência Artificial (Google Gemini)** para processar e controlar a alimentação via linguagem natural, eliminando a fricção de cadastros manuais. O backend é responsável por toda a regra de negócio, incluindo o cálculo automático de taxas metabólicas necessárias para o alcanço do objetivo do usuário (Método Mifflin-St Jeor), intergração com inteligência artificial, segurança via JWT e persistência de dados.

<br>

## 📦 Tecnologias utilizadas
- Linguagem principal: **C# (.NET Core 9)**
- Banco de dados: **PostgreSQL**
- Autenticação: **JWT / Bearer Token**
- Inteligência Artificial: **Google Gemini API**
- Testes Unitários: **xUnit + Moq**
- Arquitetura: **MVC**

<br>

## 🛠️ Como rodar localmente

```bash
# 1. Pré-requisitos: SDK .NET 9.0 e PostgreSQL instalados.

# 2. Clone o repositório
git clone [https://github.com/GustavoCosta20/NutriTrack_Backend.git](https://github.com/GustavoCosta20/NutriTrack_Backend.git)

# 3. Acesse o diretório da API
cd NutriTrack_Backend

# 4. Configuração do Ambiente (Essencial!)
# Crie/Edite o arquivo appsettings.json na raiz do projeto NutriTrack_Api
# Adicione sua ConnectionString, Chave JWT e API Key do Gemini:
# {
#   "ConnectionStrings": { "Postgresql": "Host=localhost;..." },
#   "JwtSettings": { "SecretKey": "...", "Issuer": "...", "Audience": "..." },
#   "Gemini": { "ApiKey": "SUA_CHAVE_AQUI" }
# }

# 5. Restaurar dependências
dotnet restore

# 6. Aplicar as Migrations (No seu banco de dados local)
update database

# 7. Executar a aplicação
dotnet run --project NutriTrack_Api
```
<br>

## 🚀 Funcionalidades do projeto

- **Autenticação & Segurança**: Login e Registro com criptografia (BCrypt) e emissão de Token JWT;
- **Integração com IA**: Serviço dedicado que envia descrições de refeições para o Google Gemini e retorna dados estruturados;
- **Cálculo Metabólico**: Implementação da equação de Mifflin-St Jeor para definição automática de TMB e metas de macronutrientes baseadas no objetivo do usuário;
- **Gestão de Refeições**: CRUD completo de refeições e alimentos consumidos;
- **Dashboard Data**: Endpoint otimizado (`/user/me`) que fornece dados consolidados para gráficos de progresso;
- **Testes Automatizados**: Cobertura de testes unitários (TDD) para serviços críticos de IA e Cálculos.

<br>

### 🔗 Repositório Frontend: [NutriTrack Frontend](https://github.com/GustavoCosta20/NutriTrack_Frontend)
### 🔗 API em Produção: [Acessar Site](https://nutritrack-lifestyle.vercel.app/login)
### 🔗 Documentação RFC: [Acessar documento RFC](https://onedrive.live.com/?redeem=aHR0cHM6Ly8xZHJ2Lm1zL2IvYy82MTBlYjk3MTZkMjBiYWZjL0lRQlZTZ05aRXdPd1NaN0hBLUNqT1F6c0FYeHRXX3R5SWZscXNlU2VIdDYxVWNVP2U9VU5HSHMz&cid=610EB9716D20BAFC&id=610EB9716D20BAFC%21s59034a55031349b09ec703e0a3390cec&parId=610EB9716D20BAFC%21sea8cc6beffdb43d7976fbc7da445c639&o=OneUp)
