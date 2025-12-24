"use client";

import { useEffect, useState } from "react";
import axios from "axios";

// --- TIPOS DE DATOS ---
interface Cliente { clienteId: number; tipoDocumento: string; numeroDocumento: string; razonSocialNombre: string; email: string; telefono: string; estado: boolean; }
interface Cuenta { cuentaId: number; numeroCuenta: string; moneda: string; saldo: number; tipoCuenta: string; estado: string; }
interface Transaccion { transaccionId: number; fechaOperacion: string; tipoMovimiento: string; monto: number; saldoHistorico: number; descripcion: string; }

// Tipo para el Usuario Logueado
interface UsuarioSesion { nombre: string; rol: string; }

export default function Home() {
  // --- ESTADO DE SESIÓN (LOGIN) ---
  const [usuarioLogueado, setUsuarioLogueado] = useState<UsuarioSesion | null>(null);
  const [loginForm, setLoginForm] = useState({ usuario: "", clave: "" });
  const [errorLogin, setErrorLogin] = useState("");

  // --- ESTADOS DEL SISTEMA BANCARIO ---
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [cargando, setCargando] = useState(true);
  
  // Modales
  const [mostrarFormCliente, setMostrarFormCliente] = useState(false);
  const [clienteSeleccionado, setClienteSeleccionado] = useState<Cliente | null>(null);
  
  // Datos
  const [nuevoCliente, setNuevoCliente] = useState({ tipoDocumento: "DNI", numeroDocumento: "", razonSocialNombre: "", email: "", telefono: "", direccion: "", estado: true });
  const [cuentasDelCliente, setCuentasDelCliente] = useState<Cuenta[]>([]);
  const [nuevaCuenta, setNuevaCuenta] = useState({ numeroCuenta: "", moneda: "PEN", saldo: 0, tipoCuenta: "AHORROS" });

  // Operaciones
  const [cuentaParaOperar, setCuentaParaOperar] = useState<Cuenta | null>(null);
  const [tipoOperacion, setTipoOperacion] = useState<"Deposito" | "Retiro" | "Transferencia">("Deposito");
  const [montoOperacion, setMontoOperacion] = useState<number>(0);
  const [cuentaDestino, setCuentaDestino] = useState("");

  // Historial
  const [historial, setHistorial] = useState<Transaccion[]>([]);
  const [cuentaEnHistorial, setCuentaEnHistorial] = useState<Cuenta | null>(null);

  // --- EFECTO: CARGAR CLIENTES SOLO SI HAY LOGIN ---
  useEffect(() => { 
    if (usuarioLogueado) {
      cargarClientes(); 
    }
  }, [usuarioLogueado]);

  // --- FUNCIÓN DE LOGIN ---
  const iniciarSesion = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorLogin("");
    try {
      const res = await axios.post("https://sistema-financiero-fullstack.onrender.com/api/Login", {
        usuario: loginForm.usuario,
        clave: loginForm.clave
      });
      // Si pasa, guardamos al usuario
      setUsuarioLogueado({ nombre: res.data.usuario, rol: res.data.rol });
    } catch (error) {
      setErrorLogin("❌ Usuario o contraseña incorrectos.");
    }
  };

  const cerrarSesion = () => {
    setUsuarioLogueado(null);
    setLoginForm({ usuario: "", clave: "" });
    setClientes([]);
  };

  // --- FUNCIONES DEL SISTEMA ---
  const cargarClientes = async () => {
    try {
      const res = await axios.get("https://sistema-financiero-fullstack.onrender.com/api/Clientes");
      setClientes(res.data);
      setCargando(false);
    } catch (error) { console.error(error); setCargando(false); }
  };

  const guardarCliente = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await axios.post("https://sistema-financiero-fullstack.onrender.com/api/Clientes", nuevoCliente);
      alert("✅ Cliente registrado");
      setMostrarFormCliente(false);
      cargarClientes();
      setNuevoCliente({ ...nuevoCliente, numeroDocumento: "", razonSocialNombre: "" });
    } catch (error) { alert("Error al guardar cliente"); }
  };

  const abrirCuentas = async (cliente: Cliente) => {
    setClienteSeleccionado(cliente);
    const randomNum = "100-" + Math.floor(Math.random() * 900 + 100) + "-" + Math.floor(Math.random() * 9000 + 1000);
    setNuevaCuenta({ ...nuevaCuenta, numeroCuenta: randomNum, saldo: 0 });
    cargarCuentasDeCliente(cliente.clienteId);
  };

  const cargarCuentasDeCliente = async (id: number) => {
    try {
      const res = await axios.get(`https://sistema-financiero-fullstack.onrender.com/api/Cuentas/PorCliente/${id}`);
      setCuentasDelCliente(res.data);
    } catch (error) { setCuentasDelCliente([]); }
  }

  const crearCuenta = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!clienteSeleccionado) return;
    try {
      await axios.post("https://sistema-financiero-fullstack.onrender.com/api/Cuentas", { clienteId: clienteSeleccionado.clienteId, ...nuevaCuenta });
      alert("💰 Cuenta creada");
      cargarCuentasDeCliente(clienteSeleccionado.clienteId);
    } catch (error) { alert("Error creando cuenta"); }
  };

  const abrirOperacion = (cuenta: Cuenta, tipo: "Deposito" | "Retiro" | "Transferencia") => {
    setCuentaParaOperar(cuenta);
    setTipoOperacion(tipo);
    setMontoOperacion(0);
    setCuentaDestino("");
  };

  const ejecutarOperacion = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!cuentaParaOperar) return;
    try {
      if (tipoOperacion === "Transferencia") {
        await axios.post(`https://sistema-financiero-fullstack.onrender.com/api/Transacciones/Transferencia`, {
          cuentaOrigenId: cuentaParaOperar.cuentaId, numeroCuentaDestino: cuentaDestino, monto: montoOperacion
        });
      } else {
        await axios.post(`https://sistema-financiero-fullstack.onrender.com/api/Transacciones/${tipoOperacion}`, {
          cuentaId: cuentaParaOperar.cuentaId, monto: montoOperacion
        });
      }
      alert(`✅ ${tipoOperacion} exitosa!`);
      setCuentaParaOperar(null);
      if (clienteSeleccionado) cargarCuentasDeCliente(clienteSeleccionado.clienteId);
    } catch (error: any) {
      alert("❌ Error: " + (error.response?.data || "Fallo en la operación"));
    }
  };

  const verHistorial = async (cuenta: Cuenta) => {
    setCuentaEnHistorial(cuenta);
    try {
      const res = await axios.get(`https://sistema-financiero-fullstack.onrender.com/api/Transacciones/PorCuenta/${cuenta.cuentaId}`);
      setHistorial(res.data);
    } catch (error) { setHistorial([]); }
  };

  // --- RENDERIZADO: ¿LOGIN O SISTEMA? ---
  if (!usuarioLogueado) {
    return (
      <div className="min-h-screen bg-slate-900 flex flex-col justify-center items-center p-4">
        <div className="bg-white p-8 rounded-xl shadow-2xl w-full max-w-md animate-fade-in">
          <h1 className="text-3xl font-bold text-center text-slate-800 mb-2">🏦 Banco Seguro</h1>
          <p className="text-center text-slate-500 mb-8">Acceso exclusivo para personal autorizado</p>
          
          <form onSubmit={iniciarSesion} className="space-y-4">
            <div>
              <label className="block text-sm font-bold text-slate-700 mb-1">Usuario</label>
              <input 
                type="text" 
                className="w-full border p-3 rounded-lg text-black focus:ring-2 focus:ring-blue-500 outline-none" 
                placeholder="Ej: admin"
                value={loginForm.usuario}
                onChange={e => setLoginForm({...loginForm, usuario: e.target.value})}
              />
            </div>
            <div>
              <label className="block text-sm font-bold text-slate-700 mb-1">Contraseña</label>
              <input 
                type="password" 
                className="w-full border p-3 rounded-lg text-black focus:ring-2 focus:ring-blue-500 outline-none" 
                placeholder="••••••"
                value={loginForm.clave}
                onChange={e => setLoginForm({...loginForm, clave: e.target.value})}
              />
            </div>
            
            {errorLogin && <div className="text-red-500 text-sm text-center font-bold bg-red-50 p-2 rounded">{errorLogin}</div>}

            <button className="w-full bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 rounded-lg shadow-lg transition transform hover:scale-105">
              🔐 Ingresar al Sistema
            </button>
          </form>
          <div className="mt-6 text-center text-xs text-slate-400">Sistema Financiero v1.0 • Jeanpier Dev</div>
        </div>
      </div>
    );
  }

  // --- PANTALLA DEL SISTEMA BANCARIO (SOLO SI ESTÁ LOGUEADO) ---
  return (
    <div className="min-h-screen bg-slate-100 p-8 font-sans">
      <div className="max-w-6xl mx-auto">
        {/* HEADER CON USUARIO Y LOGOUT */}
        <div className="flex justify-between items-center mb-8 bg-white p-4 rounded-xl shadow-sm border border-slate-200">
          <div>
            <h1 className="text-2xl font-bold text-slate-800 flex items-center gap-2">
              🏦 Core Bancario <span className="bg-yellow-400 text-yellow-900 text-xs px-2 py-1 rounded shadow">PRO</span>
            </h1>
            <p className="text-sm text-slate-500">Bienvenido, <span className="font-bold text-blue-600">{usuarioLogueado.nombre}</span> ({usuarioLogueado.rol})</p>
          </div>
          <button onClick={cerrarSesion} className="bg-red-50 text-red-600 px-4 py-2 rounded-lg font-bold hover:bg-red-100 border border-red-200 transition">
            🚪 Cerrar Sesión
          </button>
        </div>

        <button onClick={() => setMostrarFormCliente(!mostrarFormCliente)} className="bg-slate-800 text-white px-5 py-2 rounded mb-6 font-medium shadow hover:bg-black transition">
          {mostrarFormCliente ? "Cerrar Panel" : "+ Nuevo Cliente"}
        </button>

        {mostrarFormCliente && (
          <form onSubmit={guardarCliente} className="bg-white p-6 rounded shadow mb-6 grid grid-cols-3 gap-4 border-l-4 border-slate-800 animate-fade-in-down">
            <input placeholder="DNI" className="border p-2 text-black" value={nuevoCliente.numeroDocumento} onChange={e => setNuevoCliente({...nuevoCliente, numeroDocumento: e.target.value})} />
            <input placeholder="Nombre" className="border p-2 text-black uppercase" value={nuevoCliente.razonSocialNombre} onChange={e => setNuevoCliente({...nuevoCliente, razonSocialNombre: e.target.value})} />
            <button className="bg-green-600 text-white font-bold p-2 rounded">Guardar</button>
          </form>
        )}

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {clientes.map(c => (
            <div key={c.clienteId} className="bg-white p-5 rounded-xl shadow border border-slate-200 hover:shadow-lg transition">
              <h3 className="font-bold text-lg text-slate-800">{c.razonSocialNombre}</h3>
              <p className="text-slate-500 mb-4">{c.tipoDocumento}: {c.numeroDocumento}</p>
              <button onClick={() => abrirCuentas(c)} className="bg-indigo-50 text-indigo-700 w-full py-2 rounded font-bold hover:bg-indigo-100 transition">
                Gestionar Cuentas &rarr;
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* MODALES (Mismo código de antes) */}
      {clienteSeleccionado && !cuentaParaOperar && !cuentaEnHistorial && (
        <div className="fixed inset-0 bg-black/50 flex justify-center items-center z-40 animate-fade-in">
          <div className="bg-white w-full max-w-4xl p-8 rounded-xl shadow-2xl">
            <div className="flex justify-between mb-6 border-b pb-4">
              <div><h2 className="text-2xl font-bold text-slate-800">{clienteSeleccionado.razonSocialNombre}</h2><p className="text-slate-500">Gestión de Productos</p></div>
              <button onClick={() => setClienteSeleccionado(null)} className="text-red-500 font-bold hover:bg-red-50 px-3 rounded">CERRAR</button>
            </div>
            <form onSubmit={crearCuenta} className="flex gap-3 mb-8 bg-slate-50 p-4 rounded-lg border border-slate-200">
              <input value={nuevaCuenta.numeroCuenta} className="border p-2 flex-1 text-black font-mono" placeholder="Nro Cuenta" disabled />
              <select value={nuevaCuenta.moneda} onChange={e => setNuevaCuenta({...nuevaCuenta, moneda: e.target.value})} className="border p-2 text-black font-medium">
                <option value="PEN">Soles (S/)</option><option value="USD">Dólares ($)</option>
              </select>
              <button className="bg-green-600 text-white px-6 rounded font-bold shadow hover:bg-green-700">+ Crear Cuenta</button>
            </form>
            <div className="space-y-4 max-h-[50vh] overflow-auto pr-2">
              {cuentasDelCliente.map(cta => (
                <div key={cta.cuentaId} className="border p-5 rounded-lg flex justify-between items-center bg-white shadow-sm hover:border-indigo-300 transition group">
                  <div>
                    <div className="font-mono text-slate-500 text-sm">{cta.numeroCuenta} • {cta.tipoCuenta}</div>
                    <div className={`text-3xl font-bold ${cta.moneda === 'PEN' ? 'text-slate-800' : 'text-green-700'}`}>{cta.moneda === 'PEN' ? 'S/.' : '$'} {cta.saldo.toFixed(2)}</div>
                  </div>
                  <div className="flex gap-2">
                    <button onClick={() => abrirOperacion(cta, "Deposito")} className="bg-green-100 text-green-800 px-3 py-1 rounded font-bold text-xs">📥</button>
                    <button onClick={() => abrirOperacion(cta, "Retiro")} className="bg-red-100 text-red-800 px-3 py-1 rounded font-bold text-xs">📤</button>
                    <button onClick={() => abrirOperacion(cta, "Transferencia")} className="bg-purple-100 text-purple-800 px-3 py-1 rounded font-bold text-xs">➡️</button>
                    <button onClick={() => verHistorial(cta)} className="bg-slate-800 text-white px-3 py-1 rounded font-bold text-xs shadow hover:bg-black">📜 Historial</button>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* MODAL HISTORIAL */}
      {cuentaEnHistorial && (
        <div className="fixed inset-0 bg-black/60 flex justify-center items-center z-50 animate-fade-in">
          <div className="bg-white w-full max-w-3xl h-[80vh] rounded-xl shadow-2xl flex flex-col">
            <div className="bg-slate-800 p-6 rounded-t-xl flex justify-between items-center text-white">
              <div><h2 className="text-xl font-bold">Estado de Cuenta</h2><p className="opacity-80 font-mono text-sm">{cuentaEnHistorial.numeroCuenta}</p></div>
              <button onClick={() => setCuentaEnHistorial(null)} className="bg-white/20 hover:bg-white/30 p-2 rounded">✕ Cerrar</button>
            </div>
            <div className="flex-1 overflow-auto p-6 bg-slate-50">
              {historial.length === 0 ? (<div className="text-center text-slate-400 mt-10">No hay movimientos.</div>) : (
                <table className="w-full text-left border-collapse">
                  <thead className="text-xs uppercase text-slate-500 font-bold border-b"><tr><th className="p-3">Fecha</th><th className="p-3">Concepto</th><th className="p-3 text-right">Monto</th><th className="p-3 text-right">Saldo</th></tr></thead>
                  <tbody className="text-sm">
                    {historial.map(tx => (
                      <tr key={tx.transaccionId} className="border-b border-slate-200 hover:bg-white transition">
                        <td className="p-3 text-slate-500">{new Date(tx.fechaOperacion).toLocaleString()}</td>
                        <td className="p-3"><div className="font-bold text-slate-700">{tx.tipoMovimiento.replace('_', ' ')}</div><div className="text-xs text-slate-400">{tx.descripcion}</div></td>
                        <td className={`p-3 text-right font-bold ${['DEPOSITO', 'TRANSFERENCIA_ENTRADA'].includes(tx.tipoMovimiento) ? 'text-green-600' : 'text-red-600'}`}>{['DEPOSITO', 'TRANSFERENCIA_ENTRADA'].includes(tx.tipoMovimiento) ? '+' : '-'} {tx.monto.toFixed(2)}</td>
                        <td className="p-3 text-right text-slate-800 font-mono">{tx.saldoHistorico.toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          </div>
        </div>
      )}

      {/* MODAL OPERACIONES */}
      {cuentaParaOperar && (
        <div className="fixed inset-0 bg-black/60 flex justify-center items-center z-50">
          <div className="bg-white p-8 rounded-xl shadow-2xl w-[450px]">
            <h2 className="text-xl font-bold text-center mb-1 text-slate-800 uppercase tracking-wide">{tipoOperacion}</h2>
            <p className="text-center text-slate-400 mb-6 font-mono text-sm">Origen: {cuentaParaOperar.numeroCuenta}</p>
            <form onSubmit={ejecutarOperacion}>
              {tipoOperacion === "Transferencia" && (
                <div className="mb-4">
                  <label className="block text-xs font-bold text-slate-500 mb-1 uppercase">Cuenta Destino</label>
                  <input required placeholder="Ej: 100-xxx-xxxx" className="w-full p-3 border rounded text-black font-mono bg-yellow-50" value={cuentaDestino} onChange={e => setCuentaDestino(e.target.value)} />
                </div>
              )}
              <label className="block text-xs font-bold text-slate-500 mb-1 uppercase">Monto</label>
              <input autoFocus type="number" step="0.01" className="w-full text-4xl font-bold text-center p-4 border rounded mb-8 text-slate-800" value={montoOperacion} onChange={e => setMontoOperacion(parseFloat(e.target.value))} />
              <div className="flex gap-3">
                <button type="button" onClick={() => setCuentaParaOperar(null)} className="flex-1 py-3 bg-gray-100 text-gray-600 rounded-lg font-bold">Cancelar</button>
                <button type="submit" className="flex-1 py-3 bg-slate-800 text-white rounded-lg font-bold shadow-lg hover:bg-black">Confirmar</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}