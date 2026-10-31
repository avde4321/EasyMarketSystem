// QA visual aislado. Solo loopback, sin SQL, firmas SRI, credenciales ni cargos reales.
// Ejecutar cliente Blazor en :5177 y este proxy con node. Abrir :5178/subscription-expired.
import http from 'node:http';
const company = 'aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa';
const owner = 'bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb';
let pending = [], submissions = 0, active = false;
const plans = [
  { id: 1, nombre: 'Básico', precioMensual: 29.99, limiteFacturasMensuales: 100, limiteUsuarios: 3, limiteSucursales: 1, permiteModuloSRI: true, permiteModuloKardexAvanzado: false, esActivo: true },
  { id: 2, nombre: 'Pro', precioMensual: 79.99, limiteFacturasMensuales: 1000, limiteUsuarios: 15, limiteSucursales: 5, permiteModuloSRI: true, permiteModuloKardexAvanzado: true, esActivo: true },
  { id: 3, nombre: 'Enterprise', precioMensual: 0, limiteFacturasMensuales: 0, limiteUsuarios: 0, limiteSucursales: 0, permiteModuloSRI: true, permiteModuloKardexAvanzado: true, esActivo: true }
];
const token = [Buffer.from(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).toString('base64url'), Buffer.from(JSON.stringify({
  sub: owner, unique_name: 'QA Renovación', exp: Math.floor(Date.now()/1000)+7200, default_empresa_id: company,
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': owner,
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'QA Renovación',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'AdministradorEmpresa',
  permission: ['empresa.configurar', 'dashboard.ver'], identification: 'QA'
})).toString('base64url'), Buffer.from('not-a-real-signature').toString('base64url')].join('.');
const json = (res, value, status=200) => { res.writeHead(status, { 'Content-Type':'application/json', 'Cache-Control':'no-store' }); res.end(JSON.stringify(value)); };
http.createServer(async (req,res) => {
  const path = new URL(req.url, 'http://127.0.0.1:5178').pathname;
  if (path === '/__qa/status') return json(res, { submissions, pending, active });
  if (/^\/appsettings.*\.json$/.test(path)) return json(res, { Api: { BaseUrl:'http://127.0.0.1:5178/' } });
  if (path === '/api/saas/plans') return json(res, plans);
  if (path === '/api/empresa') return json(res, [{ id:company, razonSocial:'Empresa de prueba QA', nombreComercial:'QA', ruc:'0000000000000', ambienteSri:'1', isActive:true, isDefault:true }]);
  if (path === '/api/v1/saas/billing/portal') return json(res, { suscripcionActiva:active, estadoSuscripcion:active?'Activa':'PendientePago', fechaVencimiento:'2026-12-01T00:00:00Z', pagosPendientes:pending, ultimoPago:pending[0]??null });
  if (path === '/api/saas/subscription-status') return json(res, { activa:active, codigo:active?null:'SUBSCRIPTION_REQUIRED' });
  if (path === '/api/v1/saas/billing/checkout') return json(res, { disponible:false, mensaje:'El pago con tarjeta aún no está habilitado. No se ha realizado ningún cobro.' });
  if (path === '/api/v1/saas/billing/register-transfer' && req.method === 'POST') {
    let body=''; for await (const part of req) body+=part;
    const dto=JSON.parse(body); submissions++;
    const payment={ id:'cccccccc-cccc-4ccc-8ccc-cccccccccccc', empresaId:company, saasPlanId:dto.saasPlanId, monto:dto.monto,
      fechaPago:new Date().toISOString(), fechaPagoInformada:dto.fechaPago, metodoPago:'TransferenciaBancaria', estadoPago:'Pendiente',
      comprobanteReferencia:dto.comprobanteReferencia, fechaConfirmacion:null };
    pending=[payment]; return setTimeout(()=>json(res,payment),700);
  }
  if (path.startsWith('/api/')) return json(res, {});
  const upstream=http.request({hostname:'127.0.0.1',port:5177,path:req.url,method:req.method,headers:{...req.headers,host:'127.0.0.1:5177','accept-encoding':'identity'}}, proxy=>{
    if (proxy.headers['content-type']?.includes('text/html')) {
      let html=''; proxy.setEncoding('utf8'); proxy.on('data',part=>html+=part); proxy.on('end',()=>{
        const boot=`<script>localStorage.setItem('testdeia.security.token',${JSON.stringify(token)});localStorage.setItem('testdeia.empresa.activa','${company}');</script>`;
        res.writeHead(200,{'Content-Type':'text/html; charset=utf-8','Cache-Control':'no-store'}); res.end(html.replace('<head>','<head>'+boot));
      });
    } else { res.writeHead(proxy.statusCode,proxy.headers);proxy.pipe(res); }
  });
  upstream.on('error',()=>{res.writeHead(502);res.end('Inicia el cliente Blazor en 127.0.0.1:5177.');});req.pipe(upstream);
}).listen(5178,'127.0.0.1',()=>process.stdout.write('QA aislado: http://127.0.0.1:5178/subscription-expired\n'));
