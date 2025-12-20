import { useState } from 'react';
import './styles.css';

const EXEMPLOS = {
  tipo0: '00987564321201906262019062520190625000000100UfCard  ',
  tipo1: '12019052632165487FagammonCard0002451'
};

export default function FormularioProcessamento({ onProcessar, loading }) {
  const [conteudo, setConteudo] = useState('');
  const [nomeArquivo, setNomeArquivo] = useState('arquivo.txt');

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!conteudo.trim()) return;
    onProcessar(conteudo.trim(), nomeArquivo.trim());
    setConteudo('');
  };

  return (
    <div className="formulario-container">
      <h2>Processar Arquivo</h2>
      <form onSubmit={handleSubmit}>
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
            onClick={() => setConteudo(EXEMPLOS.tipo0)}
            disabled={loading}
          >
            Exemplo Tipo 0
          </button>
          <button 
            type="button" 
            onClick={() => setConteudo(EXEMPLOS.tipo1)}
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
  );
}