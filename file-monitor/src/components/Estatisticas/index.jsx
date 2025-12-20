import './styles.css';

export default function Estatisticas({ stats }) {
  const total = stats.recepcionados + stats.naoRecepcionados;
  const percRecep = total > 0 ? Math.round((stats.recepcionados / total) * 100) : 0;
  const percNaoRecep = total > 0 ? 100 - percRecep : 0;

  if (total === 0) {
    return (
      <div className="estatisticas-container">
        <h2>Estatísticas</h2>
        <div className="stats-vazio">
          <p>Nenhum arquivo processado ainda</p>
          <small>Processe o primeiro arquivo para ver as estatísticas</small>
        </div>
      </div>
    );
  }

  return (
    <div className="estatisticas-container">
      <h2>Estatísticas</h2>
      
      <div className="stats-grid">
        <div className="stat recepcionado">
          <div className="stat-label">Recepcionados</div>
          <div className="stat-valor">{stats.recepcionados}</div>
          <div className="stat-perc">{percRecep}%</div>
        </div>
        <div className="stat nao-recepcionado">
          <div className="stat-label">Não Recepcionados</div>
          <div className="stat-valor">{stats.naoRecepcionados}</div>
          <div className="stat-perc">{percNaoRecep}%</div>
        </div>
      </div>

      <div className="grafico">
        <div className="grafico-barra">
          <div 
            className="grafico-fill recepcionado-fill" 
            style={{ width: `${percRecep}%` }}
            title={`${stats.recepcionados} recepcionados (${percRecep}%)`}
          />
          <div 
            className="grafico-fill nao-recepcionado-fill" 
            style={{ width: `${percNaoRecep}%` }}
            title={`${stats.naoRecepcionados} não recepcionados (${percNaoRecep}%)`}
          />
        </div>
        <div className="grafico-legenda">
          <span className="legenda-item">
            <span className="legenda-cor recepcionado-cor" />
            Recepcionados
          </span>
          <span className="legenda-item">
            <span className="legenda-cor nao-recepcionado-cor" />
            Não Recepcionados
          </span>
        </div>
      </div>
    </div>
  );
}