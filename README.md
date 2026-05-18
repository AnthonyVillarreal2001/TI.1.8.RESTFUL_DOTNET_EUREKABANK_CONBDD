# WS_EUREKABANK_RESTFULLNOTNET_GR04

Solución estilo Visual Studio 2022 compuesta por:

- 01.SERVIDOR/WS_EB_DOTNET_REST_Servidor (ASP.NET Core Web API .NET 8)
- 02.CLICON/EB_DOTNET_REST_CliCon (Consola .NET 8)

## Endpoints principales

Base: http://localhost:5043 (o https://localhost:7043)

- GET api/CoreBancario/cuentas/{cuenta}/movimientos
- POST api/CoreBancario/deposito { cuenta, importe }
- POST api/CoreBancario/retiro { cuenta, importe }
- POST api/CoreBancario/transferencia { origen, destino, importe }

Swagger/UI: /swagger

## Configuración de BD (respetando scripts)

Se usa SQL Server LocalDB y la BD `EUREKABANK` (misma que el proyecto SOAP .NET).

1) Ejecuta primero los scripts en este orden desde `03.BDD`:
	- `Crea_BD.sql`
	- `Carga_Datos.sql`
2) Verifica la cadena en `01.SERVIDOR/WS_EB_DOTNET_REST_Servidor/appsettings.json` si tu instancia cambia.

El modelo de datos y las operaciones de la API usan las mismas tablas y códigos de tipo de movimiento del script: 
- Depósito: `chr_tipocodigo = '003'`
- Retiro: `chr_tipocodigo = '004'`
- Transferencia salida: `chr_tipocodigo = '009'`
- Transferencia ingreso: `chr_tipocodigo = '008'`

## Cómo ejecutar (PowerShell)

Servidor (API):

```powershell
cd WS_EUREKABANK_RESTFULLNOTNET_GR04\01.SERVIDOR\WS_EB_DOTNET_REST_Servidor
$env:ASPNETCORE_URLS="http://localhost:5043;https://localhost:7043"
dotnet run
```

Cliente consola:

```powershell
cd WS_EUREKABANK_RESTFULLNOTNET_GR04\02.CLICON\EB_DOTNET_REST_CliCon
$env:EB_API_BASE="http://localhost:5043/"
dotnet run
```

Ejemplos de payloads (JSON):

- Depósito/Retiro
```json
{ "cuenta": "00200001", "importe": 100.00, "empleado": "9999" }
```

- Transferencia
```json
{ "origen": "00200001", "destino": "00100001", "importe": 50.00, "empleado": "9999" }
```

Si no tienes el SDK .NET 8, instálalo desde https://dotnet.microsoft.com/download/dotnet/8.0.

## Notas

- La tabla Movimiento debe existir con columnas compatibles: Id (int identity), Fecha (datetime), Tipo (varchar), CuentaOrigen (varchar), CuentaDestino (varchar, null), Importe (decimal), Moneda (varchar).
- La lógica de saldo/validación es mínima y de ejemplo; puedes ampliarla (validaciones de cuenta, saldos, etc.).
