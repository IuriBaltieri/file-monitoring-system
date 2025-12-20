import './styles.css';

export default function TabelaArquivos({ arquivos, onAlterarStatus, onExcluir, editandoId }) {
  const formatarData = (data) => {
    if (!data) return '-';
    try {
      return new Date(data).toLocaleDateString('pt-BR');
    } catch {
      return '-';
    }
  };

  if (arquivos.length === 0) {
    return (
      <div className="tabela-container">
        <h2>Arquivos Processados (0)</h2>
        <div className="vazio">
          <p>📄 Nenhum arquivo processado ainda</p>
          <small>Use o formulário acima para processar seu primeiro arquivo</small>
        </div>
      </div>
    );
  }

  return (
    <div className="tabela-container">
      <h2>Arquivos Processados ({arquivos.length})</h2>
      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Tipo</th>
              <th>Empresa</th>
              <th>Estabelecimento</th>
              <th>Data Processamento</th>
              <th>Período</th>
              <th>Sequência</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {arquivos.map((arq) => (
              <tr key={arq.id}>
                <td>{arq.id}</td>
                <td>
                  <span className="tipo-badge">Tipo {arq.tipoRegistro}</span>
                </td>
                <td className="empresa-col">{arq.empresa}</td>
                <td>{arq.estabelecimento}</td>
                <td>{formatarData(arq.dataProcessamento)}</td>
                <td>
                  {arq.periodoInicial && arq.periodoFinal ? (
                    <span className="periodo">
                      {formatarData(arq.periodoInicial)} - {formatarData(arq.periodoFinal)}
                    </span>
                  ) : (
                    <span className="sem-periodo">-</span>
                  )}
                </td>
                <td>{arq.sequencia || '-'}</td>
                <td>
                  <span className={`badge ${arq.status === 'Recepcionado' ? 'badge-ok' : 'badge-erro'}`}>
                    {arq.status}
                  </span>
                </td>
                <td>
                  <div className="acoes-buttons">
                    <button
                      onClick={() => onAlterarStatus(arq.id, arq.status)}
                      disabled={editandoId === arq.id}
                      className="btn-editar"
                      title="Alterar status"
                    >
                      {editandoId === arq.id ? '...' : '✏️'}
                    </button>
                    <button
                      onClick={() => onExcluir(arq.id, arq.empresa)}
                      className="btn-excluir"
                      title="Excluir arquivo"
                    >
                      🗑️
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}