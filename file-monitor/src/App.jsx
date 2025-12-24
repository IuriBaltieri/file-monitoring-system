import { useState, useEffect } from 'react';
import FormularioProcessamento from './components/FormularioProcessamento';
import Estatisticas from './components/Estatisticas';
import TabelaArquivos from './components/TabelaArquivos';
import { arquivosApi } from './services/arquivosApi';
import './styles/global.css';

function App() {
  const [arquivos, setArquivos] = useState([]);
  const [stats, setStats] = useState({ recepcionados: 0, naoRecepcionados: 0 });
  const [loading, setLoading] = useState(false);
  const [loadingDados, setLoadingDados] = useState(true);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');
  const [warning, setWarning] = useState('');
  const [editandoId, setEditandoId] = useState(null);

  useEffect(() => {
    carregarDados();
  }, []);

  const carregarDados = async () => {
    try {
      setLoadingDados(true);
      const [dataArquivos, dataStats] = await Promise.all([
        arquivosApi.obterTodos(),
        arquivosApi.obterEstatisticas()
      ]);
      
      setArquivos(dataArquivos);
      setStats(dataStats);
    } catch (err) {
      mostrarErro(err.message);
    } finally {
      setLoadingDados(false);
    }
  };

  const processar = async (conteudo, nomeArquivo) => {
    setLoading(true);
    limparMensagens();

    try {
      const resultado = await arquivosApi.processar(conteudo, nomeArquivo);

      if (resultado.warning) {
        mostrarWarning(resultado.warning);
      } else {
        mostrarSucesso('Arquivo processado e recepcionado com sucesso!');
      }

      carregarDados();
    } catch (err) {
      mostrarErro(err.message);
    } finally {
      setLoading(false);
    }
  };

  const alternarStatus = async (id, statusAtual) => {
    const novoStatus = statusAtual === 'Recepcionado' ? 'Não Recepcionado' : 'Recepcionado';
    
    try {
      setEditandoId(id);
      await arquivosApi.atualizarStatus(id, novoStatus);
      mostrarSucesso('Status atualizado com sucesso!');
      carregarDados();
    } catch (err) {
      mostrarErro(err.message);
    } finally {
      setEditandoId(null);
    }
  };

  const excluir = async (id, empresa) => {
    if (!window.confirm(`Deseja realmente excluir o arquivo da empresa ${empresa}?`)) {
      return;
    }

    try {
      await arquivosApi.excluir(id);
      mostrarSucesso('Arquivo excluído com sucesso!');
      carregarDados();
    } catch (err) {
      mostrarErro(err.message);
    }
  };

  const mostrarErro = (mensagem) => {
    setErro(mensagem);
    setTimeout(() => setErro(''), 4000);
  };

  const mostrarSucesso = (mensagem) => {
    setSucesso(mensagem);
    setTimeout(() => setSucesso(''), 3000);
  };

  const mostrarWarning = (mensagem) => {
    setWarning(mensagem);
    setTimeout(() => setWarning(''), 6000);
  };

  const limparMensagens = () => {
    setErro('');
    setSucesso('');
    setWarning('');
  };

  if (loadingDados) {
    return (
      <div className="app">
        <div className="loading-screen">
          <div className="spinner"></div>
          <p>Carregando...</p>
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
        {erro && <div className="alert alert-erro global-alert">❌ {erro}</div>}
        {sucesso && <div className="alert alert-sucesso global-alert">✅ {sucesso}</div>}
        {warning && <div className="alert alert-warning global-alert">⚠️ {warning}</div>}

        <div className="grid">
          <div className="card">
            <FormularioProcessamento onProcessar={processar} loading={loading} />
          </div>

          <div className="card">
            <Estatisticas stats={stats} />
          </div>
        </div>

        <div className="card">
          <TabelaArquivos
            arquivos={arquivos}
            onAlterarStatus={alternarStatus}
            onExcluir={excluir}
            editandoId={editandoId}
          />
        </div>
      </div>
    </div>
  );
}

export default App;