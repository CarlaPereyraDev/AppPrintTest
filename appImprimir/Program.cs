// See https://aka.ms/new-console-template for more information

//var _imp = new appImprimir.imprTerm();

//_imp.mains();


//Servidor
using appImprimir;

///Imprimir ticket
var _server = new ServidorLocal();
await _server.ServerLocal();


