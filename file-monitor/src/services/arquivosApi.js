const API_URL = 'http://localhost:5130/api/arquivos';

export const arquivosApi = {
  obterTodos: async () => {
    const response = await fetch(API_URL);
    if (!response.ok) throw new Error('Erro ao carregar arquivos');
    return response.json();
  },

  obterEstatisticas: async () => {
    const response = await fetch(`${API_URL}/estatisticas`);
    if (!response.ok) throw new Error('Erro ao carregar estatísticas');
    return response.json();
  },

  processar: async (conteudo, nomeArquivo) => {
    const response = await fetch(API_URL, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ conteudo, nomeArquivo })
    });

    const data = await response.json();
    if (!response.ok) throw new Error(data.erro || 'Erro ao processar arquivo');
    
    return data;
  },

  atualizarStatus: async (id, novoStatus) => {
    const response = await fetch(`${API_URL}/${id}/status`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ status: novoStatus })
    });

    const data = await response.json();
    if (!response.ok) throw new Error(data.erro || 'Erro ao atualizar status');
    
    return data;
  },

  excluir: async (id) => {
    const response = await fetch(`${API_URL}/${id}`, {
      method: 'DELETE'
    });

    const data = await response.json();
    if (!response.ok) throw new Error(data.erro || 'Erro ao excluir arquivo');
    
    return data;
  }
};