# Sistema de Monitoramento de Arquivos

Desenvolvi este sistema para processar e monitorar arquivos que recebemos dos adquirentes UfCard e FagammonCard.

## Tecnologias

Usei .NET 8.0 no backend com ASP.NET Core, Entity Framework Core e SQLite. No frontend escolhi React 18 com Vite.

## O que o sistema faz

Basicamente é um CRUD de arquivos com algumas validações específicas. O sistema lê arquivos de texto com layout posicional, valida se está tudo certo, salva no banco e faz backup automático. Se tentar processar um arquivo duplicado, ele marca como "Não Recepcionado".

No portal web dá pra ver todos os arquivos processados, o status de cada um e tem um gráfico comparativo. Tudo funciona via API REST.

## Como rodar

Precisa ter o .NET 8 SDK e Node.js instalados.

Para o backend:
```bash
cd FileMonitoring
dotnet restore
dotnet run
```

Vai rodar em http://localhost:5000 e tem Swagger em /swagger

Para o frontend:
```bash
cd file-monitor
npm install
npm run dev
```

Vai rodar em http://localhost:5173

## Layouts suportados

O sistema aceita dois tipos de arquivo:

**Tipo 0 (UfCard)** - linha com 50 caracteres contendo tipo de registro, estabelecimento, datas, sequência e empresa.

**Tipo 1 (FagammonCard)** - linha com 36 caracteres contendo tipo de registro, data, estabelecimento, empresa e sequência.

Os campos seguem posições fixas conforme especificação do layout posicional.

## API

Tem três endpoints principais: listar arquivos, pegar estatísticas e processar arquivo novo.

## Observações

Corrigi uma inconsistência que tinha no exemplo original do documento onde o campo Estabelecimento tinha 9 dígitos ao invés de 10. Agora tá seguindo certinho a especificação.

O SQLite foi escolhido pra facilitar, não precisa configurar nada. Os backups ficam salvos automaticamente numa pasta separada.