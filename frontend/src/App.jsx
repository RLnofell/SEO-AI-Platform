import { useState, useRef, useEffect } from 'react';
import axios from 'axios';
import { motion, AnimatePresence } from 'framer-motion';
import { Search, Loader2, Cpu, FileText, CheckCircle } from 'lucide-react';
import './index.css';

function App() {
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [logs, setLogs] = useState([]);
  const [result, setResult] = useState(null);
  const logsEndRef = useRef(null);

  // Auto scroll logs
  useEffect(() => {
    if (logsEndRef.current) {
      logsEndRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [logs]);

  const handleRunAgent = async (e) => {
    e.preventDefault();
    if (!input.trim()) return;

    setLoading(true);
    setLogs([`Khởi động tác vụ: "${input}"...`]);
    setResult(null);

    try {
      const response = await axios.post('http://localhost:5000/api/agent/run', {
        input: input
      });
      
      const { logs: responseLogs, finalArticle, densityResult, postResult } = response.data;
      
      setLogs(responseLogs);
      setResult({ finalArticle, densityResult, postResult });
      
    } catch (error) {
      const errorMsg = error.response?.data?.error || error.message;
      setLogs(prev => [...prev, `[LỖI] ${errorMsg}`]);
      if (error.response?.data?.logs) {
        setLogs(error.response.data.logs);
      }
    } finally {
      setLoading(false);
    }
  };

  const getLogClass = (log) => {
    if (log.includes('[RAG Plugin]') || log.includes('[Seo Plugin]')) return 'highlight';
    if (log.includes('thành công') || log.includes('[v]')) return 'success';
    return '';
  };

  return (
    <div className="app-container">
      <header>
        <h1>AI SEO Agent</h1>
        <p className="subtitle">Hệ thống tạo nội dung chuẩn SEO tự động đa luồng</p>
      </header>

      <form className="input-section" onSubmit={handleRunAgent}>
        <input
          type="text"
          className="cyber-input"
          placeholder="Nhập yêu cầu (VD: 'Làm SEO từ khóa Xe Cẩu Thắng Hiền')"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          disabled={loading}
        />
        <button className="cyber-btn" type="submit" disabled={loading || !input.trim()}>
          {loading ? <Loader2 className="spinner" size={20} /> : <Search size={20} />}
          {loading ? 'Đang phân tích...' : 'Bắt đầu SEO'}
        </button>
      </form>

      <div className="main-content">
        <div className="glass-panel">
          <h2 style={{ display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '1rem', fontSize: '1.2rem', color: 'var(--accent-cyan)' }}>
            <Cpu size={20} /> Agent Workflow Logs
          </h2>
          <div className="log-container">
            {logs.length === 0 && <span style={{ color: 'var(--text-secondary)' }}>Chưa có tiến trình nào...</span>}
            <AnimatePresence>
              {logs.map((log, index) => (
                <motion.div 
                  key={index}
                  initial={{ opacity: 0, x: -10 }}
                  animate={{ opacity: 1, x: 0 }}
                  className={`log-entry ${getLogClass(log)}`}
                >
                  {log}
                </motion.div>
              ))}
            </AnimatePresence>
            <div ref={logsEndRef} />
          </div>
        </div>

        <div className="glass-panel result-container">
          <h2 style={{ display: 'flex', alignItems: 'center', gap: '8px', fontSize: '1.2rem', color: 'var(--success)' }}>
            <FileText size={20} /> Kết Quả Sinh Ra
          </h2>
          
          {!result && !loading && (
            <div style={{ color: 'var(--text-secondary)', textAlign: 'center', marginTop: '2rem' }}>
              Hãy nhập từ khóa để Agent tạo nội dung
            </div>
          )}

          {loading && (
            <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100%', gap: '1rem', color: 'var(--accent-cyan)' }}>
              <Loader2 className="spinner" size={40} />
              <p>Agent đang suy luận và thu thập dữ liệu...</p>
            </div>
          )}

          {result && (
            <motion.div 
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              className="result-content"
              style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}
            >
              {(result.densityResult || result.postResult) && (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                  {result.densityResult && (
                    <div className="stat-box">
                      <strong style={{ display: 'flex', alignItems: 'center', gap: '4px' }}><CheckCircle size={16}/> Phân tích từ khóa:</strong>
                      <span>{result.densityResult}</span>
                    </div>
                  )}
                  {result.postResult && (
                    <div className="stat-box">
                      <strong style={{ display: 'flex', alignItems: 'center', gap: '4px' }}><CheckCircle size={16}/> WordPress:</strong>
                      <span>{result.postResult}</span>
                    </div>
                  )}
                </div>
              )}
              
              <div className="article-box">
                {result.finalArticle}
              </div>
            </motion.div>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
