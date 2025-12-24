# Sistema de Monitoramento de Arquivos

Sistema web para processar e monitorar arquivos financeiros enviados pelas adquirentes UfCard e FagammonCard.

Para detalhes completos sobre requisitos e especificações, consulte o arquivo `Case - Full Stack Pleno.pdf` na raiz do repositório.

## Tecnologias Utilizadas

**Backend:**
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- SQLite

**Frontend:**
- React 18
- Vite

## Pré-requisitos

- .NET 8 SDK
- Node.js (versão 16 ou superior)

## Como Executar

### Backend

```bash
cd FileMonitoring
dotnet restore
dotnet run
```

A API estará disponível em `http://localhost:5130` e o Swagger em `http://localhost:5130/swagger`

### Frontend

```bash
cd file-monitor
npm install
npm run dev
```

O portal web estará disponível em `http://localhost:5173`

## Estrutura do Projeto

```
FileMonitoringSystem/
├── FileMonitoring/          # Backend .NET
├── file-monitor/            # Frontend React
└── Case - Full Stack Pleno.pdf
```

## Funcionalidades Implementadas

- Processamento de arquivos com validação de layout posicional
- Detecção automática de duplicados
- Backup automático dos arquivos processados
- Portal web com listagem, edição e exclusão
- Gráfico de estatísticas
- API REST documentada com Swagger

## Observações

O banco de dados SQLite (`arquivos.db`) é criado automaticamente na primeira execução.

Os arquivos de backup ficam salvos em `FileMonitoring/backups/`.