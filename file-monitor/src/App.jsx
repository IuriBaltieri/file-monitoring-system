import { useState, useEffect } from 'react';
import './App.css';

const API_URL = 'http://localhost:5130/api/arquivos';

function App() {
  const [arquivos, setArquivos] = useState([]);
  const [stats, setStats] = useState({ recepcionados: 0, naoRecepcionados: 0 });
  const [conteudo, setConteudo] = useState('');
  const [nomeArquivo, setNomeArquivo] = useState('arquivo.txt');
  const [loading, setLoading] = useState(false);
  const [loadingDados, setLoadingDados] = useState(true);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');
  const [editandoId, setEditandoId] = useState(null);

  useEffect(() => {
    carregarDados();
  }, []);

  const carregarDados = async () => {
    try {
      setLoadingDados(true);
      
      const resArquivos = await fetch(API_URL);
      if (!resArquivos.ok) throw new Error('Erro ao carregar arquivos');
      const dataArquivos = await resArquivos.json();
      setArquivos(dataArquivos);

      const resStats = await fetch(`${API_URL}/estatisticas`);
      if (!resStats.ok) throw new Error('Erro ao carregar estatísticas');
      const dataStats = await resStats.json();
      setStats(dataStats);
    } catch (err) {
      console.error('Erro ao carregar dados:', err);
      setErro('Erro ao conectar com o servidor. Verifique se a API está rodando.');
    } finally {
      setLoadingDados(false);
    }
  };

  const processar = async (e) => {
    e.preventDefault();
    
    if (!conteudo.trim()) {
      setErro('O conteúdo do arquivo não pode estar vazio');
      return;
    }

    setLoading(true);
    setErro('');
    setSucesso('');

    try {
      const res = await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          conteudo: conteudo.trim(), 
          nomeArquivo: nomeArquivo.trim() 
        })
      });

      const data = await res.json();

      if (!res.ok) {
        throw new Error(data.erro || 'Erro ao processar arquivo');
      }

      setSucesso('Arquivo processado com sucesso!');
      setConteudo('');
      carregarDados();

      setTimeout(() => setSucesso(''), 4000);
    } catch (err) {
      setErro(err.message);
    } finally {
      setLoading(false);
    }
  };

  const alternarStatus = async (id, statusAtual) => {
    const novoStatus = statusAtual === 'Recepcionado' ? 'Não Recepcionado' : 'Recepcionado';
    
    try {
      setEditandoId(id);
      
      const res = await fetch(`${API_URL}/${id}/status`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: novoStatus })
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.erro || 'Erro ao atualizar status');
      }

      carregarDados();
      setSucesso('Status atualizado com sucesso!');
      setTimeout(() => setSucesso(''), 3000);
    } catch (err) {
      setErro(err.message);
      setTimeout(() => setErro(''), 3000);
    } finally {
      setEditandoId(null);
    }
  };

  const excluir = async (id, empresa) => {
    if (!window.confirm(`Deseja realmente excluir o arquivo da empresa ${empresa}?`)) {
      return;
    }

    try {
      const res = await fetch(`${API_URL}/${id}`, {
        method: 'DELETE'
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.erro || 'Erro ao excluir arquivo');
      }

      carregarDados();
      setSucesso('Arquivo excluído com sucesso!');
      setTimeout(() => setSucesso(''), 3000);
    } catch (err) {
      setErro(err.message);
      setTimeout(() => setErro(''), 3000);
    }
  };

  const total = stats.recepcionados + stats.naoRecepcionados;
  const percRecep = total > 0 ? Math.round((stats.recepcionados / total) * 100) : 0;
  const percNaoRecep = total > 0 ? 100 - percRecep : 0;

  const formatarData = (data) => {
    if (!data) return '-';
    try {
      return new Date(data).toLocaleDateString('pt-BR');
    } catch {
      return '-';
    }
  };

  if (loadingDados) {
    return (
      <div className="app">
        <div className="loading-screen">
          <div className="spinner"></div>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <header className="header">
        <h1>Monitoramento de Arquivos</h1>
        <p>Sistema de processamento de arquivos de adquirentes</p>
      </header>

      <div className="container">
        {/* Alertas Globais */}
        {erro && (
          <div className="alert alert-erro global-alert">❌ {erro}</div>
        )}
        {sucesso && (
          <div className="alert alert-sucesso global-alert">✅ {sucesso}</div>
        )}

        <div className="grid">
          <div className="card">
            <h2>Processar Arquivo</h2>
            <form onSubmit={processar}>
              <div className="form-group">
                <label>Nome do arquivo</label>
                <input
                  type="text"
                  value={nomeArquivo}
                  onChange={(e) => setNomeArquivo(e.target.value)}
                  placeholder="arquivo.txt"
                  disabled={loading}
                />
              </div>

              <div className="form-group">
                <label>Conteúdo do arquivo</label>
                <textarea
                  value={conteudo}
                  onChange={(e) => setConteudo(e.target.value)}
                  placeholder="Cole o conteúdo do arquivo aqui..."
                  rows="4"
                  required
                  disabled={loading}
                />
                <small className="hint">
                  Tipo 0: 50 caracteres | Tipo 1: 36 caracteres
                </small>
              </div>

              <div className="exemplos">
                <button 
                  type="button" 
                  onClick={() => setConteudo('00987564321201906262019062520190625000000100UfCard  ')}
                  disabled={loading}
                >
                  Exemplo Tipo 0
                </button>
                <button 
                  type="button" 
                  onClick={() => setConteudo('12019052632165487FagammonCard0002451')}
                  disabled={loading}
                >
                  Exemplo Tipo 1
                </button>
              </div>

              <button type="submit" disabled={loading} className="btn-processar">
                {loading ? 'Processando...' : 'Processar Arquivo'}
              </button>
            </form>
          </div>

          <div className="card">
            <h2>Estatísticas</h2>
            
            {total === 0 ? (
              <div className="stats-vazio">
                <p>Nenhum arquivo processado ainda</p>
                <small>Processe o primeiro arquivo para ver as estatísticas</small>
              </div>
            ) : (
              <>
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
                    ></div>
                    <div 
                      className="grafico-fill nao-recepcionado-fill" 
                      style={{ width: `${percNaoRecep}%` }}
                      title={`${stats.naoRecepcionados} não recepcionados (${percNaoRecep}%)`}
                    ></div>
                  </div>
                  <div className="grafico-legenda">
                    <span className="legenda-item">
                      <span className="legenda-cor recepcionado-cor"></span>
                      Recepcionados
                    </span>
                    <span className="legenda-item">
                      <span className="legenda-cor nao-recepcionado-cor"></span>
                      Não Recepcionados
                    </span>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>

        <div className="card">
          <h2>Arquivos Processados ({arquivos.length})</h2>
          {arquivos.length === 0 ? (
            <div className="vazio">
              <p>📄 Nenhum arquivo processado ainda</p>
              <small>Use o formulário acima para processar seu primeiro arquivo</small>
            </div>
          ) : (
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
                            onClick={() => alternarStatus(arq.id, arq.status)}
                            disabled={editandoId === arq.id}
                            className="btn-editar"
                            title="Alterar status"
                          >
                            {editandoId === arq.id ? '...' : '✏️'}
                          </button>
                          <button
                            onClick={() => excluir(arq.id, arq.empresa)}
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
          )}
        </div>
      </div>
    </div>
  );
}

export default App;