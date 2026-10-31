// QA visual solo loopback. API ficticia en memoria; no accede a SQL/SRI ni acepta escrituras.
// Ejecutar cliente en 127.0.0.1:5177 y este proxy; abrir 127.0.0.1:5179/clientes.
import http from 'node:http';
const company='aaaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaaaa', owner='bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbbb';
const payload={ sub:owner, exp:Math.floor(Date.now()/1000)+7200, default_empresa_id:company,
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier':owner,
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name':'QA Clientes',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role':'AdministradorEmpresa', permission:['clientes.ver','empresa.configurar'] };
const token=[{alg:'HS256',typ:'JWT'},payload].map(x=>Buffer.from(JSON.stringify(x)).toString('base64url')).join('.')+'.not-a-real-signature';
const json=(res,data,status=200)=>{res.writeHead(status,{'Content-Type':'application/json','Cache-Control':'no-store'});res.end(JSON.stringify(data));};
http.createServer((req,res)=>{
  const path=new URL(req.url,'http://127.0.0.1:5179').pathname;
  if(/^\/appsettings.*\.json$/.test(path)) return json(res,{Api:{BaseUrl:'http://127.0.0.1:5179/'}});
  if(path.startsWith('/api/')) {
    if(req.method!=='GET') return json(res,{message:'QA: las escrituras están deshabilitadas.'},409);
    if(path==='/api/saas/subscription-status') return json(res,{activa:true});
    if(path==='/api/empresa') return json(res,[{id:company,razonSocial:'Empresa ficticia QA',ruc:'0000000000000',ambienteSri:'1',isActive:true,isDefault:true}]);
    if(path==='/api/clientes'||path==='/api/personas') return json(res,{items:[],totalCount:0,skip:0,take:10});
    if(path==='/api/personas/buscar') return json(res,null,404);
    if(path.startsWith('/api/geografia/')) return json(res,[]);
    if(path.includes('/TIPO_IDENTIFICACION/')) return json(res,[['04','RUC'],['05','Cédula'],['06','Pasaporte'],['07','Consumidor final']].map(([codigo,nombre])=>({codigo,nombre,isActive:true})));
    return json(res,{});
  }
  const upstream=http.request({hostname:'127.0.0.1',port:5177,path:req.url,method:req.method,headers:{...req.headers,host:'127.0.0.1:5177','accept-encoding':'identity'}},proxy=>{
    if(proxy.headers['content-type']?.includes('text/html')) {
      let html='';proxy.setEncoding('utf8');proxy.on('data',part=>html+=part);proxy.on('end',()=>{
        res.writeHead(200,{'Content-Type':'text/html; charset=utf-8','Cache-Control':'no-store'});
        res.end(html.replace('<head>',`<head><script>localStorage.setItem('testdeia.security.token',${JSON.stringify(token)});localStorage.setItem('testdeia.empresa.activa','${company}');</script>`));
      });
    } else {res.writeHead(proxy.statusCode,proxy.headers);proxy.pipe(res);}
  });
  upstream.on('error',()=>{res.writeHead(502);res.end('Iniciar cliente en :5177');});req.pipe(upstream);
}).listen(5179,'127.0.0.1',()=>process.stdout.write('QA clientes: http://127.0.0.1:5179/clientes\n'));
